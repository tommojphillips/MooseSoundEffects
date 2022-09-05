using System;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;
using static TommoJProductions.MooseSounds.MooseSoundEffectsMod;
using static TommoJProductions.MooseSounds.Extentions;
using MSCLoader;

namespace TommoJProductions.MooseSounds
{
    public class MooseRunState : MonoBehaviour
    {
        // Written, 27.08.2022

        public event Action<MooseRunState> onDestroy;

        public static float minTime = 0;
        public static float minMaxTime = 30;
        public static float maxTime = 300;

        public Moose moose;

        private float waitTime = 0;
        private float playerDistanceThreshold = 100;
        private float playerDistance;

        private bool playerNear = false;

        private AudioSource audioSource;

        private MooseSoundEffectsMod mod;

        #region Unity runtime

        private void Awake()
        {
            // Written, 27.08.2022

            audioSource = gameObject.createAudioSource();
            mod = MooseSoundEffectsMod.instance;
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
                audioSource.clip = mod.mooseRunAudioClips.getRandom();
                audioSource.Play();
                setRandomWaitTime();
                return;
            }

            waitTime -= Time.deltaTime;

            playerDistance = mod.player.getDistanceSqr(transform);

            if (!greaterThanDistanceSqr(playerDistance, playerDistanceThreshold))
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

            if (playerNear && waitTime > 10)
            {
                if (!greaterThanDistanceSqr(playerDistance, 15))
                {
                    waitTime = 10;
                }
            }
        }

        #endregion

        private void setRandomWaitTime() 
        {
            // Written, 27.08.2022

            waitTime = Random.Range(minTime, Random.Range(minMaxTime, maxTime));
        }
    }
}
