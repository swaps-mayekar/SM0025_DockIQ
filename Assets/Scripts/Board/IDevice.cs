namespace DockIQ.Board
{
    /// <summary>
    /// Extensible interactable cell. MVP: Switch / Splitter.
    /// Future: scanners, elevators, robot arms, jam-clear, speed controls.
    /// </summary>
    public interface IDevice
    {
        bool CanInteract { get; }
        void OnTap();
        Dir GetExitDir();
    }

    public sealed class SwitchDevice : IDevice
    {
        public Dir Facing { get; private set; }

        public SwitchDevice(Dir facing) => Facing = facing;

        public bool CanInteract => true;

        public void OnTap() => Facing = DirUtil.RotateCw(Facing);

        public Dir GetExitDir() => Facing;
    }

    /// <summary>
    /// Splitter cycles between two exit directions (primary + rotate CW).
    /// </summary>
    public sealed class SplitterDevice : IDevice
    {
        private readonly Dir _primary;
        private int _lane;

        public SplitterDevice(Dir primary, int lane = 0)
        {
            _primary = primary;
            _lane = lane & 1;
        }

        public bool CanInteract => true;

        public void OnTap() => _lane ^= 1;

        public Dir GetExitDir() =>
            _lane == 0 ? _primary : DirUtil.RotateCw(_primary);
    }
}
