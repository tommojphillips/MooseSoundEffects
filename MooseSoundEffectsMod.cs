using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using AudioLibrary;

using MooseSounds;

using MSCLoader;
using UnityEngine;
using UnityEngine.Audio;

namespace TommoJProductions.MooseSounds
{
    // CREDITS
    // Developed by, Tommee J. Armytage.

    // Mod Idea by, Pentti "Einari" Kervinen. "Death and random noises while moose in running state"
    // Audio files by, Pentti "Einari" Kervinen

    /// <summary>
    /// Represents the moose sounds effects mod.
    /// </summary>
    public class MooseSoundEffectsMod : Mod
    {
        // Written, 23.08.2022 (Start Date)

        public override string ID => "MooseSounds"; //Your mod ID (unique)
        public override string Name => "Moose Sounds"; //You mod name
        public override string Author => "tommojphillips"; //Your Username
        public override string Version => VersionInfo.VERSION; //Version
        public override string Description => "Adds death and random occuring running audio for the moose."; //Short description of your mod
        public override bool UseAssetsFolder => true;

        private const string fileName = "mooseSaveData.txt";
        private AudioClip[] mooseDeathAudio;
        private AudioClip[] mooseRunAudio;

        private readonly string readmeContents = "{0} sounds go in this folder\n\nYou can add any number of audio files here for {0} and when said action occurs, a random audio file will play.";
        private readonly string ragdollPath = "Offset/dead moose(xxxxx)";

        private AudioSource audioSource;

        private DirectoryInfo mooseDeathAudioDirectory;
        private DirectoryInfo mooseRunAudioDirectory;

        private GameObject animalsMoose;
        private GameObject moosePrefab;
        private GameObject mooseDeadRagdollPrefab;

        private static int numberOfExtraMoose = 3;
        private int totalMooses = 0;

        private bool reloadingAudio = false;

        private SettingsSliderInt extraMooseSlider;

        private MooseSaveData mooseSaveData;

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
            SetupFunction(Setup.OnSave, onSave);
        }
        public override void ModSettings()
        {
            MSCLoader.Settings.AddButton(this, "loadAudio", reloadingAudio ? "Reloading...." : "Reload audio", reloadAudio);
            if (animalsMoose)
            {
                MSCLoader.Settings.AddButton(this, "toggleMoosesActive", animalsMoose.activeInHierarchy ? "Deactivate Mooses" : "Activate Mooses", 
                    () =>  numberOfExtraMoose = (int)extraMooseSlider.Instance.Value);
            }
            MSCLoader.Settings.AddButton(this, "addMoose", "add moose", delegate ()
            {
                spawnMoose();
                initMoose(totalMooses);
            });
            extraMooseSlider = MSCLoader.Settings.AddSlider(this, "numberOfExtraMoose", "Number of extra moose", 0, 100, numberOfExtraMoose, 
                () => numberOfExtraMoose = extraMooseSlider.GetValue());
        }
        public override void ModSettingsLoaded()
        {
            numberOfExtraMoose = extraMooseSlider.GetValue();
        }
       
        private void onSave() 
        {
            // Written, 26.08.2022

            MooseSaveData mooseSaveData = new MooseSaveData();
            MooseTransformSaveData sd = new MooseTransformSaveData();
            for (int i = 0; i < mooseThatAreDead.Count; i++)
            {
                sd = new MooseTransformSaveData()
                {
                    position = mooseThatAreDead[i].position,
                    eulerAngles = mooseThatAreDead[i].eulerAngles,
                };
                mooseSaveData.deadMoose.Add(sd);
            }
            SaveLoad.SerializeSaveFile(this, mooseSaveData, fileName);
        }
        private void onLoad()
        {
            // Written, 23.08.2022

            // pseudo code

            // load audio files
            // find moose spawner ? / find all moose at game start ?           
            // inject death audio logic.
            // inject random running audio logic.

            loadSaveFile();
            initializeDeadMoose();

            vaildateAssetFolder();
            setupMooses();
            loadAudio();

            ModConsole.Print("[MooseSounds] - loaded");
        }


        #endregion

        #region Methods

        private void loadSaveFile()
        {
            // Written, 26.08.2022

            if (File.Exists(ModLoader.GetModSettingsFolder(this) + "/" + fileName))
            {
                mooseSaveData = SaveLoad.DeserializeSaveFile<MooseSaveData>(this, fileName);
            }
        }

        private void initializeDeadMoose()
        {
            // Written, 26.08.2022

            mooseThatAreDead.Clear();

            if (mooseSaveData != null)
            {
                ModConsole.Print("Loading all dead moose from save file " + mooseSaveData.deadMoose.Count);
                for (int i = 0; i < mooseSaveData.deadMoose.Count; i++)
                {
                    GameObject deadMoose = UnityEngine.Object.Instantiate(mooseDeadRagdollPrefab);
                    deadMoose.transform.position = mooseSaveData.deadMoose[i].position;
                    deadMoose.transform.eulerAngles = mooseSaveData.deadMoose[i].eulerAngles;
                    deadMoose.SetActive(true);
                    mooseThatAreDead.Add(deadMoose.transform);
                }
            }
        }
        private void setupMooses()
        {
            // Written, 24.08.2022

            animalsMoose = GameObject.Find("MAP/StreetLights").GetPlayMaker("Lights Switch").FsmVariables.GetFsmGameObject("Mooses").Value;
            createMoosePrefab();
            setupAudioSource();

            for (int i = 0; i < numberOfExtraMoose; i++)
            {
                spawnMoose();
            }

            for (int i = 1; i < animalsMoose.transform.childCount; i++)
            {
                initMoose(i);
            }
        }
        private void initMoose(int childIndex)
        {
            // Written, 24.08.2022

            totalMooses++;

            GameObject moose = getMooseAtIndex(childIndex);
            if (moose)
            {
                GameObject mooseDead = moose.transform.Find(ragdollPath).gameObject;
                DeadMooseCallback callback = mooseDead.AddComponent<DeadMooseCallback>();
                callback.onEnable += onMooseDead;

#if DEBUG
                ModConsole.Print($"Moose{childIndex}: initialized.");
#endif
            }
            else
            {
                ModConsole.Warning($"Moose{childIndex}: could not find moose.");
            }
        }
        private GameObject getMooseAtIndex(int childIndex)
        {
            // Written, 26.08.2022
           
            if (childIndex < animalsMoose.transform.childCount)
            {
                return animalsMoose.transform.GetChild(childIndex).gameObject;

            }
            return null;
        }
        private void createMoosePrefab()
        {
            // Written, 26.08.2022

            moosePrefab = UnityEngine.Object.Instantiate(getMooseAtIndex(1));
            moosePrefab.name = "MoosePrefab";
            moosePrefab.SetActive(false);

            mooseDeadRagdollPrefab = UnityEngine.Object.Instantiate(moosePrefab.transform.Find("Offset/dead moose(xxxxx)").gameObject);
            mooseDeadRagdollPrefab.name = "MooseRagdollPrefab";
            mooseDeadRagdollPrefab.SetActive(false);
        }
        private void spawnMoose() 
        {
            // Written, 26.08.2022

            GameObject newMoose = GameObject.Instantiate(moosePrefab);
            newMoose.name = "Moose";
            newMoose.transform.parent = animalsMoose.transform;
            newMoose.SetActive(true);

#if DEBUG
            ModConsole.Print($"spawned new moose. {animalsMoose.transform.childCount - 1}");
#endif
        }

        private void setupAudioSource() 
        {
            // Written, 26.08.2022

            GameObject audioSourceGo = new GameObject("MooseSoundsAudioSource");
            audioSource = audioSourceGo.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0;
            audioSource.maxDistance = 100;
            audioSource.playOnAwake = false;
            audioSource.volume = 1;
            audioSource.spread = 5;
        }
        private void reloadAudio()
        {
            // Written, 26.08.2022

            if (!reloadingAudio)
            {
                reloadingAudio = true;
                destoryAudio(mooseDeathAudio);
                destoryAudio(mooseRunAudio);
                loadAudio();
                reloadingAudio = false;
            }
        }
        private void loadAudio()
        {
            // Written, 23.08.2022

            mooseDeathAudio = loadAudioFromFiles(getAudioFilesInDirectory(mooseDeathAudioDirectory));
            mooseRunAudio = loadAudioFromFiles(getAudioFilesInDirectory(mooseRunAudioDirectory));
        }
        private void destoryAudio(AudioClip[] clips)
        {
            // Written, 26.08.2022

            if (clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null)
                    {
                        UnityEngine.Object.Destroy(clips[i]);
                    }
                }
            }
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
            // Written, 26.08.2022

            int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
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

        private List<Transform> mooseThatAreDead = new List<Transform>();

        private void onMooseDead(DeadMooseCallback callback)
        {
            // Written, 24.08.2022

            mooseThatAreDead.Add(callback.transform);

            if (mooseDeathAudio != null)
            {
                audioSource.transform.position = callback.transform.position;
                audioSource.clip = getRandomAudio(mooseDeathAudio);
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.Play();
            }

            UnityEngine.Object.Destroy(callback);

#if DEBUG
            ModConsole.Print("Moose death");   
#endif
        }

        #endregion
    }
}
