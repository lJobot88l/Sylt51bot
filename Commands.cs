using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static Sylt51bot.Program;
using CAttributes;
namespace Sylt51bot
{
	public class GenCommands : BaseCommandModule
	{
		[Command("ping"), Description("Zeigt an ob der bot funktioniert, oder nicht\n\nBenutzung:```ping```"), CommandClass("OtherCommands")]
		public async Task Ping(CommandContext e)
		{
			try
			{
				DiscordMessage resmsg = await e.RespondAsync(new DiscordEmbedBuilder
				{
					Description = $"**Pinging**\nWS: `{discord.Ping}`ms",
					Color = DiscordColor.Green
				});
				resmsg = await resmsg.ModifyAsync((new DiscordMessageBuilder()).WithEmbed(new DiscordEmbedBuilder
				{
					Description = $"**Pinging...**\nWS: `{discord.Ping}`ms",
					Color = DiscordColor.Green
				}));
				await resmsg.ModifyAsync((new DiscordMessageBuilder()).WithEmbed(new DiscordEmbedBuilder
				{
					Description = $"**Pong!**\nPing: `{(((TimeSpan)(resmsg.EditedTimestamp - resmsg.Timestamp)).TotalMilliseconds).ToString("#")}`ms\nWS: `{discord.Ping}`ms",
					Color = DiscordColor.Green
				}));
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
		[Command("togglemodule"), Description("Schaltet ein Modul an oder aus\n\nBenutzung:\n```=togglemodule <Modulname>```"), CommandClass("ModCommands"), RequireUserPermissions2(Permissions.ManageGuild)]
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
					foreach(var module in Enum.GetValues(typeof(Classes.Modules)))
					{
						if((int)module != 0b11)
						{
							embed.Description += $"```{module.ToString()}: {servers.Find(x => x.Id == e.Guild.Id).EnabledModules.HasFlag((Enum)module)}```";
						}
					}
					await e.RespondAsync(embed: embed);
					return;
				}
				if (Enum.TryParse(ModuleName, out Classes.Modules mod))
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