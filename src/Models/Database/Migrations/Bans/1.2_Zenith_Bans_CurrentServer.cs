using FluentMigrator;

namespace Zenith.Migrations
{
	[Migration(202410195)] // Új migráció verziója
	public class Bans_CreateZenithBansTablesUpgradeV2 : Migration
	{
		public override void Up()
		{
			// zenith_bans_players tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_players").Exists())
			{
				if (!Schema.Table("zenith_bans_players").Column("current_server").Exists())
					Alter.Table("zenith_bans_players").AddColumn("current_server").AsString(50).Nullable();
			}
		}

		public override void Down()
		{
			// Frissen hozzáadott oszlop törlése, ha a migráció visszaáll
			if (Schema.Table("zenith_bans_players").Column("current_server").Exists())
				Delete.Column("current_server").FromTable("zenith_bans_players");
		}
	}
}
