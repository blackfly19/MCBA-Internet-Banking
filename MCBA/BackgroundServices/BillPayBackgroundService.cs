using MCBA.Data;
using MCBA.Models;
using Microsoft.EntityFrameworkCore;

namespace MCBA.BackgroundServices;

public class BillPayBackgroundService(IServiceProvider services, ILogger<BillPayBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BillPay Background Service is running.");

        // Process any overdue payments immediately on startup
        await DoWorkAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("BillPay Background Service is waiting a minute.");

            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            await DoWorkAsync(cancellationToken);
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BillPay Background Service is working.");

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MCBAContext>();

        var now = DateTime.UtcNow;

        // Get all bills that are due (schedule time <= now)
        var dueBills = await context.BillPays.
            Include(b => b.Payee)
            .Where(b => b.ScheduleTimeUtc <= now && b.Status != "Blocked")
            .ToListAsync(cancellationToken);

        foreach (var bill in dueBills)
        {
            var account = await context.Accounts.FindAsync(new object[] { bill.AccountNumber }, cancellationToken);
            if (account == null)
            {
                continue;
            }

            // Check if account has sufficient funds
            var minimumBalance = account.AccountType == 'S' ? 0 : -500;
            var newBalance = account.Balance - bill.Amount;

            if (newBalance < minimumBalance)
            {
                bill.Status = "Failed";
            
                if (bill.Period == 'M')
                {
                    bill.ScheduleTimeUtc = bill.ScheduleTimeUtc.AddMonths(1);
                }
                continue;
            }

            account.Balance -= bill.Amount;

            var transaction = new Transaction
            {
                TransactionType = 'B',
                AccountNumber = bill.AccountNumber,
                DestinationAccountNumber = null,
                Amount = bill.Amount,
                Comment = $"Bill payment to {bill.Payee.Name}",
                TransactionTimeUtc = DateTime.UtcNow
            };

            context.Transactions.Add(transaction);
            
            if (bill.Period == 'O')
            {
                context.BillPays.Remove(bill);
            }
            else if (bill.Period == 'M')
            {
                bill.Status = "Pending";
                bill.ScheduleTimeUtc = bill.ScheduleTimeUtc.AddMonths(1);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("BillPay Background Service work complete.");
    }
}