using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public struct Moose 
    {
        public GameObject mooseGo;
        public GameObject deadMooseGo;
        public MooseRunState runState;
        public MooseExtendedRouteStateAction extendedRouteState;
        public PlayMakerFSM movePlayMaker;
        internal int index;
    }
}
