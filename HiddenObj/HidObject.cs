using System.Drawing;

namespace HiddenObj
{
    internal class HidObject
    {
        public int Duration;
        public string Name;
        public Color ObjColor;
        public int Range;
        public string SkinName;

        public HidObject(string name, string skinName, int duration, Color objColor, int range)
        {
            Name = name;
            SkinName = skinName;

            Duration = duration;
            ObjColor = objColor;
            Range = range;
        }
    }
}