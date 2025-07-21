namespace SiteMap
	;

public partial class SiteMapGenerator
{
	public class GenerationResult
	{
		public required string Sitemap { get; init; }
		public required HashSet<string> Issues { get; init; }
	}
}
