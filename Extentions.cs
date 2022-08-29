using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public static class Extentions
    {
        // Written, 27.08.2022


        public static AudioClip getRandomAudio(this AudioClip[] audioClips)
        {
            // Written, 26.08.2022

            int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
            return audioClips[randomIndex];
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
    }
}
