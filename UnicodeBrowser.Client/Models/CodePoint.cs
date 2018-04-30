namespace UnicodeBrowser.Client.Models
{
	public sealed class CodePoint
    {
        public int Index { get; set; }
        public string DisplayText { get; set; }
        public string Name { get; set; }
        public string OldName { get; set; }
        public NameAlias[] NameAliases { get; set; }
        public UnicodeCategory Category { get; set; }
        public string Block { get; set; }
		
        public string NumericType { get; set; }
        public string UnihanNumericType { get; set; }
        public RationalNumber NumericValue { get; set; }

        public string Definition { get; set; }
        public string MandarinReading { get; set; }
        public string CantoneseReading { get; set; }
        public string JapaneseKunReading { get; set; }
        public string JapaneseOnReading { get; set; }
        public string KoreanReading { get; set; }
        public string HangulReading { get; set; }
        public string VietnameseReading { get; set; }

        public string SimplifiedVariant { get; set; }
        public string TraditionalVariant { get; set; }
    }
}
