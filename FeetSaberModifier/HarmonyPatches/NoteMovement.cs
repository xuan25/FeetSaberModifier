using HarmonyLib;
using UnityEngine;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteMovement), nameof(NoteMovement.Init))]
    static class NoteMovementInit
    {
        [HarmonyPriority(Priority.VeryLow)]
        static void Prefix(ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos)
        {
            if (Config.feetSaber)
            {
                moveStartPos.y = Config.feetNotesY;
                moveEndPos.y = Config.feetNotesY;
                jumpEndPos.y = Config.feetNotesY;
            }
        }
    }
}
