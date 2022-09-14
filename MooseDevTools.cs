using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MSCLoader;

using static UnityEngine.GUILayout;
using static TommoJProductions.MooseSounds.Extentions;

namespace TommoJProductions.MooseSounds
{

    public class MooseDevTools : MonoBehaviour
    {
        // Written, 29.08.2022

        public MooseSoundEffectsMod mod;

        private bool debugGUIOpen = false;
        private bool routeCreationOpen = false;

        private int height = 550;
        private int width = 750;
        private int top = 25;

        private bool editingRoute = false;
        private MooseRoute route;
        private Moose lastForcedMoose;
        private Vector2 routeListScroll;
        private ScrollViewScope routeListScrollView;
        private readonly List<GameObject> routePoints = new List<GameObject>();

        void Awake() 
        {
            // Written, 09.09.2022

            mod = MooseSoundEffectsMod.instance;
        }

        void OnGUI()
        {
            // Written, 28.08.2022

            if (debugGUIOpen)
            {
                using (new AreaScope(new Rect(Screen.width / 2 - width / 2, top, width, height)))
                {
                    Label($"Moose Mod DEV\nAlive moose: {mod.allAliveMoose.Count}\nDead moose: {mod.allDeadMoose.Count}");
                    Label($"{mod.extendedRouteChance}% chance ({mod.extendedRouteChance / 100})");
                    Space(10);
                    if (Button(mod.animalsMoose.activeInHierarchy ? "Disable Mooses" : "Enable Mooses" + (editingRoute ? " <color=red>*</color>" : "")))
                    {
                        mod.animalsMoose.SetActive(!mod.animalsMoose.activeInHierarchy);
                    }
                    using (new HorizontalScope())
                    {
                        if (Button("Spawn Alive Moose"))
                        {
                            mod.spawnAliveMoose();
                        }
                        if (Button("Spawn Dead Moose"))
                        {
                            mod.spawnDeadMoose(new DeadMooseSaveData() { position = mod.player.position });
                        }
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
                        if (Button("Save Routes"))
                        {
                            mod.mooseRoutesSaveData.loadedMooseRoutes = mod.mooseRoutes.Where(r => !mod.defaultMooseRoutes.Contains(r)).ToList();
                            SaveLoad.SerializeSaveFile(mod, mod.mooseRoutesSaveData, MooseSoundEffectsMod.ROUTE_SAVE_FILE_NAME);
                        }
                        if (Button("write all routes to a file"))
                        {
                            string contents = "";
                            MooseRoute[] routes = mod.mooseRoutes.Where(r => !mod.defaultMooseRoutes.Contains(r)).ToArray();
                            for (int i = 0; i < routes.Length; i++)
                            {
                                contents += createPointsListCode(routes[i]);
                            }
                            Extentions.writeToFile(ModLoader.GetModSettingsFolder(mod) + $"/routesCode.txt", contents);
                        }
                    }
                }
            }
        }
        void Update()
        {
            // Written, 29.08.2022

            if (mod.debugGuiKeybind.GetKeybindDown())
            {
                debugGUIOpen = !debugGUIOpen;
                closeEditingRouteCheck(debugGUIOpen);
            }
        }

        void drawRouteCreation()
        {
            // Written, 30.08.2022

            using (new HorizontalScope("box"))
            {
                drawRouteList();
                if (editingRoute)
                {
                    using (new VerticalScope())
                    {

                        drawEditRoute();
                    }
                }
                else
                {
                    if (Button("New route"))
                    {
                        editingRoute = true;
                        route = new MooseRoute();
                    }
                }
            }
        }
        private void drawEditRoute()
        {

            Label(route.points.Count + " Points");
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
            if (routePoints.Count > 0)
            {
                if (Button("Teleport to route"))
                {
                    mod.player.teleport(routePoints[0].transform.position);
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
                bool routeChanged = checkRouteHasChanged();

                using (new HorizontalScope())
                {
                    if (Button("Force Moose To run route"))
                    {
                        if (mod.allAliveMoose.Count > 0)
                        {
                            Moose[] moose = mod.allAliveMoose.Where(m => !m.extendedRouteState.extendedRoute).ToArray();
                            if (moose.Length > 0)
                            {
                                lastForcedMoose = moose[0];
                                lastForcedMoose.extendedRouteState.forceExtendedRoute = true;
                                lastForcedMoose.extendedRouteState.extendedRouteToForce = route;
                                forceMooseToChooseRoute();
                            }
                            else
                            {
                                ModConsole.Warning("All alive moose are running an extended route...");
                            }
                        }
                        else
                        {
                            ModConsole.Warning("no alive moose to run extended route...");
                        }
                    }
                    if (lastForcedMoose.extendedRouteState?.currentRoute == route)
                    {
                        if (Button($"Next point (Moose{lastForcedMoose.index})"))
                        {
                            forceMooseToChooseRoute();
                        }
                        Label($"{lastForcedMoose.extendedRouteState.currentPoint}/{route.points.Count}");
                    }
                }
                if (Button("Save route" + (routeChanged ? " <color=orange>*</color>" : "")))
                {
                    updateRoutePoints();
                }                
                if (!routeChanged)
                {
                    if (Button("Remove route"))
                    {
                        mod.mooseRoutes.Remove(route);
                    }
                    if (Button("write route to a file"))
                    {
                        Extentions.writeToFile(ModLoader.GetModSettingsFolder(mod) + $"/route{mod.mooseRoutes.IndexOf(route)}.txt", createPointsListCode(route));
                    }
                }
            }
            Space(10);
            if (Button("Close route"))
            {
                closeEditingRoute();
            }
            if (!mod.animalsMoose.activeInHierarchy)
            {
                Label("Note! moose arent enabled");
            }
        }

        private void forceMooseToChooseRoute()
        {
            lastForcedMoose.movePlayMaker.enabled = false;
            lastForcedMoose.movePlayMaker.enabled = true;
        }

        private void drawRouteList()
        {
            // Written, 05.09.2022

            using (new VerticalScope("box", Width(width / 3)))
            using (routeListScrollView = new ScrollViewScope(routeListScroll, false, false))
            {
                routeListScrollView.handleScrollWheel = true;
                routeListScroll = routeListScrollView.scrollPosition;
                Label("Route Save List");
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
            routePoints.Add(go);
        }
        private string createPointsListCode(MooseRoute route)
        {
            // Written, 05.09.2022

            string code = "\r\n            defaultMooseRoutes.Add(new MooseRoute()\r\n            {\r\n                points = new List<Vector3Info>()\r\n                {\r\n{0}\r\n                }\r\n            });";
            string points= "";
            for (int i = 0; i < route.points.Count; i++)
            {
                points += $"                    new Vector3Info({route.points[i].x}f, {route.points[i].y }f, {route.points[i].z }f),\n";
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
