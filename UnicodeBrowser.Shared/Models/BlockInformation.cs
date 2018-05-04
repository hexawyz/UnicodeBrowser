namespace UnicodeBrowser.Models
{
    public sealed class BlockInformation
    {
        public string Name { get; }
		public string Description { get; }
        public CodePointRange Range { get; }

		public BlockInformation(string name, string description, CodePointRange range)
			=> (Name, Description, Range) = (name, description, range);
    }
}
