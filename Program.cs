using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Classes;
using System.Globalization;

namespace Sylt51bot
{
    class Program
    {
		public static CommandsNextExtension commands;
		public static DiscordClient discord;
		public static DiscordActivity g1;
		public static ulong LastHb = 0; // Last heartbeat message
		public static SetupInfo cInf; // The setup info
        
		public static CommandsNextConfiguration cNcfg; // The commanddsnext config
		public static DiscordConfiguration dCfg; // The discord config
		public static List<RegisteredServer> servers; // The list registered of servers
		static void Main(string[] args)
        {
            try
            {
                if (File.Exists("config/mconfig.json"))
                {
                    cInf = Newtonsoft.Json.JsonConvert.DeserializeObject<SetupInfo>(File.ReadAllText("config/mconfig.json"));
                    if(File.Exists("config/xpcfg.json"))
                    {
                        servers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RegisteredServer>>(File.ReadAllText("config/xpcfg.json"));
                    }
                }
                else
                {
                    Console.WriteLine("Missing setup info");
                    Environment.Exit(0);
                }
                cNcfg = new CommandsNextConfiguration
                {
                    StringPrefixes = cInf.Prefixes,
                    CaseSensitive = false,
                    EnableDefaultHelp = true,
                    DefaultHelpChecks = new List<CheckBaseAttribute>()
                };
                dCfg = new DiscordConfiguration
                {
                    Token = cInf.Token,
                    TokenType = TokenType.Bot
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(0);
            }
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            try
            {
                discord = new DiscordClient(dCfg);
                commands = discord.UseCommandsNext(cNcfg);

                commands.CommandErrored += CmdErrorHandler;
                commands.SetHelpFormatter<CustomHelpFormatter>();
				commands.RegisterCommands<LevelCommands>();
				commands.RegisterCommands<BotAdminCommands>();
				commands.RegisterCommands<GenCommands>();
                discord.MessageCreated += async (client, e) =>
                {
					if(!e.Message.Author.IsBot && e.Message.Content.Contains("€"))
					{
						// get the number of euros from text
						string[] split = e.Message.Content.Split('€');
						string euroamt = split[0].Substring(split[0].LastIndexOf(" ") + 1);
						if(euroamt.Contains(","))
						{
							euroamt = euroamt.Replace(",", ".");
						}
						long Schulden = 86300000000;
						if(double.TryParse(euroamt, out double amt) && amt <= 1000 && amt > 0 && !double.IsNaN(amt))
						{
							cInf.SchuldenDerDDR -= amt * 1.95583;
							await e.Message.RespondAsync($"Das sind {Math.Round(amt * 1.95583, 1)} Mark. {Math.Round(amt * 1.95583 * 2, 1)} Ostmark. {Math.Round(amt * 1.95583 * 2 * 10, 1)} Ostmark aufm Schwarzmarkt.\nVon den bisherigen Zwietracht-Pfostierungen hätte man {(1 - (double)cInf.SchuldenDerDDR/(double)Schulden).ToString("##0.#####%") } der DDR entschulden können.");
							File.WriteAllText("config/mconfig.json", Newtonsoft.Json.JsonConvert.SerializeObject(cInf));
						}
						
					}
                };

                discord.SocketClosed += async (client, e) =>
                {
                    await discord.ReconnectAsync();
                };

                await discord.ConnectAsync();
                await SendHeartbeatAsync().ConfigureAwait(false);
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {           // Next: LevelSystem & helpmenu import from zeisu
                try
				{
					Console.WriteLine("CONNECTION TERMINATED\nAttempting automatic restart...");
					File.WriteAllText("Error.log", ex.ToString());
					Main(new string[]{});
				}
				catch
				{
					Console.WriteLine("Automatic restart failed.");
				}
            }
        }
        static async  Task CmdErrorHandler(CommandsNextExtension _m, CommandErrorEventArgs e)
        {
            try
			{
				var failedChecks = ((DSharpPlus.CommandsNext.Exceptions.ChecksFailedException)e.Exception).FailedChecks;
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = "Command couldn't execute D:\nHere's why:" };
				bool canSend = true;
				foreach (var failedCheck in failedChecks)
				{
					if (failedCheck is RequireBotPermissionsAttribute)
					{
						var botperm = (RequireBotPermissionsAttribute)failedCheck;
						embed.AddField("My Required Permissions", $"```{botperm.Permissions.ToPermissionString()}```");
						if (botperm.Permissions.HasFlag(Permissions.SendMessages))
						{
							canSend = false;
						}
					}
					if (failedCheck is RequireUserPermissionsAttribute)
					{
						var botperm = (RequireUserPermissionsAttribute)failedCheck;
						embed.AddField("Your Required Permissions", $"```{botperm.Permissions.ToPermissionString()}```");
					}
					if (failedCheck is RequireGuildAttribute)
					{
						RequireGuildAttribute guild = (RequireGuildAttribute)failedCheck;
						embed.AddField("Server only", "This command can not be used in DMs.");
					}
				}
				if (canSend == true)
				{
					await e.Context.Message.RespondAsync(embed);
				}
				else
				{
					await e.Context.Guild.Owner.SendMessageAsync("I can't send messages in your server but I'm lacking perms to work so have this list in DMs instead", embed);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
				Console.WriteLine(e.Exception.ToString());
			}
        }
		public static async Task AlertException(CommandContext e, Exception ex)
		{
			await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = "An error occured" });
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
			await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), Newtonsoft.Json.JsonConvert.SerializeObject(ex));
		}

		public static async Task AlertException(MessageCreateEventArgs e, Exception ex)
		{
			await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = "An error occured" });
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
			await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), Newtonsoft.Json.JsonConvert.SerializeObject(ex));
		}

		public static async Task AlertException(MessageReactionAddEventArgs e, Exception ex)
		{
			await e.Message.RespondAsync(new DiscordEmbedBuilder { Color = DiscordColor.Red, Description = "An error occured" });
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
			await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), Newtonsoft.Json.JsonConvert.SerializeObject(ex));
		}

		public static async Task AlertException(Exception ex)
		{
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
			await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), Newtonsoft.Json.JsonConvert.SerializeObject(ex));
		}
		public static async Task SendHeartbeatAsync()
		{
			while (true)
			{
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder { Description = $"Heartbeat received!\n{discord.Ping.ToString()}ms" };
					int ping = discord.Ping;
					embed.WithFooter($"Today at [{System.DateTime.UtcNow.ToShortTimeString()}]");
					if (ping < 200)
					{
						embed.Color = DiscordColor.Green;
					}
					else if (ping < 500)
					{
						embed.Color = DiscordColor.Orange;
					}
					else
					{
						embed.Color = DiscordColor.Red;
					}
					DiscordMessage msghb = null;
					msghb = await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), embed);


					await discord.UpdateStatusAsync(g1);
					Console.WriteLine($"{System.DateTime.UtcNow.ToShortTimeString()} Ping: {discord.Ping}ms ");
					if (LastHb != 0)
					{
						try
						{
							DiscordChannel hbch = await discord.GetChannelAsync(cInf.ErrorHbChannel);
							DiscordMessage hbmsg = await hbch.GetMessageAsync(LastHb);
							await hbmsg.DeleteAsync();
						}
						catch { }
					}
					LastHb = msghb.Id;
					foreach (RegisteredServer e in servers)
					{
						foreach (KeyValuePair<ulong, DateTime> kvp in e.timedoutedusers)
						{
							if (DateTime.Now - kvp.Value >= e.CoolDown)
							{
                                servers[servers.FindIndex(x => x.Id == e.Id)].timedoutedusers.Remove(kvp.Key);
							}
						}
					}
                    File.WriteAllText("config/xpcfg.json", Newtonsoft.Json.JsonConvert.SerializeObject(servers));
				}
				catch (Exception ex)
				{
					await discord.SendMessageAsync(await discord.GetChannelAsync(cInf.ErrorHbChannel), $"Failed to heartbeat\n\n{ex.ToString()}");
				}
				await Task.Delay(TimeSpan.FromMinutes(10));
			}
		}
    }
	
}

namespace CAttributes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	public class CommandClassAttribute : System.Attribute
	{
		public string classname { get; set; }
		public CommandClassAttribute(string e)
		{
			classname = e;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class RequireAuthAttribute : CheckBaseAttribute   // Requires the user to be authenticated
	{
		public override Task<bool> ExecuteCheckAsync(CommandContext e, bool help)
		{
			return Task.FromResult(Sylt51bot.Program.cInf.AuthUsers.Contains(e.User.Id));
		}
	}
}

namespace Classes
{
	public class SetupInfo
	{
        // Main Info
		public string Token { get; set; }
		public ulong ErrorHbChannel { get; set; }
		public List<string> Prefixes { get; set; }
        // Links
		public string DiscordInvite { get; set; } = null;
		public string GitHub { get; set; } = null;
        public List<ulong> AuthUsers { get; set; } = null;
		public double SchuldenDerDDR { get; set; } = 86300000000;
	}

	public class RegisteredServer
	{
		public ulong Id { get; set; }
        public Dictionary<ulong, int> xplist = null;
        public Dictionary<ulong, DateTime> timedoutedusers = null;
        public List<LevelRole> lvlroles = null;
        public List<ulong> channelxpexclude = null;
		public int MinXp { get; set; } = 10;
		public int MaxXp { get; set; } = 20;
		public TimeSpan CoolDown { get; set; } = TimeSpan.FromMinutes(2);
	}
	public class LevelRole
	{
		public string Name { get; set; }
		public ulong RoleId { get; set; }
		public int XpReq { get; set; }
	}
}