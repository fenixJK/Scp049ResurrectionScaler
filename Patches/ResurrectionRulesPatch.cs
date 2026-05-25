namespace Scp049ResurrectionScaler.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using LabApi.Events.Arguments.Scp049Events;
    using LabApi.Features.Wrappers;
    using PlayerRoles.PlayableScps.Scp049;
    using PlayerRoles.Ragdolls;
    using Scp049ResurrectionScaler.Runtime;

    internal static class ResurrectionRulesPatch
    {
        [ThreadStatic]
        private static Scp049ResurrectAbility currentAbility;

        public static void Patch(Harmony harmony)
        {
            if (harmony is null)
                throw new ArgumentNullException(nameof(harmony));

            Type abilityType = typeof(Scp049ResurrectAbility);

            harmony.Patch(AccessTools.Method(abilityType, "ServerValidateBegin"), prefix: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(BeginContext), BindingFlags.Static | BindingFlags.NonPublic)), finalizer: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(EndContext), BindingFlags.Static | BindingFlags.NonPublic)));
            harmony.Patch(AccessTools.Method(abilityType, "ServerValidateAny"), prefix: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(BeginContext), BindingFlags.Static | BindingFlags.NonPublic)), finalizer: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(EndContext), BindingFlags.Static | BindingFlags.NonPublic)));
            harmony.Patch(AccessTools.Method(abilityType, "CheckMaxResurrections"), transpiler: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(CheckMaxResurrectionsTranspiler), BindingFlags.Static | BindingFlags.NonPublic)));
            harmony.Patch(AccessTools.Method(abilityType, "IsSpawnableSpectator"), postfix: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(IsSpawnableSpectatorPostfix), BindingFlags.Static | BindingFlags.NonPublic)));
            harmony.Patch(AccessTools.Constructor(typeof(Scp049ResurrectingBodyEventArgs), new[] { typeof(Ragdoll), typeof(ReferenceHub), typeof(ReferenceHub) }), postfix: new HarmonyMethod(typeof(ResurrectionRulesPatch).GetMethod(nameof(ResurrectingBodyEventArgsPostfix), BindingFlags.Static | BindingFlags.NonPublic)));
        }

        private static void BeginContext(Scp049ResurrectAbility __instance)
        {
            currentAbility = __instance;
        }

        private static Exception EndContext(Exception __exception)
        {
            currentAbility = null;
            return __exception;
        }

        private static IEnumerable<CodeInstruction> CheckMaxResurrectionsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> original = new List<CodeInstruction>(instructions);
            Label continueOriginal = generator.DefineLabel();

            if (original.Count > 0)
                original[0].labels.Add(continueOriginal);

            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResurrectionRulesPatch), nameof(ShouldIgnoreMaxResurrectionLimit)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, continueOriginal);
            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
            yield return new CodeInstruction(OpCodes.Ret);

            foreach (CodeInstruction instruction in original)
                yield return instruction;
        }

        private static bool ShouldIgnoreMaxResurrectionLimit()
        {
            return ResurrectionTargetSelector.CanIgnoreZombieResurrectionLimit(currentAbility);
        }

        private static void IsSpawnableSpectatorPostfix(ReferenceHub hub, ref bool __result)
        {
            if (!__result && ResurrectionTargetSelector.CanUseReplacementTarget(currentAbility, hub))
                __result = true;
        }

        private static void ResurrectingBodyEventArgsPostfix(Scp049ResurrectingBodyEventArgs __instance, Ragdoll ragdoll, ReferenceHub target, ReferenceHub hub)
        {
            if (__instance is null || ragdoll is null)
                return;

            if (!ResurrectionTargetSelector.TryGetReplacementTarget(hub, target, ragdoll.Base, out ReferenceHub replacement))
                return;

            __instance.Target = Player.Get(replacement);
        }
    }
}
