namespace SiteMap
	;

public partial class SiteMapGenerator
{
	public class GenerationResult
	{
		public required SiteMap SiteMap { get; init; }

		public required HashSet<string> Issues { get; init; }

		public string SitemapXml => SiteMap.SerializeToXml();
	}
}
