namespace DockIQ.Board
{
    public enum CellType : byte
    {
        Empty = 0,
        Belt = 1,
        Switch = 2,
        Splitter = 3,
        Dock = 4,
        Spawn = 5
    }

    [System.Flags]
    public enum MechanicsMask
    {
        None = 0,
        Switches = 1 << 0,
        Splitters = 1 << 1,
        Scanners = 1 << 2,
        Elevators = 1 << 3,
        RobotArms = 1 << 4,
        JamClear = 1 << 5,
        SpeedControl = 1 << 6,
        Barriers = 1 << 7
    }
}
