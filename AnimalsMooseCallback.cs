using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public class AnimalsMooseCallback : MonoBehaviour
    {
        public event Action onEnable;
        public event Action onDisable;

        void OnEnable()
        {
            onEnable?.Invoke();
        }
        void OnDisable()
        {
            onDisable?.Invoke();
        }
    }
}
