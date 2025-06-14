using HarmonyLib;
using UnityEngine;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteMovement), nameof(NoteMovement.Init))]
    static class NoteMovementInit
    {
        [HarmonyPriority(Priority.VeryLow)]
        static void Prefix(ref NoteSpawnData noteSpawnData)
        {
            if (Config.feetSaber)
            {
                noteSpawnData = new NoteSpawnData(
                    new Vector3(noteSpawnData.moveStartOffset.x, Config.feetNotesY, noteSpawnData.moveStartOffset.z),
                    new Vector3(noteSpawnData.moveEndOffset.x, Config.feetNotesY, noteSpawnData.moveEndOffset.z),
                    new Vector3(noteSpawnData.jumpEndOffset.x, Config.feetNotesY, noteSpawnData.jumpEndOffset.z),
                    noteSpawnData.gravityBase
                );
            }
        }
    }
}
