using System.Collections.Generic;
using ReactiveStatusBars.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStatusBars;

public static class BarHelper
{
    private sealed class BarState
    {
        public Color OriginalColor;
        public float LastRatio = -1f;
        public bool WasCritical;
    }

    private static readonly Dictionary<int, BarState> States = new();

    public static void UpdateColor(Image bar, float current, float max)
    {
        if (bar == null || max <= 0f) return;

        var state = GetOrCreateState(bar.GetInstanceID(), bar.color);
        var ratio = current / max;

        bar.color = ResolveColor(ratio, state.OriginalColor);
        state.LastRatio = ratio;
    }

    public static void UpdateColor(SpriteRenderer bar, float current, float max, bool isFriendly = true)
    {
        if (bar == null || max <= 0f) return;

        var state = GetOrCreateState(bar.GetInstanceID(), bar.color);
        var ratio = current / max;
        var baseColor = ResolveColor(ratio, state.OriginalColor);
        var shouldFlash = EvaluateFlashTrigger(state, ratio, isFriendly);

        var pulse = bar.GetComponent<SpriteColorPulse>();
        var pulseActive = pulse != null && pulse.enabled;

        if (pulseActive)
            pulse.baseColor = baseColor;
        else
            bar.color = baseColor;

        if (shouldFlash)
        {
            pulse ??= bar.gameObject.AddComponent<SpriteColorPulse>();
            pulse.baseColor = baseColor;
            pulse.flashEndTime = Time.time + Plugin.FlashDuration;
            pulse.enabled = true;
        }

        state.LastRatio = ratio;
    }

    public static void Unregister(Image bar)
    {
        if (bar != null) States.Remove(bar.GetInstanceID());
    }

    public static void Unregister(SpriteRenderer bar)
    {
        if (bar != null) States.Remove(bar.GetInstanceID());
    }

    private static BarState GetOrCreateState(int id, Color originalColor)
    {
        if (States.TryGetValue(id, out var state)) return state;
        state = new BarState { OriginalColor = originalColor };
        States[id] = state;

        return state;
    }

    private static Color ResolveColor(float ratio, Color originalColor)
    {
        if (ratio <= Plugin.CriticalThreshold)
            return Color.Lerp(originalColor, Plugin.CriticalColor, Plugin.CriticalBlend);

        return ratio <= Plugin.LowThreshold
            ? Color.Lerp(originalColor, Plugin.LowColor, Plugin.LowBlend)
            : originalColor;
    }

    private static bool EvaluateFlashTrigger(BarState state, float ratio, bool isFriendly)
    {
        var isCriticalNow = ratio <= Plugin.CriticalThreshold;
        var isFirstUpdate = state.LastRatio < 0f;
        var justBecameCritical = isCriticalNow && !state.WasCritical;
        state.WasCritical = isCriticalNow;

        if (isFirstUpdate) return false;
        if (!Plugin.FlashEnabled) return false;
        if (Plugin.FlashFriendlyOnly && !isFriendly) return false;

        return justBecameCritical;
    }
}