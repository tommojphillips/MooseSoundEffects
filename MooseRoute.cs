using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public class MooseRoute
    {
        // Written, 29.08.2022

        internal bool routeInUse;

        internal readonly GameObject routeStart;
        internal readonly GameObject routeEnd;

        internal GameObject mooseOnRoute { get; set; }

        internal List<Vector3> points { get; set; } = new List<Vector3>();

        internal int currentPoint { get; private set; }

        public MooseRoute()
        {
            routeStart = new GameObject("MooseRouteStart");
            routeEnd = new GameObject("MooseRouteEnd");
        }

        public bool setNextPoint() 
        {
            if (currentPoint < points.Count - 1)
            {
                currentPoint++;
                routeStart.transform.position = points[currentPoint - 1];
                routeEnd.transform.position = points[currentPoint];
                return true;
            }
            return false;
        }
        public void reset() 
        {
            currentPoint = 0;
        }
    }
}
