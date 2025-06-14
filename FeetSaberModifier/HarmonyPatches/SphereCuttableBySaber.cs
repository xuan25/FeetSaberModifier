using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace FeetSaberModifier.HarmonyPatches
{
    [HarmonyPatch(typeof(SphereCuttableBySaber), nameof(SphereCuttableBySaber.Cut))]
    static class SphereCuttableBySaberCut
    {
        static bool Prefix(SphereCuttableBySaber __instance, Saber saber, SphereCollider ____collider)
        {
            if (__instance.canBeCut)
            {
                if (Config.feetSaber ||
                    saber.name == FeetSaberModifierController.saberFootLName ||
                    saber.name == FeetSaberModifierController.saberFootRName)
                {
                    //Logger.log.Debug($"distance={Vector3.Distance(__instance.transform.position, saber.transform.position)}, instance={__instance.transform.position}, saber={saber.transform.position}, {saber.saberBladeBottomPos}, {saber.saberBladeTopPos}, cutPoint={cutPoint}");
                    return (Vector3.Distance(__instance.transform.position, saber.transform.position) < ____collider.radius);
                }
            }
            return true;
        }
    }
}
