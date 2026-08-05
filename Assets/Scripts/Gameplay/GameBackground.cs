using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>
    /// Full-screen world-space backdrop so level SpriteRenderers draw above it.
    /// UI Overlay canvases always render on top of the world — never put GameBG there.
    /// </summary>
    public sealed class GameBackground : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private int _sortingOrder = -1000;

        private Camera _cam;

        public void Setup(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (_renderer == null)
            {
                var go = new GameObject("GameBG");
                go.transform.SetParent(transform, false);
                _renderer = go.AddComponent<SpriteRenderer>();
            }

            _renderer.sprite = sprite;
            _renderer.sortingOrder = _sortingOrder;
            _renderer.color = Color.white;
            FitToCamera();
        }

        private void LateUpdate() => FitToCamera();

        private void FitToCamera()
        {
            if (_renderer == null || _renderer.sprite == null)
                return;

            _cam ??= Camera.main;
            if (_cam == null || !_cam.orthographic)
                return;

            float height = _cam.orthographicSize * 2f;
            float width = height * _cam.aspect;
            Vector2 spriteSize = _renderer.sprite.bounds.size;
            if (spriteSize.x < 0.001f || spriteSize.y < 0.001f)
                return;

            float scale = Mathf.Max(width / spriteSize.x, height / spriteSize.y);
            var t = _renderer.transform;
            t.position = new Vector3(_cam.transform.position.x, _cam.transform.position.y, 5f);
            t.localScale = new Vector3(scale, scale, 1f);
            _renderer.sortingOrder = _sortingOrder;
        }
    }
}
