using System;
using HutongGames.PlayMaker;
using MSCLoader;

namespace TommoJProductions.MooseSounds
{
    public class MooseExtendedRouteStateAction : FsmStateAction
    {
        // Written, 29.08.2022

        private bool extendedRoute;
        private MooseRoute currentRoute;
        public FsmGameObject routeStart;
        public FsmGameObject routeEnd;
        private MooseRunState currentMooseRunState;
        public bool forceExtendedRoute;
        internal MooseRoute extendedRouteToForce;

        private void onMooseDead(MooseRunState obj)
        {
            resetExtendedRoute();
            ModConsole.Print("[MooseRoute] Moose died while on extended route.");
        }

        public override void OnEnter()
        {
            // Written, 29.08.2022

            onRandomizeRoute();
            Finish();
        }

        void onRandomizeRoute()
        {
            // Written, 29.08.2022

            if (extendedRoute)
            {
                if (currentRoute.setNextPoint())
                {
                    routeStart.Value = currentRoute.routeStart;
                    routeEnd.Value = currentRoute.routeEnd;
                    ModConsole.Print($"[MooseRoute] Next Point {currentRoute.currentPoint}/{currentRoute.points.Count}.");
                }
                else
                {
                    ModConsole.Print($"[MooseRoute] Extended route finished. {currentRoute.currentPoint}/{currentRoute.points.Count}");

                    resetExtendedRoute();
                    getRandomGameRoute();
                }
            }
            else if (MooseSoundEffectsMod.instance.extendedRouteGo == routeStart.Value || forceExtendedRoute)
            {
                if (MooseSoundEffectsMod.instance.mooseRoutes.Count > 0)
                {
                    if (forceExtendedRoute)
                    {
                        currentRoute = extendedRouteToForce;
                        forceExtendedRoute = false;
                    }
                    else
                    {
                        currentRoute = MooseSoundEffectsMod.instance.mooseRoutes.getRandom();
                    }

                    if (!currentRoute.routeInUse)
                    {
                        extendedRoute = true;
                        currentRoute.routeInUse = true;
                        routeStart.Value = currentRoute.routeStart;
                        routeEnd.Value = currentRoute.routeEnd;
                        currentRoute.setNextPoint();
                        currentMooseRunState = Owner.GetComponent<MooseRunState>();
                        currentMooseRunState.onDestroy += onMooseDead;
                        currentRoute.mooseOnRoute = currentMooseRunState.moose.mooseGo;
                    }
                    else
                    {
                        getRandomGameRoute();
                        ModConsole.Print("[MooseRoute] Extended route is in use.. Randomizing Route");
                    }
                }
                else
                {
                    getRandomGameRoute();
                    ModConsole.Print("[MooseRoute] No Extended route set up.. using DEFAULT Randomizing Route logic");
                }
            }
        }

        private void resetExtendedRoute()
        {
            // Written, 05.09.2022

            extendedRoute = false;
            currentMooseRunState.onDestroy -= onMooseDead;
            currentRoute.reset();
            currentRoute.routeInUse = false;
            currentRoute.mooseOnRoute = null;
            currentRoute = null;
        }

        void getRandomGameRoute()
        {
            routeStart.Value = MooseSoundEffectsMod.instance.gameRoutes.getRandom();
            routeEnd.Value = routeStart.Value.getChildren().getRandom();
        }
    }
}
