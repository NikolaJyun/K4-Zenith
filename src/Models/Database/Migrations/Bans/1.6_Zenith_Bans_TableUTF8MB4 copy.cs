using FluentMigrator;

namespace Zenith.Migrations
{
	[Migration(202411182)]
	public class Bans_SetTablesUtf8mb4 : Migration
	{
		public override void Up()
		{
			if (Schema.Table("zenith_bans_players").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_players CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_player_ranks").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_ranks CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_admin_groups").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_admin_groups CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_punishments").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_punishments CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_ip_addresses").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_ip_addresses CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_player_groups").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_groups CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_player_permissions").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_permissions CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_admin_group_permissions").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_admin_group_permissions CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");

			if (Schema.Table("zenith_bans_player_overrides").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_overrides CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");
		}

		public override void Down()
		{
			if (Schema.Table("zenith_bans_players").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_players CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_player_ranks").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_ranks CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_admin_groups").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_admin_groups CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_punishments").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_punishments CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_ip_addresses").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_ip_addresses CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_player_groups").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_groups CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_player_permissions").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_permissions CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_admin_group_permissions").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_admin_group_permissions CHARACTER SET = utf8 COLLATE = utf8_general_ci;");

			if (Schema.Table("zenith_bans_player_overrides").Exists())
				Execute.Sql("ALTER TABLE zenith_bans_player_overrides CHARACTER SET = utf8 COLLATE = utf8_general_ci;");
		}
	}
}