using System;
using MSCLoader;
using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public class DeadMooseCallback : MonoBehaviour
    {
        // Written, 24.08.2022

        public event Action<DeadMooseCallback> onEnable;
        
        void OnEnable() 
        {
            onEnable?.Invoke(this);
        }
    }
}