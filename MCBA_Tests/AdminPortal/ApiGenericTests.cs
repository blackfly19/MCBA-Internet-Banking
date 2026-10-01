using MCBA_AdminPortal.Helper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MCBA_Tests.AdminPortal;

public class ApiGenericTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var mockHttpClient = new Mock<HttpClient>();
        var mockConfig = new Mock<IConfiguration>();
        var mockCache = new Mock<IMemoryCache>();
        
        mockConfig.Setup(c => c["ConnectionStrings:AdminApiUrl"]);

        var api = new ApiGeneric(mockHttpClient.Object, mockConfig.Object, mockCache.Object);

        Assert.NotNull(api);
    }
}