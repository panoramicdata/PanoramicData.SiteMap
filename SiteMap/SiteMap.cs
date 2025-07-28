using System.Xml.Linq;
using System.Security;

namespace SiteMap;

public class SiteMapUrl
{
    public required Uri Loc { get; init; }
    public string? Summary { get; init; }
}

public class SiteMap
{
    public List<SiteMapUrl> Urls { get; } = new();
}

public class SiteMapIndex
{
    public List<Uri> Sitemaps { get; } = new();
}

public static class SiteMapExtensions
{
    public static string SerializeToXml(this SiteMap siteMap)
    {
        var urlset = new XElement("urlset",
            new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
            siteMap.Urls.OrderBy(u => u.Loc.AbsoluteUri).Select(u =>
                new XElement("url",
                    new XElement("loc", SecurityElement.Escape(u.Loc.AbsoluteUri)),
                    string.IsNullOrWhiteSpace(u.Summary) ? null : new XElement("summary", SecurityElement.Escape(u.Summary!))
                ))
        );
        return urlset.ToString();
    }

    public static string SerializeToXml(this SiteMapIndex siteMapIndex)
    {
        var sitemapindex = new XElement("sitemapindex",
            new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
            siteMapIndex.Sitemaps.OrderBy(u => u.AbsoluteUri).Select(u =>
                new XElement("sitemap",
                    new XElement("loc", SecurityElement.Escape(u.AbsoluteUri))
                ))
        );
        return sitemapindex.ToString();
    }
}
