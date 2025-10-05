using UnityEngine;

namespace Services.CanvasScalerService
{
    public interface ICanvasScalerService
    {
        void AdjustUI();
        float GetCurrentAspectRatio();
        string GetMatchedPreset();
    }
}