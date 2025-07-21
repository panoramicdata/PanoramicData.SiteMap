using Ollama.Api;

namespace SiteMap.Test;

public class SiteMapGeneratorTests(ITestOutputHelper testOutputHelper) : BaseTest(testOutputHelper)
{
	[Fact]
	public async Task SiteMapGenerator_GenerateSiteMap_Succeeds()
	{
		// Arrange
		var siteMapGenerator = new SiteMapGenerator(new()
		{
			Uri = new Uri("https://example.com"),
			HttpClient = new HttpClient(),
			OllamaClient = new OllamaClient(new OllamaClientOptions
			{
				Uri = new Uri("https://pdl-merlin-01.panoramicdata.com:11434"),
			})
		});
		var expectedSiteMap = "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>";

		// Act
		var actualSiteMap = await siteMapGenerator.GenerateAsync(CancellationToken);

		// Assert
		Assert.Equal(expectedSiteMap, actualSiteMap);

	}
}
