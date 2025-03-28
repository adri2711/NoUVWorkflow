using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class SpeedLinesVFX : MonoBehaviour
{
    VisualEffect vfx;
    [SerializeField] PlayerCharacterController player;

    [SerializeField] float thresholdMin = 32f;
    [SerializeField] float thresholdMax = 50f;
    [SerializeField] float radiusMin = 1.5f;
    [SerializeField] float radiusMax = 1.2f;
    [SerializeField] float rateMin = 35f;
    [SerializeField] float rateMax = 96f;
    [SerializeField] float alphaMin = .1f;
    [SerializeField] float alphaMax = .35f;
    [SerializeField] [GradientUsage(true)] Gradient defaultGradient;
    [SerializeField] [GradientUsage(true)] Gradient chargeGradient;
    [SerializeField] [GradientUsage(true)] Gradient hookGradient;

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.enabled = false;
    }

    private void Update()
    {
        float v = player.rb.velocity.magnitude;
        vfx.enabled = v > thresholdMin;
        if (v > thresholdMin)
        {
            vfx.SetFloat("Radius", Mathf.Lerp(radiusMin, radiusMax, (v - thresholdMin) / thresholdMax));
            vfx.SetFloat("SpawnRate", Mathf.Lerp(rateMin, rateMax, (v - thresholdMin) / thresholdMax));
            vfx.SetVector2("AlphaRange", Vector2.Lerp(new Vector2(alphaMin, alphaMin * 0.1f), new Vector2(alphaMax, alphaMax * 0.1f), (v - thresholdMin) / thresholdMax));
        }
    }
}
