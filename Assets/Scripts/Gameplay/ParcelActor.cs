using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Gameplay
{
    public sealed class ParcelActor : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }
        public bool IsVip { get; private set; }
        public bool Arrived { get; private set; }

        private SpriteRenderer _body;
        private SpriteRenderer _outline;
        private Vector3 _from;
        private Vector3 _to;
        private float _t;
        private bool _moving;

        public void Init(Vector2Int cell, bool isVip, Vector3 worldPos)
        {
            Cell = cell;
            IsVip = isVip;
            transform.position = worldPos;
            _from = _to = worldPos;

            // Slight billboard lift so parcels sit "on" the diamond floor
            _body = CreateChild("Body", IsoMath.DepthOrder(cell, 5));
            _body.sprite = UI.SpriteCatalog.ParcelOrFallback(isVip);
            _body.color = isVip ? UI.PlaceholderArt.VipGold : UI.PlaceholderArt.ParcelBrown;
            // Squash slightly for a cheap isometric crate look
            transform.localScale = new Vector3(0.42f, 0.36f, 1f);
            transform.position = worldPos + new Vector3(0f, 0.12f, 0f);
            _from = _to = transform.position;

            if (isVip)
            {
                _outline = CreateChild("Outline", IsoMath.DepthOrder(cell, 4));
                _outline.sprite = UI.PlaceholderArt.WhiteSquare();
                _outline.color = new Color(1f, 0.9f, 0.2f, 0.55f);
                _outline.transform.localScale = Vector3.one * 1.25f;
            }
        }

        public void BeginMove(Vector2Int next, Vector3 worldPos)
        {
            Cell = next;
            _from = transform.position;
            _to = worldPos + new Vector3(0f, 0.12f, 0f);
            _t = 0f;
            _moving = true;
            ApplyDepth();
        }

        public void MarkArrived() => Arrived = true;

        public void TickVisual(float duration)
        {
            if (_moving)
            {
                _t += Time.deltaTime / Mathf.Max(0.01f, duration);
                if (_t >= 1f)
                {
                    _t = 1f;
                    _moving = false;
                }

                transform.position = Vector3.Lerp(_from, _to, Mathf.SmoothStep(0f, 1f, _t));
            }

            if (IsVip && _outline != null)
            {
                float pulse = 1.2f + Mathf.Sin(Time.time * 6f) * 0.08f;
                _outline.transform.localScale = Vector3.one * pulse;
            }
        }

        private void ApplyDepth()
        {
            if (_body != null)
                _body.sortingOrder = IsoMath.DepthOrder(Cell, 5);
            if (_outline != null)
                _outline.sortingOrder = IsoMath.DepthOrder(Cell, 4);
        }

        private SpriteRenderer CreateChild(string name, int sorting)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sorting;
            return sr;
        }
    }
}
