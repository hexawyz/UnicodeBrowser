using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Globalization;
using System.Text;

namespace UnicodeBrowser.Client
{
	// A markdown renderer that outputs to a RenderTreeBuilder instance.
	// Strongly inspired by the HtmlMarkdownRenderer from Markdig, but simplified for use within Blazor.
	internal class RenderFragmentMarkdownRenderer : RendererBase
	{
		private readonly RenderTreeBuilder _builder;
		private int _sequence = -1;

		public RenderFragmentMarkdownRenderer(RenderTreeBuilder builder)
		{
			_builder = builder;

			// Block renderers
			ObjectRenderers.Add(new CodeBlockRenderer());
			ObjectRenderers.Add(new ListBlockRenderer());
			ObjectRenderers.Add(new HeadingBlockRenderer());
			ObjectRenderers.Add(new HtmlBlockRenderer());
			ObjectRenderers.Add(new ParagraphBlockRenderer());
			ObjectRenderers.Add(new QuoteBlockRenderer());
			ObjectRenderers.Add(new ThematicBreakRenderer());

			// Inline renderers
			ObjectRenderers.Add(new AutolinkInlineRenderer());
			ObjectRenderers.Add(new CodeInlineRenderer());
			ObjectRenderers.Add(new DelimiterInlineRenderer());
			ObjectRenderers.Add(new EmphasisInlineRenderer());
			ObjectRenderers.Add(new LineBreakInlineRenderer());
			ObjectRenderers.Add(new HtmlInlineRenderer());
			ObjectRenderers.Add(new HtmlEntityInlineInlineRenderer());
			ObjectRenderers.Add(new LinkInlineRenderer());
			ObjectRenderers.Add(new LiteralInlineRenderer());
		}
		
		public bool ImplicitParagraph { get; set; }

		internal RenderFragmentMarkdownRenderer WriteElementStart(string elementName)
		{
			_builder.OpenElement(++_sequence, elementName);
			return this;
		}

		internal RenderFragmentMarkdownRenderer WriteAttribute(string name, string value)
		{
			_builder.AddAttribute(++_sequence, name, value);
			return this;
		}

		internal RenderFragmentMarkdownRenderer WriteAttributes(HtmlAttributes attributes)
		{
			if (attributes != null)
			{
				if (attributes.Id != null) _builder.AddAttribute(++_sequence, "id", attributes.Id);
				if (attributes.Classes != null && attributes.Classes.Count > 0) _builder.AddAttribute(++_sequence, "class", string.Join(" ", attributes.Classes));
				if (attributes.Properties != null)
				{
					foreach (var property in attributes.Properties)
					{
						_builder.AddAttribute(++_sequence, property.Key, property.Value);
					}
				}
			}
			return this;
		}

		internal RenderFragmentMarkdownRenderer WriteText(string text)
		{
			_builder.AddContent(++_sequence, text);
			return this;
		}

		internal RenderFragmentMarkdownRenderer WriteLeafLines(LeafBlock leafBlock)
		{
			var lines = leafBlock.Lines;
			if (lines.Lines != null && lines.Count > 0)
			{
				var sb = new StringBuilder();

				for (int i = 0; i < lines.Count; i++)
				{
					var line = lines.Lines[i];

					if (!line.Slice.IsEmpty)
					{
						sb.Append(line.Slice.Text, line.Slice.Start, line.Slice.Length);
					}
					sb.AppendLine();
				}

				_builder.AddContent(++_sequence, sb.ToString());
			}
			return this;
		}

		public RenderFragmentMarkdownRenderer WriteLeafInline(LeafBlock leafBlock)
		{
			var inline = (Inline)leafBlock.Inline;
			if (inline != null)
			{
				while (inline != null)
				{
					Write(inline);
					inline = inline.NextSibling;
				}
			}
			return this;
		}

		internal RenderFragmentMarkdownRenderer WriteElementEnd()
		{
			_builder.CloseElement();
			return this;
		}

		public override object Render(MarkdownObject markdownObject)
		{
			Write(markdownObject);
			return _builder;
		}
	}

	// There are a lot of renderers to implement, and I was too lazy to split them in their own files,
	// especially considering the very limited amount of code each of these renderers need.
	// Since Blazor will do all the DOM manipulation on its own, most of the renderers are pretty trivial to implement.

	internal abstract class RenderFragmentMarkdownObjectRenderer<T> : MarkdownObjectRenderer<RenderFragmentMarkdownRenderer, T>
		where T : MarkdownObject
	{
	}

	internal sealed class CodeBlockRenderer : RenderFragmentMarkdownObjectRenderer<CodeBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, CodeBlock obj)
			=> renderer.WriteElementStart("div")
				.WriteAttributes(obj.TryGetAttributes())
				.WriteLeafLines(obj)
				.WriteElementEnd();
	}

	internal sealed class ListBlockRenderer : RenderFragmentMarkdownObjectRenderer<ListBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, ListBlock obj)
		{
			renderer.WriteElementStart(obj.IsOrdered ? "ol" : "ul")
				.WriteAttributes(obj.TryGetAttributes());

			foreach (ListItemBlock item in obj)
			{
				var oldImplicitParagraph = renderer.ImplicitParagraph;
				renderer.ImplicitParagraph = !obj.IsLoose;

				renderer.WriteElementStart("li").WriteAttributes(item.TryGetAttributes());
				renderer.WriteChildren(item);
				renderer.WriteElementEnd();

				renderer.ImplicitParagraph = oldImplicitParagraph;
			}
		}
	}

	internal sealed class HeadingBlockRenderer : RenderFragmentMarkdownObjectRenderer<HeadingBlock>
	{
		private static string[] KnownHeadingTags = { "h1", "h2", "h3", "h4", "h5", "h6" };

		protected override void Write(RenderFragmentMarkdownRenderer renderer, HeadingBlock obj)
			=> renderer.WriteElementStart(obj.Level > 0 && obj.Level <= 6 ? KnownHeadingTags[obj.Level - 1] : "h" + obj.Level.ToString(CultureInfo.InvariantCulture))
				.WriteAttributes(obj.TryGetAttributes())
				.WriteLeafInline(obj)
				.WriteElementEnd();
	}

	internal sealed class ParagraphBlockRenderer : RenderFragmentMarkdownObjectRenderer<ParagraphBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, ParagraphBlock obj)
			=> renderer.WriteElementStart("p")
				.WriteAttributes(obj.TryGetAttributes())
				.WriteLeafInline(obj)
				.WriteElementEnd();
	}

	internal sealed class QuoteBlockRenderer : RenderFragmentMarkdownObjectRenderer<QuoteBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, QuoteBlock obj)
		{
			renderer.WriteElementStart("blockquote")
				.WriteAttributes(obj.TryGetAttributes());

			var oldImplicitParagraph = renderer.ImplicitParagraph;
			renderer.ImplicitParagraph = false;
			renderer.WriteChildren(obj);
			renderer.ImplicitParagraph = oldImplicitParagraph;

			renderer.WriteElementEnd();
		}
	}

	internal sealed class ThematicBreakBlockRenderer : RenderFragmentMarkdownObjectRenderer<ThematicBreakBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, ThematicBreakBlock obj)
			=> renderer.WriteElementStart("hr").WriteElementEnd();
	}

	internal sealed class AutolinkInlineRenderer : RenderFragmentMarkdownObjectRenderer<AutolinkInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, AutolinkInline obj)
			=> renderer.WriteElementStart("a")
				.WriteAttribute("href", obj.IsEmail ? "mailto:" + obj.Url : obj.Url)
				.WriteAttributes(obj.TryGetAttributes())
				.WriteText(obj.Url)
				.WriteElementEnd();
	}

	internal sealed class LinkInlineRenderer : RenderFragmentMarkdownObjectRenderer<LinkInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, LinkInline obj)
		{
			string url = obj.GetDynamicUrl?.Invoke() ?? obj.Url;
			
			renderer.WriteElementStart( obj.IsImage ? "img" : "a")
				.WriteAttribute(obj.IsImage ? "src" : "href", url);

			if (obj.IsImage)
			{
				renderer.WriteAttribute("alt", "** Image **"); // Don't wanna bother implementing this yet. It would require creating a nested HTML renderer + StringWriter.
			}

			if (!string.IsNullOrEmpty(obj.Title))
			{
				renderer.WriteAttribute("title", obj.Title);
			}

			if (!obj.IsImage)
			{
				renderer.WriteChildren(obj);
			}

			renderer.WriteElementEnd();
		}
	}

	internal sealed class CodeInlineRenderer : RenderFragmentMarkdownObjectRenderer<CodeInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, CodeInline obj)
			=> renderer.WriteElementStart("code")
				.WriteAttributes(obj.TryGetAttributes())
				.WriteText(obj.Content)
				.WriteElementEnd();
	}

	internal sealed class DelimiterInlineRenderer : RenderFragmentMarkdownObjectRenderer<DelimiterInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, DelimiterInline obj)
			=> renderer.WriteText(obj.ToLiteral()).WriteChildren(obj);
	}

	internal sealed class EmphasisInlineRenderer : RenderFragmentMarkdownObjectRenderer<EmphasisInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, EmphasisInline obj)
		{
			renderer.WriteElementStart(obj.IsDouble ? "strong" : "em")
				   .WriteAttributes(obj.TryGetAttributes())
				   .WriteChildren(obj);
			renderer.WriteElementEnd();
		}
	}

	internal sealed class HtmlEntityInlineInlineRenderer : RenderFragmentMarkdownObjectRenderer<HtmlEntityInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, HtmlEntityInline obj)
			=> renderer.WriteText(obj.Transcoded.ToString());
	}

	internal sealed class LineBreakInlineRenderer : RenderFragmentMarkdownObjectRenderer<LineBreakInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, LineBreakInline obj)
			=> renderer.WriteElementStart("br")
				.WriteElementEnd();
	}

	internal sealed class LiteralInlineRenderer : RenderFragmentMarkdownObjectRenderer<LiteralInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, LiteralInline obj)
			=> renderer.WriteText(obj.Content.ToString());
	}

	// Don't support HTML rendering for now

	internal sealed class HtmlBlockRenderer : RenderFragmentMarkdownObjectRenderer<HtmlBlock>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, HtmlBlock obj)
			=> throw new NotImplementedException();
	}

	internal sealed class HtmlInlineRenderer : RenderFragmentMarkdownObjectRenderer<HtmlInline>
	{
		protected override void Write(RenderFragmentMarkdownRenderer renderer, HtmlInline obj)
			=> throw new NotImplementedException();
	}
}
