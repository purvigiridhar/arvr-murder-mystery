using UnityEngine;

namespace MurderMystery
{
    /// <summary>
    /// Person 4 Visual Indicator:
    /// Provides subtle, natural visual prominence for evidence objects (warm amber accent glow,
    /// subtle breathing light, and gentle highlight) to help players discover clues in Floor 11
    /// without breaking the detective atmosphere.
    /// </summary>
    [RequireComponent(typeof(EvidenceCollectible))]
    public class ClueVisualCue : MonoBehaviour
    {
        [Header("Lighting Indicator")]
        [SerializeField] private bool addPointLight = true;
        [SerializeField] private Color glowColor = new Color(0.96f, 0.65f, 0.15f, 1.0f); // Warm Amber
        [SerializeField] private float baseIntensity = 0.85f;
        [SerializeField] private float pulseRange = 0.25f;
        [SerializeField] private float pulseSpeed = 1.6f;
        [SerializeField] private float lightRange = 1.8f;

        private Light clueLight;

        private void Start()
        {
            if (addPointLight)
            {
                SetupLight();
            }
        }

        private void SetupLight()
        {
            GameObject lightObj = new GameObject("Clue_GlowLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.15f, 0f);

            clueLight = lightObj.AddComponent<Light>();
            clueLight.type = LightType.Point;
            clueLight.color = glowColor;
            clueLight.range = lightRange;
            clueLight.intensity = baseIntensity;
            clueLight.shadows = LightShadows.None;
        }

        private void Update()
        {
            if (clueLight != null)
            {
                // Subtle breathing pulse effect
                float pulse = Mathf.Sin(Time.time * pulseSpeed);
                clueLight.intensity = baseIntensity + (pulse * pulseRange);
            }
        }
    }
}

