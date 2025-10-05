using UnityEngine;

namespace Services.CanvasScalerService
{
    [CreateAssetMenu(fileName = "CanvasScalerConfig", menuName = "Game/UI/Canvas Scaler Config", order = 1)]
    public class CanvasScalerConfig : ScriptableObject
    {
        [Header("Presets for Common Mobile Ratios")]
        public AspectPreset[] Presets;

        [Header("Fallback (if no match)")] public AspectPreset DefaultPreset;

        public AspectPreset GetClosestPreset(float currentAspect)
        {
            if (Presets == null || Presets.Length == 0) return DefaultPreset;

            var closest = DefaultPreset;
            var minDiff = float.MaxValue;

            foreach (var preset in Presets)
            {
                var diff = Mathf.Abs(preset.TargetAspect - currentAspect);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closest = preset;
                }
            }

            return closest;
        }
    }
}