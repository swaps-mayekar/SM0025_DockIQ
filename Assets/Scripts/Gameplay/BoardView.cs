using DockIQ.Board;
using DockIQ.UI;
using UnityEngine;

namespace DockIQ.Gameplay
{
    /// <summary>Builds and refreshes isometric 2D cell visuals.</summary>
    public sealed class BoardView : MonoBehaviour
    {
        private GridBoard _board;
        private Transform _root;
        private SpriteRenderer[,] _tiles;
        private SpriteRenderer[,] _arrows;
        private TextMesh[,] _dockLabels;

        public void Build(GridBoard board)
        {
            _board = board;
            if (_root != null)
                Destroy(_root.gameObject);

            _root = new GameObject("BoardRoot").transform;
            _root.SetParent(transform, false);

            _tiles = new SpriteRenderer[board.Width, board.Height];
            _arrows = new SpriteRenderer[board.Width, board.Height];
            _dockLabels = new TextMesh[board.Width, board.Height];

            for (int y = 0; y < board.Height; y++)
            for (int x = 0; x < board.Width; x++)
            {
                var cell = board.Get(x, y);
                if (!cell.IsTraversable)
                    continue;

                Vector3 pos = board.CellToWorld(new Vector2Int(x, y), 0f);
                int depth = IsoMath.DepthOrder(x, y);

                var tile = CreateSprite($"Cell_{x}_{y}", pos, depth);
                tile.sprite = SpriteFor(cell);
                tile.color = ColorFor(cell);
                // Diamond sprite is authored at TileWidth world units
                tile.transform.localScale = Vector3.one * board.CellSize;
                _tiles[x, y] = tile;

                if (cell.Type != CellType.Dock)
                {
                    var arrow = CreateSprite($"Arrow_{x}_{y}", pos + new Vector3(0f, 0.08f, 0f), depth + 1);
                    arrow.sprite = PlaceholderArt.WhiteSquare();
                    arrow.color = PlaceholderArt.Hazard;
                    arrow.transform.localScale = new Vector3(0.12f, 0.28f, 1f);
                    arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetExitDir()));
                    _arrows[x, y] = arrow;
                }
                else
                {
                    var labelGo = new GameObject($"DockLabel_{cell.DockId}");
                    labelGo.transform.SetParent(_root, false);
                    labelGo.transform.position = pos + new Vector3(0f, 0.12f, 0f);
                    var tm = labelGo.AddComponent<TextMesh>();
                    tm.text = cell.DockId.ToString();
                    tm.anchor = TextAnchor.MiddleCenter;
                    tm.alignment = TextAlignment.Center;
                    tm.characterSize = 0.12f;
                    tm.fontSize = 48;
                    tm.color = Color.white;
                    var mr = labelGo.GetComponent<MeshRenderer>();
                    if (mr != null)
                        mr.sortingOrder = depth + 2;
                    _dockLabels[x, y] = tm;
                }
            }
        }

        public void RefreshDevices()
        {
            if (_board == null)
                return;

            for (int y = 0; y < _board.Height; y++)
            for (int x = 0; x < _board.Width; x++)
            {
                var cell = _board.Get(x, y);
                var arrow = _arrows[x, y];
                if (arrow == null)
                    continue;

                arrow.transform.rotation = Quaternion.Euler(0f, 0f, IsoMath.DirToZDegrees(cell.GetExitDir()));
                if (cell.IsInteractive)
                    arrow.color = PlaceholderArt.Hazard;
            }
        }

        public void FlashCell(Vector2Int cell, Color color)
        {
            if (_tiles == null || !_board.InBounds(cell))
                return;
            var tile = _tiles[cell.x, cell.y];
            if (tile != null)
                tile.color = color;
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

        private static Sprite SpriteFor(CellData cell)
        {
            switch (cell.Type)
            {
                case CellType.Switch:
                    return SpriteCatalog.SwitchOrFallback();
                case CellType.Splitter:
                    return SpriteCatalog.SplitterOrFallback();
                case CellType.Dock:
                    return SpriteCatalog.DockOrFallback();
                default:
                    return SpriteCatalog.BeltOrFallback();
            }
        }

        private static Color ColorFor(CellData cell)
        {
            switch (cell.Type)
            {
                case CellType.Switch:
                case CellType.Splitter:
                    return new Color(0.45f, 0.38f, 0.15f, 1f);
                case CellType.Dock:
                    return PlaceholderArt.DockGreen;
                case CellType.Spawn:
                    return new Color(0.25f, 0.45f, 0.65f, 1f);
                default:
                    return PlaceholderArt.Belt;
            }
        }
    }
}
