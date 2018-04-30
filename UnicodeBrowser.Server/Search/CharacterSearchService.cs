using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;
using UnicodeBrowser.Services;

namespace UnicodeBrowser.Search
{
    public class CharacterSearchService : ICharacterSearchService
    {
        private sealed class MatchedCodePointComparer : IComparer<KeyValuePair<int, int>>
        {
            public static readonly MatchedCodePointComparer Default = new MatchedCodePointComparer();

            private MatchedCodePointComparer() { }

            public int Compare(KeyValuePair<int, int> x, KeyValuePair<int, int> y)
            {
                // The key is the code point (unique value)
                // The value is the exact match score (the higher, the better)

                // We want to sort by exactness in descending order, and by code point in ascending order.

                if (x.Value > y.Value)
                {
                    return -1;
                }
                else if (x.Value < y.Value)
                {
                    return 1;
                }
                else if (x.Key < y.Key)
                {
                    return -1;
                }
                else if (x.Key > y.Key)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private static int[] EmptyInt32Array = new int[0];

        private static readonly char[] wordSeparators = new[] { ' ', '-' };
        private static readonly char[] simplifiedWordSeparators = new[] { ' ' };

        private readonly Task _initializationTask;
        private Node _wordIndex;

		public CharacterSearchService()
        {
            _initializationTask = Task.Run((Action)Initialize);
        }

        private void Initialize()
        {
            var builder = new IndexBuilder();

            for (int i = 0; i <= 0x10FFFF; i++)
            {
                var charInfo = UnicodeInfo.GetCharInfo(i);
                string name = charInfo.Name;

                if (!string.IsNullOrEmpty(name))
                {
                    foreach (string word in name.Split(i == 0x1180 ? simplifiedWordSeparators : wordSeparators))
                    {
                        builder.AddWord(word, i);
                    }
                }

                if (!string.IsNullOrEmpty(charInfo.OldName))
                {
                    foreach (string word in RemoveForbiddenCharacters(charInfo.OldName).Split(wordSeparators))
                    {
                        builder.AddWord(word, i);
                    }
                }

                if (charInfo.NameAliases.Count > 0)
                {
                    foreach (var nameAlias in charInfo.NameAliases)
                    {
                        foreach (string word in nameAlias.Name.Split(wordSeparators))
                        {
                            builder.AddWord(word, i);
                        }
                    }
                }
            }

            _wordIndex = builder.Build();

            GC.Collect();
        }

        public Task<IEnumerable<int>> FindCharactersAsync(string text)
        {
            return _initializationTask.IsCompleted ?
                Task.FromResult(FindCharacters(text)) :
                FindCharactersOnceInitializedAsync(text);
        }

        private async Task<IEnumerable<int>> FindCharactersOnceInitializedAsync(string text)
        {
            await _initializationTask.ConfigureAwait(false);
            return FindCharacters(text);
        }

        private IEnumerable<int> FindCharacters(string text)
        {
            if (text != null && text.Length > 0)
            {
                var codePointEnumerators =
                (
                    from word in SplitWords(text)
                    select GetCodePoints(word).GetEnumerator()
                ).ToArray();

                var codePointList = new List<KeyValuePair<int, int>>();

                int uniqueCodePoint = -1;

                // If the text is a single unicode char, add it to the top of the search results.
                if (text.Length == 1 || text.Length == 2 && char.IsHighSurrogate(text[0]) && char.IsLowSurrogate(text[1]))
                {
                    codePointList.Add(new KeyValuePair<int, int>(uniqueCodePoint = char.ConvertToUtf32(text, 0), codePointEnumerators.Length + 1));
                }

                if (codePointEnumerators.Length > 0)
                {
                    for (int i = 0; i < codePointEnumerators.Length; i++)
                    {
                        if (!codePointEnumerators[i].MoveNext()) goto SortResults;
                    }

                    while (true)
                    {
                        int matchedCodePoint;
                        FoundCodePoint currentCodePoint;
                        int exactMatchCount = 0;
                        bool found = true;

                        currentCodePoint = codePointEnumerators[0].Current;
                        matchedCodePoint = currentCodePoint.Value;
                        if (matchedCodePoint != uniqueCodePoint) // Only process the code point if it was not already added to the list by direct match.
                        {
                            if (currentCodePoint.IsExactMatch) exactMatchCount++;

                            for (int i = 1; i < codePointEnumerators.Length; i++)
                            {
                                while ((currentCodePoint = codePointEnumerators[i].Current).Value < matchedCodePoint)
                                {
                                    if (!codePointEnumerators[i].MoveNext()) goto SortResults;
                                }

                                if (!(found = currentCodePoint.Value == matchedCodePoint)) break;
                                if (currentCodePoint.IsExactMatch) exactMatchCount++;
                            }

                            if (found) codePointList.Add(new KeyValuePair<int, int>(matchedCodePoint, exactMatchCount));
                        }
                        if (!codePointEnumerators[0].MoveNext()) goto SortResults;
                    }

                    SortResults:;
                    codePointList.Sort(MatchedCodePointComparer.Default);

                    return codePointList.Select(cp => cp.Key);
                }
            }

            return EmptyInt32Array;
        }

        private IEnumerable<FoundCodePoint> GetCodePoints(string word)
        {
            Node node;

            _wordIndex.TryFind(word, out node);

            return node.GetCodePoints();
        }

        private static string RemoveForbiddenCharacters(string text)
        {
            int o, i;
            char c;

            for (o = 0; o < text.Length; o++)
            {
                c = text[o];

                if (c != '-' && (c < '0' || c > 'z' || c > '9' && c < 'A' || c > 'Z' && c < 'a'))
                {
                    if (o == text.Length - 1) return text.Substring(0, text.Length - 1);

                    var sb = new StringBuilder(text.Length - 1);

                    if (o > 0) sb.Append(text, 0, o);

                    for (i = ++o; i < text.Length; i++)
                    {
                        c = text[i];

                        if (c != '-' && (c < '0' || c > 'z' || c > '9' && c < 'A' || c > 'Z' && c < 'a'))
                        {
                            if (i > o) sb.Append(text, o, i - o);
                            o = i + 1;
                        }
                    }

                    if (i > o) sb.Append(text, o, i - o);
                    return sb.ToString();
                }
            }

            return text;
        }

        private IEnumerable<string> SplitWords(string text)
        {
            int offset;
            int wordStart = 0;
            char previous = '\0';

            for (offset = 0; offset < text.Length; offset++)
            {
                char current = text[offset];

                // Always split on space, but only split on hyphen if it is not the first or second char of the word.
                // This let us not isolate single letters such as in U+1180 "HANGUL JUNGSEONG O-E"
                if (current == ' ' || current == '-' && offset - wordStart > 1)
                {
                    if (offset > wordStart)
                    {
                        yield return text.Substring(wordStart, offset - wordStart);
                    }

                    wordStart = offset + 1;
                }

                previous = current;
            }

            if (offset > wordStart) yield return text.Substring(wordStart, offset - wordStart);
        }
    }
}
