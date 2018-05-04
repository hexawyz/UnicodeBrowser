using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace UnicodeBrowser.Client
{
	internal static class ElementRefExtensions
    {
		public static bool CheckValidity(this ElementRef form)
			=> RegisteredFunction.Invoke<bool>("checkFormValidity", form);
	}
}
