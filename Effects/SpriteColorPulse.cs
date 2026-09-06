using System;
using UnityEngine;

namespace ReactiveStatusBars.Effects;

public sealed class SpriteColorPulse(IntPtr ptr) : MonoBehaviour(ptr)
{
    public Color baseColor;
    public float flashEndTime;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null)
            enabled = false;
    }

    private void Update()
    {
        if (!Plugin.FlashEnabled || Time.time >= flashEndTime)
        {
            _renderer.color = baseColor;
            enabled = false;
            return;
        }

        var t = (Mathf.Sin(Time.time * Plugin.FlashSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        _renderer.color = Color.Lerp(baseColor, Plugin.FlashColor, t * Plugin.FlashIntensity);
    }
}