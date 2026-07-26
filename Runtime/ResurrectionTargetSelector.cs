namespace Scp049ResurrectionScaler.Runtime
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using HarmonyLib;
    using PlayerRoles;
    using PlayerRoles.PlayableScps.Scp049;
    using PlayerRoles.Ragdolls;
    using PlayerRoles.Spectating;

    internal static class ResurrectionTargetSelector
    {
        public static bool IsConfiguredDoctor(ReferenceHub hub)
        {
            return Plugin.Instance is not null && Player.TryGet(hub, out Player player) && Scp049Compatibility.IsScaledDoctor(player, Plugin.Instance.Config);
        }

        public static bool CanIgnoreZombieResurrectionLimit(Scp049ResurrectAbility ability)
        {
            return Plugin.Instance?.Config.AllowMultipleZombieResurrections == true && IsConfiguredDoctor(ability?.Owner) && IsZombieCorpse(GetCurrentRagdoll(ability));
        }

        public static bool CanUseReplacementTarget(Scp049ResurrectAbility ability, ReferenceHub originalTarget)
        {
            return Plugin.Instance?.Config.UseSpectatorForUnavailableZombieTarget == true && IsConfiguredDoctor(ability?.Owner) && IsZombieCorpse(GetCurrentRagdoll(ability)) && !IsReadySpectator(originalTarget) && TryGetReplacementSpectator(originalTarget, ability.Owner, out _);
        }

        public static bool TryGetReplacementTarget(ReferenceHub doctor, ReferenceHub originalTarget, BasicRagdoll ragdoll, out ReferenceHub replacement)
        {
            replacement = null;

            if (Plugin.Instance?.Config.UseSpectatorForUnavailableZombieTarget != true)
                return false;

            if (!IsConfiguredDoctor(doctor) || !IsZombieCorpse(ragdoll) || IsReadySpectator(originalTarget))
                return false;

            return TryGetReplacementSpectator(originalTarget, doctor, out replacement);
        }

        public static bool IsReadySpectator(ReferenceHub hub)
        {
            return hub?.roleManager?.CurrentRole is SpectatorRole spectatorRole && spectatorRole.ReadyToRespawn;
        }

        private static BasicRagdoll GetCurrentRagdoll(Scp049ResurrectAbility ability)
        {
            return ability is null ? null : Accessors.CurRagdollGetter(ability);
        }

        private static bool IsZombieCorpse(BasicRagdoll ragdoll)
        {
            return Scp049Compatibility.IsZombieCorpse(ragdoll, Plugin.Instance?.Config);
        }

        private static bool TryGetReplacementSpectator(ReferenceHub originalTarget, ReferenceHub doctor, out ReferenceHub replacement)
        {
            List<ReferenceHub> spectators = ReferenceHub.AllHubs.Where(hub => hub != null && hub != originalTarget && hub != doctor && IsReadySpectator(hub)).ToList();

            if (spectators.Count == 0)
            {
                replacement = null;
                return false;
            }

            replacement = spectators[Exiled.Loader.Loader.Random.Next(spectators.Count)];
            return true;
        }

        private static class Accessors
        {
            private static readonly System.Reflection.FieldInfo CurRagdollField = AccessTools.Field(typeof(RagdollAbilityBase<Scp049Role>), "<CurRagdoll>k__BackingField");

            public static BasicRagdoll CurRagdollGetter(Scp049ResurrectAbility ability)
            {
                return CurRagdollField?.GetValue(ability) as BasicRagdoll;
            }
        }
    }
}
