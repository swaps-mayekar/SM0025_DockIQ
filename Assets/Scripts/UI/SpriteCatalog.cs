using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Optional bridge: if you place named sprites under Resources/Sprites/,
    /// BoardView can prefer them over PlaceholderArt (future hook).
    /// </summary>
    public static class SpriteCatalog
    {
        public static Sprite Load(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
                return null;
            return Resources.Load<Sprite>(resourcesPath);
        }

        public static Sprite BeltOrFallback() =>
            Load("Sprites/Belts/belt_straight") ?? PlaceholderArt.WhiteSquare();

        public static Sprite SwitchOrFallback() =>
            Load("Sprites/Devices/switch") ?? PlaceholderArt.WhiteSquare();

        public static Sprite SplitterOrFallback() =>
            Load("Sprites/Devices/splitter") ?? PlaceholderArt.WhiteSquare();

        public static Sprite ParcelOrFallback(bool vip) =>
            Load(vip ? "Sprites/Parcels/parcel_vip" : "Sprites/Parcels/parcel")
            ?? PlaceholderArt.WhiteSquare();

        public static Sprite DockOrFallback() =>
            Load("Sprites/Docks/dock") ?? PlaceholderArt.WhiteSquare();
    }
}
