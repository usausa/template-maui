namespace Template.MobileApp.Graphics.Scene;

//--------------------------------------------------------------------------------
// Enum
//--------------------------------------------------------------------------------
internal enum EnergyFlowMode
{
    Power,
    Heat
}

internal enum EnergyNode
{
    Grid,
    Solar,
    Cgs,
    BatteryOut,
    Bus,
    LineA,
    LineB,
    Hvac,
    Utility,
    BatteryIn,
    Gas,
    CgsEngine,
    Boiler,
    Electric,
    Steam,
    Loss
}

internal enum EnergyIcon
{
    None,
    Tower,
    Sun,
    Gear,
    Battery,
    Factory,
    Snow,
    Plug,
    Flame,
    Bolt,
    Steam,
    Loss
}

internal enum EnergyLevel
{
    Info,
    Good,
    Warn,
    Alarm
}

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------
internal sealed record EnergyEvent(
    DateTime Time,
    float SceneTime,
    EnergyLevel Level,
    string Source,
    string Text);

internal readonly record struct EnergyProfile(
    float LineA,
    float LineB,
    float Hvac,
    float Utility,
    float Solar,
    float SolarClear,
    float Cgs,
    float SteamTph,
    float Cloud,
    float Press,
    float Noise);

//--------------------------------------------------------------------------------
// Helper
//--------------------------------------------------------------------------------
internal static class EnergyMath
{
    public static float Smooth(float edge0, float edge1, float x)
    {
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - (2f * t));
    }

    public static float Window(float hour, float start, float end, float ramp) =>
        Smooth(start - ramp, start + ramp, hour) * (1f - Smooth(end - ramp, end + ramp, hour));

    public static float Pulse(float seconds, float period, float width)
    {
        var p = seconds % period;
        return Smooth(0f, 1.2f, p) * (1f - Smooth(width - 1.2f, width, p));
    }

    public static float Frac(float x) => x - MathF.Floor(x);

    public static float Approach(float value, float target, float rate) =>
        MathF.Abs(target - value) <= rate ? target : value + MathF.CopySign(rate, target - value);

    public static float Hash01(int n)
    {
        unchecked
        {
            var x = ((uint)n * 747796405u) + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return x / 4294967296f;
        }
    }

    public static float Noise(float x, int seed)
    {
        var i = (int)MathF.Floor(x);
        var u = Smooth(0f, 1f, x - i);
        var a = Hash01(i + (seed * 7919));
        var b = Hash01(i + 1 + (seed * 7919));
        return ((a + ((b - a) * u)) * 2f) - 1f;
    }
}

//--------------------------------------------------------------------------------
// Simulation
//--------------------------------------------------------------------------------
internal sealed class EnergySim
{
    public const float Target = 1300f;
    public const float Contract = 1450f;
    public const int Slots = 288;
    public const int PeriodMinutes = 30;
    public const int LayerCount = 4;

    private const float SlotSeconds = 300f;
    private const float PeriodSeconds = 1800f;
    private const float BatteryCapacity = 1200f;
    private const float BatteryPower = 300f;
    private const float ChargePower = 220f;
    private const float SteamKwPerTon = 700f;
    private const float GasKwhPerM3 = 12.5f;
    private const int MaxEvents = 6;

    private static readonly int NodeCount = Enum.GetValues<EnergyNode>().Length;

    private readonly float[][] history = [new float[Slots], new float[Slots], new float[Slots], new float[Slots]];
    private readonly float[] loadHistory = new float[Slots];
    private readonly float[] slotTime = new float[Slots];
    private readonly float[] forecast = new float[Slots];
    private readonly float[] minuteSum = new float[PeriodMinutes];
    private readonly float[] minuteTime = new float[PeriodMinutes];
    private readonly float[] todayEnergy = new float[NodeCount];

    private DateTime day;
    private int seed;
    private int period = -1;
    private float periodEnergy;
    private float todayGas;
    private float lastPressEvent = -1000f;
    private bool cloudy;
    private bool pressing;
    private bool peakCut;
    private bool alert;
    private bool charging;
    private bool cgsRunning;

    public DateTime Now { get; private set; }

    public float NowSeconds { get; private set; }

    public int CurrentSlot { get; private set; }

    public int CurrentMinute { get; private set; }

    public DateTime PeriodStart => day.AddSeconds(period * PeriodSeconds);

    public float PeriodRemaining { get; private set; }

    public float Predicted { get; private set; }

    public float LastDemand { get; private set; }

    public float TodayMaxDemand { get; private set; }

    public float LineA { get; private set; }

    public float LineB { get; private set; }

    public float Hvac { get; private set; }

    public float Utility { get; private set; }

    public float Load { get; private set; }

    public float Solar { get; private set; }

    public float SolarClear { get; private set; }

    public float Cgs { get; private set; }

    public float BatteryIn { get; private set; }

    public float BatteryOut { get; private set; }

    public float Grid { get; private set; }

    public float Soc { get; private set; }

    public float SteamTph { get; private set; }

    public float GasCgs { get; private set; }

    public float GasBoiler { get; private set; }

    public float CgsHeat { get; private set; }

    public float CgsLoss { get; private set; }

    public float BoilerSteam { get; private set; }

    public float BoilerLoss { get; private set; }

    public float GasFlow { get; private set; }

    public float CgsSteamTph => CgsHeat / SteamKwPerTon;

    public float BoilerSteamTph => BoilerSteam / SteamKwPerTon;

    public float BoilerGasFlow => GasBoiler / GasKwhPerM3;

    public float FanRatio { get; private set; }

    public float WaterTemp { get; private set; }

    public float CloudFactor { get; private set; }

    public float Press { get; private set; }

    public float TodayLoad { get; private set; }

    public float TodaySelfRatio => TodayLoad > 1f ? (todayEnergy[(int)EnergyNode.Solar] + todayEnergy[(int)EnergyNode.Cgs]) / TodayLoad : 0f;

    public float TodayCo2 => (todayEnergy[(int)EnergyNode.Grid] * 0.000441f) + (todayGas * 0.00223f);

    public float TodayCost => (todayEnergy[(int)EnergyNode.Grid] * 24f) + (todayGas * 95f);

    public List<EnergyEvent> Events { get; } = [];

    public EnergySim()
    {
        Rebuild(DateTime.Now);
    }

    public float Value(EnergyNode node) => node switch
    {
        EnergyNode.Grid => Grid,
        EnergyNode.Solar => Solar,
        EnergyNode.Cgs => Cgs,
        EnergyNode.BatteryOut => BatteryOut,
        EnergyNode.Bus => Load + BatteryIn,
        EnergyNode.LineA => LineA,
        EnergyNode.LineB => LineB,
        EnergyNode.Hvac => Hvac,
        EnergyNode.Utility => Utility,
        EnergyNode.BatteryIn => BatteryIn,
        EnergyNode.Gas => GasCgs + GasBoiler,
        EnergyNode.CgsEngine => GasCgs,
        EnergyNode.Boiler => GasBoiler,
        EnergyNode.Electric => Cgs,
        EnergyNode.Steam => CgsHeat + BoilerSteam,
        EnergyNode.Loss => CgsLoss + BoilerLoss,
        _ => 0f
    };

    public float TodayEnergy(EnergyNode node) => todayEnergy[(int)node];

    public float History(int layer, int slot) => slotTime[slot] > 0f ? history[layer][slot] / slotTime[slot] : 0f;

    public float LoadHistory(int slot) => slotTime[slot] > 0f ? loadHistory[slot] / slotTime[slot] : 0f;

    public float Forecast(int slot) => forecast[slot];

    public float MinuteDemand(int minute) => minuteTime[minute] > 0f ? minuteSum[minute] / minuteTime[minute] : float.NaN;

    public void Update(float t, float dt)
    {
        var now = DateTime.Now;
        if (now.Date != day)
        {
            Rebuild(now);
            return;
        }

        Now = now;
        var seconds = (float)(now - day).TotalSeconds;
        var before = period;
        Step(seconds, dt, true);
        if (period != before)
        {
            AddEvent(now, t, EnergyLevel.Info, "DEMAND", $"{PeriodStart.AddMinutes(-PeriodMinutes):HH:mm}-{PeriodStart:HH:mm}  {LastDemand:#,0} kW");
        }

        DetectEvents(now, t, seconds);
    }

    //--------------------------------------------------------------------------------
    // Model
    //--------------------------------------------------------------------------------

    private void Rebuild(DateTime now)
    {
        day = now.Date;
        seed = day.DayOfYear;
        period = -1;
        periodEnergy = 0f;
        todayGas = 0f;
        TodayLoad = 0f;
        LastDemand = 0f;
        TodayMaxDemand = 0f;
        Soc = 0.55f;
        BatteryIn = 0f;
        BatteryOut = 0f;
        foreach (var layer in history)
        {
            Array.Clear(layer);
        }

        Array.Clear(loadHistory);
        Array.Clear(slotTime);
        Array.Clear(todayEnergy);
        Events.Clear();

        for (var slot = 0; slot < Slots; slot++)
        {
            var p = Sample((slot + 0.5f) * SlotSeconds, seed, true);
            forecast[slot] = p.LineA + p.LineB + p.Hvac + p.Utility;
        }

        var seconds = (float)(now - day).TotalSeconds;
        for (var s = 30f; (s + 30f) <= seconds; s += 60f)
        {
            Step(s, 60f, false);
        }

        Now = now;
        Step(seconds, 0f, false);

        cloudy = CloudFactor < 0.7f;
        pressing = Press > 0.5f;
        peakCut = BatteryOut > 15f;
        alert = Predicted > Target;
        charging = BatteryIn > 15f;
        cgsRunning = Cgs > 50f;

        if (LastDemand > 0f)
        {
            AddEvent(now, -10f, EnergyLevel.Info, "DEMAND", $"LAST PERIOD  {LastDemand:#,0} kW");
        }

        AddEvent(now, -10f, EnergyLevel.Good, "EMS", "MONITORING START");
    }

    private void Step(float seconds, float dt, bool live)
    {
        var hour = seconds / 3600f;
        var p = Sample(seconds, seed, false);
        var k = live ? MathF.Min(1f, dt / 0.6f) : 1f;
        LineA += (p.LineA - LineA) * k;
        LineB += (p.LineB - LineB) * k;
        Hvac += (p.Hvac - Hvac) * k;
        Utility += (p.Utility - Utility) * k;
        Solar += (p.Solar - Solar) * k;
        Cgs += (p.Cgs - Cgs) * k;
        SteamTph += (p.SteamTph - SteamTph) * k;
        SolarClear = p.SolarClear;
        CloudFactor = p.Cloud;
        Press = p.Press;

        Load = LineA + LineB + Hvac + Utility;
        var gridWithoutBattery = Load - Solar - Cgs;
        var charge = 0f;
        var discharge = 0f;
        if ((hour >= 23f) || (hour < 6f))
        {
            if (Soc < 0.95f)
            {
                charge = Math.Clamp((Target * 0.9f) - gridWithoutBattery, 0f, ChargePower);
            }
        }
        else if (Soc > 0.12f)
        {
            discharge = Math.Clamp(gridWithoutBattery - (Target * 0.95f), 0f, BatteryPower);
        }

        var rate = live ? 150f * dt : float.MaxValue;
        BatteryIn = EnergyMath.Approach(BatteryIn, charge, rate);
        BatteryOut = EnergyMath.Approach(BatteryOut, discharge, rate);
        Soc = Math.Clamp(Soc + ((((BatteryIn * 0.95f) - (BatteryOut / 0.95f)) * dt / 3600f) / BatteryCapacity), 0f, 1f);
        Grid = gridWithoutBattery + BatteryIn - BatteryOut;

        GasCgs = Cgs / 0.4f;
        CgsHeat = Cgs * 0.85f;
        CgsLoss = GasCgs - Cgs - CgsHeat;
        BoilerSteam = MathF.Max(0f, (SteamTph * SteamKwPerTon) - CgsHeat);
        GasBoiler = BoilerSteam / 0.9f;
        BoilerLoss = GasBoiler - BoilerSteam;
        GasFlow = (GasCgs + GasBoiler) / GasKwhPerM3;
        FanRatio = Math.Clamp(Hvac / 440f, 0.2f, 1f);
        WaterTemp = 26.5f + (3.4f * FanRatio) + (0.25f * p.Noise);

        Accumulate(seconds, dt);
    }

    private void Accumulate(float seconds, float dt)
    {
        NowSeconds = seconds;
        var slot = Math.Clamp((int)(seconds / SlotSeconds), 0, Slots - 1);
        CurrentSlot = slot;
        history[0][slot] += Grid * dt;
        history[1][slot] += Cgs * dt;
        history[2][slot] += Solar * dt;
        history[3][slot] += BatteryOut * dt;
        loadHistory[slot] += Load * dt;
        slotTime[slot] += dt;

        var hours = dt / 3600f;
        for (var i = 0; i < NodeCount; i++)
        {
            todayEnergy[i] += Value((EnergyNode)i) * hours;
        }

        todayGas += GasFlow * hours;
        TodayLoad += Load * hours;

        var index = (int)(seconds / PeriodSeconds);
        if (index != period)
        {
            if (period >= 0)
            {
                LastDemand = periodEnergy / 0.5f;
                TodayMaxDemand = MathF.Max(TodayMaxDemand, LastDemand);
            }

            period = index;
            periodEnergy = 0f;
            Array.Clear(minuteSum);
            Array.Clear(minuteTime);
        }

        var minute = Math.Clamp((int)((seconds - (period * PeriodSeconds)) / 60f), 0, PeriodMinutes - 1);
        CurrentMinute = minute;
        minuteSum[minute] += Grid * dt;
        minuteTime[minute] += dt;
        periodEnergy += Grid * hours;
        PeriodRemaining = ((period + 1) * PeriodSeconds) - seconds;
        Predicted = (periodEnergy + (Grid * PeriodRemaining / 3600f)) / 0.5f;
    }

    private static EnergyProfile Sample(float seconds, int seed, bool smooth)
    {
        var hour = seconds / 3600f;
        var work = EnergyMath.Window(hour, 8f, 17f, 0.25f);
        var lunch = EnergyMath.Window(hour, 12f, 13f, 0.1f);
        var shift = EnergyMath.Window(hour, 6f, 22f, 0.5f);
        var office = EnergyMath.Window(hour, 7.5f, 19f, 0.3f);
        var press = smooth ? 0.28f : EnergyMath.Pulse(seconds, 50f, 14f);
        var compressor = smooth ? 0.69f : EnergyMath.Pulse(seconds, 26f, 18f);
        var n1 = smooth ? 0f : EnergyMath.Noise(seconds / 9f, seed + 1);
        var n2 = smooth ? 0f : EnergyMath.Noise(seconds / 7f, seed + 2);
        var n3 = smooth ? 0f : EnergyMath.Noise(seconds / 11f, seed + 3);
        var n4 = smooth ? 0f : EnergyMath.Noise(seconds / 13f, seed + 4);
        var n5 = smooth ? 0f : EnergyMath.Noise(seconds / 17f, seed + 5);
        var n6 = smooth ? 0f : EnergyMath.Noise(seconds / 23f, seed + 6);

        var lineA = 120f + (500f * work) - (230f * lunch * work) + (130f * press * work) + (12f * n1);
        var lineB = 400f + (90f * shift) + (25f * n2);
        var warm = MathF.Max(0f, MathF.Sin(MathF.PI * (hour - 8f) / 12f));
        var hvac = 170f + (90f * EnergyMath.Window(hour, 7f, 21f, 1f)) + (170f * warm) + (14f * n3);
        var utility = 150f + (130f * work) + (70f * compressor) + (110f * office) + (8f * n4);

        var sun = MathF.Max(0f, MathF.Sin(MathF.PI * (hour - 5.8f) / 12.6f));
        var clear = 610f * MathF.Pow(sun, 1.25f);
        var cloud = smooth ? 0.85f : Cloud(seconds, seed);
        var cgs = EnergyMath.Window(hour, 8f, 20f, 0.05f) * (410f + (12f * n5));
        var steam = 2.3f + (0.7f * work) + (0.15f * n6);
        return new EnergyProfile(lineA, lineB, hvac, utility, clear * cloud, clear, cgs, steam, cloud, press * work, n6);
    }

    private static float Cloud(float seconds, int seed)
    {
        var slow = (EnergyMath.Noise(seconds / 40f, seed + 9) * 0.5f) + 0.5f;
        var fast = (EnergyMath.Noise(seconds / 6f, seed + 10) * 0.5f) + 0.5f;
        var cover = EnergyMath.Smooth(0.5f, 0.76f, slow);
        return 1f - (0.6f * cover) - (0.08f * fast * cover);
    }

    //--------------------------------------------------------------------------------
    // Event
    //--------------------------------------------------------------------------------

    private void DetectEvents(DateTime now, float t, float seconds)
    {
        if (cloudy ? CloudFactor > 0.8f : CloudFactor < 0.7f)
        {
            cloudy = !cloudy;
            if (SolarClear > 80f)
            {
                AddEvent(
                    now,
                    t,
                    cloudy ? EnergyLevel.Warn : EnergyLevel.Good,
                    "PV",
                    cloudy ? $"CLOUD COVER  -{SolarClear - Solar:#,0} kW" : $"OUTPUT RECOVERED  {Solar:#,0} kW");
            }
        }

        if (pressing != (Press > 0.5f))
        {
            pressing = !pressing;
            if (pressing && ((seconds - lastPressEvent) > 180f))
            {
                lastPressEvent = seconds;
                AddEvent(now, t, EnergyLevel.Info, "LINE A", "PRESS START  +130 kW");
            }
        }

        if (peakCut ? BatteryOut < 5f : BatteryOut > 15f)
        {
            peakCut = !peakCut;
            AddEvent(now, t, peakCut ? EnergyLevel.Good : EnergyLevel.Info, "BATT", peakCut ? "PEAK CUT START" : $"PEAK CUT END  SOC {Soc * 100f:0}%");
        }

        if (alert ? Predicted < (Target * 0.98f) : Predicted > Target)
        {
            alert = !alert;
            var level = Predicted > Contract ? EnergyLevel.Alarm : EnergyLevel.Warn;
            AddEvent(now, t, alert ? level : EnergyLevel.Good, "DEMAND", alert ? $"OVER TARGET  {Predicted:#,0} kW" : "BACK UNDER TARGET");
        }

        if (charging ? BatteryIn < 5f : BatteryIn > 15f)
        {
            charging = !charging;
            AddEvent(now, t, EnergyLevel.Info, "BATT", charging ? "NIGHT CHARGE START" : $"CHARGE END  SOC {Soc * 100f:0}%");
        }

        if (cgsRunning ? Cgs < 30f : Cgs > 50f)
        {
            cgsRunning = !cgsRunning;
            AddEvent(now, t, cgsRunning ? EnergyLevel.Good : EnergyLevel.Info, "CGS", cgsRunning ? "ENGINE START" : "ENGINE STOP");
        }
    }

    private void AddEvent(DateTime time, float sceneTime, EnergyLevel level, string source, string text)
    {
        Events.Insert(0, new EnergyEvent(time, sceneTime, level, source, text));
        if (Events.Count > MaxEvents)
        {
            Events.RemoveAt(Events.Count - 1);
        }
    }
}
