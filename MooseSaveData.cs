using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace MooseSounds
{
    public struct Moose
    {
        public Vector3 position;
        public Vector3 eulerAngles;
        public int meatGiven;
    }

    public class MooseSaveData
    {
        public List<Moose> deadMoose = new List<Moose>();
    }
}
