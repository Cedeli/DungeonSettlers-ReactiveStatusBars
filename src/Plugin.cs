using System;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using ReactiveStatusBars.Effects;
using UnityEngine;

namespace ReactiveStatusBars;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("DungeonSettlers.exe")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private Harmony _harmony;

    private static ConfigEntry<float> _lowThreshold;
    private static ConfigEntry<float> _criticalThreshold;
    private static ConfigEntry<Color> _lowColor;
    private static ConfigEntry<Color> _criticalColor;
    private static ConfigEntry<float> _lowBlend;
    private static ConfigEntry<float> _critBlend;
    private static ConfigEntry<bool> _flashEnabled;
    private static ConfigEntry<bool> _flashFriendlyOnly;
    private static ConfigEntry<float> _flashDuration;
    private static ConfigEntry<float> _flashSpeed;
    private static ConfigEntry<float> _flashIntensity;
    private static ConfigEntry<Color> _flashColor;
    private static ConfigEntry<bool> _debug;

    public static float LowThreshold => _lowThreshold.Value;
    public static float CriticalThreshold => _criticalThreshold.Value;
    public static Color LowColor => _lowColor.Value;
    public static Color CriticalColor => _criticalColor.Value;
    public static float LowBlend => _lowBlend.Value;
    public static float CriticalBlend => _critBlend.Value;
    public static bool FlashEnabled => _flashEnabled.Value;
    public static bool FlashFriendlyOnly => _flashFriendlyOnly.Value;
    public static float FlashDuration => _flashDuration.Value;
    public static float FlashSpeed => _flashSpeed.Value;
    public static float FlashIntensity => _flashIntensity.Value;
    public static Color FlashColor => _flashColor.Value;
    public static bool Debug => _debug.Value;

    public override void Load()
    {
        Log = base.Log;

        RegisterColorConverter();

        var thresholdRange = new AcceptableValueRange<float>(0f, 1f);
        var blendRange = new AcceptableValueRange<float>(0f, 1f);

        _lowThreshold = Config.Bind("Thresholds", "Low", 0.50f,
            new ConfigDescription(
                "Health ratio (0-1) below which the Low warning color applies.", thresholdRange
            )
        );
        _criticalThreshold = Config.Bind("Thresholds", "Critical", 0.25f,
            new ConfigDescription(
                "Health ratio (0-1) below which the Critical warning color applies. Must stay below the Low threshold.",
                thresholdRange
            )
        );

        _lowColor = Config.Bind("Colors.Low", "Color", new Color(1f, 0.5f, 0f),
            "Color used at the Low health threshold.");
        _lowBlend = Config.Bind("Colors.Low", "BlendStrength", 0.5f,
            new ConfigDescription(
                "0 = keep original bar color, 1 = fully replace with the Low color.",
                blendRange
            )
        );

        _criticalColor = Config.Bind("Colors.Critical", "Color", new Color(1f, 0f, 0f),
            "Color used at the Critical health threshold.");
        _critBlend = Config.Bind("Colors.Critical", "BlendStrength", 0.8f,
            new ConfigDescription(
                "0 = keep original bar color, 1 = fully replace with the Critical color.",
                blendRange
            )
        );

        _flashEnabled = Config.Bind("Flash", "Enabled", true,
            "Whether bars briefly flash when entering Critical.");
        _flashFriendlyOnly = Config.Bind("Flash", "FriendlyOnly", true,
            "If true, only friendly units flash. Enemy severity is still shown via color.");
        _flashDuration = Config.Bind("Flash", "DurationSeconds", 1.5f,
            new ConfigDescription(
                "How long a triggered flash lasts.",
                new AcceptableValueRange<float>(0.1f, 10f)
            )
        );
        _flashSpeed = Config.Bind("Flash", "SpeedHz", 4f,
            new ConfigDescription(
                "Flashes per second while active.",
                new AcceptableValueRange<float>(0.5f, 15f)
            )
        );
        _flashIntensity = Config.Bind("Flash", "Intensity", 0.6f,
            new ConfigDescription(
                "0 = invisible, 1 = flashes fully to FlashColor.", blendRange
            )
        );
        _flashColor = Config.Bind("Flash", "Color", Color.white, "Color the bar flashes toward.");
        
        _debug = Config.Bind("Debug", "VerboseLogging", false,
            "Logs HUD registration, bar-state tracking, and pool release events."
        );

        _criticalThreshold.SettingChanged += OnThresholdChanged;
        _lowThreshold.SettingChanged += OnThresholdChanged;

        ClassInjector.RegisterTypeInIl2Cpp<SpriteColorPulse>();

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }

    private static void OnThresholdChanged(object sender, EventArgs e) => ClampThresholds();

    private static void ClampThresholds()
    {
        const float epsilon = 0.01f;

        if (!(_criticalThreshold.Value >= _lowThreshold.Value)) return;
        var clamped = Math.Max(0f, _lowThreshold.Value - epsilon);
        Log.LogWarning($"Critical threshold ({_criticalThreshold.Value}) must be lower than " +
                       $"Low threshold ({_lowThreshold.Value}); clamping Critical to {clamped}.");
        _criticalThreshold.Value = clamped;
    }

    private static void RegisterColorConverter()
    {
        if (TomlTypeConverter.CanConvert(typeof(Color))) return;

        TomlTypeConverter.AddConverter(typeof(Color), new TypeConverter
        {
            ConvertToString = (value, _) =>
            {
                var c = (Color)value;
                return string.Join(",",
                    c.r.ToString(CultureInfo.InvariantCulture),
                    c.g.ToString(CultureInfo.InvariantCulture),
                    c.b.ToString(CultureInfo.InvariantCulture),
                    c.a.ToString(CultureInfo.InvariantCulture)
                );
            },
            ConvertToObject = (str, _) =>
            {
                var parts = str.Split(',');
                return new Color(
                    float.Parse(parts[0], CultureInfo.InvariantCulture),
                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                    parts.Length > 3 ? float.Parse(parts[3], CultureInfo.InvariantCulture) : 1f
                );
            }
        });
    }
}