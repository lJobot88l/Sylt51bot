using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using CAttributes;
using static Sylt51bot.Program;
namespace Sylt51bot
{
	public class ModeratorCommands : BaseCommandModule
	{
		[Command("exclude"), Description("Exkludiert einen Nutzer lokal auf dem server vom Benutzen des Bots\n\nBenutzung:\n```=globalexclude < ID / @mention >```"), RequireUserPermissions2(DSharpPlus.Permissions.ManageGuild), IsExclude, CommandClass(Classes.CommandClasses.ModCommands)]
		public async Task LocalExclude(CommandContext e, DiscordUser u)
		{
			try
			{
				Classes.RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				if(!s.ServerBlockedUsers.Contains(u.Id))
				{
					servers.Find(x => x.Id == e.Guild.Id).ServerBlockedUsers.Add(u.Id);
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Benutzer {u.Username}#{u.Discriminator} wurde von der Nutzung des Bots auf dem server gebannt" });
				}
				else
				{
					servers.Find(x => x.Id == e.Guild.Id).ServerBlockedUsers.Remove(u.Id);
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Benutzer {u.Username}#{u.Discriminator} wurde von der Nutzung des Bots auf dem server entbannt" });
				}
				System.IO.File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers));
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
	}
}