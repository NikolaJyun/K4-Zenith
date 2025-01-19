using FluentMigrator;

namespace Zenith.Migrations
{
	[Migration(202410196)] // Új migráció verziója
	public class Bans_CreateZenithBansTablesUpgradeV3 : Migration
	{
		public override void Up()
		{
			// zenith_bans_punishments tábla frissítése, ha változott
			if (Schema.Table("zenith_bans_punishments").Exists())
			{
				if (Schema.Table("zenith_bans_punishments").Column("status").Exists())
				{
					// Frissítjük a status ENUM oszlopot, ha szükséges
					Alter.Table("zenith_bans_punishments").AlterColumn("status").AsCustom("ENUM('active', 'warn_ban', 'expired', 'removed', 'removed_console')").NotNullable().WithDefaultValue("active");
				}
				if (!Schema.Table("zenith_bans_punishments").Column("remove_reason").Exists())
					Alter.Table("zenith_bans_punishments").AddColumn("remove_reason").AsCustom("TEXT").Nullable();
			}
		}

		public override void Down()
		{
			// Frissen hozzáadott oszlop törlése, ha a migráció visszaáll
			if (Schema.Table("zenith_bans_punishments").Column("remove_reason").Exists())
				Delete.Column("remove_reason").FromTable("zenith_bans_punishments");

			// Visszaállítja az ENUM típusú oszlopot az előző állapotára
			if (Schema.Table("zenith_bans_punishments").Column("status").Exists())
				Alter.Table("zenith_bans_punishments").AlterColumn("status").AsCustom("ENUM('active', 'expired', 'removed', 'removed_console')").NotNullable().WithDefaultValue("active");
		}
	}
}
