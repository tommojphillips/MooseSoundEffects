﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using static UnityEngine.GUILayout;
using static TommoJProductions.MooseSounds.MooseSoundEffectsMod;
using static TommoJProductions.MooseSounds.Extentions;
using MSCLoader;

namespace TommoJProductions.MooseSounds
{
    public class MooseDevTools : MonoBehaviour
    {
        // Written, 29.08.2022

        public MooseSoundEffectsMod mod;

        private bool debugGUIOpen = false;
        private bool routeCreationOpen = false;

        private int height = 500;
        private int width = 500;
        private int top = 100;

        private bool editingRoute = false;
        private MooseRoute route;
        private string contents;
        private readonly List<GameObject> routePoints = new List<GameObject>();

        void OnGUI()
        {
            // Written, 28.08.2022

            if (debugGUIOpen)
            {
                using (new AreaScope(new Rect(Screen.width / 2 - width / 2, top, width, height)))
                {
                    Label($"Alive moose: {mod.allAliveMoose.Count}\nDead moose: {mod.allDeadMoose.Count}");

                    if (Button(mod.animalsMoose.activeInHierarchy ? "Deactivate Mooses" : "Activate Mooses" + (editingRoute ? " <color=red>*</color>" : "")))
                    {
                        mod.animalsMoose.SetActive(!mod.animalsMoose.activeInHierarchy);
                    }
                    if (Button("Spawn Alive Moose"))
                    {
                        mod.spawnAliveMoose();
                    }
                    if (Button("Spawn Dead Moose"))
                    {
                        mod.spawnDeadMoose(new MooseSaveData() { position = mod.player.position });
                    }
                    if (mod.allDeadMoose.Count > 0)
                    {
                        if (Button("Destroy all dead moose"))
                        {
                            mod.destroyDeadMoose();
                        }
                    }
                    Space(10f);
                    if (Button("Extended Route Creation"))
                    {
                        routeCreationOpen = !routeCreationOpen;
                        closeEditingRouteCheck(routeCreationOpen);
                    }
                    if (routeCreationOpen)
                    {
                        drawRouteCreation();
                    }
                }
            }
        }
        void Update()
        {
            // Written, 29.08.2022

            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                debugGUIOpen = !debugGUIOpen;
                closeEditingRouteCheck(debugGUIOpen);
            }
        }

        void drawRouteCreation()
        {
            // Written, 30.08.2022

            using (new HorizontalScope())
            {
                drawRouteList();

                using (new VerticalScope("box"))
                {
                    if (editingRoute)
                    {
                        drawEditRoute();
                    }
                    else
                    {
                        if (Button("New route"))
                        {
                            editingRoute = true;
                            route = new MooseRoute();
                            deleteRoutePoints();
                        }
                    }
                }
            }
        }
        private void drawEditRoute()
        {
            using (new HorizontalScope())
            {
                if (Button("Add point"))
                {
                    createRoutePoint(mod.player.position);
                }
                if (routePoints.Count > 0)
                {
                    if (Button("Destroy last point"))
                    {

                        UnityEngine.Object.Destroy(routePoints[routePoints.Count - 1]);
                        routePoints.RemoveAt(routePoints.Count - 1);
                    }
                }
            }
            if (!mod.mooseRoutes.Contains(route))
            {
                if (Button("Add route to list"))
                {
                    mod.mooseRoutes.Add(route);
                    updateRoutePoints();
                }
            }
            else
            {
                if (Button("Save route" + (checkRouteHasChanged() ? " <color=orange>*</color>" : "")))
                {
                    updateRoutePoints();
                }
                if (Button(route.routeInUse && route.mooseOnRoute == mod.allAliveMoose[0].mooseGo ? "Next Point" : "Force Moose To run route"))
                {
                    if (mod.allAliveMoose.Count > 0)
                    {
                        MooseExtendedRouteStateAction ext = mod.allAliveMoose[0].extendedRouteState;
                        ext.forceExtendedRoute = true;
                        ext.extendedRouteToForce = route;

                        mod.allAliveMoose[0].movePlayMaker.enabled = false;
                        mod.allAliveMoose[0].movePlayMaker.enabled = true;
                    }
                    else
                    {
                        ModConsole.Warning("no alive moose to run extended route...");
                    }
                }
                if (Button("write route to a file"))
                {
                    contents += createPointsListCode(route);
                    Extentions.writeToFile(ModLoader.GetModSettingsFolder(mod) + $"/route.txt", createPointsListCode(route));
                }
            }
            Space(10);
            if (Button("Close route"))
            {
                closeEditingRoute();
            }
            if (!mod.animalsMoose.activeInHierarchy)
            {
                Label("Note! moose arent enabled, (moose are only active during the night)");
            }
        }
        private void drawRouteList()
        {
            // Written, 05.09.2022

            using (new VerticalScope("box"))
            {
                for (int i = 0; i < mod.mooseRoutes.Count; i++)
                {
                    using (new HorizontalScope())
                    {
                        Label("Route" + i + (route == mod.mooseRoutes[i] ? "(Editing)" : ""));
                        if (!editingRoute)
                        {
                            if (Button("Edit Route"))
                            {
                                route = mod.mooseRoutes[i];
                                loadRoutePoints();
                                editingRoute = true;
                            }
                        }
                    }
                }
            }
        }
        private void updateRoutePoints()
        {
            // Written, 30.08.2022

            route.points.Clear();
            for (int i = 0; i < routePoints.Count; i++)
            {
                route.points.Add(routePoints[i].transform.position);
            }
        }
        private void deleteRoutePoints()
        {
            // Written, 30.08.2022

            for (int i = 0; i < routePoints.Count; i++)
            {
                UnityEngine.Object.Destroy(routePoints[i]);
            }
            routePoints.Clear();
        }
        private void loadRoutePoints()
        {
            // Written, 30.08.2022

            routePoints.Clear();
            for (int i = 0; i < route.points.Count; i++)
            {
                createRoutePoint(route.points[i]);
            }
        }
        private void createRoutePoint(Vector3 position)
        {
            // Written, 29.08.2022

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.25f;
            go.AddComponent<Rigidbody>();
            go.name = "RoutePoint" + routePoints.Count + "(xxxxx)";
            go.MakePickable();
            go.tag = "RAGDOLL";
            routePoints.Add(go);
        }
        private string createPointsListCode(MooseRoute route)
        {
            // Written, 05.09.2022

            string code = "\r\n            mooseRoutes.Add(new MooseRoute()\r\n            {\r\n                points = new List<Vector3>()\r\n                {\r\n                    {0}\r\n                }\r\n            });";
            string points= "";
            for (int i = 0; i < route.points.Count; i++)
            {
                points += $"new Vector3({route.points[i].x}f, {route.points[i].y }f, {route.points[i].z }f),\n";
            }
            points = points.Remove(points.Length - 2);

            return code.Replace("{0}", points);
        }
        private bool checkRouteHasChanged()
        {
            // Written, 05.09.2022

            if (routePoints.Count != route.points.Count)
                return true;

            for (int i = 0; i < routePoints.Count; i++)
            {                
                if (greaterThanDistanceSqr(Vector3.SqrMagnitude(route.points[i] - routePoints[i].transform.position), 1f))
                {
                    return true;
                }
            }
            return false;
        }
        private void closeEditingRoute()
        {
            // Written, 05.09.2022

            editingRoute = false;
            route = null;
            deleteRoutePoints();
        }
        private void closeEditingRouteCheck(bool keepOpen)
        {
            // Written, 05.09.2022

            if (!keepOpen && editingRoute)
            {
                closeEditingRoute();
            }
        }
    }
}