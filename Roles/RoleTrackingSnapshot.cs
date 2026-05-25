namespace Scp049ResurrectionScaler.Roles
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;

    internal sealed class RoleTrackingSnapshot
    {
        public RoleTrackingSnapshot(IEnumerable<Player> manualPlayers, IEnumerable<Player> automaticPlayers)
        {
            ManualPlayers = manualPlayers?.ToArray() ?? new Player[0];
            AutomaticPlayers = automaticPlayers?.ToArray() ?? new Player[0];
        }

        public IReadOnlyCollection<Player> ManualPlayers { get; }

        public IReadOnlyCollection<Player> AutomaticPlayers { get; }
    }
}
