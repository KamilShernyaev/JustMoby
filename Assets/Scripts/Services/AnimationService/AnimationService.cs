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
        private readonly Canvas canvas;

        public AnimationService(
            [Key("AnimationPool")] ObjectPool<ElementView> jumpPool,
            Transform animationsContainer,
            Canvas canvas)
        {
            this.jumpPool = jumpPool;
            this.animationsContainer = animationsContainer;
            this.canvas = canvas;

            // Предварительное создание объектов пула под контейнером анимаций
            jumpPool.PreWarm(5, this.animationsContainer);
        }

        /// <summary>
        /// Запускает анимацию прыжка элемента.
        /// Позиции должны быть в локальных координатах Canvas (anchoredPosition).
        /// </summary>
        public void PlayJump(Vector3 startLocalPos, Vector3 endLocalPos, Sprite elementSprite = null, float duration = 0.5f,
            Action onComplete = null, bool arc = true)
        {
            var jumpView = jumpPool.Get();
            jumpView.transform.SetParent(animationsContainer, false); // Устанавливаем родителя без изменения локальных координат

            var rt = jumpView.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(startLocalPos.x, startLocalPos.y);
            }
            else
            {
                // Если RectTransform отсутствует — fallback в world space
                jumpView.transform.position = canvas != null ? canvas.transform.TransformPoint(startLocalPos) : startLocalPos;
            }

            if (elementSprite != null)
            {
                jumpView.SetSprite(elementSprite);
            }

            jumpView.SetAlpha(1f);

            var canvasGroup = jumpView.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (arc)
            {
                AnimateJumpView(jumpView, endLocalPos, duration, Complete);
            }
            else
            {
                AnimateLinearJumpView(jumpView, endLocalPos, duration, Complete);
            }

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

            var startLocalPos = rt != null ? (Vector3)rt.anchoredPosition : view.transform.localPosition;

            var midY = Mathf.Max(endLocalPos.y, startLocalPos.y) + 150f;
            var midX = (startLocalPos.x + endLocalPos.x) / 2f + UnityEngine.Random.Range(-50f, 50f);
            var midPos2 = new Vector2(midX, midY);
            var endPos2 = new Vector2(endLocalPos.x, endLocalPos.y);

            var seq = Sequence.Create();

            if (rt != null)
            {
                // Анимация в локальных координатах UI
                seq.Chain(Tween.UIAnchoredPosition(rt, midPos2, duration * 0.5f, Ease.OutQuad))
                   .Chain(Tween.UIAnchoredPosition(rt, endPos2, duration * 0.5f, Ease.InQuad));
            }
            else
            {
                // Фолбэк в world space с конвертацией локальных координат в мировые
                var midWorld = canvas != null ? canvas.transform.TransformPoint(midPos2) : (Vector3)midPos2;
                var endWorld = canvas != null ? canvas.transform.TransformPoint(endPos2) : (Vector3)endPos2;

                seq.Chain(Tween.Position(view.transform, midWorld, duration * 0.5f, Ease.OutQuad))
                   .Chain(Tween.Position(view.transform, endWorld, duration * 0.5f, Ease.InQuad));
            }

            if (canvasGroup != null)
            {
                seq.Group(Tween.Delay(duration - 0.2f))
                   .Group(Tween.Alpha(canvasGroup, 0f, 0.2f, Ease.InQuad));
            }

            var startRotation = view.transform.localEulerAngles;
            seq.Group(Tween.LocalEulerAngles(view.transform, startRotation, new Vector3(0, 0, 360f), duration, Ease.Linear));
            seq.Group(Tween.Scale(view.transform, Vector3.one * 1.2f, duration * 0.3f, Ease.OutQuad)
                .Chain(Tween.Scale(view.transform, Vector3.one, duration * 0.7f, Ease.InQuad)));

            seq.OnComplete(() =>
            {
                view.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        private void AnimateLinearJumpView(ElementView view, Vector3 endLocalPos, float duration, Action onComplete)
        {
            var rt = view.GetComponent<RectTransform>();
            if (rt != null)
            {
                var endPos2 = new Vector2(endLocalPos.x, endLocalPos.y);
                Tween.UIAnchoredPosition(rt, endPos2, duration, Ease.Linear)
                    .OnComplete(() =>
                    {
                        view.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
            }
            else
            {
                var endWorld = canvas != null ? canvas.transform.TransformPoint(endLocalPos) : endLocalPos;
                Tween.Position(view.transform, endWorld, duration, Ease.Linear)
                    .OnComplete(() =>
                    {
                        view.gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
            }
        }

        public void PlayFade(Transform target, bool fadeIn, float duration = 0.3f, Action onComplete = null)
        {
            if (target == null)
            {
                onComplete?.Invoke();
                return;
            }

            var startAlpha = fadeIn ? 0f : 1f;
            var endAlpha = fadeIn ? 1f : 0f;
            var canvasGroup = target.GetComponent<CanvasGroup>();
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

            var startScale = fadeIn ? Vector3.one * 0.8f : Vector3.one;
            var endScale = fadeIn ? Vector3.one : Vector3.one * 0.8f;

            canvasGroup.alpha = startAlpha;
            target.localScale = startScale;

            Sequence.Create()
                .Chain(Tween.Alpha(canvasGroup, endAlpha, duration * 0.7f, Ease.InOutQuad))
                .Group(Tween.Scale(target, endScale, duration * 0.3f, Ease.OutQuad)
                    .Chain(Tween.Scale(target, Vector3.one, duration * 0.7f, Ease.InQuad)))
                .OnComplete(onComplete);
        }

        public void PlayDropDown(Transform[] targets, Vector3[] newPositions, float duration = 0.5f, Action onComplete = null)
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

                var rt = targets[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    seq.Group(Tween.UIAnchoredPosition(rt, new Vector2(newPositions[i].x, newPositions[i].y), duration, Ease.InOutQuad));
                }
                else
                {
                    seq.Group(Tween.Position(targets[i], newPositions[i], duration, Ease.InOutQuad));
                }
            }

            seq.OnComplete(onComplete);
        }
    }
}
