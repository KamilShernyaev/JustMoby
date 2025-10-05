using UnityEngine;

namespace Services.CanvasScalerService
{
    [System.Serializable]
    public struct AspectPreset
    {
        [Header("Aspect Ratio (width/height)")]
        public string PresetName;
        public float TargetAspect;
        
        [Header("Canvas Scaler Settings")] [Range(0f, 1f)]
        public float CanvasMatchWidthOrHeight;
        public Vector2 ReferenceResolution;

        [Header("Zone Offsets (localPosition adjust)")]
        public float ScrollOffsetY;
        public float TowerOffsetY;
        public float HoleOffsetY;

        [Header("Global Scale")] [Range(0.5f, 1.5f)]
        public float UIElementsScale;
    }
}