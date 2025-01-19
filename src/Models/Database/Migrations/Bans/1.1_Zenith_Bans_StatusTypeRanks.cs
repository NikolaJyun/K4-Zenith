using FluentMigrator;

namespace Zenith.Migrations
{
	[Migration(202410194)] // Új migráció verziója
	public class Bans_CreateZenithBansTablesUpdate : Migration
	{
		public override void Up()
		{
			// zenith_bans_players tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_players").Exists())
			{
				if (!Schema.Table("zenith_bans_players").Column("ip_addresses").Exists())
					Alter.Table("zenith_bans_players").AddColumn("ip_addresses").AsCustom("JSON").Nullable();
			}

			// zenith_bans_player_ranks tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_player_ranks").Exists())
			{
				if (!Schema.Table("zenith_bans_player_ranks").Column("groups").Exists())
					Alter.Table("zenith_bans_player_ranks").AddColumn("groups").AsCustom("JSON").Nullable();

				if (!Schema.Table("zenith_bans_player_ranks").Column("permissions").Exists())
					Alter.Table("zenith_bans_player_ranks").AddColumn("permissions").AsCustom("JSON").Nullable();
			}

			// zenith_bans_admin_groups tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_admin_groups").Exists())
			{
				if (!Schema.Table("zenith_bans_admin_groups").Column("permissions").Exists())
					Alter.Table("zenith_bans_admin_groups").AddColumn("permissions").AsCustom("JSON").Nullable();
			}

			// zenith_bans_punishments tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_punishments").Exists())
			{
				if (!Schema.Table("zenith_bans_punishments").Column("status").Exists())
					Alter.Table("zenith_bans_punishments").AddColumn("status").AsCustom("ENUM('active', 'expired', 'removed', 'removed_console')").NotNullable().WithDefaultValue("active");

				if (!Schema.Table("zenith_bans_punishments").Column("type").Exists())
					Alter.Table("zenith_bans_punishments").AddColumn("type").AsCustom("ENUM('mute', 'gag', 'silence', 'ban', 'warn', 'kick')").Nullable();
			}
		}

		public override void Down()
		{
			// Frissen hozzáadott oszlopok törlése, ha a migráció visszaáll
			if (Schema.Table("zenith_bans_players").Column("ip_addresses").Exists())
				Delete.Column("ip_addresses").FromTable("zenith_bans_players");

			if (Schema.Table("zenith_bans_player_ranks").Column("groups").Exists())
				Delete.Column("groups").FromTable("zenith_bans_player_ranks");

			if (Schema.Table("zenith_bans_player_ranks").Column("permissions").Exists())
				Delete.Column("permissions").FromTable("zenith_bans_player_ranks");

			if (Schema.Table("zenith_bans_admin_groups").Column("permissions").Exists())
				Delete.Column("permissions").FromTable("zenith_bans_admin_groups");

			if (Schema.Table("zenith_bans_punishments").Column("status").Exists())
				Delete.Column("status").FromTable("zenith_bans_punishments");

			if (Schema.Table("zenith_bans_punishments").Column("type").Exists())
				Delete.Column("type").FromTable("zenith_bans_punishments");
		}
	}
}
