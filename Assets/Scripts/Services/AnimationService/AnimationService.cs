using System;
using Services.PoolService;
using Element;
using UnityEngine;
using PrimeTween;
using UnityEngine.UI;
using VContainer;

namespace Services.AnimationService
{
    public class AnimationService : IAnimationService
    {
        private readonly ObjectPool<ElementView> jumpPool;
        private readonly Transform animationsContainer;

        public AnimationService(
            [Key("AnimationPool")] ObjectPool<ElementView> jumpPool,
            Transform animationsContainer,
            Canvas canvas)
        {
            this.jumpPool = jumpPool;
            this.animationsContainer = animationsContainer ?? canvas.transform;
            jumpPool.PreWarm(5, animationsContainer);
        }

        public void PlayJump(Vector3 startLocalPos, Vector3 endLocalPos, Sprite elementSprite = null,
            float duration = 0.5f,
            Action onComplete = null, bool arc = true)
        {
            var jumpView = jumpPool.Get();
            var rt = jumpView.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogError("AnimationService: No RectTransform on jumpView!");
                onComplete?.Invoke();
                return;
            }

            jumpView.transform.SetParent(animationsContainer, false);
            rt.anchoredPosition = new Vector2(startLocalPos.x, startLocalPos.y);

            if (elementSprite != null)
                jumpView.SetSprite(elementSprite);

            jumpView.SetAlpha(1f);

            var canvasGroup = jumpView.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            Debug.Log($"PlayJump: start={startLocalPos}, end={endLocalPos} (Canvas-local)");

            if (arc)
                AnimateJumpView(jumpView, endLocalPos, duration, Complete);
            else
                AnimateLinearJumpView(jumpView, endLocalPos, duration, Complete);

            void Complete()
            {
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.interactable = true;
                }

                jumpPool.ReturnToPool(jumpView);
                onComplete?.Invoke();
            }
        }

        private void AnimateJumpView(ElementView view, Vector3 endLocalPos, float duration, Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            var canvasGroup = view.GetComponent<CanvasGroup>();
            var startPos = rt.anchoredPosition;
            var endPos = new Vector2(endLocalPos.x, endLocalPos.y);

            var midY = Mathf.Max(startPos.y, endPos.y) + 100f;
            var midX = (startPos.x + endPos.x) / 2f + UnityEngine.Random.Range(-50f, 50f);
            var controlPos = new Vector2(midX, midY);

            var seq = Sequence.Create();

            // Single Bezier tween for smooth parabolic arc
            seq.Chain(Tween.Custom(0f, 1f, duration, (float t) =>
            {
                var u = 1f - t;
                var t2 = t * t;
                var u2 = u * u;
                var twoUT = 2f * u * t;
                var pos = u2 * startPos + twoUT * controlPos + t2 * endPos;
                rt.anchoredPosition = pos;
            }, Ease.Linear));

            seq.Group(Tween.LocalEulerAngles(view.transform, view.transform.localEulerAngles, new Vector3(0, 0, 360f),
                duration, Ease.Linear));
            seq.Group(Tween.Scale(view.transform, Vector3.one * 1.2f, duration * 0.3f, Ease.OutQuad)
                .Chain(Tween.Scale(view.transform, Vector3.one, duration * 0.7f, Ease.InQuad)));

            seq.OnComplete(() =>
            {
                if (canvasGroup != null)
                    canvasGroup.alpha = 0f;
                else
                    view.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void AnimateLinearJumpView(ElementView view, Vector3 endLocalPos, float duration, Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            var endPos = new Vector2(endLocalPos.x, endLocalPos.y);

            Tween.UIAnchoredPosition(rt, endPos, duration, Ease.Linear)
                .OnComplete(() =>
                {
                    var canvasGroup = view.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                        canvasGroup.alpha = 0f;
                    else
                        view.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        public void PlayFade(Transform target, bool fadeIn, float duration = 0.3f, Action onComplete = null)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
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
                    Tween.Color(image, startColor, endColor, duration, Ease.InOutQuad)
                        .OnComplete(() => onComplete?.Invoke());
                }
                else
                {
                    onComplete?.Invoke();
                }

                return;
            }

            canvasGroup.alpha = startAlpha;
            var startScale = fadeIn ? Vector3.one * 0.8f : Vector3.one;
            target.localScale = startScale;

            var seq = Sequence.Create();
            seq.Chain(Tween.Alpha(canvasGroup, endAlpha, duration, Ease.InOutQuad));
            seq.Group(Tween.Scale(target, fadeIn ? Vector3.one : Vector3.one * 0.8f, duration,
                fadeIn ? Ease.OutQuad : Ease.InQuad));

            seq.OnComplete(onComplete);
        }

        public void PlayDropDown(Transform[] targets, Vector3[] newPositions, float duration = 0.5f,
            Action onComplete = null)
        {
            if (targets == null || newPositions == null || targets.Length == 0 || targets.Length != newPositions.Length)
            {
                onComplete?.Invoke();
                return;
            }

            var seq = Sequence.Create();
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                seq.Group(Tween.LocalPosition(targets[i], newPositions[i], duration, Ease.InOutQuad));
            }

            seq.OnComplete(onComplete);
        }
    }
}