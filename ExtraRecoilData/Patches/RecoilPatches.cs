using ExtraRecoilData.CustomRecoil;
using System;
using GameData;
using Gear;
using HarmonyLib;
using UnityEngine;

namespace ExtraRecoilData.Patches
{
    internal static class RecoilPatches
    {
        static BulletWeapon? cachedWeapon;
        static CustomRecoilComponent? cachedManager;

        internal static void RefreshCache(BulletWeapon weapon)
        {
            if (cachedWeapon != null && weapon.GetInstanceID() == cachedWeapon.GetInstanceID())
                cachedManager = cachedWeapon.GetComponent<CustomRecoilComponent>();
        }

        private static CustomRecoilComponent? GetCustomRecoilManager(BulletWeapon? newWeapon = null)
        {
            if (newWeapon == null)
                return cachedManager;

            if (newWeapon.Owner?.IsLocallyOwned != true)
                return null;

            if (cachedWeapon == null || newWeapon.GetInstanceID() != cachedWeapon.GetInstanceID())
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

        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.OnUnWield))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void RemoveCurrentWeapon(BulletWeapon __instance)
        {
            if (__instance.Owner?.IsLocallyOwned == false) return;

            cachedWeapon = null;
            cachedManager = null;
        }

        [HarmonyPatch(typeof(BulletWeaponArchetype), nameof(BulletWeaponArchetype.PostFireCheck))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void UpdateCustomRecoil(BulletWeaponArchetype __instance)
        {
            CustomRecoilComponent? crm = GetCustomRecoilManager(__instance.m_weapon);
            if (crm == null) return;

            crm.FireTriggered(Math.Max(0, Math.Max(__instance.m_nextShotTimer, __instance.m_nextBurstTimer)));
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
