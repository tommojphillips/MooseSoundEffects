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

        internal bool routeInUse { get; set; }
        internal GameObject routeStart { get; private set; }
        internal GameObject routeEnd { get; private set; }
        internal GameObject mooseOnRoute { get; set; }

        [JsonProperty]
        public List<Vector3Info> points { get; internal set; } = new List<Vector3Info>();

        public int currentPoint { get; private set; }

        public MooseRoute()
        {
            initMooseRoute();
        }

        public void initMooseRoute()
        {
            routeStart = new GameObject("MooseRouteStart");
            routeEnd = new GameObject("MooseRouteEnd");
        }

        public bool setNextPoint() 
        {
            if (currentPoint < points.Count - 1)
            {
                currentPoint++;

                routeStart.transform.position = points[currentPoint - 1];//currentPoint == 1 ? varyPoint(points[0]) : routeEnd.transform.position;
                routeEnd.transform.position = points[currentPoint];//varyPoint(points[currentPoint]);        


                return true;
            }
            return false;
        }
        public Vector3 varyPoint(Vector3 point)
        {
            // Written, 10.09.2022
            
            Vector3 p = new Vector3();
            p.x = UnityEngine.Random.Range(point.x - 3, point.x + 3);
            p.y = point.y;
            p.z = UnityEngine.Random.Range(point.z - 3, point.z + 3);
            return p;
        }
        public void reset() 
        {
            currentPoint = 0;
        }
    }
}
