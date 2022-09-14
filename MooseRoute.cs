using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MooseRoute
    {
        // Written, 29.08.2022

        [JsonProperty]
        public List<Vector3Info> points { get; internal set; } = new List<Vector3Info>();

        public Vector3 varyPoint(Vector3 point)
        {
            // Written, 10.09.2022
            
            Vector3 p = new Vector3();
            p.x = UnityEngine.Random.Range(point.x - 3, point.x + 3);
            p.y = point.y;
            p.z = UnityEngine.Random.Range(point.z - 3, point.z + 3);
            return p;
        }
    }
}
