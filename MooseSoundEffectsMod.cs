using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using AudioLibrary;

using MSCLoader;
using UnityEngine;
using UnityEngine.Audio;

namespace TommoJProductions.MooseSounds
{
    public class MooseSoundEffectsMod : Mod
    {
        // Written, 23.08.2022 (Start Date)

        // Developed by, Tommee J. Armytage.

        // Mod Idea by, Pentti "Einari" Kervinen
        // Audio files by, Pentti "Einari" Kervinen

        public override string ID => "MooseSounds"; //Your mod ID (unique)
        public override string Name => "Moose Sounds"; //You mod name
        public override string Author => "tommojphillips"; //Your Username
        public override string Version => VersionInfo.VERSION; //Version
        public override string Description => "Adds death and random occuring running audio for the moose."; //Short description of your mod
        public override bool UseAssetsFolder => true;

        private AudioClip[] mooseDeathAudio;
        private AudioClip[] mooseRunAudio;

        private readonly string readmeContents = "{0} sounds go in this folder\n\nYou can add any number of audio files here for {0} and when said action occurs, a random audio file will play.";
        
        private AudioSource audioSource;

        private DirectoryInfo mooseDeathAudioDirectory;
        private DirectoryInfo mooseRunAudioDirectory;

        #region Constructors

        static MooseSoundEffectsMod()
        {
            // Written, 23.08.2022

        }

        #endregion

        #region Mod setup

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, onLoad);
        }
        public override void ModSettings()
        {
            MSCLoader.Settings.AddButton(this, "loadAudio", "Reload audio", loadAudio);
        }

        private void onLoad()
        {
            // Written, 23.08.2022

            // pseudo code

            // load audio files
            // find moose spawner ? / find all moose at game start ?           
            // inject death audio logic.
            // inject random running audio logic.

            vaildateAssetFolder();
            setupMooses();
            loadAudio();

            ModConsole.Print("[MooseSounds] - loaded");
        }

        #endregion

        #region Methods

        private void setupMooses()
        {
            // Written, 24.08.2022

            GameObject animalsMoose = GameObject.Find("MAP/StreetLights").GetPlayMaker("Lights Switch").FsmVariables.GetFsmGameObject("Mooses").Value;
            
            GameObject audioSourceGo = new GameObject("MooseSoundsAudioSource");
            audioSource = audioSourceGo.AddComponent<AudioSource>();
            audioSource.volume = 1;
            audioSource.minDistance = 5;
            audioSource.maxDistance = 100;
            audioSource.playOnAwake = false;

            for (int i = 1; i < animalsMoose.transform.childCount; i++)
            {
                initMoose(i, animalsMoose);
            }
        }
        private void initMoose(int childIndex, GameObject animalsMoose)
        {
            // Written, 24.08.2022

            GameObject moose = animalsMoose.transform.GetChild(childIndex).gameObject;
            GameObject mooseDead = moose.transform.Find("Offset/dead moose(xxxxx)").gameObject;
            DeadMooseCallback callback = mooseDead.AddComponent<DeadMooseCallback>();
            callback.onEnable += onMooseDead;
        }

        private void loadAudio()
        {
            // Written, 23.08.2022

            mooseDeathAudio = loadAudioFromFiles(getAudioFilesInDirectory(mooseDeathAudioDirectory));
            mooseRunAudio = loadAudioFromFiles(getAudioFilesInDirectory(mooseRunAudioDirectory));
        }
        private AudioClip[] loadAudioFromFiles(string[] files)
        {
            // Written, 24.08.2022

            List<AudioClip> clips = new List<AudioClip>();

            for (int i = 0; i < files.Length; i++)
            {
                Stream dataStream = new MemoryStream(File.ReadAllBytes(files[i]));
                AudioFormat audioFormat = Manager.GetAudioFormat(files[i]);
                string fileName = Path.GetFileName(files[i]);
                if (audioFormat == AudioFormat.unknown)
                {
                    ModConsole.Error("[MooseSounds] - Unknown audio format of file " + fileName);
                    continue;
                }

                clips.Add(Manager.Load(dataStream, audioFormat, fileName, false, false));
                ModConsole.Print($"[MooseSounds] - #{i + 1} audio found: {Path.GetFileName(files[i])}");
            }
            return clips.ToArray();
        }
        private AudioClip getRandomAudio(AudioClip[] audioClips)
        {
            // Written, 24.08.2022

            int randomIndex = Random.Range(0, audioClips.Length + 1); // min: is inclusive where max: is exclusive. thus if just passing 'array.Length'. random will never get the last audioclip in array.

            return audioClips[randomIndex];
        }
        
        private void vaildateAssetFolder() 
        {
            // Written, 23.08.2022

            string assetsDirectory = ModLoader.GetModAssetsFolder(this);

            mooseDeathAudioDirectory = getOrCreateDirectory(assetsDirectory + "/Moose/DeathSounds");

            mooseRunAudioDirectory = getOrCreateDirectory(assetsDirectory + "/Moose/RunSounds");
        }
        private DirectoryInfo getOrCreateDirectory(string directoryPath)
        {
            // Written, 23.08.2022

            DirectoryInfo info;
            if (!Directory.Exists(directoryPath))
            {
                info = Directory.CreateDirectory(directoryPath);
                
                StreamWriter sw = File.CreateText(directoryPath + "/readme.txt");
                sw.Write(string.Format(readmeContents, info.Name));
                sw.Close();
                sw.Dispose();
            }
            else
            {
                info =  new DirectoryInfo(directoryPath);
            }
            return info;
        }
        private string[] getAudioFilesInDirectory(DirectoryInfo info)
        {
            // Written, 23.08.2022
            
            string[] foundFiles = Directory.GetFiles(info.FullName, "*.*", SearchOption.TopDirectoryOnly).ToArray();

            if (foundFiles?.Length <= 0)
            {
                ModConsole.Warning($"[MooseSounds] - {info.Parent.Name}/{info.Name} - no audio files found");
                return null;
            }
            
            return foundFiles;
        }

        #endregion

        #region Event handlers

        private void onMooseDead(DeadMooseCallback callback)
        {
            // Written, 24.08.2022

            if (mooseDeathAudio != null)
            {
                audioSource.transform.position = callback.transform.position;
                audioSource.clip = getRandomAudio(mooseDeathAudio);
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.Play();
            }
#if DEBUG
            ModConsole.Print("Moose death");   
#endif
        }

        #endregion
    }
}
