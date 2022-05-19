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
					Description = $"Pong!\n`{discord.Ping}`ms",
					Color = DiscordColor.Green
				});
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
	}
}