using ExtraRecoilData.CustomRecoil;
using ExtraRecoilData.Utils;
using GameData;
using Gear;
using HarmonyLib;
using Il2CppSystem.Data;
using UnityEngine;

namespace ExtraRecoilData.Patches
{
    internal static class RecoilPatches
    {
        static Weapon? cachedWeapon;
        static CustomRecoilComponent? cachedManager;

        private static CustomRecoilComponent? GetCustomRecoilManager(Weapon? newWeapon = null)
        {
            if (newWeapon == null)
                return cachedManager;

            if (newWeapon != cachedWeapon)
            {
                cachedWeapon = newWeapon;
                cachedManager = newWeapon.GetComponent<CustomRecoilComponent>();
            }

            return cachedManager;
        }

        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.OnWield))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void UpdateCurrentWeapon(BulletWeapon __instance)
        {
            GetCustomRecoilManager(__instance);
        }

        [HarmonyPatch(typeof(BulletWeaponArchetype), nameof(BulletWeaponArchetype.PostFireCheck))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void UpdateCustomRecoil(BulletWeaponArchetype __instance)
        {
            CustomRecoilComponent? crm = GetCustomRecoilManager(__instance.m_weapon);
            if (crm == null) return;

            crm.FireTriggered(System.Math.Max(0, System.Math.Max(__instance.m_nextShotTimer, __instance.m_nextBurstTimer) - Clock.Time));
        }

        [HarmonyPatch(typeof(FPS_RecoilSystem), nameof(FPS_RecoilSystem.ApplyRecoil))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void CustomApplyRecoilAfter(FPS_RecoilSystem __instance, bool resetSimilarity, RecoilDataBlock recoilData)
        {
            if (cachedManager == null) return;

            Vector2 newDir = cachedManager.GetModifiedRecoil(__instance.recoilDir);

            // This logic is based on the leaked mono build. In testing, it is still accurate and works correctly.
            // recoilDir has already been subtracted at this point, so we need to add it back and subtract the new recoilDir.
            Vector2 newForce = __instance.currentRecoilForce;
            newForce.x += (__instance.recoilDir.x - newDir.x) * (1f - recoilData.worldToViewSpaceBlendVertical);
            newForce.y += (__instance.recoilDir.y - newDir.y) * (1f - recoilData.worldToViewSpaceBlendHorizontal);

            Vector2 newForceVP = __instance.currentRecoilForceVP;
            newForceVP.x += (__instance.recoilDir.x - newDir.x) * recoilData.worldToViewSpaceBlendVertical;
            newForceVP.y += (__instance.recoilDir.y - newDir.y) * recoilData.worldToViewSpaceBlendHorizontal;

            __instance.recoilDir = newDir;
            __instance.currentRecoilForce = newForce;
            __instance.currentRecoilForceVP = newForceVP;
        }
    }
}
