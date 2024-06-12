using ExtraRecoilData.CustomRecoil;
using Gear;
using HarmonyLib;
using Player;

namespace ExtraRecoilData.Patches
{
    internal static class WeaponSetupPatches
    {
        [HarmonyPatch(typeof(BulletWeaponArchetype), nameof(BulletWeaponArchetype.SetOwner))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void SetupCallback(BulletWeaponArchetype __instance, PlayerAgent owner)
        {
            if (owner == null) return;

            CustomRecoilData? data = CustomRecoilManager.Current.GetCustomRecoilData(__instance.m_archetypeData.persistentID);
            if (data == null) return;

            if (__instance.m_weapon.gameObject.GetComponent<CustomRecoilComponent>() != null) return;

            CustomRecoilComponent cwc = __instance.m_weapon.gameObject.AddComponent<CustomRecoilComponent>();
            cwc.Data = data;
        }
    }
}
