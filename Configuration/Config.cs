namespace Scp049ResurrectionScaler.Configuration
{
    using System;
    using System.ComponentModel;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Scp049ResurrectionScaler.Roles;

    public sealed class Config : IConfig
    {
        [Description("Whether this plugin is enabled. Reload command: resurrectionscalerreload. Reload aliases: 049reload, reloadresurrectionscaler. Reload permission: scp049resurrectionscaler.reload.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Whether debug messages should be printed to the server console.")]
        public bool Debug { get; set; } = false;

        [Description("The tracking-only custom role that receives HP-based resurrection timing. Granting this role does not change the player's base role or health. The timing change only matters while the tracked player is currently SCP-049.")]
        public LowHealthScp049Role LowHealthScp049 { get; set; } = new LowHealthScp049Role();

        [Description("Fastest resurrection duration, in seconds, used when the custom SCP-049 is effectively at 0% HP.")]
        public float MinimumResurrectionDurationSeconds { get; set; } = 2f;

        [Description("Slowest resurrection duration, in seconds, used when the custom SCP-049 is at 100% HP. Formula: min + ((max - min) * current_hp_percent). Set this to 7 to match the local game assembly's vanilla resurrection duration.")]
        public float MaximumResurrectionDurationSeconds { get; set; } = 7f;

        [Description("Whether the custom SCP-049 can resurrect SCP-049-2 corpses even when that player has already reached the vanilla resurrection limit.")]
        public bool AllowMultipleZombieResurrections { get; set; } = false;

        [Description("Whether the custom SCP-049 should use a random ready spectator when resurrecting an SCP-049-2 corpse whose original player is no longer a ready spectator.")]
        public bool UseSpectatorForUnavailableZombieTarget { get; set; } = false;

        public float GetScaledDuration(Player player, float fallbackDuration)
        {
            if (player is null)
                return fallbackDuration;

            float minimum = Math.Max(0.1f, MinimumResurrectionDurationSeconds);
            float maximum = Math.Max(minimum, MaximumResurrectionDurationSeconds);
            float maxHealth = Math.Max(1f, player.MaxHealth);
            float healthRatio = Math.Max(0f, Math.Min(1f, player.Health / maxHealth));

            return minimum + ((maximum - minimum) * healthRatio);
        }
    }
}
