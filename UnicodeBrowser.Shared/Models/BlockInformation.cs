namespace UnicodeBrowser.Models
{
    public sealed class BlockInformation
    {
        public string Name { get; }
        public CodePointRange Range { get; }

        public BlockInformation(string name, CodePointRange range)
        {
            Name = name;
            Range = range;
        }
    }
}
