using ExtraRecoilData.Patches;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using ExtraRecoilData.CustomRecoil;
using ExtraRecoilData.Utils;
using ExtraRecoilData.JSON;

namespace ExtraRecoilData;

[BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.1.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public const string MODNAME = "ExtraRecoilData";

    public override void Load()
    {
        ERDLogger.Log("Loading " + MODNAME);

        Harmony harmonyInstance = new(MODNAME);
        harmonyInstance.PatchAll(typeof(WeaponSetupPatches));
        harmonyInstance.PatchAll(typeof(RecoilPatches));

        ClassInjector.RegisterTypeInIl2Cpp<CustomRecoilComponent>();

        ERDLogger.Log("Loaded " + MODNAME);
    }
}