namespace UnicodeBrowser.Models
{
	/// <summary>Provides information on the kind of name alias provided for a code point.</summary>
	public enum UnicodeNameAliasKind : byte
	{
		/// <summary>The alias is a correction of a serious problem in the original name.</summary>
		Correction = 1,
		/// <summary>The alias provides the ISO 6429 name for C0 and C1 control functions of a control code, or another commonly occurring name for the control code.</summary>
		Control = 2,
		/// <summary>The alias is a widely used alternate name for a format character.</summary>
		Alternate = 3,
		/// <summary>The alias is a documented non-standardized label for C1 control code points.</summary>
		Figment = 4,
		/// <summary>The alias is a commonly occurring abbreviation (or acronym) for control codes, format characters, spaces, and variation selectors.</summary>
		Abbreviation = 5
	}
}