using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;
using static TommoJProductions.MooseSounds.MooseSoundEffectsMod;

namespace TommoJProductions.MooseSounds
{
    public class MooseRunStateSounds : MonoBehaviour
    {
        // Written, 27.08.2022

        public event Action<MooseRunStateSounds> onDestroy;

        public static float minTime = 0;
        public static float minMaxTime = 10;
        public static float maxTime = 60;

        private float waitTime = 0;
        private float playerDistanceThreshold = 100;
        private float playerDistance;

        private bool playerNear = false;

        private Transform player;

        private AudioSource audioSource;

        #region Unity runtime

        private void Awake()
        {
            // Written, 27.08.2022

            audioSource = gameObject.createAudioSource();
        }
        private void Start()
        {
            // Written, 27.08.2022

            setRandomWaitTime();
        }
        private void OnDestroy() 
        {
            onDestroy?.Invoke(this);
        }

        private void Update()
        {
            // Written, 27.08.2022

            if (audioSource.isPlaying)
                return;

            if (waitTime <= 0)
            {
                audioSource.clip = mooseRunAudioClips.getRandomAudio();
                audioSource.Play();
                setRandomWaitTime();
                return;
            }
            waitTime -= Time.deltaTime;

            /*playerDistance = (player.position - transform.position).sqrMagnitude;

            if (playerDistance < playerDistanceThreshold)
            {
                if (!playerNear)
                {
                    playerNear = true;
                    waitTime = 0;
                }
            }
            else if (playerNear)
            {
                playerNear = false;
            }

            if (playerNear)
            {
                if (playerDistance < 15)
                {
                    if (waitTime > 10)
                    {
                        waitTime = 10;
                    }
                }
            }*/
        }

        #endregion

        private void setRandomWaitTime() 
        {
            // Written, 27.08.2022

            waitTime = Random.Range(minTime, Random.Range(minMaxTime, maxTime));
        }
    }
}
