using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using prueba_backend.Models;

namespace prueba_backend.Tests;

public class TransactionsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TransactionsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateDeposit()
    {
        var client = _factory.CreateClient();
        var resultAccount = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Balance = 1000.0
        });

        var result = await client.PostAsJsonAsync("/deposit", new TransactionDTO
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Amount = 500
        });
    
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        
        var content = await result.Content.ReadAsStringAsync();
        var Response = JsonDocument.Parse(content);
        var message = Response.RootElement.GetProperty("resultingBalance").GetDouble();
        message.Should().Be(1500);
    }
    
    [Fact]
    public async Task CreateDepositValidatesObject()
    {
        await using var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();
        var resultAccount = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Balance = 1000.0
        });
        var result = await client.PostAsJsonAsync("/deposit", new TransactionDTO
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Amount = -500
        });
    
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
     [Fact]
    public async Task CreateWithdraw()
    {
        await using var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();
        var resultAccount = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Balance = 1500.0
        });
        var result = await client.PostAsJsonAsync("/withdraw", new TransactionDTO
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Amount = 200
        });
    
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        var content = await result.Content.ReadAsStringAsync();
        var Response = JsonDocument.Parse(content);
        var message = Response.RootElement.GetProperty("resultingBalance").GetDouble();
        message.Should().Be(1300);
    }
    
    [Fact]
    public async Task CreateWithdrawValidatesObject()
    {
        await using var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();
        var resultAccount = await client.PostAsJsonAsync("/accounts", new BankAccount
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Balance = 1500.0
        });
        var result = await client.PostAsJsonAsync("/withdraw", new TransactionDTO
        {
            AccountNumber = "NI-41-BCCE-00000010022400183307",
            Amount = 2000
        });
    
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}
