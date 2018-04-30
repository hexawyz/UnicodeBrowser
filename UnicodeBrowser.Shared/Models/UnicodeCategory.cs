namespace UnicodeBrowser.Models
{
	public sealed class UnicodeCategory
    {
        public string ShortName { get; }
        public string LongName { get; }

        public UnicodeCategory(string shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
