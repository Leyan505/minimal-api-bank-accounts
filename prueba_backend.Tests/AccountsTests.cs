using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using prueba_backend.Models;

namespace prueba_backend.Tests;

public class AccountTests
{
    [Fact]
    public async Task CreateBankAccount()
    {
        await using var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();
        var result = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Balance = 1000.0
        });
    
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateBankAccountValidatesObject()
    {
        await using var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();
        var result = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI2222-413-BCCE-AVDSDSE",
            Balance = -1000.0
        });
    
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
