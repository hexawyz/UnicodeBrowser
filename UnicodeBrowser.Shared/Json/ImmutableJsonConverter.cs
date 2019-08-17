using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnicodeBrowser.Json.Internal;

namespace UnicodeBrowser.Json
{
	// Contrieved implementation of a converter to trick System.Text.Json into deserializing immutable classes.
	// NB: It won't work for serializing. (Could make it work by generating a different set of options and calling the serializer, but don't need it now)
	public class ImmutableJsonConverter : JsonConverterFactory
	{
		// Types are theoretically unloadable, so avoid keeping them in memory by relying on ConditionalWeakTable.
		private static readonly ConditionalWeakTable<Type, StrongBox<bool>> ConvertibilityCache = new ConditionalWeakTable<Type, StrongBox<bool>>();

		public static readonly ImmutableJsonConverter Instance = new ImmutableJsonConverter();

		private ImmutableJsonConverter() { }

		private bool CanConvertInternal(Type typeToConvert)
			=> Type.GetTypeCode(typeToConvert) == TypeCode.Object && // Avoid all native types
				typeToConvert.IsClass && // Avoid all reference types
				!typeToConvert.IsArray && // Arrays in mono could be wrongly detected…
				!typeof(IEnumerable).IsAssignableFrom(typeToConvert) && // Generally avoid all collection types.
				typeToConvert.GetConstructor(Type.EmptyTypes) == null && // Exclude types having a parameterless constructor.
				typeToConvert.GetConstructors().Length == 1; // Look for exactly one constructor.

		public override bool CanConvert(Type typeToConvert)
			=> ConvertibilityCache.GetValue(typeToConvert, t => new StrongBox<bool>(CanConvertInternal(t))).Value;

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
			=> (JsonConverter)Activator.CreateInstance(typeof(SpecializedImmutableJsonConverter<>).MakeGenericType(typeToConvert));

		private delegate TTarget DeserializationHandler<TTarget>(ref Utf8JsonReader reader, JsonSerializerOptions options);

		private TTarget Deserialize<TTarget, TBuilder>(ref Utf8JsonReader reader, JsonSerializerOptions options)
			where TBuilder : IBuilder<TTarget>
		{
			var builder = JsonSerializer.Deserialize<TBuilder>(ref reader, options);
			return builder.Build();
		}

		private static readonly MethodInfo DeserializationMethod = typeof(ImmutableJsonConverter)
			.GetMethod
			(
				nameof(Deserialize),
				BindingFlags.NonPublic | BindingFlags.Instance,
				Type.DefaultBinder,
				CallingConventions.HasThis,
				new[]
				{
					typeof(Utf8JsonReader).MakeByRefType(),
					typeof(JsonSerializerOptions)
				},
				Array.Empty<ParameterModifier>()
			);

		private sealed class SpecializedImmutableJsonConverter<T> : JsonConverter<T>
			where T : class
		{
			private static readonly DeserializationHandler<T> DeserializationHandler = CreateDeserializationHandler();

			private static DeserializationHandler<T> CreateDeserializationHandler()
			{
				var constructor = typeof(T).GetConstructors().Single();

				// Generate a "TBuilder" type implementing IBuilder<T>.
				// We'll trick the JSON serializer into deserializing a "TBuilder" then instanciate the final object by calling IBuilder<T>.Build().
				// Not the cleanest way to do it, but might be one of the less "allocatey" ?
				string assemblyName = $"Serialization.Json.Builders.{typeof(T).Name}";
				var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
				var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);
				var typeBuilder = moduleBuilder.DefineType(assemblyName + "Builder", TypeAttributes.Public | TypeAttributes.Sealed, typeof(ValueType));
				var properties = Array.ConvertAll
				(
					constructor.GetParameters(),
					parameter =>
					{
						string propertyName = char.ToUpperInvariant(parameter.Name[0]) + parameter.Name.Substring(1);
						// Fields ought to be enough, but System.Text.Json seems to require properties 😥
						var field = typeBuilder.DefineField($"_{parameter.Name}", parameter.ParameterType, FieldAttributes.Public);

						var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, CallingConventions.HasThis, parameter.ParameterType, Type.EmptyTypes);
						{
							var ilGenerator = getMethodBuilder.GetILGenerator();

							ilGenerator.Emit(OpCodes.Ldarg_0);
							ilGenerator.Emit(OpCodes.Ldfld, field);
							ilGenerator.Emit(OpCodes.Ret);
						}

						var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, CallingConventions.HasThis, typeof(void), new[] { parameter.ParameterType });
						{
							var ilGenerator = setMethodBuilder.GetILGenerator();

							ilGenerator.Emit(OpCodes.Ldarg_0);
							ilGenerator.Emit(OpCodes.Ldarg_1);
							ilGenerator.Emit(OpCodes.Stfld, field);
							ilGenerator.Emit(OpCodes.Ret);
						}

						var property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, parameter.ParameterType, Type.EmptyTypes);
						property.SetGetMethod(getMethodBuilder);
						property.SetSetMethod(setMethodBuilder);

						return property;
					}
				);

				var methodBuilder = typeBuilder.DefineMethod("Build", MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.Final, CallingConventions.HasThis, typeof(T), Type.EmptyTypes);
				{
					var ilGenerator = methodBuilder.GetILGenerator();
					foreach (var property in properties)
					{
						ilGenerator.Emit(OpCodes.Ldarg_0);
						ilGenerator.Emit(OpCodes.Call, property.GetMethod);
					}
					ilGenerator.Emit(OpCodes.Newobj, constructor);
					ilGenerator.Emit(OpCodes.Ret);
				}
				typeBuilder.AddInterfaceImplementation(typeof(IBuilder<T>));

				var builderType = typeBuilder.CreateTypeInfo();

				return (DeserializationHandler<T>)DeserializationMethod.MakeGenericMethod(new[] { typeof(T), builderType })
					.CreateDelegate(typeof(DeserializationHandler<T>), ImmutableJsonConverter.Instance);
			}

			public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> DeserializationHandler(ref reader, options);

			public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
				=> throw new NotImplementedException();
		}
	}
}
