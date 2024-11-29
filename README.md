<!-- GETTING STARTED -->
## Getting Started
### Prerequisites

In order to run it, the .NET SDK or the .NET Runtime has to be installed.

#### Windows

Install Visual Studio and the ASP.NET and web development workload.


#### Linux (Fedora)

* SDK
```
  sudo dnf install dotnet-sdk-9.0
```

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/Leyan505/minimal-api-bank-accounts.git
   ```

2. Build and run the API
   ```
   cd minimal-api-bank-accounts/
   dotnet run --project prueba_backend
   ```
   
### Testing
   ```
   dotnet test
   ```

### Endpoints

- GET http://localhost:5005/accounts
- POST http://localhost:5005/accounts

- GET http://localhost:5005/check_balance/{account_number}

- POST http://localhost:5005/deposit
- POST http://localhost:5005/withdraw

- GET http://localhost:5005/transactions/{account_number}

