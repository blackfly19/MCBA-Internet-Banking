using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MCBA.Controllers;
using MCBA.Data;
using MCBA.ViewModels;
using MCBA_Tests.CustomerWebsite.Utility;
using Moq;

namespace MCBA_Tests.CustomerWebsite.Controllers;

public class ProfileControllerTests
{
    private readonly MCBAContext _context;
    private readonly ProfileController _controller;
    private readonly FakeSession _session;

    public ProfileControllerTests()
    {
        _context = InMemoryContext.GetInMemoryContext();
        _controller = new ProfileController(_context);

        _session = new FakeSession();
        _session.SetInt32("CustomerID", 2100);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { Session = _session }
        };

        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>()
        );
    }

    [Fact]
    public void Index_ValidCustomer_ReturnsViewWithProfile()
    {
        var result = _controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal(2100, model.CustomerID);
        Assert.Equal("Test Customer", model.Name);
    }

    [Fact]
    public void Index_InvalidCustomer_RedirectsToHome()
    {
        _session.SetInt32("CustomerID", 9999);
        var result = _controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void Edit_Get_ReturnsViewWithProfile()
    {
        var result = _controller.Edit();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal(2100, model.CustomerID);
    }

    [Fact]
    public void Edit_Post_ValidModel_SetsSessionAndRedirects()
    {
        var vm = new ProfileViewModel
        {
            CustomerID = 2100,
            Name = "Updated Name",
            Address = "New Address",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412345678"
        };

        var result = _controller.Edit(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ConfirmEdit", redirect.ActionName);

        Assert.Equal("Updated Name", _session.GetString("ProfileController_EditName"));
        Assert.Equal("New Address", _session.GetString("ProfileController_EditAddress"));
    }

    [Fact]
    public void Edit_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var vm = new ProfileViewModel();

        var result = _controller.Edit(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(vm, viewResult.Model);
    }

    [Fact]
    public void ConfirmEdit_ReturnsViewWithSessionData()
    {
        _session.SetString("ProfileController_EditName", "Test Name");

        var result = _controller.ConfirmEdit();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal("Test Name", model.Name);
    }

    [Fact]
    public void CompleteEdit_UpdatesCustomerAndRedirects()
    {
        _session.SetString("ProfileController_EditName", "New Name");
        _session.SetString("ProfileController_EditAddress", "New Address");

        var result = _controller.CompleteEdit();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var customer = _context.Customers.Find(2100);
        Assert.Equal("New Name", customer.Name);
        Assert.Equal("New Address", customer.Address);
    }

    [Fact]
    public void CancelEdit_ClearsSessionAndRedirects()
    {
        _session.SetString("ProfileController_EditName", "Test Name");

        var result = _controller.CancelEdit();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Null(_session.GetString("ProfileController_EditName"));
    }

    [Fact]
    public void ChangePassword_Get_ReturnsView()
    {
        var result = _controller.ChangePassword();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ChangePasswordViewModel>(viewResult.Model);
    }

    [Fact]
    public void ChangePassword_Post_InvalidModel_ReturnsView()
    {
        _controller.ModelState.AddModelError("CurrentPassword", "Required");
        var vm = new ChangePasswordViewModel();

        var result = _controller.ChangePassword(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(vm, viewResult.Model);
    }

    [Fact]
    public void ChangePassword_Post_IncorrectCurrentPassword_ReturnsViewWithError()
    {
        var vm = new ChangePasswordViewModel
        {
            CurrentPassword = "wrongpass",
            NewPassword = "newpass",
            ConfirmPassword = "newpass"
        };

        var result = _controller.ChangePassword(vm);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Contains(_controller.ModelState.Values, v => v.Errors.Count > 0);
    }

    [Fact]
    public void ChangePassword_Post_ValidPassword_UpdatesAndRedirects()
    {
        var login = _context.Logins.First(l => l.CustomerID == 2100);
        var originalHash = login.PasswordHash;

        var vm = new ChangePasswordViewModel
        {
            CurrentPassword = "abc123",
            NewPassword = "newpassword",
            ConfirmPassword = "newpassword"
        };

        var result = _controller.ChangePassword(vm);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var updatedLogin = _context.Logins.Find(login.LoginID);
        Assert.NotEqual(originalHash, updatedLogin.PasswordHash);
    }
}