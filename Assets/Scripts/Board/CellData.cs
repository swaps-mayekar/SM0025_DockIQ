using UnityEngine;

namespace DockIQ.Board
{
    public sealed class CellData
    {
        public CellType Type;
        public Dir Facing;
        public int DockId;
        public int LiftPairId = -1;
        public int ElevatorPairId = -1;
        public CellCoord LiftTarget;
        public CellCoord ElevatorTarget;
        public IDevice Device;
        public int MovableId = -1;

        public bool IsTraversable => Type != CellType.Empty;

        public bool IsInteractive =>
            (Device != null && Device.CanInteract) || MovableId >= 0;

        public bool IsDock => Type == CellType.Dock;

        public bool IsLift => Type == CellType.Lift;

        public bool IsElevator => Type == CellType.Elevator;

        /// <summary>Robot entering this cell should fail the level.</summary>
        public bool IsClashHazard
        {
            get
            {
                if (Device is ObstacleDevice obs && obs.Blocks)
                    return true;
                if (Device is LiftableDevice lift && lift.Blocks)
                    return true;
                return false;
            }
        }

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
