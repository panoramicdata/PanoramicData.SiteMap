namespace SiteMap
	;

/// <summary>
/// Generates a sitemap for a given website.
/// </summary>
public class SiteMapGenerator : IDisposable
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
	public async Task<string> GenerateAsync(CancellationToken cancellationToken)
	{
		// Placeholder for actual sitemap generation logic.
		// This should be replaced with the actual implementation.
		await Task.Delay(10, cancellationToken); // Simulate async work
		return "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>";
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
