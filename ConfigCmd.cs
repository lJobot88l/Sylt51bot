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
	public class ConfigCommands : BaseCommandModule
	{
		[Command("config"), Description("Zeigt die aktuelle Konfiguration des servers\n\nBenutzung:\n```=config```"), CommandClass("OtherCommands")]
		public async Task ListCfg(CommandContext e)
		{
			try
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Color = DiscordColor.Green, Title = $"Server Konfiguration für server {e.Guild.Name}" };
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
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
				embed.AddField("Xp Optionen", $"```Mindest vergebenes xp: {s.MinXp}xp\nMaximal vergebenes xp: {s.MaxXp}xp\nXp cooldown: {s.CoolDown}```");

				await e.RespondAsync(embed);
			}
			catch(Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("togglemodule"), Description("Schaltet ein Modul an oder aus\n\nBenutzung:\n```=togglemodule <Modulname>```"), CommandClass("ModCommands"), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild)]
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