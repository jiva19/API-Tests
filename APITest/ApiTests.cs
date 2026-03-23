using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AWMS2.IntegrationTest.Fixtures;
using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using WebApplication3.Controller;

namespace APITest;

public class Tests
{
    private DatabaseFixture _fixture = null!;
    private ExampleContext DbContext = null!;
    
    private OrderController orderControler = null!;


    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new DatabaseFixture();
        await _fixture.InitializeAsync();
        DbContext = _fixture.DbContext;

        orderControler = new OrderController(DbContext);
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.DisposeAsync();
    }
    


    [Ignore("This is just a test that can be used to see if your test container is set up correctly")]
    [Test]
    public async Task Infrastructure_ShouldConnectToDatabaseContainer()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await DbContext.Orders.AddAsync(order);
        await DbContext.SaveChangesAsync();

        var client = new RestClient("http://localhost:5195"); // <-- change to your port
        var request = new RestRequest("/order/Josue", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);
        

        // Assert
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.Content);
        Console.WriteLine(response.ErrorMessage);
        
        Assert.That(response.IsSuccessful, Is.True);

        var returnedOrder = JsonSerializer.Deserialize<Order>(response.Content!);

        Assert.That(returnedOrder, Is.Not.Null);
        Assert.That(returnedOrder.CustomerName, Is.EqualTo("Josue"));
        
        
        
    }
    
    
    [Test]
    public async Task GetOrder_Returns_Order_From_Database()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/order/Josue");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);

        //Assert
        Assert.That(content.Contains("Josue"), Is.True);
    }


    [Test]
    public async Task GetOrder_FastPerformanceTest()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var response =  await client.GetAsync("/order/Josue");
        stopwatch.Stop();
        
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(200));
        
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);

        //Assert
        Assert.That(content.Contains("Josue"), Is.True);
        
    }
    
    
    
    
    
    [Test] 
    public async Task UpdateOrder_Returns_Order_From_Database()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();
        
        var changingOrder = new Order
        {
            CustomerName = "Isaac",
            OrderDate = DateTime.UtcNow
        };

        // Act
        var response = await client.PutAsJsonAsync("/order/Josue", changingOrder);

        response.EnsureSuccessStatusCode();

        var updatedOrder = await response.Content.ReadFromJsonAsync<Order>();

        //Assert
        
        Assert.That(updatedOrder, Is.Not.Null);
        Assert.That(updatedOrder!.CustomerName, Is.EqualTo("Isaac"));
        
    }
    
    [Test]
    public async Task DeleteOrder_RemovesOrder_From_Database()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var response = await client.DeleteAsync("/order/Josue");

        response.EnsureSuccessStatusCode();
        
        var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
        
        //Assert
        Assert.That(orderResult, Is.Null);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo("Order Josue has been deleted successfully"));    }


    [Test]
    public async Task DeleteOrder_DoesNot_AllowIncorrect_User()
    {// Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        
        client.DefaultRequestHeaders.Add("Test-Role", "User");

        
        // Act
        var response = await client.DeleteAsync("/order/Josue");
        var statuscode = response.StatusCode;
        
        
        var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
        
        //Assert
        
        Assert.That(orderResult?.CustomerName, Is.EqualTo("Josue"));
        Assert.That(statuscode, Is.EqualTo(HttpStatusCode.Forbidden));
        
    }
    
    [Test]
    public async Task DeleteOrder_DoesNot_AllowUnathenticated_User()
    {// Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        
        client.DefaultRequestHeaders.Add("Test-Role", "Unauthorized");

        
        // Act
        var response = await client.DeleteAsync("/order/Josue");
        var statuscode = response.StatusCode;
        
        
        var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
        
        //Assert
        Assert.That(orderResult?.CustomerName, Is.EqualTo("Josue"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));        
    }

    [Test]
    public async Task DeleteOrder_Concurrency_test()
    {
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        
        client.DefaultRequestHeaders.Add("Test-Role", "Admin");
        
        //ACT 
        
        var task1 = client.DeleteAsync("/order/Josue");
        var task2 = client.DeleteAsync("/order/Josue");

        await Task.WhenAll(task1, task2);
        
        var response1 = task1.Result;
        var response2 = task2.Result;
        

        var statuses = new[] { response1.StatusCode, response2.StatusCode };
        
        //Assert

        Assert.That(statuses, Does.Contain(HttpStatusCode.OK));
        Assert.That(statuses, Does.Contain(HttpStatusCode.NotFound));
        
    }
    
    
    

    [Test]
    public async Task FindOrder_ShouldThrowNotfound_whenthereisnoRegister()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();

        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };

        await _fixture.DbContext.Orders.AddAsync(order);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/order/Isaac");
        
        //Assert

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        
    }

    [Test]
    public async Task PostOrder_ShouldPostTheOrderCorrectly()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();
        
        // Seed data 
        var order = new Order
        {
            CustomerName = "Josue",
            OrderDate = DateTime.UtcNow
        };
        
        //Act 
        
        var response = await client.PostAsJsonAsync("/order", order);
        
        //Assert
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        
    }
    

    [Test]
    public async Task PostOrder_SouldNotAllowBadRequest()
    {
        // Start container
        await _fixture.InitializeAsync();

        var connectionString = _fixture.DbContext.Database.GetConnectionString();

        var factory = new CustomWebApplicationFactory(connectionString!);

        var client = factory.CreateClient();
        
        // Seed data 
        var order = new Order
        {
            OrderDate = DateTime.UtcNow

        };
        
        var response = await client.PostAsJsonAsync("/order", order);
        
        
        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine(content);
        Assert.That(content.Contains("The CustomerName field is required"), Is.True);
        
    }
    
   
    
    
    
}