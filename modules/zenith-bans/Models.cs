using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using MySqlConnector;

namespace Zenith_Bans
{
	public sealed partial class Plugin : BasePlugin
	{
		public class PlayerDataRaw
		{
			public int Id { get; set; }
			public ulong SteamId { get; set; }
			public string Name { get; set; } = "";
			public string IpAddresses { get; set; } = "";
			public DateTime LastOnline { get; set; }
			public string GroupsString { get; set; } = "";
			public string PermissionsString { get; set; } = "";
			public int? Immunity { get; set; }
			public MySqlDateTime? RankExpiry { get; set; }
			public string? GroupPermissionsString { get; set; }
			public string? OverridesString { get; set; }
		}

		public class PlayerData
		{
			public ulong SteamId { get; set; }
			public string Name { get; set; } = "";
			public string IpAddress { get; set; } = "";
			public List<string> Groups { get; set; } = [];
			public List<string> Permissions { get; set; } = [];
			public int? Immunity { get; set; }
			public MySqlDateTime? RankExpiry { get; set; }
			public List<Punishment> Punishments { get; set; } = [];
			public Dictionary<string, bool> Overrides { get; set; } = [];
		}

		public class Punishment
		{
			public int Id { get; set; }
			public PunishmentType Type { get; set; }
			public int? Duration { get; set; }
			public MySqlDateTime? ExpiresAt { get; set; } = null;
			public string PunisherName { get; set; } = "Console";
			public ulong? AdminSteamId { get; set; }
			public string Reason { get; set; } = "";
		}

		public enum PunishmentType
		{
			Mute,
			Gag,
			Silence,
			Ban,
			Warn,
			Kick,
			SilentKick
		}

		public enum TargetFailureReason
		{
			TargetNotFound,
			TargetImmunity,
			TargetSelf
		}

		public class DisconnectedPlayer
		{
			public ulong SteamId { get; set; }
			public required string PlayerName { get; set; }
			public DateTime DisconnectedAt { get; set; }
		}

		private class PlayerInfo
		{
			public required ulong SteamID { get; set; }
			public required string PlayerName { get; set; }
		}

		private class AdminGroupInfo
		{
			[JsonPropertyName("flags")]
			public List<string> Flags { get; set; } = [];

			[JsonPropertyName("immunity")]
			public int Immunity { get; set; }
		}
	}
}