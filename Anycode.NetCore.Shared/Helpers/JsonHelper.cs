namespace Anycode.NetCore.Shared.Helpers;

public static class JsonHelper
{
	public static JsonSerializerOptions JsonApiOptions
	{
		get
		{
			if (field != null)
				return field;

			field = new JsonSerializerOptions();
			field.SetApiJsonSerializerOptions();
			return field;
		}
	}

	public static void SetApiJsonSerializerOptions(this JsonSerializerOptions options)
	{
		options.PropertyNameCaseInsensitive = true; // "propertyName": "value" or "PropertyName": "value"
		options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // "propertyName": "value"
		options.NumberHandling = JsonNumberHandling.AllowReadingFromString; // "value": "150"
		options.AllowTrailingCommas = true; // why not
		options.Converters.Add(new JsonStringEnumConverter()); // describes enums as strings
		options.RespectNullableAnnotations = true; // required string Val { get; init; } parsed from json can still be null without this
	}

	public static bool TryDeserialize<T>(string json, out T? result)
	{
		try
		{
			result = JsonSerializer.Deserialize<T>(json);
			return true;
		}
		catch (Exception)
		{
			result = default;
			return false;
		}
	}
}