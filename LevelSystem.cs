using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using static Sylt51bot.Program;
using Classes;
using CAttributes;
using System.Linq;
namespace Sylt51bot
{
	public static class LevelSystem
	{

		public static async void DoTheTimer(MessageCreateEventArgs e)
		{
			try
			{
				if (e.Message.Author.IsBot == true || e.Channel.IsPrivate == true || servers.FindIndex(x => x.Id == e.Guild.Id) == -1 || !servers.Find(x => x.Id == e.Guild.Id).EnabledModules.HasFlag(Modules.Levelling) || servers.Find(x => x.Id == e.Guild.Id).channelxpexclude.Contains(e.Guild.Id))
				{
					return;
				}

				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);

				if (s.timedoutedusers.ContainsKey(e.Message.Author.Id) && DateTime.Now - s.timedoutedusers[e.Message.Author.Id] >= s.CoolDown)
				{
					s.timedoutedusers[e.Message.Author.Id] = DateTime.Now;
					AddXp(e);
				}
				else
				{
					s.timedoutedusers.Add(e.Message.Author.Id, DateTime.Now);
					AddXp(e);
				}

				int userslevel = 0;
				int j = 0;
				bool isDone = false;
				try
				{
					foreach (LevelRole i in s.lvlroles)
					{
						if (i.XpReq <= s.xplist[e.Author.Id] && i.RoleId != 0)
						{
							userslevel++;
							j++;
							if ((await e.Guild.GetMemberAsync(e.Author.Id)).Roles.ToList().FindIndex(x => x.Id == i.RoleId) != -1)
							{
								break;
							}

							await (await e.Guild.GetMemberAsync(e.Author.Id)).GrantRoleAsync(e.Guild.GetRole(i.RoleId));
							if (j == s.lvlroles.Count - 1)
							{
								isDone = true;
								break;
							}
						}
						else
						{
							if (i.RoleId == 0 || (await e.Guild.GetMemberAsync(e.Author.Id)).Roles.Contains(e.Guild.GetRole(i.RoleId)))
							{
								break;
							}
							await (await e.Guild.GetMemberAsync(e.Author.Id))
							.RevokeRoleAsync(
								e
								.Guild
								.GetRole(
									s.lvlroles[
										s.lvlroles.FindIndex(
											x => x.RoleId == i.RoleId
										)
									]
									.RoleId
								)
							);
						}
					}
					if (isDone == true)
					{
						await discord.SendMessageAsync(e.Channel, new DiscordEmbedBuilder { Description = $"**{e.Author.Mention}**'s wurde aufgelevelt zu **{userslevel}**!", Color = DiscordColor.Green });
					}
					servers[servers.FindIndex(x => x.Id == e.Guild.Id)].xplist[e.Message.Author.Id] = s.xplist[e.Message.Author.Id];
					File.WriteAllText("config/RegServers.json", JsonConvert.SerializeObject(servers, Formatting.Indented));
				}
				catch (DSharpPlus.Exceptions.UnauthorizedException)
				{
					await e.Channel.SendMessageAsync("Ich habe keine Berechtigungen, um Rollen zu ändern!");
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}
		public static async void AddXp(MessageCreateEventArgs e, int amount = -1)
		{
			if(servers.FindIndex(x=> x.Id == e.Guild.Id) == -1)
			{
				servers.Add(new RegisteredServer { Id = e.Guild.Id });
			}

			RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);

			if (amount == -1)
			{
				amount = new Random().Next(s.MinXp, s.MaxXp + 1);
			}

			if (s.xplist.ContainsKey(e.Message.Author.Id))
			{
				s.xplist[e.Message.Author.Id] += amount;
			}
			else
			{
				s.xplist.Add(e.Message.Author.Id, amount);
			}

			servers[servers.FindIndex(x => x.Id == e.Guild.Id)] = s;
			File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
		}
	}

	public class LevelCommands : BaseCommandModule
	{
		[Command("lvlroles"), CommandClass("LevelCommands"), RequireGuild(), Description("Zeigt die Levelrollen an, zusammen mit den benötigten Punkten\n\nBenutzung:\n```=lvlroles```"), RequireBotPermissions2(Permissions.SendMessages)]
		public async Task LvlRoles(CommandContext e)
		{
			try
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Color = DiscordColor.Green, Title = $"Level Rollen für {e.Guild.Name}", Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Guild.IconUrl } };
				if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
				{
					servers.Add(new RegisteredServer { Id = e.Guild.Id });
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				}

				List<LevelRole> roles = servers.Find(x => x.Id == e.Guild.Id).lvlroles;
				int i = 0;
				string embedstring = "";
				if (roles.Count() == 1)
				{
					embedstring = "Es gibt noch keine Rollen die mit Leveln verbunden sind!";
					await discord.SendMessageAsync(await discord.GetChannelAsync(e.Message.Channel.Id), embed);
					return;
				}
				else
				{
					foreach (LevelRole kvp in roles)
					{
						if (kvp.XpReq != 0)
						{
							embedstring += $"**`[{i + 1}]`** | <@&{kvp.RoleId}> (**{kvp.XpReq}**xp)\n";
							i++;
						}
					}
				}
				embed.AddField("Chatte um XP zu sammeln!", embedstring, true);
				await discord.SendMessageAsync(await discord.GetChannelAsync(e.Message.Channel.Id), embed);
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("top"), CommandClass("LevelCommands"), RequireGuild(), Description("Zeigt die Bestenliste des Servers an\n\nUsage:\n```=top [page, defaults to 1]```"), Aliases("lb"), Module(Modules.Levelling)]
		public async Task Leaderboard(CommandContext e, int page = 1)
		{
			try
			{
				if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
				{
					servers.Add(new RegisteredServer { Id = e.Guild.Id });
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				}
				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Page {page}/{Math.Ceiling((double)s.xplist.Count / 5)}" }, Color = DiscordColor.Green, Title = "Server XP rangliste", Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = e.Guild.IconUrl } };
				var sortedleederboard = from entry in s.xplist orderby entry.Value descending select entry;

				string embedstring = "";
				int i = 0;
				foreach (KeyValuePair<ulong, int> kvp in sortedleederboard)
				{
					if (i >= (page - 1) * 5)
					{
						int userslevel = 0;
						foreach (var j in s.lvlroles)
						{
							if (j.XpReq <= s.xplist[kvp.Key] && j.XpReq != 0)
							{
								userslevel++;
							}
						}
						string role = "";
						if (userslevel != 0)
						{
							role = $"<@&{s.lvlroles[userslevel].RoleId.ToString()}>";
						}
						else
						{
							role = "Keine Rolle";
						}
						try
						{
							DiscordUser user = await discord.GetUserAsync(kvp.Key);
							if (kvp.Key != e.Message.Author.Id)
							{
								embedstring += $"**```#{i + 1} | {user.Username}``` {kvp.Value}xp | [{role}]**\n\n";
							}
							else
							{
								embedstring += $"**```< #{i + 1} | {user.Username} >```{kvp.Value}xp | [{role}]**\n\n";
							}
						}
						catch
						{
							embedstring += $"**```#{i + 1} | Unbekannter benutzer - ID:{kvp.Key}``` {kvp.Value}xp | [{role}]**\n\n";
						}
						i++;
						if (i == ((page - 1) * 5) + 5)
						{
							break;
						}
					}
					else
					{
						i++;
					}
				}
				embed.Description = embedstring;

				await discord.SendMessageAsync(e.Channel, embed);
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("rank"), CommandClass("LevelCommands"), RequireGuild(), Description("Zeigt den Level von dir oder einem anderen Benutzers an\n\nUsage:\n```=rank [ ID / @mention ]```"), Aliases("lvl", "level"), RequireBotPermissions2(Permissions.SendMessages)]
		public async Task Rank(CommandContext e, DiscordUser user = null)
		{
			try
			{
				if (user == null)
				{
					user = e.Message.Author;
				}
				if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
				{
					servers.Add(new RegisteredServer { Id = e.Guild.Id });
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				}

				RegisteredServer s = servers.Find(x => x.Id == e.Guild.Id);
				if (s.xplist.ContainsKey(user.Id))
				{
					int userslevel = 0;
					foreach (var i in s.lvlroles)
					{
						if (i.XpReq <= s.xplist[user.Id] && i.XpReq != 0)
						{
							userslevel++;
						}
					}
					var sortedleederboard = from entry in s.xplist orderby entry.Value descending select entry;
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder
					{
						Title = "Server Rangkarte",
						Description = $"**```{user.Username}#{user.Discriminator}  | Level {userslevel} | Rang #{sortedleederboard.ToList().FindIndex(x => x.Key == user.Id) + 1}```**\n",
						Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = user.AvatarUrl }
					};
					if (userslevel == 0)
					{
						embed.Color = DiscordColor.Black;
					}
					else
					{
						embed.Color = e.Guild.GetRole(s.lvlroles[userslevel].RoleId).Color;
						embed.Description += $"**[<@&{s.lvlroles[userslevel].RoleId}>]\n**";
					}
					string progstring = "";
					embed.AddField("Gesamt", $"**```{s.xplist[user.Id]}xp```**", true);
					if (userslevel < s.lvlroles.Count() - 1)
					{
						// 🟦
						for (int i = 0; i < 10; i++)
						{
							if (s.xplist[user.Id] - s.lvlroles[userslevel].XpReq >= ((s.lvlroles[userslevel + 1].XpReq - s.lvlroles[userslevel].XpReq) / 10) * i)
							{
								progstring += "🟦";
							}
							else
							{
								progstring += "⬜";
							}
						}
						embed.AddField("Fortschritt", $"**```{s.xplist[user.Id] - s.lvlroles[userslevel].XpReq}xp / {s.lvlroles[userslevel + 1].XpReq - s.lvlroles[userslevel].XpReq}xp```**\n" + progstring, true);
						embed.AddField("Nächstes level", $"**[Level {s.lvlroles.IndexOf(s.lvlroles[userslevel + 1])}]** | **<@&{s.lvlroles[userslevel + 1].RoleId}> | {s.lvlroles[userslevel + 1].XpReq} Gesamt XP benötigt**", false);
					}
					else
					{
						embed.Fields[0].Value = "Höchstes Level erreicht!";
						// 🟦
						for (int i = 0; i < 10; i++)
						{
							progstring += "🟦";
						}
						embed.AddField("Fortschritt", progstring, true);
					}
					await discord.SendMessageAsync(await discord.GetChannelAsync(e.Message.Channel.Id), embed);
				}
				else
				{
					await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Description = $"**{user.Username}** hat noch keine XP gesammelt!", Color = DiscordColor.Green });
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("lvledit"), CommandClass("LevelCommands"), RequireGuild(), Description("Editiert die erforderliche XP, für eine Rolle in einem Server.\nWenn kein XP Argument angegeben wurde wird die Rolle entfernt. Ansonsten wird die Anzahl der benötigten XP aktualisiert\n\nBenutzung:\n```=lvladd < ID / @mention > [score]```"), RequireUserPermissions2(Permissions.ManageGuild), RequireBotPermissions2(Permissions.ManageRoles & Permissions.SendMessages)]
		public async Task LvlAdd(CommandContext e, DiscordRole role, int score = 0)
		{
			try
			{

				if (e.Guild.Roles.Values.ToList().FindIndex(x => x.Id == role.Id) == -1)
				{
					await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Rolle **{role.Name}** existiert nicht in diesem Server!" });
					return;
				}

				if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
				{
					servers.Add(new RegisteredServer { Id = e.Guild.Id });
					File.WriteAllText("config/RegServers.json", JsonConvert.SerializeObject(servers, Formatting.Indented));
				}

				RegisteredServer s = servers[servers.FindIndex(x => x.Id == e.Guild.Id)];

				if (s.lvlroles.FindIndex(x => x.RoleId == role.Id) != -1)
				{
					LevelRole therole = s.lvlroles.Find(x => x.RoleId == role.Id);
					if (score <= 0)
					{
						s.lvlroles.Remove(s.lvlroles[s.lvlroles.FindIndex(x => x.RoleId == role.Id)]);
						await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Description = $"Rolle {role.Name} gelöscht", Color = DiscordColor.Green });
					}
					else
					{
						s.lvlroles.Find(x => x.RoleId == role.Id);
						s.lvlroles[s.lvlroles.FindIndex(x => x.RoleId == role.Id)].XpReq = score;
						var sortedleederboard = from entry in s.lvlroles orderby entry.XpReq ascending select entry;
						var list = sortedleederboard.ToList();
						s.lvlroles = list;

						await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Description = $"Rolle {role.Name} bearbeitet", Color = DiscordColor.Green });
					}
				}
				else
				{
					s.lvlroles.Add(new LevelRole { Name = role.Name, XpReq = score, RoleId = role.Id });
					var sortedleederboard = from entry in s.lvlroles orderby entry.XpReq ascending select entry;
					var list = sortedleederboard.ToList();
					s.lvlroles = list;
					await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Rolle **{role.Name}** wurde zu **{e.Guild.Name}**'s Levelrollen hinzugefügt!" });
				}

				servers[servers.FindIndex(x => x.Id == e.Guild.Id)] = s;

				File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("xpedit"), CommandClass("LevelCommands"), RequireGuild(), Description("Ändert die Anzahl an XP, die ein Benutzer hat.\nWenn kein XP Argument angegeben wird die XP zu 0 zurückgesetzt. Ansonsten wird es zur eingegebenen Eingabe aktualisiert\n\nBenutzung:\n```=addxp < ID / @mention > [xp]```"), RequireUserPermissions2(Permissions.ManageGuild), RequireBotPermissions2(Permissions.SendMessages)]
		public async Task AddXpUser(CommandContext e, DiscordUser user, int xp = 0)
		{
			try
			{
				if (xp >= 0)
				{
					if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
					{
						servers.Add(new RegisteredServer { Id = e.Guild.Id });
						File.WriteAllText("config/RegServers.json", JsonConvert.SerializeObject(servers, Formatting.Indented));
					}
					if (await e.Guild.GetMemberAsync(user.Id) != null)
					{
						RegisteredServer s = servers[servers.FindIndex(x => x.Id == e.Guild.Id)];
						if (s.xplist.ContainsKey(user.Id))
						{
							if (xp == 0)
							{
								await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"**{user.Username}#{user.Discriminator}**'s xp wurde auf 0 gesetzt!\n**```Vorher: {s.xplist[user.Id]}\nNachher: 0```**" });
								s.xplist.Remove(user.Id);
							}
							else
							{
								await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"**{user.Username}#{user.Discriminator}**'s xp wurde bearbeitet!\n**```Vorher: {s.xplist[user.Id]}xp\nNachher: {xp}xp```**" });
								s.xplist[user.Id] = xp;
							}
						}
						else
						{
							s.xplist.Add(user.Id, xp);
							await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"**{xp}**xp wurde zu **{user.Username}#{user.Discriminator}** hinzugefügt! **```Vorher: 0\nNachher: {xp}xp```**" });
						}
						servers[servers.FindIndex(x => x.Id == e.Guild.Id)] = s;
					}
					else
					{
						await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Nutzer nicht gefunden!" });
					}
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				}
				else
				{
					await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Du kannst keine negativen xp hinzufügen!" });
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("channeledit"), CommandClass("LevelCommands"), Description("Aktiviert/Deaktiviert das verdienen von XP im angegebenen Kanal\n\nBenutzung:\n```=channeledit < ID / #mention >```"), RequireGuild(), RequireUserPermissions2(Permissions.ManageGuild), RequireBotPermissions2(Permissions.SendMessages)]
		public async Task ChannelEdit(CommandContext e, DiscordChannel channel) 
		{
			try
			{
				if (servers.FindIndex(x => x.Id == e.Guild.Id) == -1)
				{
					servers.Add(new RegisteredServer { Id = e.Guild.Id });
					File.WriteAllText("config/RegServers.json", JsonConvert.SerializeObject(servers, Formatting.Indented));
				}
				RegisteredServer s = servers[servers.FindIndex(x => x.Id == e.Guild.Id)];
				if (e.Guild.GetChannel(channel.Id) != null)
				{
					if (s.channelxpexclude.Contains(channel.Id))
					{
						s.channelxpexclude.Remove(channel.Id);
						await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Kanal {channel.Mention} ist nicht mehr ausgenommen vom xp verdienen!" });
						if (s.channelxpexclude.Count == 0)
						{
							servers[servers.FindIndex(x => x.Id == e.Guild.Id)].channelxpexclude.Clear();
						}
					}
					else
					{
						s.channelxpexclude.Add(channel.Id);
						await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Kanal {channel.Mention} ist nun vom xp verdienen ausgenommen!" });
					}
					servers[servers.FindIndex(x => x.Id == e.Guild.Id)] = s;
					File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				}
				else
				{
					await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = $"Kanal nicht gefunden!" });
				}
			}
			catch (Exception ex)
			{
				await AlertException(e, ex);
			}
		}

		[Command("xpreset"), CommandClass("LevelCommands"), RequireGuild(), RequireAuth, Hidden()]
		public async Task ResetXp(CommandContext e, ulong serverid = 0)
		{
			try
			{
				if (serverid == 0)
				{
					servers[servers.FindIndex(x => x.Id == e.Guild.Id)].xplist = new Dictionary<ulong, int>();
				}
				else
				{
					servers[servers.FindIndex(x => x.Id == serverid)].xplist = new Dictionary<ulong, int>();
				}
				File.WriteAllText("config/RegServers.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers, Formatting.Indented));
				await discord.SendMessageAsync(e.Message.Channel, new DiscordEmbedBuilder { Color = DiscordColor.Green, Description = $"Setzt XP vom ganzem Server zurück{serverid}!" });
			}
			catch (Exception ex)
			{
				await AlertException(e, ex); // add addxp command for specific user!
			}
		}
	}
}