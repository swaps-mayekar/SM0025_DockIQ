using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Optional art override via Resources/Sprites/. Falls back to placeholders.
    /// </summary>
    public static class SpriteCatalog
    {
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

        public static Sprite DockOrFallback() =>
            Load("Sprites/Docks/dock") ?? PlaceholderArt.IsoDiamond();
    }
}
