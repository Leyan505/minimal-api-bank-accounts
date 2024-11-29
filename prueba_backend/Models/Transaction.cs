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