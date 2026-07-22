using UnityEngine;

namespace DockIQ.Board
{
    public enum Dir : byte
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    public static class DirUtil
    {
        public static Vector2Int ToOffset(Dir dir) => dir switch
        {
            Dir.North => new Vector2Int(0, 1),
            Dir.East => new Vector2Int(1, 0),
            Dir.South => new Vector2Int(0, -1),
            Dir.West => new Vector2Int(-1, 0),
            _ => Vector2Int.zero
        };

        public static Dir RotateCw(Dir dir) => (Dir)(((int)dir + 1) % 4);

        public static Dir Opposite(Dir dir) => (Dir)(((int)dir + 2) % 4);

        /// <summary>Z rotation for a sprite that points North by default.</summary>
        public static float ToZDegrees(Dir dir) => -(int)dir * 90f;

        public static Dir FromChar(char c) => c switch
        {
            '^' => Dir.North,
            '>' => Dir.East,
            'v' => Dir.South,
            '<' => Dir.West,
            _ => Dir.East
        };
    }
}
