using ExtraRecoilData.CustomRecoil;
using ExtraRecoilData.Patches;
using Gear;

namespace ExtraRecoilData.API
{
    public static class ChangeAPI
    {
        public static void ChangeERDComponent(uint archetypeID, BulletWeapon weapon)
        {
            // ExtraRecoilData compatibility
            CustomRecoilComponent? recoilComp = weapon.GetComponent<CustomRecoilComponent>();
            CustomRecoilData? newData = CustomRecoilManager.Current.GetCustomRecoilData(archetypeID);
            if (recoilComp != null)
                recoilComp.Data = newData ?? new CustomRecoilData();
            else if (newData != null)
            {
                recoilComp = weapon.gameObject.AddComponent<CustomRecoilComponent>();
                recoilComp.Data = newData;
            }
            RecoilPatches.RefreshCache(weapon);
        }
    }
}
