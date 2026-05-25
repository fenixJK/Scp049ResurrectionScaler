namespace Scp049ResurrectionScaler.Roles
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.API.Features.Attributes;
    using Exiled.API.Features.Spawn;
    using Exiled.CustomRoles.API.Features;
    using PlayerRoles;
    using UnityEngine;
    using YamlDotNet.Serialization;

    [CustomRole(RoleTypeId.Scp049)]
    public sealed class LowHealthScp049Role : CustomRole
    {
        private readonly HashSet<Player> manualPlayers = new HashSet<Player>();
        private readonly HashSet<Player> automaticPlayers = new HashSet<Player>();

        [Description("Unique EXILED custom-role identifier for this resurrection-scaling marker.")]
        public override uint Id { get; set; } = 49049;

        [YamlIgnore]
        public override int MaxHealth { get; set; } = 2500;

        [Description("Display name for the custom-role marker.")]
        public override string Name { get; set; } = "Low-Health SCP-049";

        [Description("Description shown by EXILED CustomRoles when this marker is referenced.")]
        public override string Description { get; set; } = "SCP-049 whose resurrection time decreases linearly as current HP falls.";

        [Description("Custom info text kept with the role definition.")]
        public override string CustomInfo { get; set; } = "Resurrection becomes faster at lower HP.";

        [YamlIgnore]
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp049;

        [Description("Chance that a naturally spawned SCP-049 automatically receives this marker. 100 = always, 25 = roughly one quarter, 0 = manual assignment only.")]
        public override float SpawnChance { get; set; } = 0f;

        [YamlIgnore]
        public override bool RemovalKillsPlayer { get; set; } = false;

        [YamlIgnore]
        public override bool KeepRoleOnDeath { get; set; } = true;

        [YamlIgnore]
        public override bool KeepRoleOnChangingRole { get; set; } = true;

        [YamlIgnore]
        public override List<CustomAbility> CustomAbilities { get; set; } = new List<CustomAbility>();

        [YamlIgnore]
        public override List<string> Inventory { get; set; } = new List<string>();

        [YamlIgnore]
        public override Dictionary<AmmoType, ushort> Ammo { get; set; } = new Dictionary<AmmoType, ushort>();

        [YamlIgnore]
        public override SpawnProperties SpawnProperties { get; set; } = new SpawnProperties();

        [YamlIgnore]
        public override int MinPlayers { get; set; }

        [YamlIgnore]
        public override bool KeepPositionOnSpawn { get; set; }

        [YamlIgnore]
        public override bool KeepInventoryOnSpawn { get; set; }

        [YamlIgnore]
        public override bool IgnoreSpawnSystem { get; set; } = true;

        [YamlIgnore]
        public override Broadcast Broadcast { get; set; } = new Broadcast();

        [YamlIgnore]
        public override bool DisplayCustomItemMessages { get; set; } = true;

        [YamlIgnore]
        public override Vector3 Scale { get; set; } = Vector3.one;

        [YamlIgnore]
        public override Vector3? Gravity { get; set; }

        [YamlIgnore]
        public override Dictionary<RoleTypeId, float> CustomRoleFFMultiplier { get; set; } = new Dictionary<RoleTypeId, float>();

        [YamlIgnore]
        public override string ConsoleMessage { get; set; } = string.Empty;

        [YamlIgnore]
        public override string AbilityUsage { get; set; } = string.Empty;

        public override void AddRole(Player player)
        {
            if (player is null)
                return;

            automaticPlayers.Remove(player);
            manualPlayers.Add(player);
            TrackedPlayers.Add(player);
        }

        public override void RemoveRole(Player player)
        {
            if (player is null)
                return;

            manualPlayers.Remove(player);
            automaticPlayers.Remove(player);
            TrackedPlayers.Remove(player);
        }

        internal void AddAutomaticRole(Player player)
        {
            if (player is null || manualPlayers.Contains(player))
                return;

            automaticPlayers.Add(player);
            TrackedPlayers.Add(player);
        }

        internal void RemoveAutomaticRole(Player player)
        {
            if (player is null || manualPlayers.Contains(player))
                return;

            automaticPlayers.Remove(player);
            TrackedPlayers.Remove(player);
        }

        internal bool IsAutomatic(Player player)
        {
            return player is not null && automaticPlayers.Contains(player);
        }

        internal RoleTrackingSnapshot CaptureTracking()
        {
            return new RoleTrackingSnapshot(manualPlayers, automaticPlayers);
        }

        internal void ClearTracking()
        {
            manualPlayers.Clear();
            automaticPlayers.Clear();
            TrackedPlayers.Clear();
        }

        internal void RestoreTracking(RoleTrackingSnapshot snapshot)
        {
            ClearTracking();

            if (snapshot is null)
                return;

            foreach (Player player in snapshot.ManualPlayers)
                AddRole(player);

            foreach (Player player in snapshot.AutomaticPlayers)
                AddAutomaticRole(player);
        }
    }
}
