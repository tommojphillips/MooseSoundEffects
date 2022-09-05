using System.Collections.Generic;
using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public struct MooseSaveData
    {
        public Vector3 position;
        public Vector3 eulerAngles;
        public int meatGiven;
    }

    public class MooseSoundsModSaveData
    {
        public List<MooseSaveData> deadMoose = new List<MooseSaveData>();
    }
}
