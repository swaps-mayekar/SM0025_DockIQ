using UnityEngine;

namespace DockIQ.Board
{
    public sealed class CellData
    {
        public CellType Type;
        public Dir Facing;
        public int DockId;
        public int LiftPairId = -1;
        public Vector2Int LiftTarget;
        public IDevice Device;

        public bool IsTraversable => Type != CellType.Empty;

        public bool IsInteractive => Device != null && Device.CanInteract;

        public bool IsDock => Type == CellType.Dock;

        public bool IsLift => Type == CellType.Lift;

        public Dir GetDisplayDir()
        {
            if (Device != null)
                return Device.GetDisplayDir();
            return Facing;
        }

        public bool TryResolveExit(Dir entryDir, out Dir exitDir)
        {
            if (Device != null)
                return Device.TryResolveExit(entryDir, out exitDir);

            // Straight track / spawn: keep driving in current facing.
            exitDir = entryDir;
            return true;
        }

        public void OnTap()
        {
            Device?.OnTap();
            if (Device is SwitchDevice sw)
                Facing = sw.Facing;
        }
    }
}
