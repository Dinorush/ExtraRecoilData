using ExtraRecoilData.Patches;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Diagnostics;
using Il2CppInterop.Runtime.Injection;
using ExtraRecoilData.CustomRecoil;

namespace ExtraRecoilData;

[BepInPlugin("Dinorush." + MODNAME, MODNAME, "1.0.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
//[BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
//[BepInDependency("GTFO.InjectLib", BepInDependency.DependencyFlags.HardDependency)]
internal sealed class Loader : BasePlugin
{
    public const string MODNAME = "ExtraRecoilData";

#if DEBUG
    private static ManualLogSource Logger;
#endif

    [Conditional("DEBUG")]
    public static void DebugLog(object data)
    {
#if DEBUG
        Logger.LogMessage(data);
#endif
    }

    public override void Load()
    {
#if DEBUG
        Logger = Log;
#endif
        Log.LogMessage("Loading " + MODNAME);

        Harmony harmonyInstance = new Harmony(MODNAME);
        harmonyInstance.PatchAll(typeof(WeaponSetupPatches));
        harmonyInstance.PatchAll(typeof(RecoilPatches));

        ClassInjector.RegisterTypeInIl2Cpp<CustomRecoilManager>();

        Log.LogMessage("Loaded " + MODNAME);
    }
}