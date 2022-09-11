using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using AudioLibrary;

using HutongGames.PlayMaker;

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
        public override string Description => "Adds death and running state audio for all moose. Implements towable moose, extends moose routes, allows the player to create their own custom routes.\nL-ALT+M toggles the moose route creation gui.";
        public override bool UseAssetsFolder => true;

        #endregion

        #region Constants

        private const string MOOSE_FILE_NAME = "dead_moose.txt";
        internal const string ROUTE_SAVE_FILE_NAME = "routes.txt";
        private const string README_CONTENTS = "{0} sounds go in this folder";
        
        private const string DEAD_MOOSE_PATH = "Offset/" + DEAD_MOOSE_NAME;
        private const string DEAD_MOOSE_NAME = "dead moose(xxxxx)";
        private const string PLAYMAKER_ADD_FORCE_NAME = "Add Force";
        private const string PLAYMAKER_CHOP_NAME = "Chop";
        private const string CHOP_PIECES_NAME = "Pieces";

        #endregion

        #region Properties / Fields

        public static MooseSoundEffectsMod instance 
        {
            get;
            private set;
        }

        private AudioClip[] mooseDeathAudioClips;
        internal AudioClip[] mooseRunAudioClips { get; private set; }
        internal GameObject[] gameRoutes { get; private set; }

        internal List<Transform> allDeadMoose = new List<Transform>();
        internal List<Moose> allAliveMoose = new List<Moose>();

        internal List<MooseRoute> mooseRoutes = new List<MooseRoute>();
        internal List<MooseRoute> defaultMooseRoutes = new List<MooseRoute>();

        private AudioSource audioSource;

        private DirectoryInfo mooseDeathAudioDirectory;
        private DirectoryInfo mooseRunAudioDirectory;

        private GameObject aliveMoosePrefab;
        private GameObject deadMoosePrefab;
        private GameObject towHookPrefab;

        internal GameObject extendedRouteGo;
        private GameObject mooseSoundsModGo;
        internal GameObject animalsMoose;
        internal Transform player;
        private AnimalsMooseCallback animalsMooseCallback;
        private int numberOfExtraMoose = 3;

        private bool reloadingAudio = false;
        private bool loadDefaultRoutes = true;

        private SettingsSliderInt extraMooseSlider;
        private SettingsCheckBox loadDefaultRoutesCheckbox;
        internal Keybind debugGuiKeybind;

        private MooseSoundsModSaveData mooseSaveData;
        internal MooseRouteSaveData mooseRoutesSaveData;
        private int totalSpawnedMoose;

        #endregion

        public MooseSoundEffectsMod() 
        {
            // Written, 05.09.2022

            instance = this;
        }

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
            
            loadDefaultRoutesCheckbox = Settings.AddCheckBox(this, "loadDefaultRoutes", "Load default routes", loadDefaultRoutes,
                () => loadDefaultRoutes = loadDefaultRoutesCheckbox.GetValue());
            
            Settings.AddDynamicHeader(this, "Save info");
            Settings.AddButton(this, "destroyDeadMoose", "Destroy dead moose", destroyDeadMoose);
            Settings.AddButton(this, "deleteSaveData", "Delete save data", deleteSaveData);
        }
        public override void ModSettingsLoaded()
        {
            numberOfExtraMoose = extraMooseSlider.GetValue();
            loadDefaultRoutes = loadDefaultRoutesCheckbox.GetValue();
        }
       
        private void onNewGame()
        {
            // Written, 26.08.20222

            deleteSaveData();
        }
        private void onSave() 
        {
            // Written, 26.08.2022

            MooseSoundsModSaveData mooseSaveData = new MooseSoundsModSaveData();
            DeadMooseSaveData sd = new DeadMooseSaveData();
            for (int i = 0; i < allDeadMoose.Count; i++)
            {
                int pieces = allDeadMoose[i].GetPlayMaker(PLAYMAKER_CHOP_NAME).FsmVariables.GetFsmInt(CHOP_PIECES_NAME).Value;
                if (pieces < 4)
                {
                    sd = new DeadMooseSaveData()
                    {
                        position = allDeadMoose[i].position,
                        eulerAngles = allDeadMoose[i].eulerAngles,
                        meatGiven = pieces
                    };
                    mooseSaveData.deadMoose.Add(sd);
                }
            }
            SaveLoad.SerializeSaveFile(this, mooseSaveData, MOOSE_FILE_NAME);
        }
        private void onLoad()
        {
            // Written, 23.08.2022

            debugGuiKeybind = Keybind.Add(this, "debugGUItoggle", "Toggle GUI", KeyCode.M, KeyCode.LeftAlt);
            mooseSoundsModGo = new GameObject("Moose Sounds");
            animalsMoose = GameObject.Find("MAP/StreetLights").GetPlayMaker("Lights Switch").FsmVariables.GetFsmGameObject("Mooses").Value;
            player = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("SavePlayer").Value.transform;

            animalsMooseCallback = animalsMoose.AddComponent<AnimalsMooseCallback>();
            animalsMooseCallback.onDisable += animalsMooseCallback_onDisable;

            initMooseRoutes();

            createDevMode();

            createMoosePrefabs();
            createTowHookPrefab();

            vaildateAssetFolder();

            initAudioSource();
            initExistingMoose();
            initExtraMoose();
            loadMooseSaveData();
            initDeadMoose();
            loadAudio();

            ModConsole.Print("[MooseSounds] - loaded");
        }

        #endregion

        #region Methods

        private void initMooseRoutes() 
        {
            // Written, 29.08.2022

            GameObject routes = animalsMoose.transform.Find("Routes").gameObject;
            gameRoutes = routes.getChildren();
            extendedRouteGo = new GameObject("ExtendedRoute");
            extendedRouteGo.transform.parent = routes.transform;
            loadRouteSaveData();
            addRoutes();
        }
        private void addRoutes() 
        {
            // Written, 05.09.2022

            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                        new Vector3Info(-86.80993f, -2.095205f, 60.45926f),
                        new Vector3Info(119.6148f, -2.543851f, -39.64285f),
                        new Vector3Info(286.8706f, -3.134251f, 13.33761f),
                        new Vector3Info(474.9058f, -2.669913f, 9.16772f),
                        new Vector3Info(697.0464f, -2.200019f, -291.4772f),
                        new Vector3Info(866.3936f, -3.462677f, -417.8925f),
                        new Vector3Info(907.1868f, -1.439777f, -582.7089f),
                        new Vector3Info(1024.39f, -0.9053732f, -742.7306f),
                        new Vector3Info(1093.876f, 3.234932f, -913.9547f),
                        new Vector3Info(1086.362f, 7.231291f, -953.6629f),
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(345.3091f, 1.008183f, -1325.826f),
                    new Vector3Info(317.2948f, 1.649235f, -1299.188f),
                    new Vector3Info(283.7922f, 0.7580128f, -1277.179f),
                    new Vector3Info(221.0388f, -1.277822f, -1255.221f),
                    new Vector3Info(20.88142f, -5.200382f, -1281.549f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(869.1173f, 9.699942f, -1031.046f),
                    new Vector3Info(804.1612f, 1.021099f, -1074.173f),
                    new Vector3Info(725.1053f, 1.710422f, -1169.151f),
                    new Vector3Info(713.2606f, 1.555121f, -1201.179f),
                    new Vector3Info(702.6904f, 1.230016f, -1228.902f),
                    new Vector3Info(680.7201f, 1.378664f, -1239.995f),
                    new Vector3Info(630.1418f, 1.363191f, -1242.473f),
                    new Vector3Info(583.7394f, 1.666411f, -1252.774f),
                    new Vector3Info(464.1745f, 1.563336f, -1308.099f),
                    new Vector3Info(416.8425f, 2.035886f, -1328.116f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(83.37943f, -3.204524f, -1232.489f),
                    new Vector3Info(177.8027f, -1.283462f, -1261.083f),
                    new Vector3Info(230.0382f, -1.199572f, -1255.468f),
                    new Vector3Info(284.3319f, 0.6422583f, -1278.188f),
                    new Vector3Info(357.8676f, 0.7662957f, -1325.287f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-48.68179f, -1.501567f, -1144.462f),
                    new Vector3Info(-108.8182f, 1.580767f, -1093.309f),
                    new Vector3Info(-157.2201f, 1.628118f, -1037.945f),
                    new Vector3Info(-228.6607f, 2.187194f, -949.7872f),
                    new Vector3Info(-258.6017f, 3.537115f, -929.7608f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-550.5998f, 1.938618f, -566.3713f),
                    new Vector3Info(-498.1014f, 1.428679f, -594.9534f),
                    new Vector3Info(-382.1788f, 1.853494f, -691.7483f),
                    new Vector3Info(-134.1819f, 2.484365f, -976.7529f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-858.0418f, -3.754723f, -188.6066f),
                    new Vector3Info(-770.8704f, -3.451779f, -189.8434f),
                    new Vector3Info(-876.9424f, 3.107953f, -407.9364f),
                    new Vector3Info(-1049.408f, 5.143838f, -589.594f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-1259.25f, 0.2561604f, -720.4559f),
                    new Vector3Info(-1335.601f, -2.344971f, -677.7721f),
                    new Vector3Info(-1509.635f, 9.745362f, -609.0811f),
                    new Vector3Info(-1582.186f, 7.601111f, -513.5831f),
                    new Vector3Info(-1654.748f, -1.206215f, -461.2475f),
                    new Vector3Info(-1980.597f, 69.34605f, -126.1617f),
                    new Vector3Info(-2095.552f, 75.383f, -71.50362f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-1608.746f, 4.042596f, -399.6715f),
                    new Vector3Info(-1635.199f, -0.6100572f, -331.1692f),
                    new Vector3Info(-1630.761f, -0.751882f, -238.1014f),
                    new Vector3Info(-1634.645f, -0.1629182f, -222.6607f),
                    new Vector3Info(-1630.575f, 2.914647f, -144.7393f),
                    new Vector3Info(-1726.52f, 17.04176f, 127.7136f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-1770.601f, 4.242015f, 1171.252f),
                    new Vector3Info(-1715.042f, 3.00796f, 1012.908f),
                    new Vector3Info(-1680.068f, 4.184901f, 906.2048f),
                    new Vector3Info(-1637.168f, 1.590569f, 772.7688f),
                    new Vector3Info(-1570.021f, 5.40274f, 667.3347f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-1444.58f, 11.39836f, 1695.024f),
                    new Vector3Info(-1312.449f, 11.29459f, 1686.574f),
                    new Vector3Info(-1115.117f, 10.69776f, 1668.754f),
                    new Vector3Info(-1071.722f, 10.58517f, 1658.277f),
                    new Vector3Info(-963.5252f, 9.351628f, 1646.514f),
                    new Vector3Info(-923.4288f, 5.578684f, 1603.24f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(991.9183f, 13.99485f, 1319.93f),
                    new Vector3Info(943.9332f, 14.67483f, 1391.824f),
                    new Vector3Info(749.2288f, 16.1487f, 1508.447f),
                    new Vector3Info(-46.68895f, 6.931365f, 1997.676f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(2243.72f, 3.175382f, -183.1377f),
                    new Vector3Info(2228.028f, 2.28878f, -252.7085f),
                    new Vector3Info(2224.595f, 0.6370261f, -386.5457f),
                    new Vector3Info(2209.564f, -1.021198f, -478.1128f),
                    new Vector3Info(2187.41f, 1.154181f, -925.6111f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(675.9297f, 7.28378f, -1541.469f),
                    new Vector3Info(843.4106f, 9.653605f, -1373.101f),
                    new Vector3Info(948.0331f, 7.290115f, -1299.542f),
                    new Vector3Info(1057.029f, 11.07816f, -1222.769f),
                    new Vector3Info(1122.087f, 10.70717f, -1197.728f),
                    new Vector3Info(1356.413f, 6.334666f, -1087.7f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-545.9938f, 0.581367f, -1306.419f),
                    new Vector3Info(-702.8441f, 1.214351f, -1285.651f),
                    new Vector3Info(-799.708f, 2.50859f, -1266.457f),
                    new Vector3Info(-884.5336f, 5.391207f, -1252.742f),
                    new Vector3Info(-929.7167f, 7.315431f, -1250.235f),
                    new Vector3Info(-1038.814f, 9.392406f, -1235.762f),
                    new Vector3Info(-1087.476f, 8.62474f, -1235.457f),
                    new Vector3Info(-1321.307f, 6.423577f, -1210.492f),
                    new Vector3Info(-1426.69f, 5.283435f, -1192.466f),
                    new Vector3Info(-1486.975f, 6.459633f, -1190.312f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-1380.627f, 3.678301f, 1294.51f),
                    new Vector3Info(-1319.644f, 1.304909f, 1250.25f),
                    new Vector3Info(-1289.641f, -0.3413493f, 1269.21f),
                    new Vector3Info(-1199.948f, 0.352421f, 1317.59f),
                    new Vector3Info(-1174.483f, 4.617094f, 1299.85f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(-523.6204f, 1.413577f, 1189.318f),
                    new Vector3Info(-389.0746f, 0.1419125f, 1228.539f),
                    new Vector3Info(-322.8465f, 5.101363f, 1217.942f),
                    new Vector3Info(-187.0225f, 2.530046f, 1246.933f),
                    new Vector3Info(-116.9373f, -2.743057f, 1266.629f),
                    new Vector3Info(54.65636f, 0.5361992f, 1308.203f),
                    new Vector3Info(107.5755f, -0.5060833f, 1337.213f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(1468.194f, -1.658897f, 784.8923f),
                    new Vector3Info(1377.345f, 5.622688f, 760.2281f),
                    new Vector3Info(1321.938f, 5.703267f, 787.2725f),
                    new Vector3Info(1114.012f, 3.583886f, 786.9676f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(1537.841f, 2.104933f, 583.2562f),
                    new Vector3Info(1647.053f, 5.235429f, 566.6331f),
                    new Vector3Info(1833.859f, 7.448469f, 526.1113f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(1985.162f, -0.6246084f, -92.69482f),
                    new Vector3Info(1962.943f, -2.583179f, -23.431f),
                    new Vector3Info(1950.208f, 2.936264f, 94.66791f),
                    new Vector3Info(1932.854f, 3.326883f, 195.1051f),
                    new Vector3Info(1897.33f, 6.02062f, 354.9279f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(1913.56f, 7.934895f, -620.4477f),
                    new Vector3Info(1903.065f, 6.650363f, -553.3216f),
                    new Vector3Info(1900.817f, 6.146604f, -394.3883f),
                    new Vector3Info(1988.797f, 5.9941f, -177.246f)
                }
            });
            defaultMooseRoutes.Add(new MooseRoute()
            {
                points = new List<Vector3Info>()
                {
                    new Vector3Info(1522.993f, 2.406036f, -1030.923f),
                    new Vector3Info(1216.183f, 4.82469f, -935.5157f),
                    new Vector3Info(1170.119f, 3.523758f, -900.7452f)
                }
            });

            if (loadDefaultRoutes)
            {
                mooseRoutes.AddRange(defaultMooseRoutes);
            }
            mooseRoutes.AddRange(mooseRoutesSaveData.loadedMooseRoutes);
        }

        private void createDevMode()
        {
            MooseDevTools dev = mooseSoundsModGo.AddComponent<MooseDevTools>();
        }

        private void createMoosePrefabs()
        {
            // Written, 26.08.2022

            aliveMoosePrefab = UnityEngine.Object.Instantiate(animalsMoose.transform.GetChild(1).gameObject);
            aliveMoosePrefab.transform.parent = mooseSoundsModGo.transform;
            aliveMoosePrefab.name = "AliveMoosePrefab";
            aliveMoosePrefab.SetActive(false);

            deadMoosePrefab = UnityEngine.Object.Instantiate(aliveMoosePrefab.transform.Find(DEAD_MOOSE_PATH).gameObject);
            deadMoosePrefab.name = "DeadMoosePrefab";
            deadMoosePrefab.tag = "RAGDOLL";
            deadMoosePrefab.SetActive(false);
            deadMoosePrefab.transform.parent = mooseSoundsModGo.transform;
            //UnityEngine.Object.Destroy(deadMoosePrefab.GetPlayMaker(PLAYMAKER_ADD_FORCE_NAME));            
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

        private void loadMooseSaveData()
        {
            // Written, 26.08.2022

            if (File.Exists(ModLoader.GetModSettingsFolder(this) + "/" + MOOSE_FILE_NAME))
            {
                mooseSaveData = SaveLoad.DeserializeSaveFile<MooseSoundsModSaveData>(this, MOOSE_FILE_NAME);
            }
        }
        private void loadRouteSaveData()
        {
            // Written, 07.09.2022

            if (File.Exists(ModLoader.GetModSettingsFolder(this) + "/" + ROUTE_SAVE_FILE_NAME))
            {
                mooseRoutesSaveData = SaveLoad.DeserializeSaveFile<MooseRouteSaveData>(this, ROUTE_SAVE_FILE_NAME);
            }
            else
            {
                mooseRoutesSaveData = new MooseRouteSaveData();
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
        private void initAliveMoose(GameObject mooseGo)
        {
            // Written, 24.08.2022

            totalSpawnedMoose++;
            Moose moose = new Moose();
            moose.index = totalSpawnedMoose;
            moose.mooseGo = mooseGo;

            // run state sounds
            moose.runState = mooseGo.AddComponent<MooseRunState>();
            moose.runState.moose = moose;
            moose.runState.onDestroy += preOnMooseDead;

            // dead moose
            moose.deadMooseGo = mooseGo.transform.Find(DEAD_MOOSE_PATH).gameObject;
            DeadMooseCallback callback = moose.deadMooseGo.AddComponent<DeadMooseCallback>();
            callback.onEnable += postOnMooseDead;

            // tow hook
            initHookOnMoose(moose.deadMooseGo);

            // extended route
            PlayMakerFSM fsm = mooseGo.GetPlayMaker("Move");
            moose.movePlayMaker = fsm;
            FsmState randRoute = fsm.GetState("Randomize route");
            moose.extendedRouteState = new MooseExtendedRouteStateAction();
            moose.extendedRouteState.routeStart = fsm.FsmVariables.FindFsmGameObject("RouteStart");
            moose.extendedRouteState.routeEnd = fsm.FsmVariables.FindFsmGameObject("RouteEnd");            
            randRoute.InsertAction(2, moose.extendedRouteState);

            allAliveMoose.Add(moose);

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
        internal void spawnDeadMoose(DeadMooseSaveData mooseData)
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
                allDeadMoose.Clear();
            }
        }
        private void deleteSaveData() 
        {
            string path = ModLoader.GetModSettingsFolder(this) + "/" + MOOSE_FILE_NAME;
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
                    if (!fileName.EndsWith(".txt"))
                    {
                        ModConsole.Error("[MooseSounds] - Unknown audio format of file " + fileName);
                    }
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

                Extentions.writeToFile(directoryPath + "/readme.txt", string.Format(README_CONTENTS, info.Name));
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
                audioSource.clip = mooseDeathAudioClips.getRandom();
                if (audioSource.isPlaying)
                    audioSource.Stop();
                audioSource.Play();
            }
            UnityEngine.Object.Destroy(callback);

            ModConsole.Print($"Moose die sound");
        }
        private void preOnMooseDead(MooseRunState callback)
        {
            // Written, 28.08.2022

            allAliveMoose.Remove(allAliveMoose.firstOrDefault(m => m.index == callback.moose.index));
            
            ModConsole.Print($"Moose{callback.moose.index} death");
        }
        private void animalsMooseCallback_onDisable()
        {
            // Written, 10.09.2022

            for (int i = 0; i < allAliveMoose.Count; i++)
            {
                if (allAliveMoose[i].extendedRouteState.currentRoute != null)
                {
                    allAliveMoose[i].extendedRouteState.resetExtendedRoute();
                }
            }
        }

        #endregion
    }
}
