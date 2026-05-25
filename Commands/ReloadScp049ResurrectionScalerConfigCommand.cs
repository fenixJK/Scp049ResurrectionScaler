namespace Scp049ResurrectionScaler.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.Loader;
    using Exiled.Permissions.Extensions;
    using Scp049ResurrectionScaler.Roles;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class ReloadScp049ResurrectionScalerConfigCommand : ICommand
    {
        public string Command => "resurrectionscalerreload";

        public string[] Aliases => new[] { "049reload", "reloadresurrectionscaler" };

        public string Description => "Reloads only the SCP-049 Resurrection Scaler config.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("scp049resurrectionscaler.reload"))
            {
                response = "You do not have the scp049resurrectionscaler.reload permission.";
                return false;
            }

            if (Plugin.Instance is null)
            {
                response = "SCP-049 Resurrection Scaler plugin instance was not found.";
                return false;
            }

            IPlugin<IConfig> plugin = Loader.GetPlugin(Plugin.Instance.Prefix);

            if (plugin is null)
            {
                response = "SCP-049 Resurrection Scaler plugin instance was not found.";
                return false;
            }

            try
            {
                RoleTrackingSnapshot snapshot = Plugin.Instance.CaptureTracking();
                plugin.LoadConfig();
                Plugin.Instance.RefreshRegisteredRoles(snapshot);

                if (plugin.Config.Debug)
                    Log.DebugEnabled.Add(plugin.Assembly);
                else
                    Log.DebugEnabled.Remove(plugin.Assembly);

                response = "SCP-049 Resurrection Scaler config reloaded. Saved YAML changes are now active.";
                return true;
            }
            catch (Exception exception)
            {
                Log.Error($"SCP-049 Resurrection Scaler config reload failed: {exception}");
                response = "SCP-049 Resurrection Scaler config reload failed. Check the server console for details.";
                return false;
            }
        }
    }
}
