using Markdig.Syntax;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace UnicodeBrowser.Client.Shared
{
	/// <summary>A Blazor component for rendering Markdown.</summary>
	/// <remarks>
	/// This component will wrap the converted markdown inside a &lt;div&gt; tag with the <c>mardkown</c> class.
	/// Other classes can be applied by using the class attribute on the component.
	/// </remarks>
	public sealed class MarkdownView : BlazorComponent
    {
		private MarkdownDocument _document;

		private string _text;

		[Parameter]
		private string Text
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

		[Parameter]
		private string Class { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			// Building the markdown render tree might sometimes be an expensive operation.
			// Thankfully, since the markdown render tree is wrapped in a component,
			// the rendering will only ever occur when the document really has changed.
			// This effectively makes displaying a markdown document cheap enough.

			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "class", !string.IsNullOrWhiteSpace(Class) ? Class + " markdown" : "markdown");
			if (_document != null)
			{
				builder.AddContent(2, b => new RenderFragmentMarkdownRenderer(b).Render(_document));
			}
			builder.CloseElement();

			base.BuildRenderTree(builder);
		}
	}
}
