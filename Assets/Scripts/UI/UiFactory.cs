using DockIQ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public static class UiFactory
    {
        public static Canvas CreateCanvas(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1170f, 2532f);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();
            return canvas;
        }

        public static RectTransform CreateSafeArea(Transform parent)
        {
            var go = new GameObject("SafeArea", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            StretchFull(rt);

            var fitter = go.AddComponent<SafeAreaFitter>();
            fitter.Apply();
            return rt;
        }

        public static Image CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = PlaceholderArt.WhiteSquare();
            img.color = color;
            img.type = Image.Type.Simple;
            // Prefer production chrome for modal-sized panels.
            if (size.x >= 500f && size.y >= 280f && color.a > 0.5f)
                UiChrome.ApplyPanel(img, large: size.y >= 700f);
            else if (color.a < 0.5f || name.IndexOf("Backdrop", System.StringComparison.OrdinalIgnoreCase) >= 0)
                UiChrome.ApplyBackdrop(img);
            return img;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size,
            FontStyles style, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = PlaceholderArt.Text;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos,
            UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(220f, 72f);
            rt.anchoredPosition = anchoredPos;

            var img = go.GetComponent<Image>();
            var btn = go.GetComponent<Button>();
            UiChrome.ApplyButton(img, btn, UiChrome.StyleForButtonName(name));
            if (img.sprite == null || img.sprite.name == "PlaceholderWhite")
                img.color = new Color(0.15f, 0.35f, 0.55f, 1f);

            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            CreateText(go.transform, "Label", label, 28, FontStyles.Bold, Vector2.zero, new Vector2(200f, 60f));
            return btn;
        }

        public static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(es);
        }
    }

    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _last;

        private void Awake()
        {
            _rt = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (_last != Screen.safeArea)
                Apply();
        }

        public void Apply()
        {
            _rt ??= (RectTransform)transform;
            _last = Screen.safeArea;
            var safe = _last;
            var min = safe.position;
            var max = safe.position + safe.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
