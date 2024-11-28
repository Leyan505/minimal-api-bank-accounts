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
}).AddEndpointFilter(async (context, next) => {
    var accountArgument = context.GetArgument<bankAccount>(0);
    var errors = new Dictionary<string, string[]>();

    if (accountArgument.Balance < 0)
    {
        errors.Add(nameof(accountArgument.Balance), ["Initial balance cannot be negative."]);
    }
    if(accountArgument.AccountNumber == null || accountArgument.AccountNumber.Length != 20)
    {
        errors.Add(nameof(accountArgument.AccountNumber), ["Invalid account number."]);
    }
    else 
    {
        var service = context.GetArgument<InMemoryBankAccountService>(1);
        if (service.Duplicate(accountArgument.AccountNumber) == true)
        {
            errors.Add(nameof(accountArgument.AccountNumber), ["Duplicate account number is not allowed."]);
        }
    }

    if(errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return await next(context);
});




app.Run();

public record bankAccount(string? AccountNumber, double Balance);

interface IBankAccountService
{
    bankAccount AddAccount(bankAccount account);
    bool Duplicate(string AccountNumber);
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

    public bool Duplicate(string AccountNumber)
    {
        return _accounts.Find(x => x.AccountNumber.Equals(AccountNumber)) != null;
    } 

    public double CheckBalance(bankAccount account)
    {
        return account.Balance;
    }
}