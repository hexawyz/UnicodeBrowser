namespace UnicodeBrowser.Models
{
	public sealed class RationalNumber
    {
        public double Numerator { get; }
        public double Denominator { get; }

        public RationalNumber(double numerator, double denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }
}
