namespace Scp049ResurrectionScaler.Patches
{
    using System;
    using System.Reflection;
    using Exiled.API.Features;
    using HarmonyLib;
    using PlayerRoles.PlayableScps.Scp049;

    internal static class ResurrectionDurationPatch
    {
        public static void Patch(Harmony harmony)
        {
            if (harmony is null)
                throw new ArgumentNullException(nameof(harmony));

            MethodInfo durationGetter = AccessTools.PropertyGetter(typeof(Scp049ResurrectAbility), "Duration");

            if (durationGetter is null)
            {
                Log.Warn("Could not find SCP-049 resurrection duration getter. HP-based resurrection timing is disabled.");
                return;
            }

            HarmonyMethod postfix = new HarmonyMethod(typeof(ResurrectionDurationPatch).GetMethod(nameof(Postfix), BindingFlags.Static | BindingFlags.NonPublic));
            harmony.Patch(durationGetter, postfix: postfix);
        }

        private static void Postfix(Scp049ResurrectAbility __instance, ref float __result)
        {
            Plugin plugin = Plugin.Instance;

            if (plugin is null || __instance?.Owner is null || plugin.Config.LowHealthScp049 is null)
                return;

            if (!Player.TryGet(__instance.Owner, out Player player) || !plugin.Config.LowHealthScp049.Check(player))
                return;

            float scaledDuration = plugin.Config.GetScaledDuration(player, __result);

            if (plugin.Config.Debug)
                Log.Debug($"Scaled resurrection duration for {player.Nickname} to {scaledDuration:0.###}s at {player.Health:0.###}/{player.MaxHealth:0.###} HP.");

            __result = scaledDuration;
        }
    }
}
