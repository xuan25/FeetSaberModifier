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
	//[HarmonyPatch(typeof(NoteController), nameof(NoteController.Init))]
	[HarmonyPatch(typeof(NoteController), "Init")]
    static class NoteControllerInit
	{
		static void Prefix(ref NoteData noteData, ref NoteSpawnData noteSpawnData, Transform ____noteTransform, out NoteLineLayer __state)
		{
			NoteLineLayer noteLineLayer;
			if ((int)noteData.noteLineLayer <= 2)
			{
				noteLineLayer = noteData.noteLineLayer;
			}
			else if ((int)noteData.noteLineLayer < 1667)
			{
				noteLineLayer = NoteLineLayer.Base;
			}
			else if ((int)noteData.noteLineLayer < 2334)
			{
				noteLineLayer = NoteLineLayer.Upper;
			}
			else
			{
				noteLineLayer = NoteLineLayer.Top;
			}
			__state = noteLineLayer;

            if (Config.feetSaber)
			{
				if (noteData.cutDirection != NoteCutDirection.None)
				{
					noteData.SetNonPublicProperty("cutDirection", NoteCutDirection.Any);
                    ____noteTransform.localScale = new Vector3(1f, 0.5f, 1f);
                }
			}

			if ((Config.topNotesToFeet && (noteLineLayer == NoteLineLayer.Top)) ||
				(Config.middleNotesToFeet && (noteLineLayer == NoteLineLayer.Upper)) ||
				(Config.bottomNotesToFeet && (noteLineLayer == NoteLineLayer.Base)))
			{
				noteData.SetNonPublicProperty("cutDirection", NoteCutDirection.Any);
				____noteTransform.localScale = new Vector3(1f, 0.5f, 1f);
			}
			else
			{
				____noteTransform.localScale = Vector3.one;
			}

			if (Config.topNotesToFeet && (noteLineLayer == NoteLineLayer.Top))
			{
				noteSpawnData = new NoteSpawnData(
					new Vector3(noteSpawnData.moveStartOffset.x, Config.feetNotesY, noteSpawnData.moveStartOffset.z),
					new Vector3(noteSpawnData.moveEndOffset.x, Config.feetNotesY, noteSpawnData.moveEndOffset.z),
					new Vector3(noteSpawnData.jumpEndOffset.x, Config.feetNotesY, noteSpawnData.jumpEndOffset.z),
					noteSpawnData.gravityBase
				);
			}
			if (Config.middleNotesToFeet && (noteLineLayer == NoteLineLayer.Upper))
			{
                noteSpawnData = new NoteSpawnData(
                    new Vector3(noteSpawnData.moveStartOffset.x, Config.feetNotesY, noteSpawnData.moveStartOffset.z),
                    new Vector3(noteSpawnData.moveEndOffset.x, Config.feetNotesY, noteSpawnData.moveEndOffset.z),
                    new Vector3(noteSpawnData.jumpEndOffset.x, Config.feetNotesY, noteSpawnData.jumpEndOffset.z),
                    noteSpawnData.gravityBase
                );
            }
			if (Config.bottomNotesToFeet && (noteLineLayer == NoteLineLayer.Base))
			{
                noteSpawnData = new NoteSpawnData(
                    new Vector3(noteSpawnData.moveStartOffset.x, Config.feetNotesY, noteSpawnData.moveStartOffset.z),
                    new Vector3(noteSpawnData.moveEndOffset.x, Config.feetNotesY, noteSpawnData.moveEndOffset.z),
                    new Vector3(noteSpawnData.jumpEndOffset.x, Config.feetNotesY, noteSpawnData.jumpEndOffset.z),
                    noteSpawnData.gravityBase
                );
            }
		}

		static void Postfix(Transform ____noteTransform, NoteLineLayer __state)
		{
			NoteLineLayer noteLineLayer = __state;

            if ((Config.topNotesToFeet && (noteLineLayer == NoteLineLayer.Top)) ||
                (Config.middleNotesToFeet && (noteLineLayer == NoteLineLayer.Upper)) ||
                (Config.bottomNotesToFeet && (noteLineLayer == NoteLineLayer.Base)))
            {
                ____noteTransform.localScale = new Vector3(____noteTransform.localScale.x, 0.5f, ____noteTransform.localScale.z);
            }

			// if (Config.feetSaber)
			// {
			// 	____noteTransform.localScale = new Vector3(____noteTransform.localScale.x, 0.5f, ____noteTransform.localScale.z);
			// }
		}
	}
}
