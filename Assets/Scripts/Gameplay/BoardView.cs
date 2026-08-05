using DockIQ.Board;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>Isometric multi-layer track network visuals.</summary>
    public sealed class BoardView : MonoBehaviour
    {
        private GridBoard _board;
        private Transform _root;
        private SpriteRenderer[,,] _tiles;
        private SpriteRenderer[,,] _arrows;
        private TextMesh[,,] _labels;
        private Transform _pathRoot;

        public void Build(GridBoard board)
        {
            _board = board;
            if (_root != null)
                Destroy(_root.gameObject);

            _root = new GameObject("BoardRoot").transform;
            _root.SetParent(transform, false);

            _tiles = new SpriteRenderer[board.LayerCount, board.Width, board.Height];
            _arrows = new SpriteRenderer[board.LayerCount, board.Width, board.Height];
            _labels = new TextMesh[board.LayerCount, board.Width, board.Height];

            for (int L = 0; L < board.LayerCount; L++)
            for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
            {
                var cell = board.Get(L, x, y);
                if (!cell.IsTraversable)
                    continue;

                var coord = new CellCoord(x, y, L);
                Vector3 pos = board.CellToWorld(coord, 0f);
                int depth = IsoMath.DepthOrder(coord);
                float alpha = L > 0 ? 0.88f : 1f;

                var tile = CreateSprite($"Cell_{L}_{x}_{y}", pos, depth);
                Sprite art = SpriteForCell(coord, cell);
                tile.sprite = art;
                ApplyTileVisual(tile, art, cell);
                var col = ColorFor(cell, art);
                col.a *= alpha;
                tile.color = col;
                _tiles[L, x, y] = tile;

                bool hasArt = SpriteCatalog.IsProductionArt(art);
                if (cell.Type == CellType.Dock)
                {
                    if (!hasArt)
                        AddLabel(L, x, y, pos, depth, cell.DockId.ToString());
                }
                else if (NeedsDeviceLabel(cell) && !hasArt)
                {
                    AddLabel(L, x, y, pos, depth, DeviceLabel(cell));
                }
                else if (ShowsDirectionArrow(coord, cell, hasArt))
                {
                    var arrow = CreateSprite($"Arrow_{L}_{x}_{y}", pos + new Vector3(0f, 0.08f, 0f), depth + 1);
                    Sprite arrowArt = SpriteCatalog.DirectionArrowOrFallback();
                    arrow.sprite = arrowArt;
                    bool arrowArtReady = SpriteCatalog.IsProductionArt(arrowArt);
                    // Keep production arrows untinted so yellow MarkerColor doesn't hide direction.
                    arrow.color = arrowArtReady
                        ? new Color(0.45f, 1f, 0.85f, 0.95f)
                        : MarkerColor(cell);
                    if (!arrowArtReady)
                        arrow.transform.localScale = new Vector3(0.12f, 0.28f, 1f);
                    else
                        arrow.transform.localScale =
                            Vector3.one * SpriteCatalog.FitWidthScale(arrowArt, board.CellSize * 0.22f);
                    arrow.transform.rotation =
                        Quaternion.Euler(0f, 0f, IsoMath.ArrowZDegrees(cell.GetDisplayDir()));
                    _arrows[L, x, y] = arrow;
                }
            }

            DrawMovablePaths();
            DrawElevatorShafts();
            RefreshDevices();
        }

        /// <summary>
        /// Draw a thin vertical connector between co-located elevator pads so the shaft reads clearly
        /// across the increased floor gap.
        /// </summary>
        private void DrawElevatorShafts()
        {
            var drawn = new System.Collections.Generic.HashSet<long>();
            for (int L = 0; L < _board.LayerCount; L++)
            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var cell = _board.Get(L, x, y);
                if (!cell.IsElevator)
                    continue;

                CellCoord from = new CellCoord(x, y, L);
                CellCoord to = cell.ElevatorTarget;
                if (!_board.InBounds(to) || to.Layer == from.Layer)
                    continue;
                if (to.X != from.X || to.Y != from.Y)
                    continue;

                long key = (((long)from.X * 37 + from.Y) * 37 +
                            System.Math.Min(from.Layer, to.Layer)) * 37 +
                           System.Math.Max(from.Layer, to.Layer);
                if (!drawn.Add(key))
                    continue;

                Vector3 a = _board.CellToWorld(from, 0.02f);
                Vector3 b = _board.CellToWorld(to, 0.02f);
                Vector3 mid = (a + b) * 0.5f;
                float span = Mathf.Abs(b.y - a.y);
                if (span < 0.05f)
                    continue;

                var sr = CreateSprite($"Shaft_{from.X}_{from.Y}", mid, IsoMath.DepthOrder(from, -2));
                sr.sprite = PlaceholderArt.IsoDiamond();
                sr.color = new Color(0.45f, 0.75f, 0.90f, 0.35f);
                // Stretch the diamond into a narrow vertical bar between floors.
                float w = _board.CellSize * 0.12f;
                float h = span * 0.92f;
                sr.transform.localScale = new Vector3(w, h, 1f);
            }
        }

        private void DrawMovablePaths()
        {
            if (_pathRoot != null)
                Destroy(_pathRoot.gameObject);
            _pathRoot = new GameObject("Paths").transform;
            _pathRoot.SetParent(_root, false);

            Sprite waypoint = SpriteCatalog.PathWaypointOrFallback();
            bool art = SpriteCatalog.IsProductionArt(waypoint);
            float scale = art
                ? SpriteCatalog.FitWidthScale(waypoint, _board.CellSize * 0.35f)
                : 0.22f;

            foreach (var piece in _board.Movables)
            {
                for (int i = 0; i < piece.Path.Length; i++)
                {
                    var slot = piece.Path[i];
                    Vector3 pos = _board.CellToWorld(slot, 0.05f);
                    var sr = CreateSprite($"Path_{piece.Id}_{i}", pos, IsoMath.DepthOrder(slot, 1));
                    sr.sprite = waypoint;
                    sr.color = art ? new Color(1f, 1f, 1f, 0.55f) : new Color(1f, 1f, 1f, 0.22f);
                    sr.transform.localScale = Vector3.one * scale;
                    sr.transform.SetParent(_pathRoot, true);
                }
            }
        }

        public void RefreshDevices()
        {
            if (_board == null)
                return;

            for (int L = 0; L < _board.LayerCount; L++)
            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var cell = _board.Get(L, x, y);
                var tile = _tiles[L, x, y];
                if (tile != null)
                {
                    var coord = new CellCoord(x, y, L);
                    Sprite art = SpriteForCell(coord, cell);
                    if (tile.sprite != art)
                        tile.sprite = art;
                    ApplyTileVisual(tile, art, cell);

                    var col = ColorFor(cell, art);
                    if (L > 0)
                        col.a *= 0.88f;
                    tile.color = col;
                    tile.enabled = cell.IsTraversable;
                }

                var arrow = _arrows[L, x, y];
                if (arrow != null)
                {
                    arrow.transform.rotation =
                        Quaternion.Euler(0f, 0f, IsoMath.ArrowZDegrees(cell.GetDisplayDir()));
                    if (!SpriteCatalog.IsProductionArt(arrow.sprite))
                        arrow.color = MarkerColor(cell);
                    var coord = new CellCoord(x, y, L);
                    arrow.enabled = cell.IsTraversable &&
                                    ShowsDirectionArrow(coord, cell,
                                        SpriteCatalog.IsProductionArt(tile != null ? tile.sprite : null));
                    if (cell.Type == CellType.Bridge)
                        arrow.enabled = cell.Device is BridgeDevice b && b.IsOpen;
                }

                var label = _labels[L, x, y];
                if (label == null)
                    continue;

                if (cell.Type == CellType.Bridge)
                    label.text = cell.Device is BridgeDevice br && br.IsOpen ? "B↑" : "B↓";
                else if (cell.Type == CellType.Liftable)
                    label.text = cell.Device is LiftableDevice lf && lf.IsRaised ? "X↑" : "X";
                else if (cell.Type == CellType.Obstacle)
                    label.text = "O";
                else if (cell.IsElevator)
                    label.text = "E";
                else if (cell.Type == CellType.Reflector)
                    label.text = "M";
            }

            foreach (var piece in _board.Movables)
            {
                var c = piece.Current;
                if (_tiles[c.Layer, c.X, c.Y] != null)
                {
                    var cell = _board.Get(c);
                    Sprite art = SpriteForCell(c, cell);
                    _tiles[c.Layer, c.X, c.Y].sprite = art;
                    ApplyTileVisual(_tiles[c.Layer, c.X, c.Y], art, cell);
                    _tiles[c.Layer, c.X, c.Y].color = ColorFor(cell, art);
                }
            }
        }

        private void ApplyTileVisual(SpriteRenderer tile, Sprite art, CellData cell)
        {
            bool isDock = cell.Type == CellType.Dock;
            if (isDock && SpriteCatalog.IsProductionArt(art))
            {
                tile.transform.localScale = Vector3.one * (_board.CellSize * 0.28f);
                tile.transform.localRotation = Quaternion.identity;
                return;
            }

            float scale = SpriteCatalog.IsProductionArt(art)
                ? SpriteCatalog.FitWidthScale(art, _board.CellSize)
                : _board.CellSize;
            tile.transform.localScale = Vector3.one * scale;

            // Only straight belt/spawn art orients with facing. Switch/rotator slices stay upright.
            if (OrientsWithFacing(cell.Type) && IsOrientableBeltArt(art) &&
                SpriteCatalog.IsProductionArt(art))
            {
                Dir dir = cell.GetDisplayDir();
                bool reverse = dir == Dir.West || dir == Dir.South;
                tile.transform.localRotation = reverse
                    ? Quaternion.Euler(0f, 0f, 180f)
                    : Quaternion.identity;
            }
            else
            {
                tile.transform.localRotation = Quaternion.identity;
            }
        }

        private static bool OrientsWithFacing(CellType type) =>
            type == CellType.Track || type == CellType.Spawn;

        private static bool IsOrientableBeltArt(Sprite art)
        {
            if (art == null)
                return false;
            string n = art.name;
            return n.IndexOf("rotator", System.StringComparison.OrdinalIgnoreCase) < 0;
        }

        private Sprite SpriteForCell(CellCoord coord, CellData cell)
        {
            switch (cell.Type)
            {
                case CellType.Dock:
                    return SpriteCatalog.GateForDockId(cell.DockId) ?? SpriteCatalog.DockOrFallback(cell.DockId);
                case CellType.Spawn:
                    return SpriteCatalog.SpawnOrFallback();
                case CellType.Switch:
                    // Legacy safety: any old switch-typed cell is displayed as a straight rotator.
                    return SpriteCatalog.RotatorForModeOrFallback(0);
                case CellType.Rotator:
                    if (cell.Device is not RotatorDevice rotator)
                        return SpriteCatalog.TrackOrFallback();
                    return SpriteCatalog.RotatorForModeOrFallback(rotator.Mode);
                case CellType.Bridge:
                    bool open = cell.Device is BridgeDevice b && b.IsOpen;
                    return SpriteCatalog.BridgeOrFallback(open);
                case CellType.Lift:
                    return SpriteCatalog.LiftOrFallback();
                case CellType.Elevator:
                    return SpriteCatalog.ElevatorOrFallback();
                case CellType.Reflector:
                    return SpriteCatalog.ReflectorOrFallback();
                case CellType.Obstacle:
                    return SpriteCatalog.ObstacleOrFallback();
                case CellType.Liftable:
                    bool raised = cell.Device is LiftableDevice lf && lf.IsRaised;
                    return SpriteCatalog.LiftableOrFallback(raised);
                default:
                    return SpriteCatalog.TrackOrFallback();
            }
        }

        private static bool NeedsDeviceLabel(CellData cell) =>
            cell.IsLift || cell.IsElevator || cell.Type == CellType.Reflector ||
            cell.Type == CellType.Liftable || cell.Type == CellType.Obstacle ||
            cell.Type == CellType.Bridge;

        private static string DeviceLabel(CellData cell)
        {
            if (cell.IsLift) return "▲";
            if (cell.IsElevator) return "E";
            switch (cell.Type)
            {
                case CellType.Reflector: return "M";
                case CellType.Liftable: return "X";
                case CellType.Obstacle: return "O";
                case CellType.Bridge: return "B";
                default: return string.Empty;
            }
        }

        private static bool ShowsDirectionArrow(CellCoord coord, CellData cell, bool tileHasProductionArt) => false;

        private void AddLabel(int layer, int x, int y, Vector3 pos, int depth, string text)
        {
            var labelGo = new GameObject($"Label_{layer}_{x}_{y}");
            labelGo.transform.SetParent(_root, false);
            labelGo.transform.position = pos + new Vector3(0f, 0.12f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.11f;
            tm.fontSize = 48;
            tm.color = Color.white;
            var mr = labelGo.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sortingOrder = depth + 2;
            _labels[layer, x, y] = tm;
        }

        private SpriteRenderer CreateSprite(string name, Vector3 pos, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderArt.IsoDiamond();
            sr.sortingOrder = order;
            return sr;
        }

        private static Color ColorFor(CellData cell, Sprite art)
        {
            // Production art carries its own palette — keep white (with layer alpha applied by caller).
            if (SpriteCatalog.IsProductionArt(art))
            {
                if (cell.Type == CellType.Dock)
                    return Color.white;
                return Color.white;
            }

            switch (cell.Type)
            {
                case CellType.Switch:
                    return new Color(0.45f, 0.38f, 0.15f, 1f);
                case CellType.Rotator:
                    return PlaceholderArt.Rotator;
                case CellType.Bridge:
                    return cell.Device is BridgeDevice b && b.IsOpen
                        ? PlaceholderArt.BridgeOpen
                        : PlaceholderArt.BridgeClosed;
                case CellType.Lift:
                    return PlaceholderArt.LiftPad;
                case CellType.Elevator:
                    return PlaceholderArt.Elevator;
                case CellType.Reflector:
                    return PlaceholderArt.Reflector;
                case CellType.Obstacle:
                    return PlaceholderArt.Obstacle;
                case CellType.Liftable:
                    return cell.Device is LiftableDevice lf && lf.IsRaised
                        ? PlaceholderArt.LiftableUp
                        : PlaceholderArt.Obstacle;
                case CellType.Dock:
                    return PlaceholderArt.DockGreen;
                case CellType.Spawn:
                    return new Color(0.25f, 0.45f, 0.65f, 1f);
                default:
                    return PlaceholderArt.Track;
            }
        }

        private static Color MarkerColor(CellData cell)
        {
            if (cell.IsInteractive)
                return PlaceholderArt.Hazard;
            if (cell.Type == CellType.Rotator)
                return Color.white;
            return new Color(0.85f, 0.85f, 0.2f, 0.85f);
        }
    }
}
