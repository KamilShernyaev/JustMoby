using System;
using UnityEngine;

namespace Services.AnimationService
{
    public interface IAnimationService
    {
        void PlayJump(Vector3 startPos, Vector3 endPos, Sprite elementSprite = null, float duration = 0.5f,
            Action onComplete = null, bool arc = true);

        void PlayFade(Transform target, bool fadeIn, float duration = 0.3f, Action onComplete = null);

        void PlayDropDown(Transform[] targets, Vector3[] newPositions, float duration = 0.5f, Action onComplete = null);
        void CancelAnimation(Transform vTransform);
    }
}