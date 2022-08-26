using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace MooseSounds
{
    public class MooseTransformSaveData
    {
        public Vector3 position;
        public Vector3 eulerAngles;
    }

    public class MooseSaveData
    {
        public List<MooseTransformSaveData> deadMoose;
    }
}
