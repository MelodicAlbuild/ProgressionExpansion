using System.IO;
using HarmonyLib;
using UnityEngine;

public class ProgressionExpansion : GameMod {
    public override void Load() {
        base.Load();

        var log     = new StringWriter();
        var modName = nameof(ProgressionExpansion);

        log.Write($"{modName} loading.");

        var harmony = new Harmony(GUID.Create().ToString());
        harmony.PatchAll(GetType().Assembly);

        var i = 0;
        foreach (var patchedMethod in harmony.GetPatchedMethods()) {
            log.Write($"\r\nPatched: {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            i++;
        }
        log.Write($"\r\nPatched {i} methods.");
        log.Write($"\r\n{modName} loaded.");
        Debug.Log(log.ToString());
    }
}