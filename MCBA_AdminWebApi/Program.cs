using MCBA_AdminWebApi.Data;
using MCBA_AdminWebApi.Models.DataManager;
using MCBA_AdminWebApi.Models.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IPayeeRepository, PayeeManager>();
builder.Services.AddScoped<IBillPayRepository, BillPayManager>();

builder.Services.AddControllers();
builder.Services.AddDbContext<MCBAContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("MCBAContext")));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
