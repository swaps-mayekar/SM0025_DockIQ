namespace DockIQ.Board
{
    public sealed class CellData
    {
        public CellType Type;
        public Dir Facing;
        public int DockId;
        public IDevice Device;

        public bool IsTraversable => Type != CellType.Empty;

        public bool IsInteractive => Device != null && Device.CanInteract;

        public bool IsDock => Type == CellType.Dock;

        public Dir GetExitDir()
        {
            if (Device != null)
                return Device.GetExitDir();
            return Facing;
        }

        public void OnTap()
        {
            Device?.OnTap();
            if (Device != null)
                Facing = Device.GetExitDir();
        }
    }
}
