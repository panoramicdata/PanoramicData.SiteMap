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
			Uri = new Uri("https://docs.magicsuite.net/"),
			HttpClient = new HttpClient(),
			OllamaClient = new OllamaClient(new OllamaClientOptions
			{
				Uri = new Uri("http://pdl-merlin-01.panoramicdata.com:11434"),
			}),
			OllamaClientModel = null // Prevents comment generation
									 // OllamaClientModel = "llama3.1" // Generates comments
		});

		var expectedSiteMap = "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\"></urlset>";

		// Act
		var generationResult = await siteMapGenerator
			.GenerateAsync(CancellationToken);

		// Assert
		Assert.NotNull(generationResult);
		Assert.NotNull(generationResult.Issues);
		Assert.NotEmpty(generationResult.SitemapXml);
		Assert.Contains("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">", generationResult.SitemapXml);

		// Write output to file for debugging purposes
		var outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sitemap.xml");
		await File.WriteAllTextAsync(outputFilePath, generationResult.SitemapXml, CancellationToken);

		// Write out the errors to a .txt file for debugging purposes
		var issuesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sitemap_issues.txt");
		await File.WriteAllLinesAsync(issuesFilePath, generationResult.Issues, CancellationToken);
	}
}
