namespace SiteMap.Test;

public abstract class BaseTest(ITestOutputHelper testOutputHelper)
{
	protected ITestOutputHelper TestOutputHelper { get; } = testOutputHelper;

	protected CancellationToken CancellationToken { get; } = new CancellationTokenSource(TimeSpan.FromSeconds(100)).Token;
}
