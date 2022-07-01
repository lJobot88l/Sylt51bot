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

			foreach (var cclass in Enum.GetValues(typeof(Classes.CommandClasses)))
			{
				string cmdinmod = "|";
				List<string> e = new List<string>();
				foreach(Command cmd in cmds)
				{
					if(((CommandClassAttribute)cmd.CustomAttributes.ToList().Find(x => x.GetType() == typeof(CommandClassAttribute))).Classname.HasFlag((Enum)cclass))
					{
						e.Add(cmd.Name);
					}
				}
				e = (from entry in e orderby (short)entry[0] ascending select entry).ToList<string>();
				foreach(string i in e)
				{
					cmdinmod += $"{i} |";
				}
				_embed.AddField(cclass.ToString(), cmdinmod, true);
			}
			
			_embed.AddField("Nützliche Links", $"[Discord]({cInf.DiscordInvite}) | [GitHub]({cInf.GitHub}) | [Bot Einladung](https://discord.com/oauth2/authorize?client_id={discord.CurrentUser.Id}&scope=bot&permissions=805317632)", false);
			_embed.WithFooter($"Version {cInf.Version}");
			return this;
		}
		public override CommandHelpMessage Build()
		{
			return new CommandHelpMessage(embed: _embed);
			// return new CommandHelpMessage(content: _strBuilder.ToString());
		}
	}
}
