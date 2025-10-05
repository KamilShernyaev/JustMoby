using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Zones.ScrollArea;
using Zones.DropZones.Tower;
using Zones.DropZones.Hole;

namespace Services.CanvasScalerService
{
    public class CanvasScalerService : ICanvasScalerService
    {
        private readonly CanvasScalerConfig config;
        private readonly Canvas canvas;
        private readonly ScrollContainerView scrollView;
        private readonly TowerContainerView towerView;
        private readonly HoleView holeView;
        private float currentAspect;
        private string matchedPreset;

        public CanvasScalerService(CanvasScalerConfig config, Canvas canvas, ScrollContainerView scrollView,
            TowerContainerView towerView, HoleView holeView)
        {
            this.config = config;
            this.canvas = canvas;
            this.scrollView = scrollView;
            this.towerView = towerView;
            this.holeView = holeView;
        }

        public void AdjustUI()
        {
            if (canvas == null || config == null)
            {
                Debug.LogWarning("CanvasScalerService: Canvas or Config missing — skip adjust");
                return;
            }

            currentAspect = (float)Screen.width / Screen.height;
            var closestPreset = config.GetClosestPreset(currentAspect);
            matchedPreset = closestPreset.PresetName;

            Debug.Log(
                $"CanvasScaler: Detected {currentAspect:F3} ({Screen.width}x{Screen.height}), matched '{matchedPreset}'");

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                Debug.Log("CanvasScalerService: Added CanvasScaler component");
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.matchWidthOrHeight = closestPreset.CanvasMatchWidthOrHeight;
            scaler.referenceResolution = closestPreset.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100f;

            AdjustZonePositions(closestPreset);

            ApplyGlobalScale(closestPreset.UIElementsScale);
        }

        private void AdjustZonePositions(AspectPreset preset)
        {
            if (scrollView != null)
            {
                var scrollRt = scrollView.GetComponent<RectTransform>();
                if (scrollRt != null)
                {
                    var pos = scrollRt.anchoredPosition;
                    pos.y += preset.ScrollOffsetY;
                    scrollRt.anchoredPosition = pos;
                    Debug.Log($"Adjusted Scroll Y: {pos.y} (offset {preset.ScrollOffsetY})");
                }
            }

            if (towerView != null)
            {
                var towerRt = towerView.GetComponent<RectTransform>();
                if (towerRt != null)
                {
                    var pos = towerRt.anchoredPosition;
                    pos.y += preset.TowerOffsetY;
                    towerRt.anchoredPosition = pos;
                    Debug.Log($"Adjusted Tower Y: {pos.y} (offset {preset.TowerOffsetY})");
                }
            }

            if (holeView == null) return;
            {
                var holeRt = holeView.GetComponent<RectTransform>();
                if (holeRt == null) return;
                var pos = holeRt.anchoredPosition;
                pos.y += preset.HoleOffsetY;
                holeRt.anchoredPosition = pos;
                Debug.Log($"Adjusted Hole Y: {pos.y} (offset {preset.HoleOffsetY})");
            }
        }

        private void ApplyGlobalScale(float scale)
        {
            if (Mathf.Approximately(scale, 1f)) return;
            Debug.Log($"Global UI Scale: {scale} — apply to pools/prefabs if needed");
        }

        public float GetCurrentAspectRatio() => currentAspect;

        public string GetMatchedPreset() => matchedPreset;
    }
}