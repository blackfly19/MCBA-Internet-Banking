using MCBA_AdminPortal.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MCBA_Tests.AdminPortal.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsView()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }
}