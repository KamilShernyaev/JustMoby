using System;
using System.Collections.Generic;
using Services.PoolService;
using Element;
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;
using VContainer;

namespace Services.AnimationService
{
    public class AnimationService : IAnimationService, IDisposable
    {
        private readonly ObjectPool<ElementView> jumpPool;
        private readonly Transform animationsContainer;
        private readonly Dictionary<Transform, Sequence> activeSequences = new();
        
        private const float JumpAlpha = 0.5f;
        private const float JumpArcHeight = 100f;
        private const float JumpArcRandomX = 50f;
        private const float JumpRotationAngle = 360f;
        private const float JumpScalePeak = 1.2f;
        private const float FadeScaleStart = 0.8f;
        private const float FadeScaleEnd = 1;
        private const int JumpPoolPrewarm = 10;

        [Inject]
        public AnimationService(
            [Key("AnimationPool")] ObjectPool<ElementView> jumpPool,
            Transform animationsContainer,
            Canvas canvas)
        {
            this.jumpPool = jumpPool;
            this.animationsContainer = animationsContainer ?? canvas.transform;
            jumpPool.PreWarm(JumpPoolPrewarm, this.animationsContainer);
        }

        public void PlayJump(Vector3 startLocalPos, Vector3 endLocalPos, Sprite elementSprite = null,
            float duration = 0.5f, Action onComplete = null, bool arc = true)
        {
            var jumpView = jumpPool.Get();
            var rt = jumpView.GetComponent<RectTransform>();
            var canvasGroup = jumpView.GetComponent<CanvasGroup>();

            if (rt == null || canvasGroup == null)
            {
                Debug.LogError("AnimationService: Missing RectTransform or CanvasGroup on jumpView!");
                jumpPool.ReturnToPool(jumpView);
                onComplete?.Invoke();
                return;
            }

            // Stop any existing sequence on this transform
            if (activeSequences.TryGetValue(jumpView.transform, out var existingSequence))
            {
                existingSequence.Stop();
                activeSequences.Remove(jumpView.transform);
            }

            jumpView.transform.SetParent(animationsContainer, false);
            rt.anchoredPosition = new Vector2(startLocalPos.x, startLocalPos.y);

            if (elementSprite != null)
                jumpView.SetSprite(elementSprite);

            jumpView.SetAlpha(JumpAlpha);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var useSimpleAnimation = (!arc || SystemInfo.deviceType != DeviceType.Handheld) && arc;
            var sequence = Sequence.Create();
            activeSequences[jumpView.transform] = sequence;

            if (useSimpleAnimation)
                AnimateJumpView(jumpView, endLocalPos, duration, sequence,
                    () => CompleteAnimation(jumpView, canvasGroup, onComplete));
            else
                AnimateLinearJumpView(jumpView, endLocalPos, duration, sequence,
                    () => CompleteAnimation(jumpView, canvasGroup, onComplete));
        }

        private void AnimateJumpView(ElementView view, Vector3 endLocalPos, float duration, Sequence sequence,
            Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            var startPos = rt.anchoredPosition;
            var endPos = new Vector2(endLocalPos.x, endLocalPos.y);

            var midY = Mathf.Max(startPos.y, endPos.y) + JumpArcHeight;
            var midX = (startPos.x + endPos.x) / 2f + UnityEngine.Random.Range(-JumpArcRandomX, JumpArcRandomX);
            var controlPos = new Vector2(midX, midY);

            sequence.Chain(Tween.Custom(0f, 1f, duration, (float t) =>
            {
                var u = 1f - t;
                var t2 = t * t;
                var u2 = u * u;
                var twoUt = 2f * u * t;
                var pos = u2 * startPos + twoUt * controlPos + t2 * endPos;
                rt.anchoredPosition = pos;
            }, Ease.Linear));

            sequence.Group(Tween.LocalEulerAngles(view.transform, view.transform.localEulerAngles,
                new Vector3(0, 0, JumpRotationAngle), duration, Ease.Linear));
            sequence.Group(Tween.Scale(view.transform, Vector3.one * JumpScalePeak, duration * 0.3f, Ease.OutQuad)
                .Chain(Tween.Scale(view.transform, Vector3.one, duration * 0.7f, Ease.InQuad)));

            sequence.OnComplete(onComplete);
        }

        private void AnimateLinearJumpView(ElementView view, Vector3 endLocalPos, float duration, Sequence sequence,
            Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            var endPos = new Vector2(endLocalPos.x, endLocalPos.y);

            sequence.Chain(Tween.UIAnchoredPosition(rt, endPos, duration, Ease.Linear));
            sequence.OnComplete(onComplete);
        }

        private void CompleteAnimation(ElementView view, CanvasGroup canvasGroup, Action onComplete)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 0f;
            activeSequences.Remove(view.transform);
            jumpPool.ReturnToPool(view);
            onComplete?.Invoke();
        }

        public void PlayFade(Transform target, bool fadeIn, float duration = 0.3f, Action onComplete = null)
        {
            if (target == null)
            {
                Debug.LogWarning("AnimationService: Target is null in PlayFade");
                onComplete?.Invoke();
                return;
            }

            // Stop any existing sequence on this transform
            if (activeSequences.TryGetValue(target, out var existingSequence))
            {
                existingSequence.Stop();
                activeSequences.Remove(target);
            }

            var canvasGroup = target.GetComponent<CanvasGroup>();
            var startAlpha = fadeIn ? 0f : 1f;
            var endAlpha = fadeIn ? 1f : 0f;

            if (canvasGroup == null)
            {
                var image = target.GetComponent<Image>();
                if (image != null)
                {
                    var startColor = new Color(image.color.r, image.color.g, image.color.b, startAlpha);
                    var endColor = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
                    var sequence = Sequence.Create();
                    activeSequences[target] = sequence;
                    sequence.Chain(Tween.Color(image, startColor, endColor, duration, Ease.InOutQuad));
                    sequence.OnComplete(() =>
                    {
                        activeSequences.Remove(target);
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    onComplete?.Invoke();
                }

                return;
            }

            canvasGroup.alpha = startAlpha;
            var startScale = fadeIn ? Vector3.one * FadeScaleStart : Vector3.one;
            target.localScale = startScale;

            var seq = Sequence.Create();
            activeSequences[target] = seq;
            seq.Chain(Tween.Alpha(canvasGroup, endAlpha, duration, Ease.InOutQuad));
            seq.Group(Tween.Scale(target, fadeIn ? Vector3.one : Vector3.one * FadeScaleEnd, duration,
                fadeIn ? Ease.OutQuad : Ease.InQuad));

            seq.OnComplete(() =>
            {
                activeSequences.Remove(target);
                onComplete?.Invoke();
            });
        }

        public void PlayDropDown(Transform[] targets, Vector3[] newPositions, float duration = 0.5f,
            Action onComplete = null)
        {
            if (targets == null || newPositions == null || targets.Length == 0 || targets.Length != newPositions.Length)
            {
                Debug.LogWarning("AnimationService: Invalid targets or positions in PlayDropDown");
                onComplete?.Invoke();
                return;
            }

            var seq = Sequence.Create();
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                // Stop any existing sequence on this transform
                if (activeSequences.TryGetValue(targets[i], out var existingSequence))
                {
                    existingSequence.Stop();
                    activeSequences.Remove(targets[i]);
                }

                activeSequences[targets[i]] = seq;
                seq.Group(Tween.LocalPosition(targets[i], newPositions[i], duration, Ease.InOutQuad));
            }

            seq.OnComplete(() =>
            {
                foreach (var target in targets)
                {
                    if (target != null)
                        activeSequences.Remove(target);
                }

                onComplete?.Invoke();
            });
        }

        public void CancelAnimation(Transform target)
        {
            if (target == null || !activeSequences.TryGetValue(target, out var sequence)) return;
            sequence.Stop();
            activeSequences.Remove(target);
        }

        public void Dispose()
        {
            foreach (var sequence in activeSequences.Values)
            {
                sequence.Stop();
            }

            activeSequences.Clear();
            jumpPool.Clear();
        }
    }
}