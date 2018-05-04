using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Reflection;

namespace UnicodeBrowser.Mvc
{
	internal sealed class NonPublicControllerFeatureProvider : ControllerFeatureProvider
	{
		private const string ControllerTypeNameSuffix = "Controller";

		protected override bool IsController(TypeInfo typeInfo)
			=> typeInfo.IsClass
				&& !typeInfo.IsAbstract
				&& !typeInfo.ContainsGenericParameters
				&& !typeInfo.IsDefined(typeof(NonControllerAttribute))
				&& (typeInfo.Name.EndsWith(ControllerTypeNameSuffix, StringComparison.OrdinalIgnoreCase) || typeInfo.IsDefined(typeof(ControllerAttribute)));
	}
}
