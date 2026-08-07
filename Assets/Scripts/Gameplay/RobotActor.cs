using DockIQ.Board;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>Tiny warehouse robot that drives continuously on tracks.</summary>
    public sealed class RobotActor : MonoBehaviour
    {
        public CellCoord Coord { get; private set; }
        public Vector2Int Cell => Coord.XY;
        public int Layer => Coord.Layer;
        public Dir Facing { get; private set; }
        public bool IsRescue { get; private set; }
        public bool Arrived { get; private set; }
        public string Callsign { get; private set; }

        /// <summary>
        /// After a lift/elevator drop, ignore transfer for one tick so the robot can step off.
        /// </summary>
        public bool SuppressLift { get; set; }

        /// <summary>Extra simulation ticks to linger on a lift/elevator pad after arriving.</summary>
        public int PadHoldTicks { get; set; }

        private const float VisualYOffset = 0.14f;

        private SpriteRenderer _body;
        private SpriteRenderer _outline;
        private Vector3 _from;
        private Vector3 _to;
        private float _t;
        private bool _moving;
        private bool _useParcelArt;
        private float _outlineBaseScale = 1.2f;
        /// <summary>Board Origin.y — strip from world Y so depth matches <see cref="IsoMath.DepthOrder"/>.</summary>
        private float _boardOriginY;

        public void Init(CellCoord cell, Dir facing, bool isRescue, string callsign, Vector3 worldPos,
            int levelId = 0)
        {
            Coord = cell;
            Facing = facing;
            IsRescue = isRescue;
            Callsign = callsign;
            _boardOriginY = worldPos.y - IsoMath.CellToWorld(cell).y;
            transform.position = worldPos + new Vector3(0f, VisualYOffset, 0f);
            _from = _to = transform.position;

            _body = CreateChild("Body", 0);

            Sprite parcel = isRescue ? SpriteCatalog.ParcelForLevel(levelId) : null;
            if (parcel != null)
            {
                _useParcelArt = true;
                _body.sprite = parcel;
                _body.color = Color.white;
                // Parcel cells are ~150px at 100 PPU — keep them readable on the iso board.
                transform.localScale = new Vector3(0.52f, 0.52f, 1f);
            }
            else
            {
                _useParcelArt = false;
                Sprite robot = SpriteCatalog.RobotOrFallback(isRescue);
                _body.sprite = robot;
                bool painted = SpriteCatalog.IsProductionArt(robot);
                _body.color = painted
                    ? Color.white
                    : (isRescue ? PlaceholderArt.VipGold : PlaceholderArt.RobotGrey);
                if (painted)
                {
                    float s = SpriteCatalog.FitWidthScale(robot, 0.55f);
                    transform.localScale = new Vector3(s, s, 1f);
                }
                else
                {
                    transform.localScale = new Vector3(0.38f, 0.32f, 1f);
                }
            }

            ApplyFacingVisual();

            if (isRescue)
            {
                _outline = CreateChild("Outline", 0);
                Sprite ring = SpriteCatalog.SelectionRingOrFallback();
                _outline.sprite = ring;
                bool ringArt = SpriteCatalog.IsProductionArt(ring);
                _outline.color = ringArt
                    ? new Color(1f, 1f, 1f, _useParcelArt ? 0.7f : 0.85f)
                    : new Color(1f, 0.9f, 0.2f, _useParcelArt ? 0.35f : 0.5f);
                if (ringArt)
                {
                    float ringScale = SpriteCatalog.FitWidthScale(ring, _useParcelArt ? 0.85f : 0.7f)
                                     / Mathf.Max(0.01f, transform.localScale.x);
                    _outlineBaseScale = ringScale;
                    _outline.transform.localScale = Vector3.one * ringScale;
                }
                else
                {
                    _outlineBaseScale = _useParcelArt ? 1.15f : 1.3f;
                    _outline.transform.localScale = Vector3.one * _outlineBaseScale;
                }
            }

            ApplyDepth();
        }

        public void BeginMove(CellCoord next, Dir newFacing, Vector3 worldPos)
        {
            Coord = next;
            Facing = newFacing;
            _from = transform.position;
            _to = worldPos + new Vector3(0f, VisualYOffset, 0f);
            _t = 0f;
            _moving = true;
            ApplyDepth();
            ApplyFacingVisual();
        }

        public void MarkArrived()
        {
            Arrived = true;
            // Decoys vanish into the gate once their slide finishes (or immediately if already there).
            if (!IsRescue && !_moving)
                gameObject.SetActive(false);
        }

        public void TickVisual(float duration)
        {
            if (_moving)
            {
                _t += Time.deltaTime / Mathf.Max(0.01f, duration);
                if (_t >= 1f)
                {
                    _t = 1f;
                    _moving = false;
                    if (Arrived && !IsRescue)
                        gameObject.SetActive(false);
                }

                transform.position = Vector3.Lerp(_from, _to, Mathf.SmoothStep(0f, 1f, _t));
                ApplyDepth();
            }

            if (IsRescue && _outline != null)
            {
                float pulse = _outlineBaseScale * (1f + Mathf.Sin(Time.time * 6f) * 0.06f);
                _outline.transform.localScale = Vector3.one * pulse;
            }
        }

        private void ApplyFacingVisual()
        {
            if (_body == null)
                return;

            // Cargo art stays upright; placeholder robots rotate with facing.
            if (_useParcelArt)
            {
                _body.transform.localRotation = Quaternion.identity;
                return;
            }

            // Production robot/decoy art is authored facing East (iso SE), same as
            // belt arrows — use East-relative rotation so East stays at 0°.
            float z = SpriteCatalog.IsProductionArt(_body.sprite)
                ? IsoMath.ArrowZDegrees(Facing)
                : IsoMath.DirToZDegrees(Facing);
            _body.transform.localRotation = Quaternion.Euler(0f, 0f, z);
        }

        private void ApplyDepth()
        {
            // Match IsoMath.DepthOrder: use board-local ground Y (not centered world Y / visual lift).
            // Otherwise parcels sort above every device after the board Origin shift.
            float localY = transform.position.y - _boardOriginY - VisualYOffset
                           - Coord.Layer * IsoMath.LayerHeight;
            int dynamicDepth = Coord.Layer * 1000 - Mathf.RoundToInt(localY * 40f);
            if (_body != null)
                _body.sortingOrder = dynamicDepth + 5;
            if (_outline != null)
                _outline.sortingOrder = dynamicDepth + 4;
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
