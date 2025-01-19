using FluentMigrator;

namespace Zenith.Migrations
{
	[Migration(202410198)] // Új migráció verziója
	public class Bans_CreateZenithBansTablesUpgradeV4 : Migration
	{
		public override void Up()
		{
			// zenith_bans_ip_addresses tábla létrehozása, ha nem létezik
			if (!Schema.Table("zenith_bans_ip_addresses").Exists())
			{
				Create.Table("zenith_bans_ip_addresses")
					.WithColumn("id").AsInt32().PrimaryKey().Identity()
					.WithColumn("player_id").AsInt32().ForeignKey("FK_ip_addresses_player_id", "zenith_bans_players", "id")
					.WithColumn("ip_address").AsString(45).NotNullable();

				Create.UniqueConstraint("unique_player_ip").OnTable("zenith_bans_ip_addresses").Columns("player_id", "ip_address");
			}

			// zenith_bans_players tábla frissítése
			if (Schema.Table("zenith_bans_players").Exists())
			{
				if (Schema.Table("zenith_bans_players").Column("ip_addresses").Exists())
				{
					Execute.Sql(@"INSERT INTO zenith_bans_ip_addresses (player_id, ip_address)
                                 SELECT id, JSON_UNQUOTE(JSON_EXTRACT(ip_addresses, '$[*]'))
                                 FROM zenith_bans_players WHERE ip_addresses IS NOT NULL;");

					Delete.Column("ip_addresses").FromTable("zenith_bans_players");
				}
			}

			// zenith_bans_player_ranks tábla frissítése
			if (Schema.Table("zenith_bans_player_ranks").Exists())
			{
				if (Schema.Table("zenith_bans_player_ranks").Constraint("FK_player_ranks_steam_id").Exists())
					Delete.ForeignKey("FK_player_ranks_steam_id").OnTable("zenith_bans_player_ranks");

				if (Schema.Table("zenith_bans_player_ranks").Column("steam_id").Exists())
				{
					Rename.Column("steam_id").OnTable("zenith_bans_player_ranks").To("player_id");

					Execute.Sql(@"UPDATE zenith_bans_player_ranks r
								 SET player_id = (
									 SELECT id
									 FROM zenith_bans_players
									 WHERE steam_id = r.player_id
									 LIMIT 1
								 );");
				}

				if (!Schema.Table("zenith_bans_player_ranks").Constraint("FK_player_ranks_player_id").Exists())
					Alter.Column("player_id").OnTable("zenith_bans_player_ranks").AsInt32().ForeignKey("FK_player_ranks_player_id", "zenith_bans_players", "id");

				if (Schema.Table("zenith_bans_player_ranks").Column("groups").Exists())
					Delete.Column("groups").FromTable("zenith_bans_player_ranks");

				if (Schema.Table("zenith_bans_player_ranks").Column("permissions").Exists())
					Delete.Column("permissions").FromTable("zenith_bans_player_ranks");
			}

			// zenith_bans_player_groups tábla létrehozása, ha nem létezik
			if (!Schema.Table("zenith_bans_player_groups").Exists())
			{
				Create.Table("zenith_bans_player_groups")
					.WithColumn("id").AsInt32().PrimaryKey().Identity()
					.WithColumn("player_rank_id").AsInt32().ForeignKey("FK_player_groups_rank_id", "zenith_bans_player_ranks", "id")
					.WithColumn("group_name").AsString(50).NotNullable();
			}

			// zenith_bans_player_permissions tábla létrehozása, ha nem létezik
			if (!Schema.Table("zenith_bans_player_permissions").Exists())
			{
				Create.Table("zenith_bans_player_permissions")
					.WithColumn("id").AsInt32().PrimaryKey().Identity()
					.WithColumn("player_rank_id").AsInt32().ForeignKey("FK_player_permissions_rank_id", "zenith_bans_player_ranks", "id")
					.WithColumn("permission").AsString(100).NotNullable();
			}

			// zenith_bans_admin_groups tábla frissítése
			if (Schema.Table("zenith_bans_admin_groups").Exists() && Schema.Table("zenith_bans_admin_groups").Column("permissions").Exists())
				Delete.Column("permissions").FromTable("zenith_bans_admin_groups");

			// zenith_bans_admin_group_permissions tábla létrehozása, ha nem létezik
			if (!Schema.Table("zenith_bans_admin_group_permissions").Exists())
			{
				Create.Table("zenith_bans_admin_group_permissions")
					.WithColumn("id").AsInt32().PrimaryKey().Identity()
					.WithColumn("group_id").AsInt32().ForeignKey("FK_admin_group_permissions_group_id", "zenith_bans_admin_groups", "id")
					.WithColumn("permission").AsString(100).NotNullable();
			}

			if (Schema.Table("zenith_bans_punishments").Exists())
			{
				if (Schema.Table("zenith_bans_punishments").Column("steam_id").Exists())
				{
					if (Schema.Table("zenith_bans_punishments_old").Exists())
						Delete.Table("zenith_bans_punishments_old");

					Rename.Table("zenith_bans_punishments").To("zenith_bans_punishments_old");

					// Step 2: Create the new table with the updated structure
					Create.Table("zenith_bans_punishments")
						.WithColumn("id").AsInt32().PrimaryKey().Identity()
						.WithColumn("player_id").AsInt32()
						.WithColumn("status").AsCustom("ENUM('active', 'warn_ban', 'expired', 'removed', 'removed_console')").NotNullable().WithDefaultValue("active")
						.WithColumn("type").AsCustom("ENUM('mute', 'gag', 'silence', 'ban', 'warn', 'kick')").Nullable()
						.WithColumn("duration").AsInt32().Nullable()
						.WithColumn("created_at").AsDateTime().Nullable()
						.WithColumn("expires_at").AsDateTime().Nullable()
						.WithColumn("admin_id").AsInt32().Nullable().ForeignKey("FK_punishments_admin_id", "zenith_bans_players", "id")
						.WithColumn("removed_at").AsDateTime().Nullable()
						.WithColumn("remove_admin_id").AsInt32().Nullable().ForeignKey("FK_punishments_remove_admin_id", "zenith_bans_players", "id")
						.WithColumn("server_ip").AsString(50).NotNullable().WithDefaultValue("all")
						.WithColumn("reason").AsCustom("TEXT").Nullable()
						.WithColumn("remove_reason").AsCustom("TEXT").Nullable();

					if (!Schema.Table("zenith_bans_punishments").Constraint("FK_punishments_player_id").Exists())
						Create.ForeignKey("FK_punishments_player_id").FromTable("zenith_bans_punishments").ForeignColumn("player_id").ToTable("zenith_bans_players").PrimaryColumn("id");

					// Step 3: Migrate data from the old table to the new table
					Execute.Sql(@"
						INSERT INTO zenith_bans_punishments (player_id, status, type, duration, created_at, expires_at, admin_id, removed_at, remove_admin_id, server_ip, reason)
						SELECT
							(SELECT id FROM zenith_bans_players WHERE steam_id = old_punishments.steam_id LIMIT 1) AS player_id,
							old_punishments.type,
							old_punishments.status,
							old_punishments.duration,
							old_punishments.created_at,
							old_punishments.expires_at,
							(SELECT id FROM zenith_bans_players WHERE steam_id = old_punishments.admin_steam_id LIMIT 1) AS admin_id,
							old_punishments.removed_at,
							(SELECT id FROM zenith_bans_players WHERE steam_id = old_punishments.remove_admin_steam_id LIMIT 1) AS remove_admin_id,
							old_punishments.server_ip,
							old_punishments.reason
						FROM zenith_bans_punishments_old AS old_punishments;
					");

					// Step 4: Drop the old table after data migration
					if (Schema.Table("zenith_bans_punishments_old").Exists())
						Delete.Table("zenith_bans_punishments_old");
				}
			}
		}

		public override void Down()
		{
			// zenith_bans_ip_addresses rollback
			if (Schema.Table("zenith_bans_ip_addresses").Exists())
			{
				if (Schema.Table("zenith_bans_players").Exists() && !Schema.Table("zenith_bans_players").Column("ip_addresses").Exists())
				{
					// Add the ip_addresses column back to zenith_bans_players
					Alter.Table("zenith_bans_players").AddColumn("ip_addresses").AsCustom("JSON").Nullable();

					// Restore IP addresses to zenith_bans_players from zenith_bans_ip_addresses
					Execute.Sql(@"
						UPDATE zenith_bans_players p
						SET ip_addresses = (
							SELECT JSON_ARRAYAGG(ip.ip_address)
							FROM zenith_bans_ip_addresses ip
							WHERE ip.player_id = p.id
						)
						WHERE EXISTS (
							SELECT 1
							FROM zenith_bans_ip_addresses ip
							WHERE ip.player_id = p.id
						);
					");
				}

				// Delete the zenith_bans_ip_addresses table
				Delete.Table("zenith_bans_ip_addresses");
			}

			// zenith_bans_player_ranks rollback
			if (Schema.Table("zenith_bans_player_ranks").Exists())
			{
				// Restore original steam_id column
				if (!Schema.Table("zenith_bans_player_ranks").Column("steam_id").Exists())
				{
					Rename.Column("player_id").OnTable("zenith_bans_player_ranks").To("steam_id");
				}

				// Restore original foreign key
				if (!Schema.Table("zenith_bans_player_ranks").Constraint("FK_player_ranks_steam_id").Exists())
				{
					Create.ForeignKey("FK_player_ranks_steam_id")
						.FromTable("zenith_bans_player_ranks").ForeignColumn("steam_id")
						.ToTable("zenith_bans_players").PrimaryColumn("steam_id");
				}

				// Restore deleted columns
				if (!Schema.Table("zenith_bans_player_ranks").Column("groups").Exists())
				{
					Alter.Table("zenith_bans_player_ranks").AddColumn("groups").AsString(255).Nullable();
				}

				if (!Schema.Table("zenith_bans_player_ranks").Column("permissions").Exists())
				{
					Alter.Table("zenith_bans_player_ranks").AddColumn("permissions").AsString(255).Nullable();
				}
			}

			// zenith_bans_player_groups rollback
			if (Schema.Table("zenith_bans_player_groups").Exists())
			{
				Delete.Table("zenith_bans_player_groups");
			}

			// zenith_bans_player_permissions rollback
			if (Schema.Table("zenith_bans_player_permissions").Exists())
			{
				Delete.Table("zenith_bans_player_permissions");
			}

			// zenith_bans_admin_groups rollback (restore permissions column)
			if (Schema.Table("zenith_bans_admin_groups").Exists() && !Schema.Table("zenith_bans_admin_groups").Column("permissions").Exists())
			{
				Alter.Table("zenith_bans_admin_groups").AddColumn("permissions").AsString(255).Nullable();
			}

			// zenith_bans_admin_group_permissions rollback
			if (Schema.Table("zenith_bans_admin_group_permissions").Exists())
			{
				Delete.Table("zenith_bans_admin_group_permissions");
			}

			// zenith_bans_punishments rollback (restoring the old table structure)
			if (Schema.Table("zenith_bans_punishments").Exists())
			{
				if (!Schema.Table("zenith_bans_punishments_old").Exists())
				{
					Rename.Table("zenith_bans_punishments").To("zenith_bans_punishments_old");

					// Recreate the old zenith_bans_punishments table
					Create.Table("zenith_bans_punishments")
						.WithColumn("id").AsInt32().PrimaryKey().Identity()
						.WithColumn("steam_id").AsString(255)
						.WithColumn("type").AsCustom("ENUM('active', 'expired', 'removed')")
						.WithColumn("status").AsString(255)
						.WithColumn("duration").AsInt32().Nullable()
						.WithColumn("created_at").AsDateTime().Nullable()
						.WithColumn("expires_at").AsDateTime().Nullable()
						.WithColumn("admin_steam_id").AsString(255).Nullable()
						.WithColumn("removed_at").AsDateTime().Nullable()
						.WithColumn("remove_admin_steam_id").AsString(255).Nullable()
						.WithColumn("server_ip").AsString(50).NotNullable().WithDefaultValue("all")
						.WithColumn("reason").AsCustom("TEXT").Nullable()
						.WithColumn("remove_reason").AsCustom("TEXT").Nullable();

					// Migrate the data back to the old table structure
					Execute.Sql(@"
						INSERT INTO zenith_bans_punishments (steam_id, type, status, duration, created_at, expires_at, admin_steam_id, removed_at, remove_admin_steam_id, server_ip, reason, remove_reason)
						SELECT
							(SELECT steam_id FROM zenith_bans_players WHERE id = new_punishments.player_id LIMIT 1),
							new_punishments.type,
							new_punishments.status,
							new_punishments.duration,
							new_punishments.created_at,
							new_punishments.expires_at,
							(SELECT steam_id FROM zenith_bans_players WHERE id = new_punishments.admin_id LIMIT 1),
							new_punishments.removed_at,
							(SELECT steam_id FROM zenith_bans_players WHERE id = new_punishments.remove_admin_id LIMIT 1),
							new_punishments.server_ip,
							new_punishments.reason,
							new_punishments.remove_reason
						FROM zenith_bans_punishments_old AS new_punishments;
					");

					// Delete the old temporary table
					if (Schema.Table("zenith_bans_punishments_old").Exists())
					{
						Delete.Table("zenith_bans_punishments_old");
					}
				}
			}
		}
	}
}
