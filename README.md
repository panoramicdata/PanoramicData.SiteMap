# PanoramicData.SiteMap

PanoramicData.SiteMap is a .NET library for generating sitemaps for your website.

## Installation

Add the NuGet package to your project:

```
dotnet add package PanoramicData.SiteMap
```

## Usage

```csharp
using SiteMapGenerator;

var options = new SiteMapGeneratorOptions
{
	Uri = new Uri("https://example.com"),
	FileInfo = new FileInfo("sitemap.xml")
};

using var generator = new SiteMapGenerator(options);
await generator.GenerateAsync(CancellationToken.None);
```

### Custom HttpClient

You can provide your own `HttpClient` via `SiteMapGeneratorOptions.HttpClient`. If not provided, the generator will create and dispose its own instance.

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.
