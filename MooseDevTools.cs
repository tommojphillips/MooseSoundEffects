using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using static UnityEngine.GUILayout;

namespace TommoJProductions.MooseSounds
{
    public class MooseDevTools : MonoBehaviour
    {
        // Written, 29.08.2022

        public MooseSoundEffectsMod mod;
        private bool debugGUIOpen = false;

        int height = 500;
        int width = 500;
        int top = 100;

        void OnGUI()
        {
            if (debugGUIOpen)
            {
                using (new AreaScope(new Rect(Screen.width / 2 - width / 2, top, width, height)))
                {
                    Label($"Alive moose: {mod.allAliveMoose.Count}\nDead moose: {mod.allDeadMoose.Count}");

                    if (Button(mod.animalsMoose.activeInHierarchy ? "Deactivate Mooses" : "Activate Mooses"))
                    {
                        mod.animalsMoose.SetActive(!mod.animalsMoose.activeInHierarchy);
                    }
                    if (Button("Spawn Alive Moose"))
                    {
                        mod.spawnAliveMoose();
                    }
                    if (Button("Spawn Dead Moose"))
                    {
                        mod.spawnDeadMoose(new Moose() { position = MooseSoundEffectsMod.player.position });
                    }
                    if (Button("Destroy all dead moose"))
                    {
                        mod.destroyDeadMoose();
                    }
                }
            }
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                debugGUIOpen = !debugGUIOpen;
            }
        }
    }
}
