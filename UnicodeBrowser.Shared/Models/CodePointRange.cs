namespace UnicodeBrowser.Models
{
	public sealed class CodePointRange
    {
        public int FirstCodePoint { get; }
        public int LastCodePoint { get; }

        public CodePointRange(int firstCodePoint, int lastCodePoint)
        {
            FirstCodePoint = firstCodePoint;
            LastCodePoint = lastCodePoint;
        }
    }
}
