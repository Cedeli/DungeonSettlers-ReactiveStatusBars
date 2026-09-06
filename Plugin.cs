using System;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
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

        _criticalThreshold.SettingChanged += (_, _) => WarnIfThresholdsInverted();
        _lowThreshold.SettingChanged += (_, _) => WarnIfThresholdsInverted();
        WarnIfThresholdsInverted();

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }

    private static void WarnIfThresholdsInverted()
    {
        if (_criticalThreshold.Value >= _lowThreshold.Value)
            Log.LogWarning($"Critical threshold ({_criticalThreshold.Value}) should be lower than " +
                           $"Low threshold ({_lowThreshold.Value})");
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