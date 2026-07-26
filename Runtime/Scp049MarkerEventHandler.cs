namespace Scp049ResurrectionScaler.Runtime
{
    using Exiled.API.Enums;
    using Exiled.Loader;
    using Exiled.Events.EventArgs.Player;
    using PlayerRoles;
    using Scp049ResurrectionScaler.Roles;

    internal sealed class Scp049MarkerEventHandler
    {
        private readonly Plugin plugin;

        public Scp049MarkerEventHandler(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            LowHealthScp049Role role = plugin.Config.LowHealthScp049;

            if (role is null || ev.Player is null)
                return;

            if (ev.Player.Role.Type != RoleTypeId.Scp049)
                return;

            if (!IsNaturalSpawn(ev.Reason) || Scp049Compatibility.IsScaledDoctor(ev.Player, plugin.Config))
                return;

            float chance = role.SpawnChance;

            if (chance <= 0f)
                return;

            if (chance >= 100f || Loader.Random.NextDouble() * 100d < chance)
                role.AddAutomaticRole(ev.Player);
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            LowHealthScp049Role role = plugin.Config.LowHealthScp049;

            if (role is null || ev.Player is null || ev.NewRole == RoleTypeId.Scp049)
                return;

            if (role.IsAutomatic(ev.Player))
                role.RemoveAutomaticRole(ev.Player);
        }

        private static bool IsNaturalSpawn(SpawnReason reason)
        {
            return reason == SpawnReason.RoundStart || reason == SpawnReason.LateJoin || reason == SpawnReason.Respawn || reason == SpawnReason.Escaped || reason == SpawnReason.Revived || reason == SpawnReason.ItemUsage;
        }
    }
}
