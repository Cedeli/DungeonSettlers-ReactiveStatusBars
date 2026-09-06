using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStatusBars;

public static class BarHelper
{
    private sealed class BarState
    {
        public Color OriginalColor;
        public float LastRatio;
    }

    private static readonly Dictionary<int, BarState> ImageStates = new();
    private static readonly Dictionary<int, BarState> SpriteStates = new();

    public static void UpdateColor(Image bar, float current, float max)
    {
        if (bar == null || max <= 0f) return;

        var id = bar.GetInstanceID();
        if (!ImageStates.TryGetValue(id, out var state))
        {
            state = new BarState { OriginalColor = bar.color };
            ImageStates[id] = state;
        }

        state.LastRatio = current / max;
        bar.color = ResolveColor(state.LastRatio, state.OriginalColor);
    }

    public static void UpdateColor(SpriteRenderer bar, float current, float max)
    {
        if (bar == null || max <= 0f) return;

        var id = bar.GetInstanceID();
        if (!SpriteStates.TryGetValue(id, out var state))
        {
            state = new BarState { OriginalColor = bar.color };
            SpriteStates[id] = state;
        }

        state.LastRatio = current / max;
        bar.color = ResolveColor(state.LastRatio, state.OriginalColor);
    }

    private static Color ResolveColor(float ratio, Color originalColor)
    {
        if (ratio <= Plugin.CriticalThreshold)
            return Color.Lerp(originalColor, Plugin.CriticalColor, Plugin.CriticalBlend);

        return ratio <= Plugin.LowThreshold
            ? Color.Lerp(originalColor, Plugin.LowColor, Plugin.LowBlend)
            : originalColor;
    }
}