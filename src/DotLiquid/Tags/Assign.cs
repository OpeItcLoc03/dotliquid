using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid.Tags
{
	/// <summary>
	/// Assign sets a variable in your template.
	///
	/// {% assign foo = 'monkey' %}
	///
	/// You can then use the variable later in the page.
	///
	/// {{ foo }}
	/// </summary>
	public class Assign : Tag
	{
		protected static readonly Regex Syntax = R.B(R.Q(@"({0}+)\s*=\s*(.*)\s*"), Liquid.VariableSignature);

		protected string to;
		protected Variable from;

		public override void Initialize(string tagName, string markup, List<string> tokens)
		{
			Match syntaxMatch = Syntax.Match(markup);
			if (syntaxMatch.Success)
			{
				to = syntaxMatch.Groups[1].Value;
				from = new Variable(syntaxMatch.Groups[2].Value);
			}
			else
			{
				throw new SyntaxException(Liquid.ResourceManager.GetString("AssignTagSyntaxException"));
			}

			base.Initialize(tagName, markup, tokens);
		}

		public override void Render(Context context, TextWriter result)
		{
			context.Scopes.Last()[to] = from.Render(context);
		}
	}
}