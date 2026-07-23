using UnityEngine;

namespace DockIQ.Board
{
    /// <summary>
    /// Tap-to-reconfigure track gadgets (Chip-style), while robots keep driving.
    /// </summary>
    public interface IDevice
    {
        bool CanInteract { get; }
        void OnTap();

        /// <summary>Resolve outbound travel given inbound facing. False = blocked.</summary>
        bool TryResolveExit(Dir entryDir, out Dir exitDir);

        /// <summary>Direction used for arrow / turntable visuals.</summary>
        Dir GetDisplayDir();
    }

    /// <summary>Railway switch / turnout — forces robots onto the selected exit.</summary>
    public sealed class SwitchDevice : IDevice
    {
        public Dir Facing { get; private set; }

        public SwitchDevice(Dir facing) => Facing = facing;

        public bool CanInteract => true;

        public void OnTap() => Facing = DirUtil.RotateCw(Facing);

        public bool TryResolveExit(Dir entryDir, out Dir exitDir)
        {
            exitDir = Facing;
            return true;
        }

        public Dir GetDisplayDir() => Facing;
    }

    /// <summary>
    /// Rotating intersection: tap cycles Straight / Left / Right relative to entry.
    /// </summary>
    public sealed class RotatorDevice : IDevice
    {
        // 0 = straight through, 1 = turn left (CCW), 2 = turn right (CW)
        private int _mode;

        public RotatorDevice(int mode = 0) => _mode = mode % 3;

        public bool CanInteract => true;

        public void OnTap() => _mode = (_mode + 1) % 3;

        public bool TryResolveExit(Dir entryDir, out Dir exitDir)
        {
            // Straight keeps driving forward; left/right turn relative to travel.
            exitDir = _mode switch
            {
                1 => DirUtil.RotateCcw(entryDir),
                2 => DirUtil.RotateCw(entryDir),
                _ => entryDir
            };
            return true;
        }

        public Dir GetDisplayDir() => _mode switch
        {
            1 => Dir.West,
            2 => Dir.East,
            _ => Dir.North
        };

        public string ModeLabel => _mode switch
        {
            1 => "L",
            2 => "R",
            _ => "|"
        };
    }

    /// <summary>Drawbridge — tap to open/close. Closed blocks robots.</summary>
    public sealed class BridgeDevice : IDevice
    {
        public bool IsOpen { get; private set; }

        public BridgeDevice(bool startOpen = false) => IsOpen = startOpen;

        public bool CanInteract => true;

        public void OnTap() => IsOpen = !IsOpen;

        public bool TryResolveExit(Dir entryDir, out Dir exitDir)
        {
            exitDir = entryDir;
            return IsOpen;
        }

        public Dir GetDisplayDir() => Dir.East;
    }

    /// <summary>Freight lift — paired pads teleport robots between floors.</summary>
    public sealed class LiftDevice : IDevice
    {
        public Vector2Int LinkedCell { get; set; }

        public bool CanInteract => false;

        public void OnTap() { }

        public bool TryResolveExit(Dir entryDir, out Dir exitDir)
        {
            // Teleport handled by GridBoard; exit facing unchanged.
            exitDir = entryDir;
            return true;
        }

        public Dir GetDisplayDir() => Dir.North;
    }
}
