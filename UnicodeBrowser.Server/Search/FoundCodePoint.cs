using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnicodeBrowser.Search
{
    public struct FoundCodePoint
    {
        private readonly int _codePoint;

        public int Value => _codePoint & 0x1FFFFF;

        public bool IsExactMatch => (_codePoint & ~0x7FFFFFFF) == 0;

        internal FoundCodePoint(int codePoint, bool isExactMatch)
        {
            _codePoint = isExactMatch ? codePoint : codePoint | ~0x7FFFFFFF;
        }
    }
}
