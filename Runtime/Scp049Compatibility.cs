namespace Scp049ResurrectionScaler.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Exiled.API.Features;
    using Exiled.CustomRoles.API;
    using Exiled.CustomRoles.API.Features;
    using PlayerRoles;
    using PlayerRoles.Ragdolls;
    using PlayerStatsSystem;
    using Scp049ResurrectionScaler.Configuration;

    internal static class Scp049Compatibility
    {
        private static readonly Regex RichTextTagRegex = new Regex("<.*?>", RegexOptions.Compiled);

        public static bool IsScaledDoctor(Player player, Config config)
        {
            if (player is null || config is null || player.Role.Type != RoleTypeId.Scp049)
                return false;

            if (config.ScaleAllScp049Players)
                return true;

            if (config.LowHealthScp049 is not null && config.LowHealthScp049.Check(player))
                return true;

            return HasCompatibleScp049CustomRole(player, config);
        }

        public static bool IsZombieCorpse(BasicRagdoll ragdoll, Config config)
        {
            if (config?.AffectScp0492ZombieCorpses != true)
                return false;

            return ragdoll is not null && ragdoll.Info.RoleType == RoleTypeId.Scp0492 && ragdoll.Info.Handler is AttackerDamageHandler;
        }

        private static bool HasCompatibleScp049CustomRole(Player player, Config config)
        {
            List<uint> compatibleIds = config.CompatibleScp049CustomRoleIds ?? new List<uint>();
            List<string> compatibleNames = config.CompatibleScp049CustomRoleNames ?? new List<string>();

            foreach (CustomRole customRole in player.GetCustomRoles() ?? Enumerable.Empty<CustomRole>())
            {
                if (customRole is null)
                    continue;

                if (config.ScaleExternalScp049CustomRoles && customRole.Role == RoleTypeId.Scp049)
                    return true;

                if (compatibleIds.Contains(customRole.Id))
                    return true;

                if (HasConfiguredName(customRole, compatibleNames))
                    return true;
            }

            return false;
        }

        private static bool HasConfiguredName(CustomRole customRole, IEnumerable<string> configuredNames)
        {
            foreach (string configuredName in configuredNames ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(configuredName))
                    continue;

                if (MatchesName(customRole.Name, configuredName) || MatchesName(customRole.CustomInfo, configuredName))
                    return true;
            }

            return false;
        }

        private static bool MatchesName(string actual, string configured)
        {
            return string.Equals(NormalizeName(actual), NormalizeName(configured), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : RichTextTagRegex.Replace(value, string.Empty).Trim();
        }
    }
}
