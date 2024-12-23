﻿using HarmonyLib;
using System.Reflection;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(SaberClashChecker), nameof(SaberClashChecker.AreSabersClashing))]
    internal static class SaberClashCheckerAreSabersClashing
    {
        internal static bool disabled = false;

        static bool Prefix(ref bool __result)
        {
            if (disabled)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
