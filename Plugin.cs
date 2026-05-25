namespace Scp049ResurrectionScaler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.CustomRoles.API.Features;
    using HarmonyLib;
    using Scp049ResurrectionScaler.Configuration;
    using Scp049ResurrectionScaler.Patches;
    using Scp049ResurrectionScaler.Roles;
    using Scp049ResurrectionScaler.Runtime;
    using ExiledPlugin = Exiled.API.Features.Plugin<Scp049ResurrectionScaler.Configuration.Config>;
    using PlayerEvents = Exiled.Events.Handlers.Player;

    public sealed class Plugin : ExiledPlugin
    {
        private const string HarmonyIdPrefix = "scp049resurrectionscaler";

        private Harmony harmony;
        private List<CustomRole> registeredRoles;
        private Scp049MarkerEventHandler markerEventHandler;

        public static Plugin Instance { get; private set; }

        public override string Author => "Ferox";

        public override string Name => "SCP-049 Resurrection Scaler";

        public override string Prefix => "scp049_resurrection_scaler";

        public override Version Version => new Version(1, 0, 0);

        public override Version RequiredExiledVersion => new Version(9, 13, 3);

        public override void OnEnabled()
        {
            Instance = this;
            registeredRoles = CustomRole.RegisterRoles(false, Config).ToList();
            markerEventHandler = new Scp049MarkerEventHandler(this);
            harmony = new Harmony($"{HarmonyIdPrefix}.{DateTime.UtcNow.Ticks}");

            try
            {
                ResurrectionDurationPatch.Patch(harmony);
                ResurrectionRulesPatch.Patch(harmony);
            }
            catch
            {
                Cleanup();
                throw;
            }

            if (Config.Debug)
                Log.Debug($"Registered {registeredRoles.Count} custom SCP-049 role(s) and patched resurrection duration scaling.");

            PlayerEvents.Spawned += markerEventHandler.OnSpawned;
            PlayerEvents.ChangingRole += markerEventHandler.OnChangingRole;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Cleanup();
            base.OnDisabled();
        }

        internal RoleTrackingSnapshot CaptureTracking()
        {
            LowHealthScp049Role role = Config.LowHealthScp049;

            if (role is null)
                return new RoleTrackingSnapshot(Array.Empty<Player>(), Array.Empty<Player>());

            return role.CaptureTracking();
        }

        internal void RefreshRegisteredRoles(RoleTrackingSnapshot snapshot)
        {
            UnregisterRegisteredRoles();

            registeredRoles = CustomRole.RegisterRoles(false, Config).ToList();

            LowHealthScp049Role role = Config.LowHealthScp049;

            if (role is null)
                return;

            role.RestoreTracking(snapshot);
        }

        private void Cleanup()
        {
            if (markerEventHandler is not null)
            {
                PlayerEvents.Spawned -= markerEventHandler.OnSpawned;
                PlayerEvents.ChangingRole -= markerEventHandler.OnChangingRole;
            }

            harmony?.UnpatchAll(harmony.Id);
            harmony = null;

            UnregisterRegisteredRoles();

            markerEventHandler = null;
            registeredRoles = null;
            Instance = null;
        }

        private void UnregisterRegisteredRoles()
        {
            if (registeredRoles is null || registeredRoles.Count == 0)
                return;

            foreach (LowHealthScp049Role role in registeredRoles.OfType<LowHealthScp049Role>())
                role.ClearTracking();

            CustomRole.UnregisterRoles(registeredRoles).ToList();
        }
    }
}
