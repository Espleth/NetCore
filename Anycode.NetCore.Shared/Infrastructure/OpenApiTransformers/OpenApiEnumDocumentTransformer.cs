namespace Anycode.NetCore.Shared.Infrastructure.OpenApiTransformers;

/// <summary>
/// Adds XML documentation comments to enum schemas in the final OpenAPI document
/// </summary>
public class OpenApiEnumDocumentTransformer : IOpenApiDocumentTransformer
{
	private static readonly Dictionary<string, XDocument> _xmlDocs = new();
	private static bool _initialized;

	public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
		CancellationToken cancellationToken)
	{
		if (!_initialized)
		{
			LoadAllXmlDocs();
			_initialized = true;
		}

		if (document.Components?.Schemas == null)
			return Task.CompletedTask;

		foreach (var (schemaName, schema) in document.Components.Schemas)
		{
			// Check if this is an enum schema
			if (schema.Enum == null || schema.Enum.Count == 0)
				continue;

			// Find the enum type
			var enumType = FindEnumType(schemaName);
			if (enumType == null)
				continue;

			// Build description from XML comments
			var descriptions = new List<string>();
			foreach (var enumName in Enum.GetNames(enumType))
			{
				var summary = GetEnumValueSummary(enumType, enumName);
				if (!string.IsNullOrEmpty(summary))
				{
					descriptions.Add($"- `{enumName}`: {summary}");
				}
			}

			if (descriptions.Count > 0)
				schema.Description = string.Join("\n", descriptions);
		}

		return Task.CompletedTask;
	}

	private void LoadAllXmlDocs()
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

		foreach (var assembly in assemblies)
		{
			var assemblyName = assembly.GetName().Name;
			if (assemblyName == null || _xmlDocs.ContainsKey(assemblyName))
				continue;

			var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
			if (!File.Exists(xmlPath))
				continue;

			try
			{
				using var stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new StreamReader(stream, Encoding.UTF8);
				_xmlDocs[assemblyName] = XDocument.Load(reader);
			}
			catch
			{
				// Ignore errors loading XML
			}
		}
	}

	private Type? FindEnumType(string schemaName)
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic);

		foreach (var assembly in assemblies)
		{
			try
			{
				var types = assembly.GetTypes();
				var enumType = types.FirstOrDefault(t => t.IsEnum && t.Name == schemaName);
				if (enumType != null)
					return enumType;
			}
			catch
			{
				// Some assemblies might not be accessible
			}
		}

		return null;
	}

	private string GetEnumValueSummary(Type enumType, string enumName)
	{
		var assemblyName = enumType.Assembly.GetName().Name;
		if (assemblyName == null || !_xmlDocs.TryGetValue(assemblyName, out var xmlDoc))
			return string.Empty;

		var memberName = $"F:{enumType.FullName}.{enumName}";
		var summaryNode = xmlDoc.Descendants("member")
			.FirstOrDefault(m => m.Attribute("name")?.Value == memberName)?
			.Element("summary");

		return summaryNode?.Value.Trim() ?? string.Empty;
	}
}