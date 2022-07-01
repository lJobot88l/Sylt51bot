using System;
using System.IO;
using System.Threading.Tasks;
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
		[Command("ping"), Description("Zeigt an ob der bot funktioniert, oder nicht\n\nBenutzung:```ping```"), CommandClass(Classes.CommandClasses.OtherCommands)]
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
	}
}