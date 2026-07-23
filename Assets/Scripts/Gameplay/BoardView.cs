using DockIQ.Board;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>Isometric track network visuals (switches, rotators, bridges, lifts).</summary>
    public sealed class BoardView : MonoBehaviour
    {
        private GridBoard _board;
        private Transform _root;
        private SpriteRenderer[,] _tiles;
        private SpriteRenderer[,] _arrows;
        private TextMesh[,] _labels;

        public void Build(GridBoard board)
        {
            _board = board;
            if (_root != null)
                Destroy(_root.gameObject);

            _root = new GameObject("BoardRoot").transform;
            _root.SetParent(transform, false);

            _tiles = new SpriteRenderer[board.Width, board.Height];
            _arrows = new SpriteRenderer[board.Width, board.Height];
            _labels = new TextMesh[board.Width, board.Height];

            for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
            {
                var cell = board.Get(x, y);
                if (!cell.IsTraversable)
                    continue;

                Vector3 pos = board.CellToWorld(new Vector2Int(x, y), 0f);
                int depth = IsoMath.DepthOrder(x, y);

                var tile = CreateSprite($"Cell_{x}_{y}", pos, depth);
                tile.sprite = PlaceholderArt.IsoDiamond();
                tile.color = ColorFor(cell);
                tile.transform.localScale = Vector3.one * board.CellSize;
                _tiles[x, y] = tile;

                if (cell.Type == CellType.Dock)
                {
                    AddLabel(x, y, pos, depth, cell.DockId.ToString());
                }
                else if (cell.IsLift)
                {
                    AddLabel(x, y, pos, depth, "▲");
                }
                else if (cell.Type != CellType.Bridge)
                {
                    var arrow = CreateSprite($"Arrow_{x}_{y}", pos + new Vector3(0f, 0.08f, 0f), depth + 1);
                    arrow.sprite = PlaceholderArt.WhiteSquare();
                    arrow.color = MarkerColor(cell);
                    arrow.transform.localScale = new Vector3(0.12f, 0.28f, 1f);
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetDisplayDir()));
                    _arrows[x, y] = arrow;
                }
                else
                {
                    AddLabel(x, y, pos, depth, "B");
                }
            }

            RefreshDevices();
        }

        public void RefreshDevices()
        {
            if (_board == null)
                return;

            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var cell = _board.Get(x, y);
                if (_tiles[x, y] != null)
                    _tiles[x, y].color = ColorFor(cell);

                var arrow = _arrows[x, y];
                if (arrow != null)
                {
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetDisplayDir()));
                    arrow.color = MarkerColor(cell);
                    arrow.enabled = cell.Type != CellType.Bridge ||
                                    (cell.Device is BridgeDevice b && b.IsOpen);
                }

                if (cell.Type == CellType.Bridge && _labels[x, y] != null)
                    _labels[x, y].text = cell.Device is BridgeDevice br && br.IsOpen ? "B↑" : "B↓";

                if (cell.Type == CellType.Rotator && _labels[x, y] == null && cell.Device is RotatorDevice)
                {
                    // mode shown via arrow color
                }
            }
        }

        private void AddLabel(int x, int y, Vector3 pos, int depth, string text)
        {
            var labelGo = new GameObject($"Label_{x}_{y}");
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
            _labels[x, y] = tm;
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
