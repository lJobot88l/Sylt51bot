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
		[Command("addauth"), CommandClass(Classes.CommandClasses.OwnerCommands), Description("Fügt/Entfernt einen Benutzer aus der Liste der Adminbots\n\nBenutzung:\n```=addauth < ID / @mention >```"), RequireAuth()]
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

		[Command("status"), CommandClass(Classes.CommandClasses.OwnerCommands), Description("Setzt den Botstatus zum angegebenen Text. \"clear\" um den Text Blank zu machen.\n\nBenutzung:\n```=status <New Status>```"), RequireAuth()]
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
				msgb.WithEmbed(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Status zu {NewStatus} aktualisiert" });
				msgb.WithReply(e.Message.Id);
				await e.Message.RespondAsync(msgb);
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("resetschulden"), Description("Bearbeitet die Schulden der DDR!\n\nBenutzung:\n```=resetschulden (DMark)```"), CommandClass(Classes.CommandClasses.OwnerCommands), RequireAuth()]
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

		[Command("globalexclude"), Description("Exkludiert einen Nutzer global vom Benutzen des Bots\n\nBenutzung:\n```=globalexclude < ID / @mention >```"), RequireAuth, CommandClass(Classes.CommandClasses.OwnerCommands), IsExclude]
		public async Task GlobalExclude(CommandContext e, DiscordUser u)
		{
			try
			{
				if(!cInf.GlobalBlockedUsers.Contains(u.Id))
				{
					cInf.GlobalBlockedUsers.Add(u.Id);
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Benutzer {u.Username}#{u.Discriminator} wurde von der globalen Nutzung des Bots gebannt"});
				}
				else
				{
					cInf.GlobalBlockedUsers.Remove(u.Id);
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Benutzer {u.Username}#{u.Discriminator} wurde von der globalen Nutzung des Bots entbannt" });

				}
					File.WriteAllText("config/mconfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(cInf));
					File.WriteAllText("config/mconfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(cInf));
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
	}
}
