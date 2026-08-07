using System.Collections;
using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Lightweight uGUI motion helpers (SmoothStep / coroutines — no tween package).
    /// </summary>
    public static class UiMotion
    {
        public static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            if (go == null)
                return null;

            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        public static float Smooth01(float t) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

        public static IEnumerator Fade(
            CanvasGroup group,
            float from,
            float to,
            float duration,
            bool setInteractable = true)
        {
            if (group == null)
                yield break;

            group.gameObject.SetActive(true);
            group.alpha = from;
            if (setInteractable)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            if (duration <= 0.001f)
            {
                group.alpha = to;
            }
            else
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.unscaledDeltaTime / duration;
                    group.alpha = Mathf.Lerp(from, to, Smooth01(t));
                    yield return null;
                }

                group.alpha = to;
            }

            bool visible = to > 0.01f;
            if (setInteractable)
            {
                group.interactable = visible;
                group.blocksRaycasts = visible;
            }

            if (!visible)
                group.gameObject.SetActive(false);
        }

        public static IEnumerator FadeScale(
            CanvasGroup group,
            RectTransform scaleTarget,
            float fromAlpha,
            float toAlpha,
            float fromScale,
            float toScale,
            float duration)
        {
            if (group == null)
                yield break;

            group.gameObject.SetActive(true);
            group.alpha = fromAlpha;
            group.interactable = false;
            group.blocksRaycasts = false;

            if (scaleTarget != null)
                scaleTarget.localScale = Vector3.one * fromScale;

            if (duration <= 0.001f)
            {
                group.alpha = toAlpha;
                if (scaleTarget != null)
                    scaleTarget.localScale = Vector3.one * toScale;
            }
            else
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.unscaledDeltaTime / duration;
                    float s = Smooth01(t);
                    group.alpha = Mathf.Lerp(fromAlpha, toAlpha, s);
                    if (scaleTarget != null)
                        scaleTarget.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, s);
                    yield return null;
                }

                group.alpha = toAlpha;
                if (scaleTarget != null)
                    scaleTarget.localScale = Vector3.one * toScale;
            }

            bool visible = toAlpha > 0.01f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
            if (!visible)
                group.gameObject.SetActive(false);
        }

        public static IEnumerator SlideFadeIn(
            CanvasGroup group,
            RectTransform rt,
            Vector2 restPos,
            float duration,
            float fromYOffset)
        {
            if (group == null || rt == null)
                yield break;

            group.gameObject.SetActive(true);
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            rt.anchoredPosition = restPos + new Vector2(0f, fromYOffset);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration);
                float s = Smooth01(t);
                group.alpha = s;
                rt.anchoredPosition = Vector2.Lerp(restPos + new Vector2(0f, fromYOffset), restPos, s);
                yield return null;
            }

            group.alpha = 1f;
            rt.anchoredPosition = restPos;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }
}
