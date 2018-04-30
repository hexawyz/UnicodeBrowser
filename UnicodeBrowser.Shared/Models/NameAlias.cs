namespace UnicodeBrowser.Models
{
	public sealed class NameAlias
    {
        public UnicodeNameAliasKind Kind { get; }
        public string Name { get; }

        public NameAlias(UnicodeNameAliasKind kind, string name)
        {
            Kind = kind;
            Name = name;
        }
    }
}
