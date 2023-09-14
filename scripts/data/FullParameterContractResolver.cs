using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Com.Astral.GodotHub.Data
{
	/// <summary>
	/// <see cref="IContractResolver"/> used to select the most complete constructor when deserializing a .json file
	/// </summary>
	internal class FullParameterContractResolver : DefaultContractResolver
	{
		protected override JsonObjectContract CreateObjectContract(Type pObjectType)
		{
			JsonObjectContract lContract = base.CreateObjectContract(pObjectType);
			ConstructorInfo lConstructor = pObjectType
				.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.OrderBy(constructor => constructor.GetParameters().Length)
				.LastOrDefault();

			if (lConstructor == null)
				return lContract;

			lContract.OverrideCreator = (@params) => lConstructor.Invoke(@params);
			IList<JsonProperty> lProperties = CreateConstructorParameters(lConstructor, lContract.Properties);

			for (int i = 0; i < lProperties.Count; i++)
			{
				lContract.CreatorParameters.Add(lProperties[i]);
			}

			return lContract;
		}
	}
}
