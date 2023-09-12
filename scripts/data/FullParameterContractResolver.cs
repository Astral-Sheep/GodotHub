using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Com.Astral.GodotHub.Data
{
	internal class FullParameterContractResolver : DefaultContractResolver
	{
		protected override JsonObjectContract CreateObjectContract(Type pObjectType)
		{
			JsonObjectContract lContract = base.CreateObjectContract(pObjectType);
			ConstructorInfo lConstructor = pObjectType
				.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.OrderBy(constructor => constructor.GetParameters().Length)
				.ToList()
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
