using System;
using System.Collections;
using System.Collections.Generic;

namespace UnicodeBrowser.Search
{
	internal struct Node
    {
        public struct Builder
        {
            private Builder[] _children;
            private List<FoundCodePoint> _codePoints;

            private bool IsEmpty => _children == null && _codePoints == null;

            internal void AddWord(IndexBuilder state, string text, int codePoint) => AddWord(state, text, 0, codePoint);

            private void AddWord(IndexBuilder state, string text, int index, int codePoint)
            {
                if (index > 0)
                {
                    var foundCodePoint = new FoundCodePoint(codePoint, index == text.Length);

                    if (_codePoints == null)
                    {
                        _codePoints = state.GetCodePointList();
                    }
                    // Code points will be added in ascending order, so we can easily verify that we don't add the same code point twice.
                    else if (_codePoints[_codePoints.Count - 1].Value == codePoint)
                    {
                        if (foundCodePoint.IsExactMatch && !_codePoints[_codePoints.Count - 1].IsExactMatch)
                        {
                            _codePoints[_codePoints.Count - 1] = foundCodePoint;
                        }

                        goto ProcessNextLetter;
                    }

                    _codePoints.Add(foundCodePoint);
                }

                ProcessNextLetter:;
                if (index < text.Length)
                {
                    int letterIndex = GetAlphabetIndex(text[index]);

                    if (letterIndex < 0) throw new ArgumentException();

                    if (_children == null) _children = state.GetNodeArray();

                    _children[letterIndex].AddWord(state, text, index + 1, codePoint);
                }
            }

            internal Node BuildAndDispose(IndexBuilder state)
            {
                Node[] children = null;
                FoundCodePoint[] codePoints = null;
                ulong availableChildren = 0;

                if (_codePoints != null)
                {
                    codePoints = _codePoints.ToArray();
                    state.ReleaseCodePointList(ref _codePoints);
                }

                if (_children != null)
                {
                    int childIndex = 0;

                    for (int i = 0; i < _children.Length; i++)
                    {
                        if (!_children[i].IsEmpty)
                        {
                            availableChildren |= 1UL << i;
                            childIndex++;
                        }
                    }
                    children = new Node[childIndex];

                    childIndex = 0;

                    for (int i = 0; i < _children.Length; i++)
                    {
                        if (!_children[i].IsEmpty)
                        {
                            children[childIndex++] = _children[i].BuildAndDispose(state);
                        }
                    }

                    state.ReleaseNodeArray(ref _children);
                }

                return new Node(children, codePoints, availableChildren);
            }
        }

        public struct CodePointEnumerable : IEnumerable<FoundCodePoint>
        {
            private readonly FoundCodePoint[] _codePoints;

            internal CodePointEnumerable(FoundCodePoint[] codePoints)
            {
                _codePoints = codePoints;
            }

            public CodePointEnumerator GetEnuemrator() => new CodePointEnumerator(_codePoints);
            IEnumerator IEnumerable.GetEnumerator() => GetEnuemrator();
            IEnumerator<FoundCodePoint> IEnumerable<FoundCodePoint>.GetEnumerator() => GetEnuemrator();
        }

        public struct CodePointEnumerator : IEnumerator<FoundCodePoint>
        {
            private readonly FoundCodePoint[] _codePoints;
            private int _index;

            internal CodePointEnumerator(FoundCodePoint[] codePoints)
            {
                _codePoints = codePoints;
                _index = -1;
            }

            public FoundCodePoint Current => _codePoints[_index];
            object IEnumerator.Current => Current;
            public void Dispose() { }
            public bool MoveNext() => ++_index < _codePoints.Length;
            public void Reset() => _index = -1;
        }

        private static FoundCodePoint[] EmptyCodePointArray = new FoundCodePoint[0];

        private readonly Node[] _children;
        private readonly FoundCodePoint[] _codePoints;
        private readonly ulong _availableChildren;

        private Node(Node[] children, FoundCodePoint[] codePoints, ulong availableChildren)
        {
            _children = children;
            _codePoints = codePoints;
            _availableChildren = availableChildren;
        }

        public bool TryFind(string word, out Node node)
        {
            if (string.IsNullOrEmpty(word))
            {
                node = default;
                return false;
            }

            // NB: We don't have to worry about surrogate pairs since only 0-9, A-Z and - are allowed for Unicode names.
            return TryFind(word, 0, out node);
        }

        private bool TryFind(string word, int index, out Node node)
        {
            int i;

            if (index >= word.Length)
            {
                node = this;
                return true;
            }
            else if ((i = GetNodeIndex(word[index])) < 0)
            {
                node = default;
                return false;
            }
            else
            {
                return _children[i].TryFind(word, index + 1, out node);
            }
        }

        public CodePointEnumerable GetCodePoints()
        {
            return new CodePointEnumerable(_codePoints ?? EmptyCodePointArray);
        }

        private int GetNodeIndex(char c)
        {
            int theoricalIndex = GetAlphabetIndex(c);

            if (theoricalIndex >= 0)
            {
                ulong indexMask = 1UL << theoricalIndex;

                if ((_availableChildren & indexMask) != 0)
                {
                    ulong remainingBits = _availableChildren & (indexMask - 1);

                    if (remainingBits == 0)
                    {
                        return 0;
                    }
                    else if (remainingBits == indexMask - 1)
                    {
                        return theoricalIndex;
                    }
                    else
                    {
                        theoricalIndex = 0;

                        indexMask >>= 1;

                        while (indexMask != 0)
                        {
                            if ((_availableChildren & indexMask) != 0) theoricalIndex++;
                            indexMask >>= 1;
                        }

                        return theoricalIndex;
                    }
                }
            }

            return -1;
        }

        private static int GetAlphabetIndex(char c)
        {
            if (c == '-') return 36;
            else if (c >= '0' && c <= 'z')
            {
                if (c <= '9')
                {
                    return c - '0' + 26;
                }
                else if (c >= 'A')
                {
                    if (c <= 'Z')
                    {
                        return c - 'A';
                    }
                    else if (c >= 'a')
                    {
                        return c - 'a';
                    }
                }
            }

            return -1;
        }
    }
}
