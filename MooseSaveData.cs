using System.Collections.Generic;
using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public struct DeadMooseSaveData
    {
        public Vector3Info position;
        public Vector3Info eulerAngles;
        public int meatGiven;
    }

    public class MooseSoundsModSaveData
    {
        public List<DeadMooseSaveData> deadMoose = new List<DeadMooseSaveData>();
    }

    public class MooseRouteSaveData
    {
        public List<MooseRoute> loadedMooseRoutes = new List<MooseRoute>();
    }
}
