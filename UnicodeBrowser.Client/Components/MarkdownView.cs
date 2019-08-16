using System.Collections.Generic;
using System.Linq;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace UnicodeBrowser.Client.Components
{
	/// <summary>A Blazor component for rendering Markdown.</summary>
	/// <remarks>
	/// This component will wrap the converted markdown inside a &lt;div&gt; tag with the <c>mardkown</c> class.
	/// Other classes can be applied by using the class attribute on the component.
	/// </remarks>
	public sealed class MarkdownView : ComponentBase
    {
		private MarkdownDocument _document;

		private string _text;

		[Parameter]
		public string Text
		{
			get => _text;
			set
			{
				if (value != _text)
				{
					_document = (_text = value) != null ?
						Markdig.Markdown.Parse(_text) :
						null;
					StateHasChanged();
				}
			}
		}

		[Parameter(CaptureUnmatchedValues = true)]
		public Dictionary<string, object> InputAttributes { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			// Building the markdown render tree might sometimes be an expensive operation.
			// Thankfully, since the markdown render tree is wrapped in a component,
			// the rendering will only ever occur when the document really has changed.
			// This effectively makes displaying a markdown document cheap enough.

			var inputAttributes = InputAttributes;

			if (inputAttributes.TryGetValue("class", out object obj) && obj is string @class && @class.Length > 0)
			{
				@class += " markdown";
			}
			else
			{
				@class = "markdown";
			}

			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "class", @class);
			builder.AddMultipleAttributes(2, InputAttributes.Where(kvp => !string.Equals(kvp.Key, "class", System.StringComparison.OrdinalIgnoreCase)));
			if (_document != null)
			{
				builder.AddContent(3, b => new RenderFragmentMarkdownRenderer(b).Render(_document));
			}
			builder.CloseElement();

			base.BuildRenderTree(builder);
		}
	}
}
