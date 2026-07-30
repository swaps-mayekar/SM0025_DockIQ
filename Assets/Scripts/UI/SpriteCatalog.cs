using System;
using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Optional art override via Resources/Sprites/. Falls back to placeholders.
    /// </summary>
    public static class SpriteCatalog
    {
        private static Sprite[] _levelParcels;
        private static Sprite[] _dockGates;

        public static Sprite Load(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
                return null;
            return Resources.Load<Sprite>(resourcesPath);
        }

        public static Sprite TrackOrFallback() =>
            Load("Sprites/Belts/belt_straight") ?? Load("Sprites/Tracks/track") ?? PlaceholderArt.IsoDiamond();

        public static Sprite SwitchOrFallback() =>
            Load("Sprites/Devices/switch") ?? PlaceholderArt.IsoDiamond();

        public static Sprite RotatorOrFallback() =>
            Load("Sprites/Devices/rotator") ?? PlaceholderArt.IsoDiamond();

        public static Sprite BridgeOrFallback() =>
            Load("Sprites/Devices/bridge") ?? PlaceholderArt.IsoDiamond();

        public static Sprite LiftOrFallback() =>
            Load("Sprites/Devices/lift") ?? PlaceholderArt.IsoDiamond();

        public static Sprite RobotOrFallback(bool rescue) =>
            Load(rescue ? "Sprites/Parcels/parcel_vip" : "Sprites/Parcels/parcel")
            ?? Load(rescue ? "Sprites/Robots/robot_rescue" : "Sprites/Robots/robot")
            ?? PlaceholderArt.RobotBody();

        public static Sprite DockOrFallback(int dockId = 1) =>
            GateForDockId(dockId)
            ?? Load("Sprites/Docks/dock")
            ?? PlaceholderArt.IsoDiamond();

        /// <summary>
        /// Level-specific cargo sprite from Resources/UI/Parcels (Parcels_0 = level 1).
        /// </summary>
        public static Sprite ParcelForLevel(int levelId)
        {
            EnsureLevelParcels();
            if (_levelParcels == null || _levelParcels.Length == 0)
                return null;

            int index = levelId - 1;
            if (index < 0 || index >= _levelParcels.Length)
                return null;

            return _levelParcels[index];
        }

        /// <summary>
        /// Dock gate sprite from Resources/UI/Gates (Gates_0 = dock 1 blue … Gates_3 = dock 4 yellow).
        /// </summary>
        public static Sprite GateForDockId(int dockId)
        {
            EnsureDockGates();
            if (_dockGates == null || _dockGates.Length == 0)
                return null;

            int index = dockId - 1;
            if (index < 0 || index >= _dockGates.Length)
                return null;

            return _dockGates[index];
        }

        private static void EnsureLevelParcels()
        {
            if (_levelParcels != null)
                return;

            var loaded = Resources.LoadAll<Sprite>("UI/Parcels");
            if (loaded == null || loaded.Length == 0)
            {
                _levelParcels = Array.Empty<Sprite>();
                return;
            }

            Array.Sort(loaded, (a, b) => SliceIndex(a).CompareTo(SliceIndex(b)));
            _levelParcels = loaded;
        }

        private static void EnsureDockGates()
        {
            if (_dockGates != null)
                return;

            var loaded = Resources.LoadAll<Sprite>("UI/Gates");
            if (loaded == null || loaded.Length == 0)
            {
                _dockGates = Array.Empty<Sprite>();
                return;
            }

            Array.Sort(loaded, (a, b) => SliceIndex(a).CompareTo(SliceIndex(b)));
            _dockGates = loaded;
        }

        private static int SliceIndex(Sprite sprite)
        {
            if (sprite == null || string.IsNullOrEmpty(sprite.name))
                return int.MaxValue;

            string name = sprite.name;
            int underscore = name.LastIndexOf('_');
            if (underscore >= 0 && underscore + 1 < name.Length &&
                int.TryParse(name.Substring(underscore + 1), out int index))
                return index;

            return int.MaxValue;
        }
    }
}
