using System.Collections;
using DockIQ.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public sealed class SplashController : MonoBehaviour
    {
        [SerializeField] private Image _logoImage;
        [SerializeField] private Sprite _fallbackLogo;

        private void Awake()
        {
            if (_logoImage != null)
            {
                var sprite = UiChrome.GameLogo;
                if (sprite == null)
                    sprite = _fallbackLogo;
                if (sprite != null)
                    _logoImage.sprite = sprite;
            }

            StartCoroutine(GoMenu());
        }

        private IEnumerator GoMenu()
        {
            yield return new WaitForSecondsRealtime(GameConstants.SplashSeconds);
            SceneRouter.LoadMenu();
        }
    }
}
