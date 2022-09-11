using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HutongGames.PlayMaker;

using MSCLoader;

using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public static class Extentions
    {
        // Written, 27.08.2022


        public static T getRandom<T>(this T[] array) where T : class
        {
            // Written, 26.08.2022

            int randomIndex = getRandomIndex(array.Length);
            return array[randomIndex];
        }
        public static T getRandom<T>(this List<T> array) where T : class
        {
            // Written, 26.08.2022

            int randomIndex = getRandomIndex(array.Count);
            return array[randomIndex];
        }
        public static int getRandomIndex(int length)
        {
            return UnityEngine.Random.Range(0, length);
        }

        public static AudioSource createAudioSource(this GameObject gameObject) 
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0;
            audioSource.maxDistance = 100;
            audioSource.playOnAwake = false;
            audioSource.volume = 1;
            audioSource.spread = 5;

            return audioSource;
        }

        public static float getDistanceSqr(this Transform t1, Transform t2)
        {
            // Written, 29.08.2022

            return (t1.position - t2.position).sqrMagnitude;
        }
        public static bool greaterThanDistanceSqr(float f1, float f2)
        {
            // Written, 29.08.2022

            return f1 > f2 * f2;
        }

        public static FsmEvent addEvent(this PlayMakerFSM pm, string eventName)
        {
            try
            {
                pm.Fsm.InitData();
                List<FsmEvent> list = pm.Fsm.Events.ToList();
                FsmEvent fsmEvent = new FsmEvent(eventName);
                list.Add(fsmEvent);
                pm.Fsm.Events = list.ToArray();
                pm.Fsm.InitData();
                return fsmEvent;
            }
            catch (Exception ex)
            {
                ModConsole.Error(ex.ToString());
            }
            return null;
        }
        public static void addGlobalTransition(this PlayMakerFSM pm, FsmEvent fsmEvent, string fsmState)
        {
            try
            {
                pm.Fsm.InitData();
                List<FsmTransition> list = pm.Fsm.GlobalTransitions.ToList();
                list.Add(new FsmTransition
                {
                    FsmEvent = fsmEvent,
                    ToState = fsmState
                });
                pm.Fsm.GlobalTransitions = list.ToArray();
                pm.Fsm.InitData();
            }
            catch (Exception ex)
            {
                ModConsole.Error(ex.ToString());
            }
        }

        public static T firstOrDefault<T>(this List<T> list, Func<T, bool> predicate) where T : struct
        {
            // Written, 31.08.2022

            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    return list[i];
                }
            }
            return default;
        }
        public static GameObject[] getChildren(this GameObject gameObject)
        {
            GameObject[] children = new GameObject[gameObject.transform.childCount];
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                children[i] = gameObject.transform.GetChild(i).gameObject;
            }
            return children;
        }

        public static void writeToFile(string path, string contents) 
        {
            StreamWriter sw = File.CreateText(path);
            sw.Write(contents);
            sw.Close();
            sw.Dispose();
        }
        /// <summary>
        /// Teleports a transform to a world position.
        /// </summary>
        /// <param name="transform">The transform to teleport</param>
        /// <param name="position">The position to teleport the go to.</param>
        public static void teleport(this Transform transform, Vector3 position)
        {
            // Written, 09.07.2022

            Rigidbody rb = transform.GetComponent<Rigidbody>();
            if (rb)
                if (!rb.isKinematic)
                    rb = null;
                else
                    rb.isKinematic = true;
            transform.root.position = position;
            if (rb)
                rb.isKinematic = false;
        }
    }
}
