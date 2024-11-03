using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteBasicCutInfoHelper), nameof(NoteBasicCutInfoHelper.GetBasicCutInfo))]
    static class NoteBasicCutInfoGetBasicCutInfo
    {
        static void Prefix(ref float saberBladeSpeed)
        {
            if (Config.fourSabers || Config.feetSaber)
            {
                saberBladeSpeed = 3.0f;
            }
        }
    }
}
