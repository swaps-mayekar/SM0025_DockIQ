using System.Collections;
using DockIQ.Core;
using DockIQ.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public sealed class SplashController : MonoBehaviour
    {
        private void Awake()
        {
            BuildUi();
            StartCoroutine(GoMenu());
        }

        private void BuildUi()
        {
            var cam = Camera.main;
            if (cam != null)
                cam.backgroundColor = PlaceholderArt.Navy;

            var canvas = UiFactory.CreateCanvas(transform, "SplashCanvas");
            var safe = UiFactory.CreateSafeArea(canvas.transform);

            var logo = new GameObject("Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            logo.transform.SetParent(safe, false);
            var rt = logo.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.55f);
            rt.sizeDelta = new Vector2(720f, 720f);

            var img = logo.GetComponent<Image>();
            var sprite = LoadLogo();
            img.sprite = sprite != null ? sprite : PlaceholderArt.WhiteSquare();
            img.preserveAspect = true;
            img.color = Color.white;

            UiFactory.CreateText(safe, "Tag", "WAREHOUSE RESCUE", 36, TMPro.FontStyles.Bold,
                new Vector2(0f, -420f), new Vector2(800f, 60f)).color = PlaceholderArt.Hazard;
        }

        private static Sprite LoadLogo()
        {
            // Resources path optional; also try Resources.Load from UI folder via AssetDatabase only in editor.
            return Resources.Load<Sprite>("GameLogo");
        }

        private IEnumerator GoMenu()
        {
            yield return new WaitForSecondsRealtime(GameConstants.SplashSeconds);
            SceneRouter.LoadMenu();
        }
    }
}
