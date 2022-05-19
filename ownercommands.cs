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
	public class BotAdminCommands : BaseCommandModule
	{
		[Command("addauth"), CommandClass("OwnerCommands"), Description("Adds/Removes a user to the list of bot admins\n\nUsage:\n```=addauth <ID / @mention >```"), RequireAuth()]
		public async Task AddAuth(CommandContext e, DiscordUser NewAdmin)
		{
			try
			{
				DiscordMessage resmsg = await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = !cInf.AuthUsers.Contains(NewAdmin.Id) ? $"{NewAdmin.Mention} wurde autorisiert" : $"{NewAdmin.Mention} wurde unautorisiert" });
				if (!cInf.AuthUsers.Contains(NewAdmin.Id))
				{
					cInf.AuthUsers.Add(NewAdmin.Id);
				}
				else
				{
					cInf.AuthUsers.Remove(NewAdmin.Id);
				}
				File.WriteAllText("config/mconfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(cInf));
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("status"), CommandClass("OwnerCommands"), Description("Sets the bots status to a given text. \"clear\" to clear.\n\nUsage:\n```=status <New Status>```"), RequireAuth()]
		public async Task Status(CommandContext e, [RemainingText] string NewStatus)
		{
			try
			{
				if (NewStatus != "clear")
				{
					g1.Name = NewStatus;
					await discord.UpdateStatusAsync(g1);
				}
				else
				{
					await discord.UpdateStatusAsync();
				}
				DiscordMessageBuilder msgb = new DiscordMessageBuilder();
				msgb.WithEmbed(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Updated status to {NewStatus}" });
				msgb.WithReply(e.Message.Id);
				await e.Message.RespondAsync(msgb);
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("resetschulden"), Description("Bearbeitet die Schulden der DDR!\n\nBenutzung:\n```=resetschulden (DMark)```"), CommandClass("OwnerCommands"), RequireAuth()]
		public async Task ResetSchulden(CommandContext e, long newSchulden = 86000000000)
		{
			try
			{
				if(newSchulden <= 86000000000)
				{
					cInf.SchuldenDerDDR = newSchulden;
					File.WriteAllText("config/mconfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(cInf));
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Schulden wurden auf `{newSchulden}` DM gesetzt" });
				}
				else
				{
					await e.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Schulden können nicht größer als 86M DM sein" });
				}
				if(newSchulden <= 0)
				{
					await e.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Schulden können nicht kleiner als 0 DM sein" });
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
	}
}