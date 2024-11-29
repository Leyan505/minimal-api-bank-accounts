using System.Text.RegularExpressions;
using prueba_backend.Models;

namespace prueba_backend.Services
{
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
                ResultingBalance = account.Balance
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
                ResultingBalance = account.Balance
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
            //IBAN standardization NI-41-BCCE-00000010022400183307  	
            if (accountNumber == null || accountNumber.Length != 31 || !Regex.IsMatch(accountNumber, @"^NI-[0-9]{2}-[A-Z]{4}-[0-9]{20}$"))
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