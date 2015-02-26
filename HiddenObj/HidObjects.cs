using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HiddenObj
{
    internal class HidObjects
    {
        public static List<HidObject> HObjects = new List<HidObject>();

        static HidObjects()
        {
            HObjects.Add(new HidObject("VisionWard", "VisionWard", -1, Color.Purple, 1450));
            HObjects.Add(new HidObject("SightWard", "SightWard", 180, Color.Green, 1450));
            HObjects.Add(new HidObject("SightWard", "YellowTrinket", 60, Color.Green, 1450));
            HObjects.Add(new HidObject("SightWard", "TeemoMushroom", 600, Color.Red, 1450));
            HObjects.Add(new HidObject("Cupcake Trap", "CaitlynTrap", 240, Color.Red, 1450));
        }

        public static HidObject IsHidObj(string hidName)
        {
            return
                HObjects.FirstOrDefault(
                    hidObj => String.Equals(hidObj.SkinName, hidName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}