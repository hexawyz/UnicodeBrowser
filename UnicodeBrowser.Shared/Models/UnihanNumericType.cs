namespace UnicodeBrowser.Models
{
	/// <summary>Represents the different numeric types from the Unihan database.</summary>
	public enum UnihanNumericType : byte
	{
		/// <summary>Indicates that there is no Unihan numeric property defined for the code point.</summary>
		None = 0,
		/// <summary>Indicates that the propery kPrimaryNumeric is defined for this code point.</summary>
		/// <remarks>The kPrimaryNumeric property is used for ideographs wich are standard numerals.</remarks>
		Primary = 1,
		/// <summary>Indicates that the propery kAccountingNumeric is defined for this code point.</summary>
		/// <remarks>The kAccountingNumeric property is used for ideographs used as accounting numerals.</remarks>
		Accounting = 2,
		/// <summary>Indicates that the propery kOtherNumeric is defined for this code point.</summary>
		/// <remarks>The kOtherNumeric property is used for ideographs wich are used as numerals in non common contexts.</remarks>
		Other = 3,
	}
}