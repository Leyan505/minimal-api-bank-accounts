using prueba_backend;
using prueba_backend.Interfaces;
using prueba_backend.Models;
using prueba_backend.Services;

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


//get and create bank accounts
app.MapGet("/accounts", (IBankAccountService service) => service.GetBankAccounts());

app.MapPost("/accounts", (BankAccount newAccount, IBankAccountService service) =>
{
    service.AddAccount(newAccount);
    return TypedResults.Created("/accounts/{id}", newAccount);
}).AddEndpointFilter(async (context, next) => {
    var accountArgument = context.GetArgument<BankAccount>(0);
    var service = context.GetArgument<InMemoryBankAccountService>(1);
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


//Check balance given an account number
app.MapGet("/check_balance/{accountNumber}", (string accountNumber, IBankAccountService service) =>
{
    var balance = service.CheckBalance(accountNumber);
    var response = new{
        accountNumber = accountNumber,
        balance = balance  
    };
    return TypedResults.Ok(response);

})
.AddEndpointFilter(async (context, next) => {
    var accountNumber = context.GetArgument<string>(0);
    var service = context.GetArgument<InMemoryBankAccountService>(1);
    var errors = new Dictionary<string, string[]>();

    if(!service.AccountIsValid(accountNumber))
    {
        errors.Add(nameof(accountNumber), ["Invalid account number."]);
    }

    if(errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return await next(context);
});


//Deposit to a given account
app.MapPost("/deposit", (TransactionDTO newTransaction, IBankAccountService service) =>
{
    var transaction = service.Deposit(newTransaction);
    return TypedResults.Created("/transactions/{id}", transaction);
})
.AddEndpointFilter(async (context, next) => {
    var transactionArgument = context.GetArgument<TransactionDTO>(0);
    var service = context.GetArgument<InMemoryBankAccountService>(1);
    var errors = new Dictionary<string, string[]>();

    if(!service.AccountIsValid(transactionArgument.AccountNumber))
    {
        errors.Add(nameof(transactionArgument.AccountNumber), ["Invalid account number."]);
    }

    if(transactionArgument.Amount <= 0)
    {
        errors.Add(nameof(transactionArgument.Amount), ["Invalid amount, must be postive."]);
    }

    if(errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return await next(context);
});


//Withdraw from a given account
app.MapPost("/withdraw", (TransactionDTO newTransaction, IBankAccountService service) =>
{
    var transaction = service.Withdraw(newTransaction);
    return TypedResults.Created("/transactions/{id}", transaction);
})
.AddEndpointFilter(async (context, next) => {
    var transactionArgument = context.GetArgument<TransactionDTO>(0);
    var service = context.GetArgument<InMemoryBankAccountService>(1);
    var errors = new Dictionary<string, string[]>();

    if(!service.AccountIsValid(transactionArgument.AccountNumber))
    {
        errors.Add(nameof(transactionArgument.AccountNumber), ["Invalid account number."]);
    }

    if(transactionArgument.Amount <= 0)
    {
        errors.Add(nameof(transactionArgument.Amount), ["Invalid amount, must be postive."]);
    }
    if(transactionArgument.AccountNumber != null)
    {
        if((service.CheckBalance(transactionArgument.AccountNumber) - transactionArgument.Amount) < 0)
        {
            errors.Add(nameof(transactionArgument.Amount), ["Not enough balance."]);
        }
    }

    if(errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    return await next(context);
});

//Get all transactions
app.MapGet("/transactions/{accountNumber}", (string accountNumber, IBankAccountService service) => {
    var response = new {
        transactions = service.GetTransactionsByAccount(accountNumber),
        final_balance = service.CheckBalance(accountNumber)
    };
    return response;
    
    });

app.Run();
