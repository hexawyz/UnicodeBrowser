using System;
using System.Collections.Generic;

namespace UnicodeBrowser.Client.Models
{
    public sealed class BlockInformation : IEquatable<BlockInformation>
    {
        public string Name { get; set; }
        public string Description { get; set; }
		public CodePointRange Range { get; set; }

		public override bool Equals(object obj)
			=> Equals(obj as BlockInformation);

		public bool Equals(BlockInformation other)
			=> other != null &&
				Name == other.Name &&
				Description == other.Description &&
				EqualityComparer<CodePointRange>.Default.Equals(Range, other.Range);

		public override int GetHashCode()
		{
			var hashCode = -1020047000;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
			hashCode = hashCode * -1521134295 + EqualityComparer<CodePointRange>.Default.GetHashCode(Range);
			return hashCode;
		}
	}
}
