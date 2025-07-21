using Ollama.Api;

namespace SiteMap;

/// <summary>
/// Options for configuring the <see cref="SiteMapGenerator"/>.
/// </summary>
public class SiteMapGeneratorOptions
{
	/// <summary>
	/// The base URI to generate the sitemap for.
	/// </summary>
	public required Uri Uri { get; init; }

	/// <summary>
	/// Optional <see cref="HttpClient"/> to use for requests. If not provided, a new instance will be created and disposed by <see cref="SiteMapGenerator"/>.
	/// </summary>
	public HttpClient? HttpClient { get; init; }

	/// <summary>
	/// Gets the instance of <see cref="OllamaClient"/> used for interacting with the Ollama service.
	/// </summary>
	public OllamaClient? OllamaClient { get; init; }
}
