using MCBA.Data;
using MCBA.Models;
using Microsoft.EntityFrameworkCore;

namespace MCBA_Tests.CustomerWebsite.Utility;

public static class InMemoryContext
{
    public static MCBAContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MCBAContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
            .Options;

        var context = new MCBAContext(options);
        
        // Seed with test data
        SeedTestData(context);
        
        return context;
    }

    private static void SeedTestData(MCBAContext context)
    {
        var customer = new Customer
        {
            CustomerID = 2100,
            Name = "Test Customer",
            Address = "123 Test St",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };
        context.Customers.Add(customer);

        var login = new Login
        {
            LoginID = "12345678",
            CustomerID = 2100,
            PasswordHash = "Rfc2898DeriveBytes$50000$XAk/IwrIJocBt4jD8nqhlA==$xbrjjqJiQCJMC5v5pdlVfPYzR8HDCH7ibGuNoU0Aq7w="
        };
        context.Logins.Add(login);

        var checkingAccount = new Account
        {
            AccountNumber = 4100,
            AccountType = 'C',
            CustomerID = 2100,
            Balance = 1000m,
            FreeTransactions = 2
        };
        
        var savingsAccount = new Account
        {
            AccountNumber = 4101,
            AccountType = 'S',
            CustomerID = 2100,
            Balance = 500m,
            FreeTransactions = 2
        };
        
        context.Accounts.AddRange(checkingAccount, savingsAccount);

        var payee = new Payee
        {
            PayeeID = 1,
            Name = "Test Payee",
            Address = "456 Payee St",
            City = "Sydney",
            State = "NSW",
            Postcode = "2000",
            Phone = "(02) 1234 5678"
        };
        context.Payees.Add(payee);

        context.SaveChanges();
    }
}