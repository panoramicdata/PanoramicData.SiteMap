using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ollama.Api.Models;

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
	private readonly ILogger _logger;
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

		_logger = options.Logger ?? NullLogger.Instance;
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
		var issues = new HashSet<string>();
		var siteMap = new SiteMap();
		toVisit.Enqueue(baseUri);

		while (toVisit.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var current = toVisit.Dequeue();

			if (!visited.Add(current))
			{
				continue;
			}

			_logger.LogInformation("Processing URL: {Url}", current);

			string html;
			try
			{
				html = await _httpClient.GetStringAsync(current, cancellationToken);
			}
			catch (Exception ex)
			{
				issues.Add($"Failed to fetch {current}: {ex.Message}");
				continue;
			}

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
						Prompt = "Provide a summary, suitable for a Site Map of this web page text content.  Do not provide any introductory text such as 'Here is a summary...'.  Just the content.: \n---\n" + doc.DocumentNode.InnerText,
					}, cancellationToken);
					summary = response.Response;
				}
				catch (Exception ex)
				{
					issues.Add($"Failed to summarize {current}: {ex.Message}");
				}
			}

			siteMap.Urls.Add(new SiteMapUrl { Loc = current, Summary = summary });

			foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
			{
				var href = link.GetAttributeValue("href", null);
				if (string.IsNullOrWhiteSpace(href))
				{
					continue;
				}

				Uri? foundUri = null;
				if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
				{
					foundUri = absoluteUri;
				}
				else if (href.StartsWith('/'))
				{
					Uri.TryCreate(baseUri, href, out foundUri);
				}
				else
				{
					Uri.TryCreate(current, href, out foundUri);
				}

				if (foundUri != null && foundUri.Host == baseUri.Host && foundUri.Scheme == baseUri.Scheme)
				{
					var canonical = new Uri(foundUri.GetLeftPart(UriPartial.Path));

					if (canonical.PathAndQuery.StartsWith("/ReportMagic/ReportMagic"))
					{
						continue;
					}

					if (!visited.Contains(canonical))
					{
						toVisit.Enqueue(canonical);
					}
				}
			}
		}

		return new GenerationResult
		{
			SiteMap = siteMap,
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
