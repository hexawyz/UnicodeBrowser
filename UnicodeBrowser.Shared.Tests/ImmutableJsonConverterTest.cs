using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnicodeBrowser.Json;
using Xunit;

namespace UnicodeBrowser.Tests
{
    public class ImmutableJsonConverterTest
	{
		private static JsonSerializerOptions CreateJsonSerializerOptions() => new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			IgnoreNullValues = true,
			IgnoreReadOnlyProperties = false,
			Converters =
			{
				new JsonStringEnumConverter(),
			}
		};

		private static JsonSerializerOptions CreateJsonSerializerOptionsWithConverter() => new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			IgnoreNullValues = true,
			IgnoreReadOnlyProperties = false,
			Converters =
			{
				ImmutableJsonConverter.Instance,
				new JsonStringEnumConverter(),
			}
		};

		public sealed class Foo : IEquatable<Foo>
		{
			public Foo(string text, int number)
			{
				Text = text;
				Number = number;
			}

			public string Text { get; }
			public int Number { get; }

			public override bool Equals(object obj) => Equals(obj as Foo);
			public bool Equals(Foo other) => other != null && Text == other.Text && Number == other.Number;
			public override int GetHashCode() => HashCode.Combine(Text, Number);
		}

		public sealed class Bar : IEquatable<Bar>
		{
			public Bar(Foo nested) => Nested = nested;

			public Foo Nested { get; }

			public override bool Equals(object obj) => Equals(obj as Bar);
			public bool Equals(Bar other) => other != null && EqualityComparer<Foo>.Default.Equals(Nested, other.Nested);
			public override int GetHashCode() => HashCode.Combine(Nested);
		}

		[Fact]
		public void ShouldRoundTrip()
		{
			var value = new Bar(new Foo("Hello", 18));

			string json = JsonSerializer.Serialize(value, CreateJsonSerializerOptions());
			var result = JsonSerializer.Deserialize<Bar>(json, CreateJsonSerializerOptionsWithConverter());

			Assert.Equal(value, result);
		}
    }
}
