using ExtraRecoilData.CustomRecoil;
using ExtraRecoilData.Utils;
using Gear;
using HarmonyLib;

namespace ExtraRecoilData.Patches
{
    internal static class WeaponSetupPatches
    {
        [HarmonyPatch(typeof(BulletWeapon), nameof(BulletWeapon.SetupArchetype))]
        [HarmonyWrapSafe]
        [HarmonyPostfix]
        private static void AddRecoilManager(BulletWeapon __instance)
        {
            CustomRecoilData? data = CustomRecoilManager.Current.GetCustomRecoilData(__instance.ArchetypeID);
            if (data == null) return;

            CustomRecoilComponent crm = __instance.gameObject.AddComponent<CustomRecoilComponent>();
            crm.Data = data;
        }
    }
}
