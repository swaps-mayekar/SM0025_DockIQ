namespace DockIQ.Board
{
    public enum CellType : byte
    {
        Empty = 0,
        Track = 1,
        Switch = 2,
        Rotator = 3,
        Bridge = 4,
        Lift = 5,
        Dock = 6,
        Spawn = 7
    }

    [System.Flags]
    public enum MechanicsMask
    {
        None = 0,
        Switches = 1 << 0,
        Rotators = 1 << 1,
        Bridges = 1 << 2,
        Lifts = 1 << 3,
        Scanners = 1 << 4,
        Barriers = 1 << 5
    }
}
