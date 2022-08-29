using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using AudioLibrary;

using MSCLoader;
using UnityEngine;
using UnityEngine.Audio;

using static UnityEngine.GUILayout;

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

        #region Mod Properties

        public override string ID => "MooseSounds";
        public override string Name => "Moose Sounds";
        public override string Author => "tommojphillips";
        public override string Version => VersionInfo.VERSION;
        public override string Description => "Adds death and random occuring running audio for the moose.";
        public override bool UseAssetsFolder => true;

        #endregion

        #region Constants

        private const string FILE_NAME = "mooseSaveData.txt";
        private const string README_CONTENTS = "{0} sounds go in this folder";
        
        private const string DEAD_MOOSE_PATH = "Offset/" + DEAD_MOOSE_NAME;
        private const string DEAD_MOOSE_NAME = "dead moose(xxxxx)";
        private const string PLAYMAKER_ADD_FORCE_NAME = "Add Force";
        private const string PLAYMAKER_CHOP_NAME = "Chop";
        private const string CHOP_PIECES_NAME = "Pieces";

        #endregion

        #region Properties / Fields

        internal static AudioClip[] mooseRunAudioClips { get; private set; }

        private AudioClip[] mooseDeathAudioClips;

        internal List<Transform> allDeadMoose = new List<Transform>();
        internal List<Transform> allAliveMoose = new List<Transform>();

        private AudioSource audioSource;

        private DirectoryInfo mooseDeathAudioDirectory;
        private DirectoryInfo mooseRunAudioDirectory;

        private GameObject aliveMoosePrefab;
        private GameObject deadMoosePrefab;
        private GameObject towHookPrefab;

        private GameObject mooseSoundsModGo;
        internal GameObject animalsMoose;
        internal static Transform player;

        private int numberOfExtraMoose = 3;

        private bool reloadingAudio = false;

        private SettingsSliderInt extraMooseSlider;

        private MooseSaveData mooseSaveData;

        #endregion

        #region Mod setup

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, onLoad);
            SetupFunction(Setup.OnSave, onSave);
        }
        public override void ModSettings()
        {
            Settings.AddDynamicHeader(this, "Audio");
            Settings.AddButton(this, "loadAudio", reloadingAudio ? "Reloading...." : "Reload audio", reloadAudio);

            Settings.AddDynamicHeader(this, "Moose");
            extraMooseSlider = Settings.AddSlider(this, "numberOfExtraMoose", "Number of extra moose", 0, 100, numberOfExtraMoose,
                () => numberOfExtraMoose = extraMooseSlider.GetValue());
            Settings.AddDynamicText(this, "The number of additional moose to spawn when game starts.");

            Settings.AddDynamicHeader(this, "Save info");
            Settings.AddButton(this, "destroyDeadMoose", "Destroy dead moose", destroyDeadMoose);
            Settings.AddButton(this, "deleteSaveData", "Delete save data", deleteSaveData);
        }
        public override void ModSettingsLoaded()
        {
            numberOfExtraMoose = extraMooseSlider.GetValue();
        }
       
        private void onNewGame()
        {
            // Written, 26.08.20222

            deleteSaveData();
        }
        private void onSave() 
        {
            // Written, 26.08.2022

            MooseSaveData mooseSaveData = new MooseSaveData();
            Moose sd = new Moose();
            for (int i = 0; i < allDeadMoose.Count; i++)
            {
                int pieces = allDeadMoose[i].GetPlayMaker(PLAYMAKER_CHOP_NAME).FsmVariables.GetFsmInt(CHOP_PIECES_NAME).Value;
                if (pieces < 4)
                {
                    sd = new Moose()
                    {
                        position = allDeadMoose[i].position,
                        eulerAngles = allDeadMoose[i].eulerAngles,
                        meatGiven = pieces
                    };
                    mooseSaveData.deadMoose.Add(sd);
                }
            }
            SaveLoad.SerializeSaveFile(this, mooseSaveData, FILE_NAME);
        }
        private void onLoad()
        {
            // Written, 23.08.2022

            mooseSoundsModGo = new GameObject("Moose Sounds");
            animalsMoose = GameObject.Find("MAP/StreetLights").GetPlayMaker("Lights Switch").FsmVariables.GetFsmGameObject("Mooses").Value;
            player = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("SavePlayer").Value.transform;

            createDevMode();

            createMoosePrefabs();
            createTowHookPrefab();

            vaildateAssetFolder();

            initAudioSource();
            initExistingMoose();
            initExtraMoose();
            loadSaveData();
            initDeadMoose();
            loadAudio();

            ModConsole.Print("[MooseSounds] - loaded");
        }

        #endregion

        #region Methods

        private void createDevMode()
        {
            MooseDevTools dev = mooseSoundsModGo.AddComponent<MooseDevTools>();
            dev.mod = this;
        }

        private void createMoosePrefabs()
        {
            // Written, 26.08.2022

            aliveMoosePrefab = UnityEngine.Object.Instantiate(animalsMoose.transform.GetChild(1).gameObject);
            aliveMoosePrefab.name = "AliveMoosePrefab";
            aliveMoosePrefab.SetActive(false);
            aliveMoosePrefab.transform.parent = mooseSoundsModGo.transform;

            deadMoosePrefab = UnityEngine.Object.Instantiate(aliveMoosePrefab.transform.Find(DEAD_MOOSE_PATH).gameObject);
            deadMoosePrefab.name = "DeadMoosePrefab";
            deadMoosePrefab.tag = "RAGDOLL";
            deadMoosePrefab.SetActive(false);
            deadMoosePrefab.transform.parent = mooseSoundsModGo.transform;
            UnityEngine.Object.Destroy(deadMoosePrefab.GetPlayMaker(PLAYMAKER_ADD_FORCE_NAME));
            
        }
        private void createTowHookPrefab()
        {
            // Written, 26.08.2022

            GameObject hayosikoHookFront = GameObject.Find("HAYOSIKO(1500kg, 250)/HookFront");
            if (hayosikoHookFront)
            {
                towHookPrefab = UnityEngine.Object.Instantiate(hayosikoHookFront);
                towHookPrefab.name = "TowHookPrefab";
                towHookPrefab.transform.parent = mooseSoundsModGo.transform;
                setHookObject(towHookPrefab, null);
            }
            else
            {
                ModConsole.Error("Could not find the hook gameobject. towing hooks wont work.");
            }
        }

        private void loadSaveData()
        {
            // Written, 26.08.2022

            if (File.Exists(ModLoader.GetModSettingsFolder(this) + "/" + FILE_NAME))
            {
                mooseSaveData = SaveLoad.DeserializeSaveFile<MooseSaveData>(this, FILE_NAME);
            }
        }

        private void initAudioSource() 
        {
            // Written, 26.08.2022

            GameObject audioSourceGo = new GameObject("MooseDeathSoundsAudioSource");
            audioSource = audioSourceGo.createAudioSource();
        }
        private void initExistingMoose()
        {
            // Written, 26.08.2022

            for (int i = 1; i < animalsMoose.transform.childCount; i++)
            {
                initAliveMoose(animalsMoose.transform.GetChild(i).gameObject);
            }
        }
        private void initExtraMoose()
        {
            // Written, 26.08.2022

            for (int i = 0; i < numberOfExtraMoose; i++)
            {
                spawnAliveMoose();
            }
        }
        private void initDeadMoose()
        {
            // Written, 26.08.2022

            allDeadMoose.Clear();

            if (mooseSaveData != null && mooseSaveData.deadMoose.Count > 0)
            {
                ModConsole.Print("Loading all dead moose from the save file " + mooseSaveData.deadMoose.Count);
                for (int i = 0; i < mooseSaveData.deadMoose.Count; i++)
                {
                    spawnDeadMoose(mooseSaveData.deadMoose[i]);
                }
            }
        }
        private void initAliveMoose(GameObject moose)
        {
            // Written, 24.08.2022

            allAliveMoose.Add(moose.transform);

            GameObject deadMooseGo = moose.transform.Find(DEAD_MOOSE_PATH).gameObject;
            DeadMooseCallback callback = deadMooseGo.AddComponent<DeadMooseCallback>();
            callback.onEnable += postOnMooseDead;

            initHookOnMoose(deadMooseGo);

            MooseRunStateSounds runState = moose.AddComponent<MooseRunStateSounds>();
            runState.onDestroy += preOnMooseDead;

            ModConsole.Print("[MooseSounds] init alive moose" + allAliveMoose.Count);
        }

        private void initHookOnMoose(GameObject deadMoose)
        {
            // Written, 26.08.2022

            if (towHookPrefab)
            {
                GameObject hook = GameObject.Instantiate(towHookPrefab);
                hook.transform.parent = deadMoose.transform;
                hook.transform.localPosition = Vector3.zero;
                hook.transform.localEulerAngles = Vector3.zero;
                hook.name = "Hook";
                setHookObject(hook, deadMoose);
            }
        }
        private void setHookObject(GameObject hook, GameObject toGameObject)
        {
            // Written, 27.08.2022

            hook.SetActive(toGameObject);
            PlayMakerFSM logic = hook.GetPlayMaker("Logic");
            logic.FsmVariables.FindFsmGameObject("ThisCar").Value = toGameObject;
            logic.enabled = toGameObject;
        }

        internal void spawnAliveMoose() 
        {
            // Written, 26.08.2022

            GameObject newMoose = GameObject.Instantiate(aliveMoosePrefab);
            newMoose.name = "Moose";
            newMoose.transform.parent = animalsMoose.transform;
            initAliveMoose(newMoose);
            newMoose.SetActive(true);
        }
        internal void spawnDeadMoose(Moose mooseData)
        {
            // Written, 27.08.2022

            GameObject deadMoose = UnityEngine.Object.Instantiate(deadMoosePrefab);
            deadMoose.GetPlayMaker(PLAYMAKER_CHOP_NAME).FsmVariables.GetFsmInt(CHOP_PIECES_NAME).Value = Mathf.Clamp(mooseData.meatGiven, 0, 3);
            deadMoose.transform.position = mooseData.position;
            deadMoose.transform.eulerAngles = mooseData.eulerAngles;
            deadMoose.name = DEAD_MOOSE_NAME;
            allDeadMoose.Add(deadMoose.transform);
            initHookOnMoose(deadMoose);
            deadMoose.SetActive(true);

            ModConsole.Print("[MooseSounds] init dead moose" + allDeadMoose.Count);
        }

        internal void destroyDeadMoose() 
        {
            // Written, 29.08.2022

            if (ModLoader.CurrentScene == CurrentScene.Game)
            {
                for (int i = 0; i < allDeadMoose.Count; i++)
                {
                    UnityEngine.Object.Destroy(allDeadMoose[i].gameObject);
                }
            }
        }
        private void deleteSaveData() 
        {
            string path = ModLoader.GetModSettingsFolder(this) + "/" + FILE_NAME;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void reloadAudio()
        {
            // Written, 26.08.2022

            if (!reloadingAudio)
            {
                reloadingAudio = true;
                destoryAudio(mooseDeathAudioClips);
                destoryAudio(mooseRunAudioClips);
                loadAudio();
                reloadingAudio = false;
            }
        }
        private void loadAudio()
        {
            // Written, 23.08.2022

            mooseDeathAudioClips = loadAudioFromFiles(getAudioFilesInDirectory(mooseDeathAudioDirectory));
            mooseRunAudioClips = loadAudioFromFiles(getAudioFilesInDirectory(mooseRunAudioDirectory));
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
                sw.Write(string.Format(README_CONTENTS, info.Name));
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

        private void postOnMooseDead(DeadMooseCallback callback)
        {
            // Written, 24.08.2022

            allDeadMoose.Add(callback.transform);

            if (mooseDeathAudioClips != null)
            {
                audioSource.transform.position = callback.transform.position;
                audioSource.clip = mooseDeathAudioClips.getRandomAudio();
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.Play();
            }

            UnityEngine.Object.Destroy(callback.transform.GetPlayMaker(PLAYMAKER_ADD_FORCE_NAME));
            UnityEngine.Object.Destroy(callback);

            ModConsole.Print("Post Moose death");   
        }

        private void preOnMooseDead(MooseRunStateSounds callback)
        {
            // Written, 28.08.2022

            allAliveMoose.Remove(callback.transform);

            ModConsole.Print("Pre Moose death");   
        }

        #endregion
    }
}
