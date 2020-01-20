﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dissonance;
using Mirror;
using UnityEngine;

namespace EXILED.Patches
{
    [HarmonyPatch(typeof(Scp914.Scp914Machine), "ProcessItems")]
    public class _914MachineOverride
    {
        public static bool Prefix(Scp914.Scp914Machine __instance)
        {
            if (!NetworkServer.active)
                return true;
            Collider[] colliderArray = Physics.OverlapBox(__instance.intake.position, __instance.inputSize / 2f);
            __instance.players.Clear();
            __instance.items.Clear();
            foreach (Collider collider in colliderArray)
            {
                CharacterClassManager component1 = collider.GetComponent<CharacterClassManager>();
                if (component1 != null)
                {
                    __instance.players.Add(component1);
                }
                else
                {
                    Pickup component2 = collider.GetComponent<Pickup>();
                    if (component2 != null)
                        __instance.items.Add(component2);
                }
            }

            List<Pickup> pickups = __instance.items;
            bool allowUpgrade = true;
            
            Events.InvokeSCP914Upgrade(__instance, __instance.players, ref pickups, __instance.knobState, ref allowUpgrade);
            
            __instance.items.Clear();
            foreach (Pickup p in pickups)
                __instance.items.Add(p);
            try
            {
                __instance.MoveObjects(__instance.items, __instance.players);
            }
            finally
            {
                if (allowUpgrade)
                    __instance.UpgradeObjects(__instance.items, __instance.players);
            }

            return false;
        }
    }
}
