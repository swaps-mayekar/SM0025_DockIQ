using UnityEngine;

namespace DockIQ.UI
{
        /// <summary>
        /// Runtime placeholder sprites until generated 2D art is dropped into Assets/Sprites/.
        /// Board view is isometric 2D — robots drive on track diamonds.
        /// See Sprites/README for naming (tracks, switches, robots, docks).
        /// </summary>
    public static class PlaceholderArt
    {
        private static Sprite _white;
        private static Sprite _circle;
        private static Sprite _diamond;
        private static Sprite _robot;

        public static readonly Color Navy = new Color(0.04f, 0.10f, 0.17f, 1f);
        public static readonly Color Slate = new Color(0.29f, 0.33f, 0.41f, 1f);
        public static readonly Color Track = new Color(0.42f, 0.46f, 0.52f, 1f);
        public static readonly Color Belt = Track; // alias
        public static readonly Color Hazard = new Color(0.96f, 0.77f, 0.09f, 1f);
        public static readonly Color ParcelBrown = new Color(0.65f, 0.49f, 0.32f, 1f);
        public static readonly Color RobotGrey = new Color(0.55f, 0.60f, 0.68f, 1f);
        public static readonly Color VipGold = new Color(1f, 0.84f, 0.0f, 1f);
        public static readonly Color DockGreen = new Color(0.18f, 0.80f, 0.44f, 1f);
        public static readonly Color DockWrong = new Color(0.90f, 0.30f, 0.28f, 1f);
        public static readonly Color BridgeClosed = new Color(0.55f, 0.22f, 0.20f, 1f);
        public static readonly Color BridgeOpen = new Color(0.30f, 0.55f, 0.75f, 1f);
        public static readonly Color LiftPad = new Color(0.55f, 0.35f, 0.75f, 1f);
        public static readonly Color Elevator = new Color(0.35f, 0.70f, 0.85f, 1f);
        public static readonly Color Reflector = new Color(0.70f, 0.85f, 0.95f, 1f);
        public static readonly Color Obstacle = new Color(0.75f, 0.25f, 0.22f, 1f);
        public static readonly Color LiftableUp = new Color(0.45f, 0.55f, 0.35f, 1f);
        public static readonly Color Rotator = new Color(0.75f, 0.55f, 0.20f, 1f);
        public static readonly Color Panel = new Color(0.08f, 0.14f, 0.22f, 0.92f);
        public static readonly Color Text = Color.white;

        public static Sprite WhiteSquare()
        {
            if (_white != null)
                return _white;

            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _white = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            _white.name = "PlaceholderWhite";
            return _white;
        }

        /// <summary>Isometric floor diamond (2:1) for placeholder belts/devices.</summary>
        public static Sprite IsoDiamond()
        {
            if (_diamond != null)
                return _diamond;

            const int w = 64;
            const int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 c = new Vector2(w * 0.5f, h * 0.5f);
            float hw = w * 0.5f - 1f;
            float hh = h * 0.5f - 1f;

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float nx = Mathf.Abs(x + 0.5f - c.x) / hw;
                float ny = Mathf.Abs(y + 0.5f - c.y) / hh;
                // Diamond: |nx| + |ny| <= 1
                tex.SetPixel(x, y, nx + ny <= 1f ? Color.white : Color.clear);
            }

            tex.Apply();
            _diamond = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 64f);
            _diamond.name = "PlaceholderIsoDiamond";
            return _diamond;
        }

        public static Sprite Circle()
        {
            if (_circle != null)
                return _circle;

            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = size * 0.5f - 1f;
            Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }

            tex.Apply();
            _circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            _circle.name = "PlaceholderCircle";
            return _circle;
        }

        /// <summary>Simple robot chassis (rounded rectangle).</summary>
        public static Sprite RobotBody()
        {
            if (_robot != null)
                return _robot;

            const int w = 24;
            const int h = 18;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool edge = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                bool cabin = x >= 14 && x <= 20 && y >= 5 && y <= 12;
                tex.SetPixel(x, y, edge ? new Color(0.15f, 0.15f, 0.18f, 1f)
                    : cabin ? new Color(0.4f, 0.85f, 1f, 1f)
                    : Color.white);
            }

            tex.Apply();
            _robot = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 24f);
            _robot.name = "PlaceholderRobot";
            return _robot;
        }
    }
}
