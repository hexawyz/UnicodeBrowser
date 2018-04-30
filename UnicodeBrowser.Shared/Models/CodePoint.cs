namespace UnicodeBrowser.Models
{
	public sealed class CodePoint
    {
        public int Index { get; }
        public string DisplayText { get; }
        public string Name { get; }
        public string OldName { get; }
        public NameAlias[] NameAliases { get; }
        public UnicodeCategory Category { get; }
        public string Block { get; }
		
        public UnicodeNumericType NumericType { get; }
        public UnihanNumericType UnihanNumericType { get; }
        public RationalNumber NumericValue { get; }

        public string Definition { get; }
        public string MandarinReading { get; }
        public string CantoneseReading { get; }
        public string JapaneseKunReading { get; }
        public string JapaneseOnReading { get; }
        public string KoreanReading { get; }
        public string HangulReading { get; }
        public string VietnameseReading { get; }

        public string SimplifiedVariant { get; }
        public string TraditionalVariant { get; }

        public CodePoint
        (
            int index,
            string displayText,
            string name,
            string oldName,
            NameAlias[] nameAliases,
            UnicodeCategory category,
            string block,

            UnicodeNumericType numericType,
            UnihanNumericType unihanNumericType,
            RationalNumber numericValue,

            string definition,
            string mandarinReading,
            string cantoneseReading,
            string japaneseKunReading,
            string japaneseOnReading,
            string koreanReading,
            string hangulReading,
            string vietnameseReading,

            string simplifiedVariant,
            string traditionalVariant
        )
        {
            Index = index;
            DisplayText = displayText;
            Name = name;
            OldName = oldName;
            NameAliases = nameAliases;
            Category = category;
            Block = block;

            NumericType = numericType;
            UnihanNumericType = unihanNumericType;
            NumericValue = numericValue;

            Definition = definition;
            MandarinReading = mandarinReading;
            CantoneseReading = cantoneseReading;
            JapaneseKunReading = japaneseKunReading;
            JapaneseOnReading = japaneseOnReading;
            KoreanReading = koreanReading;
            HangulReading = hangulReading;
            VietnameseReading = vietnameseReading;

            SimplifiedVariant = simplifiedVariant;
            TraditionalVariant = traditionalVariant;
        }
    }
}
