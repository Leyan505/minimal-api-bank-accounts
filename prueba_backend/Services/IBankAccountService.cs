using prueba_backend.Models;

namespace prueba_backend.Services{

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
}