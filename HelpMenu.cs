using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Converters;
using static Sylt51bot.Program;
using CAttributes;
namespace Sylt51bot
{
	public class CustomHelpFormatter : BaseHelpFormatter
	{
		protected DiscordEmbedBuilder _embed;
		// protected StringBuilder _strBuilder;

		public CustomHelpFormatter(CommandContext ctx) : base(ctx)
		{
			_embed = new DiscordEmbedBuilder();

			// _strBuilder = new StringBuilder();

			// Help formatters do support dependency injection.
			// Any required services can be specified by declaring constructor parameters. 

			// Other required initialization here ...
		}

		public override BaseHelpFormatter WithCommand(Command command)
		{
			// _strBuilder.AppendLine($"{command.Name} - {command.Description}");
			_embed.Title = "Hilfe";
			_embed.Color = DiscordColor.Green;
			if (string.IsNullOrEmpty(command.Description))
			{
				_embed.AddField(command.Name, "Keine Beschreibung vorhanden");
			}
			else
			{
				_embed.AddField(command.Name, command.Description);
			}
			if (command.Aliases.Count != 0)
			{
				string alstring = "";
				foreach (var alias in command.Aliases)
				{
					alstring += $"{alias} ";
				}
				_embed.AddField("Aliases", alstring);
			}
			var p = command.ExecutionChecks.ToList();
			string permstr = "";
			string permstr2 = "";
			foreach (var p1 in p)
			{
				if (p1.GetType() == typeof(RequireBotPermissionsAttribute))
				{
					permstr += ((RequireBotPermissionsAttribute)p1).Permissions.ToString() + " ";
				}
				if (p1.GetType() == typeof(RequireUserPermissionsAttribute))
				{
					permstr2 += ((RequireUserPermissionsAttribute)p1).Permissions.ToString() + " ";
				}
			}
			if (permstr != "")
			{
				permstr = $"**Meine Berechtigungen:** ```{permstr}```\n";
			}
			if (permstr2 != "")
			{
				permstr2 = $"**Deine Berechtigungen:** ```{permstr2}```";
			}
			if (permstr != "" || permstr2 != "")
			{
				_embed.AddField("Berechtigungen", permstr + permstr2);
			}
			_embed.WithFooter("<> sind Pflichtargumente, [] sind optional");
			return this;
		}

		public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
		{
			_embed.Title = "Hilfe";
			_embed.Description = "Mehr Informationen über einen Befehl mit `=help <befehlname>`";
			_embed.Color = DiscordColor.Green;
			string owneronlycommands = "";
			string othercommands = "";
			string xpcommands = "";
			List<string> e = new List<string>();
			foreach (var cmd in cmds)
			{
				var a = cmd.CustomAttributes.ToList().Find(x => x.GetType() == typeof(CommandClassAttribute));
				CommandClassAttribute t = (CommandClassAttribute)a;
				try
				{
					switch (t.classname)
					{
						case "OwnerCommands":
							owneronlycommands += $" `{cmd.Name}`";
							break;
						case "OtherCommands":
							othercommands += $" `{cmd.Name}`";
							break;
						case "LevelCommands":
							xpcommands += $" `{cmd.Name}`";
							break;
						default:
							Console.WriteLine("err");
							break;
					}
				}
				catch
				{

				}
				// _strBuilder.AppendLine($"{cmd.Name} - {cmd.Description}");
			}
			if(owneronlycommands != "")
			{
				_embed.AddField("Besitzerbefehle", owneronlycommands, true);
			}
			if (othercommands != "")
			{
				_embed.AddField("Sonstige Befehle", othercommands, true);
			}
			//_embed.AddField("Self Role Commands", selfrolecommands);
			if (xpcommands != "")
			{
				_embed.AddField("Level Befehle", xpcommands, true);
			}
			_embed.AddField("Nützliche Links", $"[[Discord]({cInf.DiscordInvite})] | [[GitHub]({cInf.GitHub})] | [[Bot Einladung](https://discord.com/oauth2/authorize?client_id={discord.CurrentUser.Id}&scope=bot&permissions=805317632)]", false);
			return this;
		}
		public override CommandHelpMessage Build()
		{
			return new CommandHelpMessage(embed: _embed);
			// return new CommandHelpMessage(content: _strBuilder.ToString());
		}
	}
}
