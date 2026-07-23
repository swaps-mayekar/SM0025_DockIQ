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
        Spawn = 7,
        Reflector = 8,
        Obstacle = 9,
        Liftable = 10,
        Elevator = 11
    }

    [System.Flags]
    public enum MechanicsMask
    {
        None = 0,
        Switches = 1 << 0,
        Rotators = 1 << 1,
        Bridges = 1 << 2,
        Lifts = 1 << 3,
        Reflectors = 1 << 4,
        Movables = 1 << 5,
        Obstacles = 1 << 6,
        Liftables = 1 << 7,
        Elevators = 1 << 8
    }
}
