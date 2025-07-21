using HtmlAgilityPack;
using Ollama.Api.Models;
using System.Security;

namespace SiteMap
	;

/// <summary>
/// Generates a sitemap for a given website.
/// </summary>
public partial class SiteMapGenerator : IDisposable
{
	private readonly SiteMapGeneratorOptions _options;
	private readonly HttpClient _httpClient;
	private readonly bool _disposeHttpClient;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="SiteMapGenerator"/> class.
	/// </summary>
	/// <param name="options">The options for sitemap generation.</param>
	public SiteMapGenerator(SiteMapGeneratorOptions options)
	{
		_options = options;
		if (options.HttpClient is not null)
		{
			_httpClient = options.HttpClient;
			_disposeHttpClient = false;
		}
		else
		{
			_httpClient = new HttpClient();
			_disposeHttpClient = true;
		}
	}

	/// <summary>
	/// Generates the sitemap asynchronously.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task<GenerationResult> GenerateAsync(CancellationToken cancellationToken)
	{
		var baseUri = _options.Uri;
		var visited = new HashSet<Uri>();
		var toVisit = new Queue<Uri>();
		var urlSummaries = new Dictionary<Uri, string?>();
		var issues = new HashSet<string>();
		toVisit.Enqueue(baseUri);

		while (toVisit.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var current = toVisit.Dequeue();
			if (!visited.Add(current))
			{
				continue;
			}

			string html;
			try
			{
				html = await _httpClient.GetStringAsync(current, cancellationToken);
			}
			catch (Exception ex)
			{
				issues.Add($"Failed to fetch {current}: {ex.Message}");
				continue; // Skip unreachable pages
			}

			// Parse links
			var doc = new HtmlDocument();
			try
			{
				doc.LoadHtml(html);
			}
			catch (Exception ex)
			{
				issues.Add($"Failed to parse HTML for {current}: {ex.Message}");
				continue;
			}

			string? summary = null;
			if (_options.OllamaClient is not null && _options.OllamaClientModel is not null)
			{
				try
				{
					var response = await _options.OllamaClient.Generate.GenerateAsync(new GenerateRequest
					{
						Model = _options.OllamaClientModel,
						Prompt = "Provide a summary, suitable for a Site Map of this web page text content: \n---\n" + doc.Text,
					}, cancellationToken);
					summary = response.Response;
				}
				catch (Exception ex)
				{
					issues.Add($"Failed to summarize {current}: {ex.Message}");
				}
			}

			urlSummaries[current] = summary;

			foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
			{
				var href = link.GetAttributeValue("href", null);
				if (string.IsNullOrWhiteSpace(href))
				{
					continue;
				}

				if (Uri.TryCreate(current, href, out var foundUri))
				{
					if (foundUri.Host == baseUri.Host && foundUri.Scheme == baseUri.Scheme)
					{
						var canonical = new Uri(foundUri.GetLeftPart(UriPartial.Path));
						if (!visited.Contains(canonical))
						{
							toVisit.Enqueue(canonical);
						}
					}
				}
			}
		}

		// Build sitemap XML
		var sb = new System.Text.StringBuilder();
		sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
		foreach (var kvp in urlSummaries.OrderBy(kvp => kvp.Key.AbsoluteUri))
		{
			sb.AppendLine("  <url>");
			sb.AppendLine($"    <loc>{SecurityElement.Escape(kvp.Key.AbsoluteUri)}</loc>");
			if (!string.IsNullOrWhiteSpace(kvp.Value))
			{
				sb.AppendLine($"    <summary>{SecurityElement.Escape(kvp.Value!)}</summary>");
			}

			sb.AppendLine("  </url>");
		}

		sb.AppendLine("</urlset>");

		return new GenerationResult
		{
			Sitemap = sb.ToString(),
			Issues = issues
		};
	}

	/// <summary>
	/// Disposes the resources used by the <see cref="SiteMapGenerator"/>.
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			if (_disposeHttpClient)
			{
				_httpClient.Dispose();
			}

			_disposed = true;
		}

		GC.SuppressFinalize(this);
	}
}
