namespace Zenith
{
	using CounterStrikeSharp.API.Core;
	using CounterStrikeSharp.API.Core.Capabilities;
	using K4ArenaSharedApi;
	using Microsoft.Extensions.Logging;

	public sealed partial class Plugin : BasePlugin
	{
		public static IK4ArenaSharedApi? SharedAPI_Arena { get; private set; }
		public (bool ArenaFound, bool Checked) ArenaSupport = (false, false);
		public string GetPlayerArenaName(CCSPlayerController player)
		{
			if (!ArenaSupport.Checked)
			{
				string arenaPath = Path.GetFullPath(Path.Combine(ModuleDirectory, "..", "K4-Arenas"));
				ArenaSupport.ArenaFound = Directory.Exists(arenaPath);
				ArenaSupport.Checked = true;
			}

			if (!ArenaSupport.ArenaFound)
				return string.Empty;

			if (SharedAPI_Arena is null)
			{
				PluginCapability<IK4ArenaSharedApi> Capability_SharedAPI = new("k4-arenas:sharedapi");
				SharedAPI_Arena = Capability_SharedAPI.Get();
			}

			return SharedAPI_Arena?.GetArenaName(player) ?? string.Empty;
		}
	}
}
