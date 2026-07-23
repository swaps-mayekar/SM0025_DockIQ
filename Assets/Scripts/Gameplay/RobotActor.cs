using DockIQ.Board;
using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>Tiny warehouse robot that drives continuously on tracks.</summary>
    public sealed class RobotActor : MonoBehaviour
    {
        public Vector2Int Cell { get; private set; }
        public Dir Facing { get; private set; }
        public bool IsRescue { get; private set; }
        public bool Arrived { get; private set; }
        public string Callsign { get; private set; }

        /// <summary>
        /// After a lift drop that could not step off, ignore teleport for one tick.
        /// </summary>
        public bool SuppressLift { get; set; }

        private SpriteRenderer _body;
        private SpriteRenderer _outline;
        private Vector3 _from;
        private Vector3 _to;
        private float _t;
        private bool _moving;

        public void Init(Vector2Int cell, Dir facing, bool isRescue, string callsign, Vector3 worldPos)
        {
            Cell = cell;
            Facing = facing;
            IsRescue = isRescue;
            Callsign = callsign;
            transform.position = worldPos + new Vector3(0f, 0.14f, 0f);
            _from = _to = transform.position;

            _body = CreateChild("Body", IsoMath.DepthOrder(cell, 5));
            _body.sprite = UI.SpriteCatalog.RobotOrFallback(isRescue);
            _body.color = isRescue ? UI.PlaceholderArt.VipGold : UI.PlaceholderArt.RobotGrey;
            transform.localScale = new Vector3(0.38f, 0.32f, 1f);
            ApplyFacingVisual();

            if (isRescue)
            {
                _outline = CreateChild("Outline", IsoMath.DepthOrder(cell, 4));
                _outline.sprite = UI.PlaceholderArt.WhiteSquare();
                _outline.color = new Color(1f, 0.9f, 0.2f, 0.5f);
                _outline.transform.localScale = Vector3.one * 1.3f;
            }
        }

        public void BeginMove(Vector2Int next, Dir newFacing, Vector3 worldPos)
        {
            Cell = next;
            Facing = newFacing;
            _from = transform.position;
            _to = worldPos + new Vector3(0f, 0.14f, 0f);
            _t = 0f;
            _moving = true;
            ApplyDepth();
            ApplyFacingVisual();
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

            if (IsRescue && _outline != null)
            {
                float pulse = 1.2f + Mathf.Sin(Time.time * 6f) * 0.08f;
                _outline.transform.localScale = Vector3.one * pulse;
            }
        }

        private void ApplyFacingVisual()
        {
            float z = IsoMath.DirToZDegrees(Facing);
            if (_body != null)
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, z);
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
