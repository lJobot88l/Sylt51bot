using Classes;
using CAttributes;
using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static Sylt51bot.Program;
namespace Sylt51bot
{
	[Group("config"), Description("Beinhaltet Befehle für die Konfiguration innerhalb des servers\n\nBenutzung:\n```config <befehlname>```"), IsExclude(), CommandClass(CommandClasses.ConfigCommands)]
	public class ConfigCommands : BaseCommandModule
	{
		[Command("show"), Description("Zeigt die aktuelle Konfiguration des servers\n\nBenutzung:\n```config show```"), CommandClass(CommandClasses.ConfigCommands)]
		public async Task ListCfg(CommandContext e, string format = "n")
		{
			try
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Color = DiscordColor.Green, Title = $"Server Konfiguration für server {e.Guild.Name}" };
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				
				if(format == "n")
				{
					string enModules = "";
					string lvlroles = "";

					foreach (var module in Enum.GetValues(typeof(Classes.Modules)))
					{
						if ((int)module != 0b11)
						{
							enModules += $"{module.ToString()}: {servers.Find(x => x.Id == e.Guild.Id).EnabledModules.HasFlag((Enum)module)}\n";
						}
					}

					foreach(LevelRole l in s.lvlroles)
					{
						if(l.RoleId != 0)
						{
							lvlroles += $"{l.Name} : {l.XpReq}xp (ID:{l.RoleId})\n";
						}
					}

					embed.AddField("Generelles",
					$"```Id: {s.Id.ToString()}\nName: {e.Guild.Name}```");
					embed.AddField("Module", $"```{enModules}```");
					if(!string.IsNullOrEmpty(lvlroles)) { embed.AddField("Levelrollen", $"```{lvlroles}```"); }
					embed.AddField("Xp Optionen", $"```MinXp: {s.MinXp}xp\nMaxXp: {s.MaxXp}xp\nXp cooldown: {s.CoolDown}```");
				}
				else if(format == "json")
				{
					embed.Description = $"```json\n{Newtonsoft.Json.JsonConvert.SerializeObject(s, Newtonsoft.Json.Formatting.Indented)}```";
				}
				await e.RespondAsync(embed);
			}
			catch(Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("minxp"), Description("Ändert die mindestanzahl an erhaltbaren xp pro nachricht im jeweiligen Zeitinterval.\n\nBenutzung:\n```config minxp <anzahl>```"), CommandClass(CommandClasses.ConfigCommands), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild)]
		public async Task SetMinXp(CommandContext e, int newxp)
		{
			try
			{
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				if(newxp <= 0 || newxp >= s.MaxXp)
				{
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Description = $"Die neue MinXp-Anzahl muss größer als 0, und kleiner als MaxXp sein!\n```Angegebene Anzahl: {newxp}\nMaxXp: {s.MaxXp}```", Color = DiscordColor.Red});
					return;
				}
				servers.Find(x => x.Id == s.Id).MinXp = newxp;
				await e.RespondAsync(new DiscordEmbedBuilder { Description = $"MinXp wurde aktualisiert!\n```Vorher: {s.MinXp}\nJetzt: {newxp}", Color = DiscordColor.Green});
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("maxxp"), Description("Ändert die maximalanzahl an erhaltbaren xp pro nachricht im jeweiligen Zeitinterval.\n\nBenutzung:\n```config maxxp <anzahl>```"), CommandClass(CommandClasses.ConfigCommands), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild)]
		public async Task SetMaxXp(CommandContext e, int newxp)
		{
			try
			{
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				if (newxp <= s.MinXp)
				{
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Description = $"Die neue MaxXp-Anzahl muss größer als MinXp sein!\n```Angegebene Anzahl: {newxp}\nMinXp: {s.MinXp}```", Color = DiscordColor.Red });
					return;
				}
				servers.Find(x => x.Id == s.Id).MaxXp = newxp;
				await e.RespondAsync(new DiscordEmbedBuilder { Description = $"MaxXp wurde aktualisiert!\n```Vorher: {s.MaxXp}\nJetzt: {newxp}", Color = DiscordColor.Green });

			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("cooldown"), Description("Ändert das Zeitintervall für den Cooldown zwischen einem xp-erhalt und dem nächsten\nBenutzung:\n```config cooldown <Neue Cooldownzeit>```"), CommandClass(CommandClasses.ConfigCommands), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild)]
		public async Task SetCoolDown(CommandContext e, TimeSpan d)
		{
			try
			{
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				if(d.Milliseconds <= 0)
				{
					await e.RespondAsync(new DiscordEmbedBuilder { Description = "Die Cooldownzeit muss größer als oder gleich zu 0 sein"});
					return;
				}
				servers.Find(x => x.Id == e.Guild.Id).CoolDown = d;
				await e.RespondAsync(new DiscordEmbedBuilder { Description = $"MaxXp wurde aktualisiert!\n```Vorher: {s.CoolDown.ToString("HH:mm:ss")}\nJetzt: {d.ToString("HH:mm:ss")}", Color = DiscordColor.Green });
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
		[Command("togglemodule"), Description("Schaltet ein Modul an oder aus\n\nBenutzung:\n```config togglemodule <Modulname>```"), CommandClass(CommandClasses.ConfigCommands), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild)]
		public async Task ToggleModule(CommandContext e, string ModuleName = "help")
		{
			try
			{
				if (ModuleName == "help")
				{
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder
					{
						Title = $"Module in server {e.Guild.Name}",
						Color = DiscordColor.Green
					};
					foreach (var module in Enum.GetValues(typeof(Classes.Modules)))
					{
						if ((int)module != 0b11)
						{
							embed.Description += $"```{module.ToString()}: {servers.Find(x => x.Id == e.Guild.Id).EnabledModules.HasFlag((Enum)module)}```";
						}
					}
					await e.RespondAsync(embed: embed);
					return;
				}
				if (Enum.TryParse(ModuleName, true, out Classes.Modules mod))
				{
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Color = DiscordColor.Green };
					servers.Find(x => x.Id == e.Guild.Id).EnabledModules ^= mod;
					embed.Description = $"Modul **`{mod}`** wurde geändert";
					await e.RespondAsync(embed: embed);
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers));
				}
				else
				{
					await e.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Modul {ModuleName} konnte nicht gefunden werden" });
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
	}
}