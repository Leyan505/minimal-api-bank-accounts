using System.ComponentModel;
using System.Timers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IBankAccountService>(new InMemoryBankAccountService());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var accounts = new List<bankAccount>();

app.MapPost("/accounts", (bankAccount newAccount, IBankAccountService service) =>
{
    service.AddAccount(newAccount);
    return TypedResults.Created("/accounts/{id}", newAccount);
});




app.Run();

public record bankAccount(string AccountNumber, double Balance);

interface IBankAccountService
{
    bankAccount AddAccount(bankAccount account);
    double CheckBalance(bankAccount account);

}

class InMemoryBankAccountService : IBankAccountService
{
    private readonly List<bankAccount> _accounts = [];
    
    public bankAccount AddAccount(bankAccount account)
    {
        _accounts.Add(account);
        return account;
    }

    public double CheckBalance(bankAccount account)
    {
        return account.Balance;
    }
}