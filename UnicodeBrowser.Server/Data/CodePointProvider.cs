using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Unicode;

namespace UnicodeBrowser.Data
{
	public static class CodePointProvider
    {
        public static Models.CodePoint[] GetCodePoints(int offset, int limit)
        {
            if (offset < 0 || offset > 0x10FFFF) throw new ArgumentOutOfRangeException(nameof(offset));
            if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));

            var codePoints = new List<Models.CodePoint>();

            int max = Math.Min(offset + limit - 1, 0x10FFFF);

            for (int i = offset; i <= max; i++)
            {
                var charInfo = UnicodeInfo.GetCharInfo(i);

                if (charInfo.Category != UnicodeCategory.OtherNotAssigned)
                {
                    codePoints.Add(GetCodePoint(charInfo));
                }
            }

            return codePoints.ToArray();
        }

        public static Models.CodePoint GetCodePoint(int codePoint) => GetCodePoint(UnicodeInfo.GetCharInfo(codePoint));

        public static Models.CodePoint GetCodePoint(UnicodeCharInfo charInfo)
        {
            var category = UnicodeCategoryInfo.Get(charInfo.Category);
            
            return new Models.CodePoint
            (
                charInfo.CodePoint,
                UnicodeInfo.GetDisplayText(charInfo),
                charInfo.Name,
                charInfo.OldName,
                charInfo.NameAliases.Select(na => new Models.NameAlias((Models.UnicodeNameAliasKind)(int)na.Kind, na.Name)).ToArray(),
                new Models.UnicodeCategory(category.ShortName, category.LongName),
                charInfo.Block,

                (Models.UnicodeNumericType)(int)charInfo.NumericType,
                (Models.UnihanNumericType)(int)charInfo.UnihanNumericType,
                charInfo.NumericValue != null ?
                    new Models.RationalNumber(charInfo.NumericValue.GetValueOrDefault().Numerator, charInfo.NumericValue.GetValueOrDefault().Denominator) :
                    null,

                charInfo.Definition,
                charInfo.MandarinReading,
                charInfo.CantoneseReading,
                charInfo.JapaneseKunReading,
                charInfo.JapaneseOnReading,
                charInfo.KoreanReading,
                charInfo.HangulReading,
                charInfo.VietnameseReading,

                charInfo.SimplifiedVariant,
                charInfo.TraditionalVariant
            );
        }
    }
}
