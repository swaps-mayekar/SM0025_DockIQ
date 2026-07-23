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
                tile.sprite = PlaceholderArt.IsoDiamond();
                var col = ColorFor(cell);
                col.a *= alpha;
                tile.color = col;
                tile.transform.localScale = Vector3.one * board.CellSize;
                _tiles[L, x, y] = tile;

                if (cell.Type == CellType.Dock)
                {
                    AddLabel(L, x, y, pos, depth, cell.DockId.ToString());
                }
                else if (cell.IsLift)
                {
                    AddLabel(L, x, y, pos, depth, "▲");
                }
                else if (cell.IsElevator)
                {
                    AddLabel(L, x, y, pos, depth, "E");
                }
                else if (cell.Type == CellType.Reflector)
                {
                    AddLabel(L, x, y, pos, depth, "M");
                }
                else if (cell.Type == CellType.Liftable)
                {
                    AddLabel(L, x, y, pos, depth, "X");
                }
                else if (cell.Type == CellType.Obstacle)
                {
                    AddLabel(L, x, y, pos, depth, "O");
                }
                else if (cell.Type == CellType.Bridge)
                {
                    AddLabel(L, x, y, pos, depth, "B");
                }
                else
                {
                    var arrow = CreateSprite($"Arrow_{L}_{x}_{y}", pos + new Vector3(0f, 0.08f, 0f), depth + 1);
                    arrow.sprite = PlaceholderArt.WhiteSquare();
                    arrow.color = MarkerColor(cell);
                    arrow.transform.localScale = new Vector3(0.12f, 0.28f, 1f);
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetDisplayDir()));
                    _arrows[L, x, y] = arrow;
                }
            }

            DrawMovablePaths();
            RefreshDevices();
        }

        private void DrawMovablePaths()
        {
            if (_pathRoot != null)
                Destroy(_pathRoot.gameObject);
            _pathRoot = new GameObject("Paths").transform;
            _pathRoot.SetParent(_root, false);

            foreach (var piece in _board.Movables)
            {
                for (int i = 0; i < piece.Path.Length; i++)
                {
                    var slot = piece.Path[i];
                    Vector3 pos = _board.CellToWorld(slot, 0.05f);
                    var sr = CreateSprite($"Path_{piece.Id}_{i}", pos, IsoMath.DepthOrder(slot, 1));
                    sr.sprite = PlaceholderArt.Circle();
                    sr.color = new Color(1f, 1f, 1f, 0.22f);
                    sr.transform.localScale = Vector3.one * 0.22f;
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
                if (_tiles[L, x, y] != null)
                {
                    var col = ColorFor(cell);
                    if (L > 0)
                        col.a *= 0.88f;
                    _tiles[L, x, y].color = col;
                    _tiles[L, x, y].enabled = cell.IsTraversable;
                }

                var arrow = _arrows[L, x, y];
                if (arrow != null)
                {
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetDisplayDir()));
                    arrow.color = MarkerColor(cell);
                    arrow.enabled = cell.IsTraversable &&
                                    (cell.Type == CellType.Track || cell.Type == CellType.Switch ||
                                     cell.Type == CellType.Spawn || cell.Type == CellType.Rotator ||
                                     cell.Device != null);
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
                    label.text = cell.MovableId >= 0 ? "O" : "O";
                else if (cell.IsElevator)
                    label.text = "E";
                else if (cell.Type == CellType.Reflector)
                    label.text = "M";
            }

            // Re-attach labels/arrows for cells that gained movables
            foreach (var piece in _board.Movables)
            {
                var c = piece.Current;
                if (_tiles[c.Layer, c.X, c.Y] != null)
                    _tiles[c.Layer, c.X, c.Y].color = ColorFor(_board.Get(c));
            }
        }

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

        private static Color ColorFor(CellData cell)
        {
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
