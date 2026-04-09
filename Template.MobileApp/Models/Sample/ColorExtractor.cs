namespace Template.MobileApp.Models.Sample;

using System.Runtime.InteropServices;

public readonly record struct ColorCount(byte R, byte G, byte B, long Count);

public sealed unsafe class ColorExtractor : IDisposable
{
    // ------------------------------------------------------------
    // Struct
    // ------------------------------------------------------------

    internal readonly record struct Rgb24(byte R, byte G, byte B);

    internal struct ColorBox(int start, int length)
    {
        public int Start { get; } = start;

        public int Length { get; } = length;

        public byte SortChannel { get; set; }

        public long Priority { get; set; }

        public long Weight { get; set; }

        public Rgb24 Average { get; set; }
    }

    internal struct MergeCluster(byte seedR, byte seedG, byte seedB, long sumR, long sumG, long sumB, long count, int nextBucketIndex)
    {
        public byte SeedR { get; } = seedR;

        public byte SeedG { get; } = seedG;

        public byte SeedB { get; } = seedB;

        public long SumR { get; set; } = sumR;

        public long SumG { get; set; } = sumG;

        public long SumB { get; set; } = sumB;

        public long Count { get; set; } = count;

        public int NextBucketIndex { get; } = nextBucketIndex;
    }

    // ------------------------------------------------------------
    // Const
    // ------------------------------------------------------------

    private const int NoCluster = -1;

    // ------------------------------------------------------------
    // Field
    // ------------------------------------------------------------

    private readonly uint* samplingBuffer;

    private readonly int[] samplingWeights;

    private readonly int samplingSize;

    private readonly uint[] sortBuffer;

    private readonly int[] sortWeights;

    private bool disposed;

    // ------------------------------------------------------------
    // Property
    // ------------------------------------------------------------

    public int SampleWidth { get; private set; }

    public int SampleHeight { get; private set; }

    public int SampleCount => SampleWidth * SampleHeight;

    // ------------------------------------------------------------
    // Constructor
    // ------------------------------------------------------------

    public ColorExtractor(int samplingSize = 256)
    {
        this.samplingSize = samplingSize;
        var samplingCapacity = samplingSize * samplingSize;
        samplingBuffer = (uint*)NativeMemory.Alloc((nuint)samplingCapacity, sizeof(uint));
        samplingWeights = new int[samplingCapacity];
        sortBuffer = new uint[samplingCapacity];
        sortWeights = new int[samplingCapacity];
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        NativeMemory.Free(samplingBuffer);
        disposed = true;
    }

    // ------------------------------------------------------------
    // Extract
    // ------------------------------------------------------------

    public List<ColorCount> Extract(SKBitmap source, int colorCount = 16, double mergeDistance = 0)
    {
        if ((source.ColorType != SKColorType.Bgra8888) && (source.ColorType != SKColorType.Rgba8888))
        {
            throw new NotSupportedException($"Unsupported color type. colorType=[{source.ColorType}]");
        }

        SamplePixels(source);
        var list = QuantizeSampledBuffer(colorCount);
        if ((list.Count > 0) && (mergeDistance > 0))
        {
            list = MergeSimilarColors(list, mergeDistance);
        }

        return list;
    }

    // ------------------------------------------------------------
    // Sample
    // ------------------------------------------------------------

    private void SamplePixels(SKBitmap source)
    {
        var srcWidth = source.Width;
        var srcHeight = source.Height;

        SampleWidth = Math.Min(samplingSize, srcWidth);
        SampleHeight = Math.Min(samplingSize, srcHeight);

        var rowBytes = source.RowBytes;
        var basePointer = (byte*)source.GetPixels();
        if (source.ColorType == SKColorType.Bgra8888)
        {
            SampleBgraPixels(basePointer, rowBytes, srcWidth, srcHeight);
        }
        else
        {
            SampleRgbaPixels(basePointer, rowBytes, srcWidth, srcHeight);
        }
    }

    private void SampleBgraPixels(byte* basePointer, int rowBytes, int srcWidth, int srcHeight)
    {
        for (var ySample = 0; ySample < SampleHeight; ySample++)
        {
            var srcYStart = ySample * srcHeight / SampleHeight;
            var srcYEnd = Math.Max(srcYStart + 1, ((ySample + 1) * srcHeight) / SampleHeight);
            var yStep = Math.Max(1, (srcYEnd - srcYStart) / 2);

            for (var xSample = 0; xSample < SampleWidth; xSample++)
            {
                var srcXStart = xSample * srcWidth / SampleWidth;
                var srcXEnd = Math.Max(srcXStart + 1, ((xSample + 1) * srcWidth) / SampleWidth);
                var xStep = Math.Max(1, (srcXEnd - srcXStart) / 2);
                var sampleIndex = (ySample * SampleWidth) + xSample;
                samplingWeights[sampleIndex] = (srcXEnd - srcXStart) * (srcYEnd - srcYStart);

                samplingBuffer[sampleIndex] = FindBestPixel(basePointer, rowBytes, srcYStart, srcYEnd, yStep, srcXStart, srcXEnd, xStep);
            }
        }

        return;

        static uint FindBestPixel(byte* basePointer, int rowBytes, int srcYStart, int srcYEnd, int yStep, int srcXStart, int srcXEnd, int xStep)
        {
            var bestPixel = 0u;
            var maxSaturation = -1;
            for (var y = srcYStart; y < srcYEnd; y += yStep)
            {
                var row = basePointer + (y * rowBytes);
                for (var x = srcXStart; x < srcXEnd; x += xStep)
                {
                    var pixel = row + (x * 4);
                    var b = pixel[0];
                    var g = pixel[1];
                    var r = pixel[2];
                    var saturation = CalcSaturation(r, g, b);
                    if (saturation <= maxSaturation)
                    {
                        continue;
                    }

                    maxSaturation = saturation;
                    bestPixel = PackRgb(r, g, b);
                    if (saturation == 255)
                    {
                        return bestPixel;
                    }
                }
            }

            return bestPixel;
        }
    }

    private void SampleRgbaPixels(byte* basePointer, int rowBytes, int srcWidth, int srcHeight)
    {
        for (var ySample = 0; ySample < SampleHeight; ySample++)
        {
            var srcYStart = ySample * srcHeight / SampleHeight;
            var srcYEnd = Math.Max(srcYStart + 1, ((ySample + 1) * srcHeight) / SampleHeight);
            var yStep = Math.Max(1, (srcYEnd - srcYStart) / 2);

            for (var xSample = 0; xSample < SampleWidth; xSample++)
            {
                var srcXStart = xSample * srcWidth / SampleWidth;
                var srcXEnd = Math.Max(srcXStart + 1, ((xSample + 1) * srcWidth) / SampleWidth);
                var xStep = Math.Max(1, (srcXEnd - srcXStart) / 2);
                var sampleIndex = (ySample * SampleWidth) + xSample;
                samplingWeights[sampleIndex] = (srcXEnd - srcXStart) * (srcYEnd - srcYStart);

                samplingBuffer[sampleIndex] = FindBestPixel(basePointer, rowBytes, srcYStart, srcYEnd, yStep, srcXStart, srcXEnd, xStep);
            }
        }

        return;

        static uint FindBestPixel(byte* basePointer, int rowBytes, int srcYStart, int srcYEnd, int yStep, int srcXStart, int srcXEnd, int xStep)
        {
            var bestPixel = 0u;
            var maxSaturation = -1;
            for (var y = srcYStart; y < srcYEnd; y += yStep)
            {
                var row = basePointer + (y * rowBytes);
                for (var x = srcXStart; x < srcXEnd; x += xStep)
                {
                    var pixel = row + (x * 4);
                    var r = pixel[0];
                    var g = pixel[1];
                    var b = pixel[2];
                    var saturation = CalcSaturation(r, g, b);
                    if (saturation <= maxSaturation)
                    {
                        continue;
                    }

                    maxSaturation = saturation;
                    bestPixel = PackRgb(r, g, b);
                    if (saturation == 255)
                    {
                        return bestPixel;
                    }
                }
            }

            return bestPixel;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalcSaturation(byte r, byte g, byte b)
    {
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        return max - min;
    }

    // ------------------------------------------------------------
    // Quantize
    // ------------------------------------------------------------

    private List<ColorCount> QuantizeSampledBuffer(int maxColors)
    {
        var sampleCount = SampleCount;
        if (sampleCount == 0)
        {
            return [];
        }

        for (var i = 0; i < sampleCount; i++)
        {
            sortBuffer[i] = samplingBuffer[i];
            sortWeights[i] = samplingWeights[i];
        }

        var initialBox = new ColorBox(0, sampleCount);
        UpdateBoxStats(ref initialBox);

        var boxes = new List<ColorBox>(Math.Min(maxColors, sampleCount))
        {
            initialBox
        };

        while (boxes.Count < maxColors)
        {
            var targetIndex = -1;
            long maxPriority = -1;

            for (var i = 0; i < boxes.Count; i++)
            {
                if ((boxes[i].Length > 1) && (boxes[i].Priority > maxPriority))
                {
                    maxPriority = boxes[i].Priority;
                    targetIndex = i;
                }
            }

            if (targetIndex < 0)
            {
                break;
            }

            var target = boxes[targetIndex];
            boxes.RemoveAt(targetIndex);

            SortRange(target.Start, target.Length, target.SortChannel);

            var leftLength = target.Length / 2;
            var rightLength = target.Length - leftLength;
            if ((leftLength == 0) || (rightLength == 0))
            {
                boxes.Add(target);
                break;
            }

            var left = new ColorBox(target.Start, leftLength);
            var right = new ColorBox(target.Start + leftLength, rightLength);
            UpdateBoxStats(ref left);
            UpdateBoxStats(ref right);
            boxes.Add(left);
            boxes.Add(right);
        }

        var merged = new Dictionary<uint, long>();
        foreach (var box in boxes)
        {
            var packed = PackRgb(box.Average.R, box.Average.G, box.Average.B);
            ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(merged, packed, out _);
            count += box.Weight;
        }

        var result = new List<ColorCount>(merged.Count);
        foreach (var pair in merged)
        {
            result.Add(new ColorCount((byte)(pair.Key >> 16), (byte)(pair.Key >> 8), (byte)pair.Key, pair.Value));
        }

        result.Sort(SortByCountDescendingRgbAscending);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SortRange(int start, int length, byte channel)
    {
        Array.Sort(sortBuffer, sortWeights, start, length, channel switch { 0 => SortByRed, 1 => SortByGreen, _ => SortByBlue });
    }

    private void UpdateBoxStats(ref ColorBox box)
    {
        var minR = Byte.MaxValue;
        var minG = Byte.MaxValue;
        var minB = Byte.MaxValue;
        var maxR = Byte.MinValue;
        var maxG = Byte.MinValue;
        var maxB = Byte.MinValue;
        var sumR = 0L;
        var sumG = 0L;
        var sumB = 0L;
        var totalWeight = 0L;
        for (var i = box.Start; i < box.Start + box.Length; i++)
        {
            var color = sortBuffer[i];
            var weight = sortWeights[i];
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)color;

            if (r < minR)
            {
                minR = r;
            }
            if (g < minG)
            {
                minG = g;
            }
            if (b < minB)
            {
                minB = b;
            }
            if (r > maxR)
            {
                maxR = r;
            }
            if (g > maxG)
            {
                maxG = g;
            }
            if (b > maxB)
            {
                maxB = b;
            }

            sumR += (long)r * weight;
            sumG += (long)g * weight;
            sumB += (long)b * weight;
            totalWeight += weight;
        }

        var rangeR = maxR - minR;
        var rangeG = maxG - minG;
        var rangeB = maxB - minB;

        box.SortChannel = SelectSortChannel(rangeR, rangeG, rangeB);
        box.Weight = totalWeight;
        var priorityWidth = (long)rangeR + 1;
        var priorityHeight = (long)rangeG + 1;
        var priorityDepth = (long)rangeB + 1;
        box.Priority = (((priorityWidth * priorityHeight) * priorityDepth) << 16) + totalWeight;
        // ReSharper disable IntDivisionByZero
        box.Average = new Rgb24((byte)(sumR / totalWeight), (byte)(sumG / totalWeight), (byte)(sumB / totalWeight));
        // ReSharper restore IntDivisionByZero
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte SelectSortChannel(int rangeR, int rangeG, int rangeB)
    {
        if ((rangeR >= rangeG) && (rangeR >= rangeB))
        {
            return 0;
        }

        if (rangeG >= rangeB)
        {
            return 1;
        }

        return 2;
    }

    // ------------------------------------------------------------
    // Merge
    // ------------------------------------------------------------

    private static List<ColorCount> MergeSimilarColors(List<ColorCount> colors, double maxDistance)
    {
        colors.Sort(SortByCountDescendingRgbAscending);

        var colorSpan = CollectionsMarshal.AsSpan(colors);
        var clusters = new MergeCluster[colorSpan.Length];
        var bucketHeads = new Dictionary<int, int>(colorSpan.Length);
        var cellSize = Math.Max(1, (int)Math.Ceiling(maxDistance));
        var maxCellIndex = 255 / cellSize;
        var maxDistanceSquared = maxDistance * maxDistance;
        var clusterCount = 0;

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < colorSpan.Length; i++)
        {
            var color = colorSpan[i];
            var rCell = CalcCellIndex(color.R, cellSize);
            var gCell = CalcCellIndex(color.G, cellSize);
            var bCell = CalcCellIndex(color.B, cellSize);
            var bestClusterIndex = NoCluster;

            for (var r = Math.Max(0, rCell - 1); r <= Math.Min(maxCellIndex, rCell + 1); r++)
            {
                for (var g = Math.Max(0, gCell - 1); g <= Math.Min(maxCellIndex, gCell + 1); g++)
                {
                    for (var b = Math.Max(0, bCell - 1); b <= Math.Min(maxCellIndex, bCell + 1); b++)
                    {
                        if (!bucketHeads.TryGetValue(CreateBucketKey(r, g, b), out var clusterIndex))
                        {
                            continue;
                        }

                        while (clusterIndex != NoCluster)
                        {
                            ref var cluster = ref clusters[clusterIndex];
                            if ((CalcDistanceSquared(cluster.SeedR, cluster.SeedG, cluster.SeedB, color.R, color.G, color.B) <= maxDistanceSquared)
                                && ((bestClusterIndex == NoCluster) || (clusterIndex < bestClusterIndex)))
                            {
                                bestClusterIndex = clusterIndex;
                            }

                            clusterIndex = cluster.NextBucketIndex;
                        }
                    }
                }
            }

            if (bestClusterIndex != NoCluster)
            {
                ref var cluster = ref clusters[bestClusterIndex];
                cluster.SumR += color.R * color.Count;
                cluster.SumG += color.G * color.Count;
                cluster.SumB += color.B * color.Count;
                cluster.Count += color.Count;
                continue;
            }

            var bucketKey = CreateBucketKey(rCell, gCell, bCell);
            var headIndex = bucketHeads.GetValueOrDefault(bucketKey, NoCluster);
            clusters[clusterCount] = new MergeCluster(
                color.R,
                color.G,
                color.B,
                color.R * color.Count,
                color.G * color.Count,
                color.B * color.Count,
                color.Count,
                headIndex);
            bucketHeads[bucketKey] = clusterCount;
            clusterCount++;
        }

        var merged = new List<ColorCount>(clusterCount);
        for (var i = 0; i < clusterCount; i++)
        {
            var cluster = clusters[i];
            merged.Add(new ColorCount(
                (byte)Math.Round((double)cluster.SumR / cluster.Count),
                (byte)Math.Round((double)cluster.SumG / cluster.Count),
                (byte)Math.Round((double)cluster.SumB / cluster.Count),
                cluster.Count));
        }

        merged.Sort(SortByCountDescendingRgbAscending);

        return merged;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalcCellIndex(byte value, int cellSize) => value / cellSize;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CreateBucketKey(int rCell, int gCell, int bCell) => (rCell << 16) | (gCell << 8) | bCell;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalcDistanceSquared(byte xR, byte xG, byte xB, byte yR, byte yG, byte yB)
    {
        var dr = xR - yR;
        var dg = xG - yG;
        var db = xB - yB;
        return (dr * dr) + (dg * dg) + (db * db);
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PackRgb(byte r, byte g, byte b) => ((uint)r << 16) | ((uint)g << 8) | b;

    // ------------------------------------------------------------
    // Comparer
    // ------------------------------------------------------------

    public static IComparer<uint> SortByRed { get; } = Comparer<uint>.Create(static (x, y) =>
    {
        var xr = (byte)(x >> 16);
        var yr = (byte)(y >> 16);
        var result = xr.CompareTo(yr);
        if (result != 0)
        {
            return result;
        }

        var xg = (byte)(x >> 8);
        var yg = (byte)(y >> 8);
        result = xg.CompareTo(yg);
        if (result != 0)
        {
            return result;
        }

        var xb = (byte)x;
        var yb = (byte)y;
        result = xb.CompareTo(yb);
        return result != 0 ? result : x.CompareTo(y);
    });

    // ------------------------------------------------------------
    // Sort
    // ------------------------------------------------------------

    public static IComparer<uint> SortByGreen { get; } = Comparer<uint>.Create(static (x, y) =>
    {
        var xg = (byte)(x >> 8);
        var yg = (byte)(y >> 8);
        var result = xg.CompareTo(yg);
        if (result != 0)
        {
            return result;
        }

        var xr = (byte)(x >> 16);
        var yr = (byte)(y >> 16);
        result = xr.CompareTo(yr);
        if (result != 0)
        {
            return result;
        }

        var xb = (byte)x;
        var yb = (byte)y;
        result = xb.CompareTo(yb);
        return result != 0 ? result : x.CompareTo(y);
    });

    public static IComparer<uint> SortByBlue { get; } = Comparer<uint>.Create(static (x, y) =>
    {
        var xb = (byte)x;
        var yb = (byte)y;
        var result = xb.CompareTo(yb);
        if (result != 0)
        {
            return result;
        }

        var xr = (byte)(x >> 16);
        var yr = (byte)(y >> 16);
        result = xr.CompareTo(yr);
        if (result != 0)
        {
            return result;
        }

        var xg = (byte)(x >> 8);
        var yg = (byte)(y >> 8);
        result = xg.CompareTo(yg);
        return result != 0 ? result : x.CompareTo(y);
    });

    public static IComparer<ColorCount> SortByCountDescendingRgbAscending { get; } = Comparer<ColorCount>.Create(static (x, y) =>
    {
        var result = y.Count.CompareTo(x.Count);
        if (result != 0)
        {
            return result;
        }

        result = x.R.CompareTo(y.R);
        if (result != 0)
        {
            return result;
        }

        result = x.G.CompareTo(y.G);
        if (result != 0)
        {
            return result;
        }

        return x.B.CompareTo(y.B);
    });
}
