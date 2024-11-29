using System.ComponentModel;
using System.Timers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.OpenApi.Services;
using Microsoft.VisualBasic;
using prueba_backend;

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

namespace prueba_backend
{
    public class BankAccount{
        public required string AccountNumber {get; set;}
        public required double Balance {get; set;}
    };
    public class Transaction
    {
        private static int _idCounter = 0;
        public int Id {get; private set;}
        public required string Type {get; set;}
        public required double Amount {get; set;}
        public required string AccountNumber {get; set;}
        public required double SaldoResultante {get; set;}
        public Transaction(string Type, double Amount, string AccountNumber, double SaldoResultante )
        {
            Id = _idCounter + 1;
            this.Type = Type;
            this.Amount = Amount;
            this.AccountNumber = AccountNumber;
            this.SaldoResultante = SaldoResultante;

        }

        public Transaction()
        {
            Id = ++_idCounter;
        }
    }

    public class TransactionDTO{
        public required double Amount {get; set;}
        public required string AccountNumber {get; set;}
    }

    interface IBankAccountService
    {
        List<BankAccount> GetBankAccounts();
        List<Transaction> GetTransactions();
        List<Transaction> GetTransactionsByAccount(string accountNumber);


        BankAccount AddAccount(BankAccount account);

        bool Duplicate(string accountNumber);
        double? CheckBalance(string accountNumber);
        Transaction? Deposit(TransactionDTO newTransaction);
        Transaction? Withdraw(TransactionDTO newTransaction);

        public bool AccountExists(string accountNumber);
        public bool AccountIsValid(string accountNumber);
    }

    class InMemoryBankAccountService : IBankAccountService
    {
        private readonly List<BankAccount> _accounts = [];
        private readonly List<Transaction> _transactions = [];
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

        public Transaction Deposit(TransactionDTO newTransaction)
        {
            var account = _accounts.Find(x => x.AccountNumber.Equals(newTransaction.AccountNumber));
            
            account.Balance += newTransaction.Amount;
            var transaction = new Transaction(){
                Amount = newTransaction.Amount,
                AccountNumber = newTransaction.AccountNumber,
                Type = "DEPOSIT",
                SaldoResultante = account.Balance
            };
            _transactions.Add(transaction);
            return transaction;
        }

        public Transaction Withdraw(TransactionDTO newTransaction)
        {
            var account = _accounts.Find(x => x.AccountNumber.Equals(newTransaction.AccountNumber));
            
            account.Balance -= newTransaction.Amount;
            var transaction = new Transaction(){
                Amount = newTransaction.Amount,
                AccountNumber = newTransaction.AccountNumber,
                Type = "WITHDRAW",
                SaldoResultante = account.Balance
            };
            _transactions.Add(transaction);
            return transaction;
        }

        public bool AccountExists(string accountNumber)
        {
            var account = _accounts.Find(x => x.AccountNumber.Equals(accountNumber));
            return account != null;

        }
        public bool AccountIsValid(string accountNumber)
        {
            if (accountNumber == null || accountNumber.Length != 20 || !accountNumber.All(char.IsDigit) || !AccountExists(accountNumber))
            {
               return false;
            }
            return true;

        }

        public List<Transaction> GetTransactions()
        {
            return _transactions;
        }

        public List<Transaction> GetTransactionsByAccount(string accountNumber)
        {
            var transactions = _transactions.FindAll(x => x.AccountNumber.Equals(accountNumber));
            return transactions;
        }
    }
}