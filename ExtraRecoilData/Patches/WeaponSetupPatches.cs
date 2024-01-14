using ExtraRecoilData.CustomRecoil;
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

            MinMaxValue recoilPatternPower = new() { Min = 0.7f, Max = 0.7f };

            List<float> firstPattern = new();
            for (int i = 0; i < 10; i++)
                firstPattern.Add(360f);

            List<float> pattern = new();
            pattern.Add(90f);

            CustomRecoilData crd = new()
            {
                RecoilScaleDecayDelay = 0.1f,
                RecoilScaleGrowth = .05f,
                RecoilScaleMax = 5f,
                RecoilPatternPower = recoilPatternPower,
                RecoilPattern = pattern,
                RecoilPatternFirst = firstPattern
            };

            crm.CRD = crd;
        }
    }
}
