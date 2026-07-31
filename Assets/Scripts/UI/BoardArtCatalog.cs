using UnityEngine;

namespace DockIQ.UI
{
    /// <summary>
    /// Direct references to board parcel and dock-gate spritesheets (no Resources load).
    /// </summary>
    [CreateAssetMenu(fileName = "BoardArtCatalog", menuName = "DockIQ/Board Art Catalog")]
    public sealed class BoardArtCatalog : ScriptableObject
    {
        [SerializeField] private Sprite[] _parcels;
        [SerializeField] private Sprite[] _gates;

        public Sprite ParcelForLevel(int levelId) => Slice(_parcels, levelId - 1);

        public Sprite GateForDockId(int dockId) => Slice(_gates, dockId - 1);

        private static Sprite Slice(Sprite[] sprites, int index)
        {
            if (sprites == null || index < 0 || index >= sprites.Length)
                return null;
            return sprites[index];
        }
    }
}
