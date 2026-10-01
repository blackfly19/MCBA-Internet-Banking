using MCBA.Models;

namespace MCBA.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<MCBAContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        if (!context.Payees.Any())
        {
            context.Payees.AddRange(
                new Payee
                {
                    Name = "Melbourne Water",
                    Address = "123 Water St",
                    City = "Melbourne",
                    State = "VIC",
                    Postcode = "3000",
                    Phone = "(03) 1234 5678"
                },
                new Payee
                {
                    Name = "AGL Energy",
                    Address = "456 Power Rd",
                    City = "Sydney",
                    State = "NSW",
                    Postcode = "2000",
                    Phone = "(02) 9876 5432"
                },
                new Payee
                {
                    Name = "Telstra",
                    Address = "789 Telco Ave",
                    City = "Brisbane",
                    State = "QLD",
                    Postcode = "4000",
                    Phone = "(07) 5555 1234"
                }
            );
        
            context.SaveChanges();
        }
        
        if (context.Customers.Any())
            return;
        
        var connectionString = configuration.GetConnectionString("CustomerWebService");

        var customers = LoadData(connectionString);
        
        WriteData(context, customers);
    }
    
    private static Customer[] LoadData(string url)
    {
        using var client = new HttpClient();
        var json = client.GetStringAsync(url).Result;

        var customers = Utilities.Utilities.LoadJson<Customer[]>(json);
        
        return customers;
    }

    private static void WriteData(MCBAContext context, Customer[] customers)
    {

        var allAccounts = customers
            .Where(c => c.Accounts != null)
            .SelectMany(c => c.Accounts.Select(a =>
            {
                a.FreeTransactions = 2;
                return a;
            }))
            .ToList();

        var allTransactions = customers
            .Where(c => c.Accounts != null)
            .SelectMany(c => c.Accounts)
            .Where(a => a.Transactions != null)
            .SelectMany(a => a.Transactions.Select(t =>
            {
                t.AccountNumber = a.AccountNumber;
                t.TransactionType = 'D';
                return t;
            }))
            .ToList();

        var allLogins = customers
            .Where(c => c.Login != null)
            .Select(c =>
            {
                c.Login.CustomerID = c.CustomerID;
                return c.Login;
            })
            .ToList();
        
        foreach (var account in allAccounts)
        {
            account.Balance = allTransactions
                .Where(t => t.AccountNumber == account.AccountNumber)
                .Sum(t => t.Amount);
        }
        
        context.Customers.AddRange(customers);
        context.Accounts.AddRange(allAccounts);
        context.Transactions.AddRange(allTransactions);
        context.Logins.AddRange(allLogins);
        
        context.SaveChanges();
    }
}