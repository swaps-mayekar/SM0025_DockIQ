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
                var sprite = LoadLogo();
                _logoImage.sprite = sprite != null ? sprite : _fallbackLogo;
            }

            StartCoroutine(GoMenu());
        }

        private static Sprite LoadLogo()
        {
            return Resources.Load<Sprite>("GameLogo");
        }

        private IEnumerator GoMenu()
        {
            yield return new WaitForSecondsRealtime(GameConstants.SplashSeconds);
            SceneRouter.LoadMenu();
        }
    }
}
