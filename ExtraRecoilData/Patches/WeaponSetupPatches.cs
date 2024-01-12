using GameData;
using Gear;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ExtraRecoilData.Patches
{
    internal static class WeaponSetupPatches
    {
        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.SetupArchetype))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void AddRecoilManager(BulletWeapon __instance)
        {
            Loader.DebugLog("Attached to weapon " + __instance.ToString() + " | " + __instance.gameObject);
            CustomRecoilManager crm = __instance.gameObject.AddComponent<CustomRecoilManager>();
            SetupRecoilManager(crm);
        }

        private static void SetupRecoilManager(CustomRecoilManager crm)
        {
            // Should pull from custom datablock. Temporarily hardcoding.

            crm.recoilScaleDecayDelay = 0.1f;
            crm.recoilScaleGrowth = .05f;
            crm.recoilScaleMax = 1f;

            crm.recoilPatternPower.Min = 0.7f;
            crm.recoilPatternPower.Max = 0.7f;

            List<float> firstPattern = new();
            for (int i = 0; i < 10; i++)
                firstPattern.Add(360f);

            List<float> pattern = new();
            pattern.Add(90f);

            crm.SetRecoilPatterns(pattern, firstPattern);
        }
    }
}
