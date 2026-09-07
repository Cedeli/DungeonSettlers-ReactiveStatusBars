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
        public SpriteColorPulse Pulse;
        public bool PulseChecked;
    }

    private static readonly Dictionary<int, BarState> States = new();
    private static readonly Dictionary<int, List<int>> BarIdsByOwner = new();

    public static void UpdateColor(Image bar, float current, float max, int ownerId)
    {
        if (bar == null || max <= 0f) return;

        var id = bar.GetInstanceID();
        var state = GetOrCreateState(id, ownerId, bar.color);
        var ratio = current / max;

        bar.color = ResolveColor(ratio, state.OriginalColor);
        state.LastRatio = ratio;
    }

    public static void UpdateColor(SpriteRenderer bar, float current, float max, int ownerId, bool isFriendly = true)
    {
        if (bar == null || max <= 0f) return;

        var id = bar.GetInstanceID();
        var state = GetOrCreateState(id, ownerId, bar.color);
        var ratio = current / max;
        var baseColor = ResolveColor(ratio, state.OriginalColor);
        var shouldFlash = EvaluateFlashTrigger(state, ratio, isFriendly);

        if (!state.PulseChecked)
        {
            state.Pulse = bar.GetComponent<SpriteColorPulse>();
            state.PulseChecked = true;
        }

        var pulse = state.Pulse;
        var pulseActive = pulse != null && pulse.enabled;

        if (pulseActive)
            pulse.baseColor = baseColor;
        else
            bar.color = baseColor;

        if (shouldFlash)
        {
            if (pulse == null)
            {
                pulse = bar.gameObject.AddComponent<SpriteColorPulse>();
                state.Pulse = pulse;
            }

            pulse.baseColor = baseColor;
            pulse.flashEndTime = Time.time + Plugin.FlashDuration;
            pulse.enabled = true;

            if (Plugin.Debug)
                Plugin.Log.LogInfo($"Flash triggered on bar id={id} owner={ownerId}");
        }

        state.LastRatio = ratio;
    }

    public static void UnregisterOwner(int ownerId)
    {
        if (!BarIdsByOwner.TryGetValue(ownerId, out var ids)) return;

        foreach (var id in ids)
            States.Remove(id);

        if (Plugin.Debug)
            Plugin.Log.LogInfo($"UnregisterOwner owner={ownerId} clearedBars={ids.Count} " +
                               $"remainingOwners={BarIdsByOwner.Count - 1} remainingBars={States.Count - ids.Count}");

        BarIdsByOwner.Remove(ownerId);
    }

    public static void RestoreAndUnregisterOwner(int ownerId, params Component[] bars)
    {
        if (BarIdsByOwner.TryGetValue(ownerId, out var ids))
        {
            foreach (var barComponent in bars)
            {
                if (barComponent == null) continue;

                var id = barComponent.GetInstanceID();
                if (!States.TryGetValue(id, out var state)) continue;

                switch (barComponent)
                {
                    case Image img:
                        img.color = state.OriginalColor;
                        break;
                    case SpriteRenderer sr:
                        sr.color = state.OriginalColor;
                        break;
                }

                if (state.Pulse != null)
                    state.Pulse.enabled = false;
            }

            if (Plugin.Debug)
                Plugin.Log.LogInfo($"RestoreAndUnregisterOwner owner={ownerId} restoredBars={ids.Count}");
        }

        UnregisterOwner(ownerId);
    }

    private static BarState GetOrCreateState(int id, int ownerId, Color originalColor)
    {
        if (States.TryGetValue(id, out var existing)) return existing;

        var state = new BarState { OriginalColor = originalColor };
        States[id] = state;

        if (!BarIdsByOwner.TryGetValue(ownerId, out var ids))
        {
            ids = [];
            BarIdsByOwner[ownerId] = ids;
        }

        ids.Add(id);

        if (Plugin.Debug)
            Plugin.Log.LogInfo($"New BarState bar id={id} owner={ownerId} " +
                               $"originalColor={originalColor} totalBars={States.Count} totalOwners={BarIdsByOwner.Count}");

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