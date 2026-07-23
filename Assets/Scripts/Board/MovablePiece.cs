using DockIQ.Levels;

namespace DockIQ.Board
{
    /// <summary>Runtime path-cycled piece. Tap advances along path (rotators also rotate after a full lap).</summary>
    public sealed class MovablePiece
    {
        public int Id { get; }
        public char Kind { get; }
        public CellCoord[] Path { get; }
        public IDevice Device { get; }
        public int Index { get; private set; }

        private bool _awaitingRotate;
        private bool _justRotated;

        public CellCoord Current => Path[Index];

        public bool IsRotator => Kind == 'R' || Kind == 'r';

        public MovablePiece(int id, MovableDef def, IDevice device)
        {
            Id = id;
            Kind = def.Kind;
            Device = device;
            Path = new CellCoord[def.Path.Length];
            for (int i = 0; i < def.Path.Length; i++)
                Path[i] = CellCoord.From(def.Path[i]);
            Index = def.StartIndex % Path.Length;
            if (Index < 0)
                Index = 0;
        }

        public struct Snapshot
        {
            public int Index;
            public bool AwaitingRotate;
            public bool JustRotated;
            public int RotatorMode;
        }

        public Snapshot Capture()
        {
            int mode = Device is RotatorDevice r ? r.Mode : 0;
            return new Snapshot
            {
                Index = Index,
                AwaitingRotate = _awaitingRotate,
                JustRotated = _justRotated,
                RotatorMode = mode
            };
        }

        public void Restore(Snapshot s)
        {
            Index = s.Index;
            _awaitingRotate = s.AwaitingRotate;
            _justRotated = s.JustRotated;
            if (Device is RotatorDevice rot)
                rot.SetMode(s.RotatorMode);
        }

        /// <summary>
        /// Advance state. Returns true if the piece moved to a new cell.
        /// For moving rotators: after the last path slot, next tap rotates mode, then next tap wraps.
        /// </summary>
        public bool TryAdvance(out CellCoord from, out CellCoord to, out bool rotatedOnly)
        {
            from = Current;
            to = Current;
            rotatedOnly = false;

            if (IsRotator)
            {
                if (Path.Length <= 1)
                {
                    Device.OnTap();
                    rotatedOnly = true;
                    return false;
                }

                if (_awaitingRotate)
                {
                    Device.OnTap();
                    _awaitingRotate = false;
                    _justRotated = true;
                    rotatedOnly = true;
                    return false;
                }

                int next = (Index + 1) % Path.Length;
                if (next == 0 && !_justRotated)
                {
                    _awaitingRotate = true;
                    return false;
                }

                _justRotated = false;
                Index = next;
                to = Current;
                return true;
            }

            Index = (Index + 1) % Path.Length;
            to = Current;
            return from != to;
        }
    }
}
