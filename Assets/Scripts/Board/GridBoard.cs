using System;
using System.Collections.Generic;
using DockIQ.Levels;
using UnityEngine;

namespace DockIQ.Board
{
    public sealed class GridBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int LayerCount { get; private set; }
        public float CellSize { get; private set; }
        public Vector3 Origin { get; private set; }

        private CellData[,,] _cells; // [layer, x, y]
        private readonly List<MovablePiece> _movables = new List<MovablePiece>();
        private readonly Dictionary<CellCoord, CellUnderlay> _underlays = new Dictionary<CellCoord, CellUnderlay>();

        private struct CellUnderlay
        {
            public CellType Type;
            public IDevice Device;
            public Dir Facing;
            public int DockId;
            public int LiftPairId;
            public int ElevatorPairId;
            public CellCoord LiftTarget;
            public CellCoord ElevatorTarget;
        }

        public IReadOnlyList<MovablePiece> Movables => _movables;

        public CellData Get(int layer, int x, int y) => _cells[layer, x, y];

        public CellData Get(CellCoord p) => _cells[p.Layer, p.X, p.Y];

        public bool InBounds(int layer, int x, int y) =>
            layer >= 0 && layer < LayerCount &&
            x >= 0 && y >= 0 && x < Width && y < Height;

        public bool InBounds(CellCoord p) => InBounds(p.Layer, p.X, p.Y);

        public void Build(string[][] layers, MovableDef[] movables = null, float cellSize = 1f)
        {
            if (layers == null || layers.Length == 0)
                throw new ArgumentException("Level layers required");

            LayerCount = layers.Length;
            var ground = layers[0];
            if (ground == null || ground.Length == 0)
                throw new ArgumentException("Layer 0 rows required");

            Height = ground.Length;
            Width = ground[0].Length;

            for (int L = 0; L < LayerCount; L++)
            {
                var rows = layers[L];
                if (rows == null || rows.Length != Height)
                    throw new ArgumentException($"Layer {L} height mismatch");
                for (int r = 0; r < rows.Length; r++)
                {
                    if (rows[r].Length != Width)
                        throw new ArgumentException($"Layer {L} row {r} width mismatch");
                }
            }

            CellSize = cellSize;

            IsoMath.GetBounds(Width, Height, LayerCount, out Vector2 min, out Vector2 max);
            Vector2 center = (min + max) * 0.5f;
            Origin = new Vector3(-center.x, -center.y, 0f);

            _cells = new CellData[LayerCount, Width, Height];
            var liftsByPair = new Dictionary<int, List<CellCoord>>();
            var elevatorsByPair = new Dictionary<int, List<CellCoord>>();

            for (int L = 0; L < LayerCount; L++)
            {
                var rows = layers[L];
                for (int y = 0; y < Height; y++)
                {
                    int rowIndex = Height - 1 - y;
                    string row = rows[rowIndex];
                    for (int x = 0; x < Width; x++)
                    {
                        var cell = ParseCell(row[x]);
                        _cells[L, x, y] = cell;
                        var coord = new CellCoord(x, y, L);

                        if (cell.IsLift)
                        {
                            if (!liftsByPair.TryGetValue(cell.LiftPairId, out var list))
                            {
                                list = new List<CellCoord>();
                                liftsByPair[cell.LiftPairId] = list;
                            }

                            list.Add(coord);
                        }

                        if (cell.IsElevator)
                        {
                            if (!elevatorsByPair.TryGetValue(cell.ElevatorPairId, out var elist))
                            {
                                elist = new List<CellCoord>();
                                elevatorsByPair[cell.ElevatorPairId] = elist;
                            }

                            elist.Add(coord);
                        }
                    }
                }
            }

            NormalizeRotatorMarkers();

            foreach (var kv in liftsByPair)
            {
                var pads = kv.Value;
                if (pads.Count != 2)
                {
                    Debug.LogWarning($"Lift pair {kv.Key} has {pads.Count} pads (need 2)");
                    continue;
                }

                LinkLift(pads[0], pads[1]);
                LinkLift(pads[1], pads[0]);
            }

            foreach (var kv in elevatorsByPair)
            {
                var pads = kv.Value;
                if (pads.Count != 2)
                {
                    Debug.LogWarning($"Elevator pair {kv.Key} has {pads.Count} pads (need 2)");
                    continue;
                }

                LinkElevator(pads[0], pads[1]);
                LinkElevator(pads[1], pads[0]);
            }

            _movables.Clear();
            _underlays.Clear();
            if (movables != null)
            {
                for (int i = 0; i < movables.Length; i++)
                    PlaceMovable(i, movables[i]);
            }
        }

        /// <summary>
        /// '+' marks a configurable route in level data. It is unnecessary on a straight,
        /// two-sided path, so only corners/forks become interactive rotators.
        /// </summary>
        private void NormalizeRotatorMarkers()
        {
            for (int layer = 0; layer < LayerCount; layer++)
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                CellData cell = Get(layer, x, y);
                if (cell.Type != CellType.Switch)
                    continue;

                var coord = new CellCoord(x, y, layer);
                bool west = IsRouteCell(coord.WithOffset(DirUtil.ToOffset(Dir.West)));
                bool east = IsRouteCell(coord.WithOffset(DirUtil.ToOffset(Dir.East)));
                bool north = IsRouteCell(coord.WithOffset(DirUtil.ToOffset(Dir.North)));
                bool south = IsRouteCell(coord.WithOffset(DirUtil.ToOffset(Dir.South)));
                int count = (west ? 1 : 0) + (east ? 1 : 0) +
                            (north ? 1 : 0) + (south ? 1 : 0);
                bool straight = count == 2 && ((west && east) || (north && south));

                if (straight || count < 2)
                {
                    cell.Type = CellType.Track;
                    cell.Device = null;
                }
                else
                {
                    cell.Type = CellType.Rotator;
                    cell.Device = new RotatorDevice(0);
                }
            }
        }

        private bool IsRouteCell(CellCoord coord)
        {
            if (!InBounds(coord))
                return false;

            CellType type = Get(coord).Type;
            return type == CellType.Track
                || type == CellType.Spawn
                || type == CellType.Switch
                || type == CellType.Rotator
                || type == CellType.Bridge
                || type == CellType.Lift
                || type == CellType.Elevator
                || type == CellType.Dock;
        }

        public void Build(string[] rows, float cellSize = 1f) =>
            Build(new[] { rows }, null, cellSize);

        private void PlaceMovable(int id, MovableDef def)
        {
            if (def?.Path == null || def.Path.Length == 0)
            {
                Debug.LogWarning($"Movable {id} has empty path");
                return;
            }

            IDevice device = def.Kind switch
            {
                'R' or 'r' => new RotatorDevice(def.RotatorMode),
                'm' or 'M' => new ReflectorDevice(),
                'O' or 'o' => new ObstacleDevice(),
                _ => new ObstacleDevice()
            };

            var piece = new MovablePiece(id, def, device);
            foreach (var slot in piece.Path)
            {
                if (!InBounds(slot))
                {
                    Debug.LogWarning($"Movable {id} path slot out of bounds: {slot}");
                    return;
                }

                // Snapshot underlay once so detaching restores switches/bridges/etc.
                if (!_underlays.ContainsKey(slot))
                    _underlays[slot] = CaptureUnderlay(Get(slot));
            }

            _movables.Add(piece);
            AttachMovable(piece);
        }

        private static CellUnderlay CaptureUnderlay(CellData cell) => new CellUnderlay
        {
            Type = cell.Type == CellType.Empty ? CellType.Track : cell.Type,
            Device = cell.Device,
            Facing = cell.Facing,
            DockId = cell.DockId,
            LiftPairId = cell.LiftPairId,
            ElevatorPairId = cell.ElevatorPairId,
            LiftTarget = cell.LiftTarget,
            ElevatorTarget = cell.ElevatorTarget
        };

        private void AttachMovable(MovablePiece piece)
        {
            var cell = Get(piece.Current);
            if (!cell.IsTraversable && cell.Type == CellType.Empty)
                cell.Type = CellType.Track;

            if (piece.Kind == 'R' || piece.Kind == 'r')
                cell.Type = CellType.Rotator;
            else if (piece.Kind == 'm' || piece.Kind == 'M')
                cell.Type = CellType.Reflector;
            else
                cell.Type = CellType.Obstacle;

            cell.Device = piece.Device;
            cell.MovableId = piece.Id;
        }

        private void DetachMovable(MovablePiece piece, CellCoord at)
        {
            if (!InBounds(at))
                return;
            var cell = Get(at);
            if (cell.MovableId != piece.Id)
                return;

            cell.MovableId = -1;
            if (_underlays.TryGetValue(at, out var under))
            {
                cell.Type = under.Type;
                cell.Device = under.Device;
                cell.Facing = under.Facing;
                cell.DockId = under.DockId;
                cell.LiftPairId = under.LiftPairId;
                cell.ElevatorPairId = under.ElevatorPairId;
                cell.LiftTarget = under.LiftTarget;
                cell.ElevatorTarget = under.ElevatorTarget;
            }
            else
            {
                cell.Device = null;
                if (cell.Type == CellType.Rotator || cell.Type == CellType.Reflector ||
                    cell.Type == CellType.Obstacle)
                    cell.Type = CellType.Track;
            }
        }

        /// <summary>Advance movable if next slot is free of robots and other movables.</summary>
        public bool TryAdvanceMovable(int id, Func<CellCoord, bool> isRobotOn)
        {
            if (id < 0 || id >= _movables.Count)
                return false;

            var piece = _movables[id];
            var snap = piece.Capture();

            if (!piece.TryAdvance(out CellCoord from, out CellCoord to, out bool rotatedOnly))
                return rotatedOnly;

            var dest = Get(to);
            if ((dest.MovableId >= 0 && dest.MovableId != piece.Id) ||
                (isRobotOn != null && isRobotOn(to)))
            {
                piece.Restore(snap);
                return false;
            }

            DetachMovable(piece, from);
            AttachMovable(piece);
            return true;
        }

        private void LinkLift(CellCoord from, CellCoord to)
        {
            var cell = Get(from);
            cell.LiftTarget = to;
            if (cell.Device is LiftDevice lift)
                lift.LinkedCell = to;
        }

        private void LinkElevator(CellCoord from, CellCoord to)
        {
            var cell = Get(from);
            cell.ElevatorTarget = to;
            if (cell.Device is ElevatorDevice elev)
                elev.LinkedCell = to;

            if (from.X != to.X || from.Y != to.Y)
            {
                Debug.LogWarning(
                    $"Elevator pair pads should share X/Y for a vertical shaft; got {from} ↔ {to}");
            }
        }

        public Vector3 CellToWorld(CellCoord cell, float z = 0f)
        {
            Vector3 local = IsoMath.CellToWorld(cell, z);
            return Origin + local;
        }

        public Vector3 CellToWorld(Vector2Int cell, float z = 0f) =>
            CellToWorld(CellCoord.From(cell), z);

        /// <summary>
        /// Map world tap to a cell, preferring upper floors and interactive tiles.
        /// </summary>
        public bool TryWorldToCell(Vector3 world, out CellCoord cell)
        {
            cell = default;
            Vector3 local = world - Origin;
            CellCoord best = default;
            bool found = false;
            float bestDist = float.MaxValue;

            for (int L = LayerCount - 1; L >= 0; L--)
            {
                Vector2Int xy = IsoMath.WorldToCell(local, L);
                if (!InBounds(L, xy.x, xy.y))
                    continue;

                var cand = new CellCoord(xy.x, xy.y, L);
                var data = Get(cand);
                if (!data.IsTraversable)
                    continue;

                Vector3 center = IsoMath.CellToWorld(cand);
                float dist = Vector2.Distance(new Vector2(local.x, local.y), new Vector2(center.x, center.y));
                bool interactive = data.IsInteractive;
                bool bestInteractive = found && Get(best).IsInteractive;

                if (!found ||
                    (interactive && !bestInteractive) ||
                    (interactive == bestInteractive && dist < bestDist) ||
                    (interactive == bestInteractive && Mathf.Approximately(dist, bestDist) && L > best.Layer))
                {
                    found = true;
                    best = cand;
                    bestDist = dist;
                }
            }

            if (!found || bestDist > 0.55f)
                return false;

            cell = best;
            return true;
        }

        /// <summary>
        /// Advance a robot one cell. <paramref name="clash"/> is set when stepping onto a hazard.
        /// </summary>
        public bool TryStep(CellCoord from, Dir facing, out CellCoord next, out Dir newFacing,
            out bool clash, bool suppressTransfer = false)
        {
            next = from;
            newFacing = facing;
            clash = false;

            if (!InBounds(from))
                return false;

            var data = Get(from);
            if (!data.IsTraversable || data.IsDock)
                return false;

            if (data.IsElevator && !suppressTransfer)
                return TryExitTransfer(data.ElevatorTarget, facing, out next, out newFacing, out clash);

            if (data.IsLift && !suppressTransfer)
                return TryExitTransfer(data.LiftTarget, facing, out next, out newFacing, out clash);

            if (!data.TryResolveExit(facing, out Dir exit))
                return false;

            newFacing = exit;
            next = from.WithOffset(DirUtil.ToOffset(exit));
            return TryEnter(next, out clash);
        }

        private bool TryExitTransfer(CellCoord pad, Dir facing, out CellCoord next, out Dir newFacing,
            out bool clash)
        {
            // Always land on the far pad first so the parcel is visible there for a beat.
            // Step-off happens on a later tick once SuppressLift is set by the controller.
            next = pad;
            newFacing = facing;
            clash = false;

            if (!InBounds(pad) || !Get(pad).IsTraversable)
                return false;

            return TryEnter(next, out clash);
        }

        private bool TryEnter(CellCoord cell, out bool clash)
        {
            clash = false;
            if (!InBounds(cell))
                return false;

            var dest = Get(cell);
            if (!dest.IsTraversable)
                return false;

            if (dest.Type == CellType.Bridge &&
                dest.Device is BridgeDevice bridge &&
                !bridge.IsOpen)
                return false;

            if (dest.IsClashHazard)
            {
                clash = true;
                return true;
            }

            // Liftable/obstacle that blocks via TryResolveExit but not hazard — shouldn't happen
            if (dest.Device is LiftableDevice lf && lf.Blocks)
            {
                clash = true;
                return true;
            }

            if (dest.Device is ObstacleDevice)
            {
                clash = true;
                return true;
            }

            return true;
        }

        private bool CanEnterIgnoringHazard(CellCoord cell)
        {
            if (!InBounds(cell))
                return false;
            var dest = Get(cell);
            if (!dest.IsTraversable)
                return false;
            if (dest.Type == CellType.Bridge &&
                dest.Device is BridgeDevice bridge &&
                !bridge.IsOpen)
                return false;
            return true;
        }

        private static CellData ParseCell(char c)
        {
            var cell = new CellData { Type = CellType.Empty, Facing = Dir.East };

            switch (c)
            {
                case '.':
                case ' ':
                    break;

                case '^':
                case '>':
                case 'v':
                case '<':
                    cell.Type = CellType.Track;
                    cell.Facing = DirUtil.FromChar(c);
                    break;

                case '+':
                    // Temporary marker; normalized after the whole layer is parsed.
                    cell.Type = CellType.Switch;
                    cell.Device = new RotatorDevice(0);
                    break;

                case 'R':
                    cell.Type = CellType.Rotator;
                    cell.Device = new RotatorDevice(0);
                    break;

                case 'B':
                    cell.Type = CellType.Bridge;
                    cell.Device = new BridgeDevice(startOpen: false);
                    break;

                case 'A':
                case 'a':
                    cell.Type = CellType.Lift;
                    cell.LiftPairId = 0;
                    cell.Device = new LiftDevice();
                    break;

                case 'C':
                case 'c':
                    cell.Type = CellType.Lift;
                    cell.LiftPairId = 1;
                    cell.Device = new LiftDevice();
                    break;

                case 'E':
                    cell.Type = CellType.Elevator;
                    cell.ElevatorPairId = 0;
                    cell.Device = new ElevatorDevice();
                    break;

                case 'e':
                    cell.Type = CellType.Elevator;
                    cell.ElevatorPairId = 1;
                    cell.Device = new ElevatorDevice();
                    break;

                case 'M':
                    cell.Type = CellType.Reflector;
                    cell.Device = new ReflectorDevice();
                    break;

                case 'X':
                    cell.Type = CellType.Liftable;
                    cell.Device = new LiftableDevice(startRaised: false);
                    break;

                case 'O':
                    cell.Type = CellType.Obstacle;
                    cell.Device = new ObstacleDevice();
                    break;

                case 'S':
                case 's':
                    cell.Type = CellType.Spawn;
                    cell.Facing = Dir.East;
                    break;

                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    cell.Type = CellType.Dock;
                    cell.DockId = c - '0';
                    break;

                default:
                    Debug.LogWarning($"Unknown cell char '{c}', treating as empty");
                    break;
            }

            return cell;
        }
    }
}
