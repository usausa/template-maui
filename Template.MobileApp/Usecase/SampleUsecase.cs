namespace Template.MobileApp.Usecase;

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

public record ColorCount(
    byte R,
    byte G,
    byte B,
    int Count);

public sealed class SampleUsecase
{
#pragma warning disable SA1401
#pragma warning disable CA1051
#pragma warning disable CS0414
#pragma warning disable CS0649
    private sealed class RgbData
    {
        public float R;
        public float G;
        public float B;
    }

    private sealed class ClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint ClusterId;

        [ColumnName("Score")]
        public float[] Distances = default!;
    }
#pragma warning restore CS0414
#pragma warning restore CS0649
#pragma warning restore CA1051
#pragma warning restore SA1401

    //--------------------------------------------------------------------------------
    // Image
    //--------------------------------------------------------------------------------

#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public List<ColorCount> ClusterColors(
        SKBitmap bitmap,
        int maxClusters,
        int maxIterations,
        float tolerance)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;

        var colors = new HashSet<SKColor>();
        var pixels = new RgbData[width * height];
        var index = 0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                pixels[index++] = new RgbData { R = color.Red, G = color.Green, B = color.Blue };

                if (colors.Count < maxClusters)
                {
                    colors.Add(color);
                }
            }
        }

        var actualClusters = Math.Min(maxClusters, colors.Count);

        // Load data view
        var mlContext = new MLContext();
        var dataView = mlContext.Data.LoadFromEnumerable(pixels);

        // KMeans
        var options = new KMeansTrainer.Options
        {
            FeatureColumnName = "Features",
            NumberOfClusters = actualClusters,
            MaximumNumberOfIterations = maxIterations,
            OptimizationTolerance = tolerance,
            //InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.KMeansPlusPlus
            InitializationAlgorithm = KMeansTrainer.InitializationAlgorithm.Random
        };
        var pipeline = mlContext.Transforms
            .Concatenate("Features", nameof(RgbData.R), nameof(RgbData.G), nameof(RgbData.B))
            .Append(mlContext.Clustering.Trainers.KMeans(options));

        var model = pipeline.Fit(dataView);

        var transformed = model.Transform(dataView);

        // Get center
        var centroids = default(VBuffer<float>[]);
        model.LastTransformer.Model.GetClusterCentroids(ref centroids, out _);

        // Count
        var counts = new int[actualClusters];
        foreach (var prediction in mlContext.Data.CreateEnumerable<ClusterPrediction>(transformed, reuseRowObject: false))
        {
            counts[prediction.ClusterId - 1]++;
        }

        var list = new List<ColorCount>(actualClusters);
        for (var i = 0; i < counts.Length; i++)
        {
            var centroid = centroids[i].DenseValues().ToArray();
            var r = (byte)Math.Round(centroid[0]);
            var g = (byte)Math.Round(centroid[1]);
            var b = (byte)Math.Round(centroid[2]);
            list.Add(new ColorCount(r, g, b, counts[i]));
        }

        list.Sort(static (x, y) => y.Count - x.Count);
        return list;
    }
#pragma warning restore CA1822
}
