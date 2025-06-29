﻿using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteJump), nameof(NoteJump.Init))]
    static class NoteJumpInit
    {
        [HarmonyPriority(Priority.VeryLow)]
        static void Postfix(
            ref Quaternion ____middleRotation,
            ref Quaternion ____endRotation,
            Vector3 moveEndOffset)
        {
            if (Config.feetSaber || (Config.fourSabers && moveEndOffset.y == Config.feetNotesY))
            {
                ____middleRotation = Quaternion.identity;
                ____endRotation = Quaternion.identity;
            }
        }
    }

    [HarmonyPatch(typeof(NoteJump), nameof(NoteJump.ManualUpdate))]
    static class NoteJumpManualUpdate
    {
        [HarmonyPriority(Priority.VeryLow)]
        static void Postfix(
            NoteJump __instance,
            ref Vector3 __result,
            Vector3 ____localPosition,
            Vector3 ____startPos,
            Quaternion ____worldRotation,
            Quaternion ____middleRotation)
        {
            if (Config.feetSaber)
            {
                ____localPosition.y = Config.feetNotesY;
                __result = ____worldRotation * ____localPosition;
                __instance.transform.position = __result;

                Transform noteCube = __instance.transform.Find("NoteCube");
                if (noteCube != null)
                {
                    foreach (Transform noteCubeChild in noteCube)
                    {
                        if (noteCubeChild.name.StartsWith("customNote"))
                        {
                            foreach (Transform noteCubeGrandChild in noteCubeChild)
                            {
                                if (noteCubeGrandChild.name.StartsWith("Feet"))
                                {
                                    noteCubeGrandChild.rotation = ____middleRotation;
                                }
                            }
                        }
                    }
                }
            }

            if (Config.topNotesToFeet || Config.middleNotesToFeet || Config.bottomNotesToFeet)
            {
                if (____startPos.y == Config.feetNotesY)
                {
                    ____localPosition.y = Config.feetNotesY;
                    __result = ____worldRotation * ____localPosition;
                    __instance.transform.position = __result;
                }
            }
        }
    }
}
