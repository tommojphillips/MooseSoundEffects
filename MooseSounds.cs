using System.Collections.Generic;
using System.IO;
using System.Linq;

using MSCLoader;

using UnityEngine;

namespace MooseSounds
{
    public class MooseSounds : Mod
    {
        // Written, 23.08.2022 (Start Date)

        // Developed by, Tommee J. Armytage.

        // Mod Idea by, Pentti "Einari" Kervinen
        // Audio files by, Pentti "Einari" Kervinen

        public override string ID => "MooseSounds"; //Your mod ID (unique)
        public override string Name => "Moose Sounds"; //You mod name
        public override string Author => "tommojphillips"; //Your Username
        public override string Version => "1.0"; //Version
        public override string Description => "Adds death and random occuring running audio for the moose."; //Short description of your mod
        public override bool UseAssetsFolder => true;

        private AudioClip[] mooseDeathAudio;
        private AudioClip[] mooseRunAudio;

        private DirectoryInfo mooseDeathAudioDirectory;
        private DirectoryInfo mooseRunAudioDirectory;

        private string readmeContents = "{0} sounds go in this folder\nSupported audio extentions are: {1}\n\nYou can add any number of audio files here for {0} and when said action occurs, a random audio file will play.";
        private string allSupportedAudioExtentions;

        private Dictionary<string, AudioType> supportedAudioExtentions;

        public MooseSounds()
        {
            // Written, 23.08.2022

            supportedAudioExtentions = new Dictionary<string, AudioType>();
            supportedAudioExtentions.Add(".wav", AudioType.WAV);
            supportedAudioExtentions.Add(".ogg", AudioType.OGGVORBIS);
            supportedAudioExtentions.Add(".mp3", AudioType.MPEG);

            allSupportedAudioExtentions = string.Join(", ", supportedAudioExtentions.Keys.ToArray());
        }

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, onLoad);
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
            mooseDeathAudio = getAudioInDirectory(mooseDeathAudioDirectory);
            mooseRunAudio = getAudioInDirectory(mooseRunAudioDirectory);

            ModConsole.Print("[MooseSounds] - loaded");
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
                sw.Write(string.Format(readmeContents, info.Name, allSupportedAudioExtentions));
                sw.Close();
                sw.Dispose();
            }
            else
            {
                info =  new DirectoryInfo(directoryPath);
            }
            return info;
        }

        private AudioClip[] getAudioInDirectory(DirectoryInfo info)
        {
            // Written, 23.08.2022
            
            string[] foundFiles = Directory.GetFiles(info.FullName, "*.*", SearchOption.TopDirectoryOnly)
                .Where(fileName => supportedAudioExtentions.Keys
                .Any(extention => fileName
                .ToLower().Contains(extention))).ToArray();

            if (foundFiles?.Length <= 0)
            {
                ModConsole.Warning($"[MooseSounds] - {info.Parent.Name}/{info.Name} - no audio files found\n- Supported file extentions are: {allSupportedAudioExtentions}");
                return null;
            }

            AudioClip[] results = new AudioClip[foundFiles.Length];
            WWW www;
            string fileExtention;

            for (int i = 0; i < foundFiles.Length; i++)
            {
                fileExtention = Path.GetExtension(foundFiles[i]);
                www = new WWW("file:///" + foundFiles[i]);
                results[i] = www.GetAudioClip(true, false, supportedAudioExtentions[fileExtention]);
                www.Dispose();

                ModConsole.Print($"[MooseSounds] - #{i + 1} audio found: {results[i].name}");
            }
            return results;
        }
    }
}
