using SharpDX;
using Color = System.Drawing.Color;

namespace HiddenObj
{
    internal class ListedHO
    {
        public float CreatedAt;
        public int Duration;
        public string Name;
        public int NetworkId;
        public Color ObjColor;
        public Vector3 Position;
        public int Range;

        public ListedHO(int netId, string name, int duration, Color objColor, int range, Vector3 position,
            float createdAt)
        {
            NetworkId = netId;
            Name = name;
            Duration = duration;
            ObjColor = objColor;
            Range = range;
            Position = position;
            CreatedAt = createdAt;
        }
    }
}