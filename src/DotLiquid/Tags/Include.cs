using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.FileSystems;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	public class Include : DotLiquid.Block
	{
		protected static readonly Regex Syntax = new Regex(string.Format(@"({0}+)(\s+(?:with|for)\s+({0}+))?", Liquid.QuotedFragment));

		protected string templateName, variableName;
		protected Dictionary<string, string> attributes;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				templateName = syntaxMatch.Groups[1].Value;
				variableName = syntaxMatch.Groups[3].Value;
				if (variableName == string.Empty)
					variableName = null;
				attributes = new Dictionary<string, string>(Template.NamingConvention.StringComparer);
				R.Scan(markup, Liquid.TagAttributes, (key, value) => attributes[key] = value);
			}
			else
				throw new SyntaxException(Liquid.ResourceManager.GetString("IncludeTagSyntaxException"));

			base.Initialize(tagName, markup, tokens);
		}

		protected override void Parse(List<string> tokens)
		{
		}

		public override void Render(Context context, TextWriter result)
		{
			IFileSystem fileSystem = context.Registers["file_system"] as IFileSystem ?? Template.FileSystem;
			string source = fileSystem.ReadTemplateFile(context, templateName);
			Template partial = Template.Parse(source);

			string shortenedTemplateName = templateName.Substring(1, templateName.Length - 2);
			object variable = context[variableName ?? shortenedTemplateName];

			context.Stack(() =>
			{
				foreach (var keyValue in attributes)
					context[keyValue.Key] = context[keyValue.Value];

				if (variable is IEnumerable)
				{
					((IEnumerable) variable).Cast<object>().ToList().ForEach(v =>
					{
						context[shortenedTemplateName] = v;
						partial.Render(result, RenderParameters.FromContext(context));
					});
					return;
				}

				context[shortenedTemplateName] = variable;
				partial.Render(result, RenderParameters.FromContext(context));
			});
		}
	}
}