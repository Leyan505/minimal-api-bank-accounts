using System.ComponentModel;
using System.Timers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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


app.MapGet("/accounts", (IBankAccountService service) => service.GetBankAccounts());
app.MapPost("/accounts", (BankAccount newAccount, IBankAccountService service) =>
{
    service.AddAccount(newAccount);
    return TypedResults.Created("/accounts/{id}", newAccount);
}).AddEndpointFilter(async (context, next) => {
    var accountArgument = context.GetArgument<BankAccount>(0);
    var errors = new Dictionary<string, string[]>();

    if (accountArgument.Balance < 0)
    {
        errors.Add(nameof(accountArgument.Balance), ["Initial balance cannot be negative."]);
    }
    if(accountArgument.AccountNumber == null || accountArgument.AccountNumber.Length != 20 || !accountArgument.AccountNumber.All(char.IsDigit))
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

app.MapGet("/check_balance/{accountNumber}", (string accountNumber, IBankAccountService service) =>
{
    var balance = service.CheckBalance(accountNumber);
    return TypedResults.Ok(balance);

})
.AddEndpointFilter(async (context, next) => {
    var accountNumber = context.GetArgument<string>(0);
    var errors = new Dictionary<string, string[]>();

    if(accountNumber == null || accountNumber.Length != 20 || !accountNumber.All(char.IsDigit))
    {
        errors.Add(nameof(accountNumber), ["Invalid account number."]);
    }

    if(errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return await next(context);
});

app.Run();

public record BankAccount(string AccountNumber, double Balance);

interface IBankAccountService
{
    List<BankAccount> GetBankAccounts();
    BankAccount AddAccount(BankAccount account);
    bool Duplicate(string accountNumber);
    double? CheckBalance(string accountNumber);

}

class InMemoryBankAccountService : IBankAccountService
{
    private readonly List<BankAccount> _accounts = [];
    public List<BankAccount> GetBankAccounts()
    {
        return _accounts;
    }

    public BankAccount AddAccount(BankAccount account)
    {
        _accounts.Add(account);
        return account;
    }

    public bool Duplicate(string accountNumber)
    {
        return _accounts.Find(x => x.AccountNumber.Equals(accountNumber)) != null;
    } 

    public double? CheckBalance(string accountNumber)
    {
        var account = _accounts.Find(x => x.AccountNumber.Equals(accountNumber));
        return account?.Balance;
    }

}