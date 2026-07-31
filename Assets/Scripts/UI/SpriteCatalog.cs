using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Optional art overrides. Parcels/gates come from a scene-assigned <see cref="BoardArtCatalog"/>.
    /// Device/robot fallbacks may still load from Resources/Sprites/ when present.
    /// </summary>
    public static class SpriteCatalog
    {
        private static BoardArtCatalog _boardArt;

        public static void Bind(BoardArtCatalog boardArt) => _boardArt = boardArt;

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

        /// <summary>Level cargo sprite (Parcels_0 = level 1) from the bound board art catalog.</summary>
        public static Sprite ParcelForLevel(int levelId) =>
            _boardArt != null ? _boardArt.ParcelForLevel(levelId) : null;

        /// <summary>Dock gate sprite (Gates_0 = dock 1) from the bound board art catalog.</summary>
        public static Sprite GateForDockId(int dockId) =>
            _boardArt != null ? _boardArt.GateForDockId(dockId) : null;
    }
}
