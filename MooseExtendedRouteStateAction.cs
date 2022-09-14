using System;
using HutongGames.PlayMaker;
using MSCLoader;

using UnityEngine;

namespace TommoJProductions.MooseSounds
{
    public class MooseExtendedRouteStateAction : FsmStateAction
    {
        // Written, 29.08.2022

        public bool forceExtendedRoute;
        internal FsmGameObject routeStartFsm;
        internal FsmGameObject routeEndFsm;
        internal MooseRoute extendedRouteToForce;
        private Moose moose;

        internal bool extendedRoute { get; private set; }
        internal int currentPoint { get; private set; }
        internal MooseRoute currentRoute { get; private set; }
        internal GameObject routeStart { get; private set; }
        internal GameObject routeEnd { get; private set; }

        public MooseExtendedRouteStateAction(Moose moose)
        {
            this.moose = moose;

            routeStartFsm = moose.movePlayMaker.FsmVariables.FindFsmGameObject("RouteStart");
            routeEndFsm = moose.movePlayMaker.FsmVariables.FindFsmGameObject("RouteEnd");
            routeStart = new GameObject("MooseRouteStart");
            routeEnd = new GameObject("MooseRouteEnd");

            FsmState randRoute = moose.movePlayMaker.GetState("Randomize route");
            randRoute.InsertAction(2, this);
        }

        public override void OnEnter()
        {
            // Written, 29.08.2022

            randomizeRoute();
            Finish();
        }

        private void randomizeRoute()
        {
            // Written, 29.08.2022

            float chance = UnityEngine.Random.value;
            bool extendedRouteChancePicked = chance < MooseSoundEffectsMod.instance.extendedRouteChance / 100;
            int randomIndex = -1;

            if (extendedRoute)
            {
                if (!setNextPoint())
                {
                    resetExtendedRoute();
                    getRandomGameRoute();
                }
            }
            else
            {
                if (extendedRouteChancePicked || forceExtendedRoute)
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
                            currentRoute = MooseSoundEffectsMod.instance.mooseRoutes.getRandom(out randomIndex);
                        }

                        extendedRoute = true;
                        setNextPoint();
                        moose.runState.onDestroy += onMooseDead;
                    }
                    else
                    {
                        getRandomGameRoute();
                        ModConsole.Print($"[MooseSounds] - No extended routes. (count 0).");
                    }
                }
                ModConsole.Print($"[MooseSounds] - DB - Moose{moose.index} {MooseSoundEffectsMod.instance.extendedRouteChance}% ({chance * 100}) {(extendedRouteChancePicked ? $"Extended Route {randomIndex}" : "Stock")}");
            }
        }
        private void getRandomGameRoute()
        {
            routeStartFsm.Value = MooseSoundEffectsMod.instance.gameRoutes.getRandom();
            routeEndFsm.Value = routeStartFsm.Value.getChildren().getRandom();
        }
        internal void resetExtendedRoute()
        {
            // Written, 05.09.2022

            extendedRoute = false;
            moose.runState.onDestroy -= onMooseDead;
            currentPoint = 0;
            currentRoute = null;
        }

        public bool setNextPoint()
        {
            if (currentPoint < currentRoute.points.Count - 1)
            {
                currentPoint++;

                routeStartFsm.Value = routeStart;
                routeEndFsm.Value = routeEnd;

                routeStart.transform.position = currentRoute.points[currentPoint - 1];
                routeEnd.transform.position = currentRoute.points[currentPoint];
                
                return true;
            }
            return false;
        }

        #region Event Handlers

        private void onMooseDead(MooseRunState obj)
        {
            resetExtendedRoute();
        }

        #endregion
    }
}
