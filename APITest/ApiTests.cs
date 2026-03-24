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
        
        private HttpClient _client ;


        
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            
            //DATABASE CONTAINER 
            _fixture = new DatabaseFixture();
            await _fixture.InitializeAsync();
            DbContext = _fixture.DbContext;
            
            
            // TEST WEBSERVER
            
            var connectionString = _fixture.DbContext.Database.GetConnectionString();
            var factory = new CustomWebApplicationFactory(connectionString!);
            _client = factory.CreateClient();
            
            

            orderControler = new OrderController(DbContext);
        }
        
        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _fixture.DisposeAsync();
             _client.Dispose();
        }


        [SetUp]
        public async Task Setup()
        {
            _fixture.DbContext.Orders.RemoveRange(_fixture.DbContext.Orders);
            await _fixture.DbContext.SaveChangesAsync();
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

            var client = new RestClient("http://localhost:5195");
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
           

            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/order/Josue");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            //Assert
            Assert.That(content.Contains("Josue"), Is.True);
        }


        [Test]
        public async Task GetOrder_FastPerformanceTest()
        {

            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response =  await _client.GetAsync("/order/Josue");
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


            // Assing 
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
            var response = await _client.PutAsJsonAsync("/order/Josue", changingOrder);

            response.EnsureSuccessStatusCode();

            var updatedOrder = await response.Content.ReadFromJsonAsync<Order>();

            //Assert
            
            Assert.That(updatedOrder, Is.Not.Null);
            Assert.That(updatedOrder!.CustomerName, Is.EqualTo("Isaac"));
            
        }
        
        [Test]
        public async Task DeleteOrder_RemovesOrder_From_Database()
        {


            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var response = await _client.DeleteAsync("/order/Josue");

            response.EnsureSuccessStatusCode();
            
            var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
            
            //Assert
            Assert.That(orderResult, Is.Null);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("Order Josue has been deleted successfully"));    }


        [Test]
        public async Task DeleteOrder_DoesNot_AllowIncorrect_User()
        {

            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            
          
            // Create a specific request HTTP REQUEST to add different headers for roles
            var request = new HttpRequestMessage(HttpMethod.Delete, "/order/Josue");
            request.Headers.Add("Test-Role", "User"); 

            // Act
            var response = await _client.SendAsync(request);
            
            var statuscode = response.StatusCode;
            
            
            var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
            
            //Assert
            
            Assert.That(orderResult?.CustomerName, Is.EqualTo("Josue"));
            Assert.That(statuscode, Is.EqualTo(HttpStatusCode.Forbidden));
            
        }
        
 
        
        [Test]
        public async Task DeleteOrder_DoesNot_AllowUnathenticated_User()
        {
            // Assing
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            
         
            
            
            
            // Create a specific request HTTP REQUEST to add different headers for roles
            var request = new HttpRequestMessage(HttpMethod.Delete, "/order/Josue");
            request.Headers.Add("Test-Role", "Unauthorized");

            // Act
            var response = await _client.SendAsync(request);
            
            var statuscode = response.StatusCode;
            
            
            var orderResult = await _fixture.DbContext.Orders.FirstOrDefaultAsync(x=>x.CustomerName=="Josue");
            
            //Assert
            Assert.That(orderResult?.CustomerName, Is.EqualTo("Josue"));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));        
        }

        [Test]
        public async Task DeleteOrder_Concurrency_test()
        {
           

            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            
            _client.DefaultRequestHeaders.Add("Test-Role", "Admin");
            
            //ACT 
            
            var task1 = _client.DeleteAsync("/order/Josue");
            var task2 = _client.DeleteAsync("/order/Josue");

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
           

            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };

            await _fixture.DbContext.Orders.AddAsync(order);
            await _fixture.DbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/order/Isaac");
            
            //Assert

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            
        }

        [Test]
        public async Task PostOrder_ShouldPostTheOrderCorrectly()
        {

            
            // Assing 
            var order = new Order
            {
                CustomerName = "Josue",
                OrderDate = DateTime.UtcNow
            };
            
            //Act 
            
            var response = await _client.PostAsJsonAsync("/order", order);
            
            //Assert
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            
            
        }
        

        [Test]
        public async Task PostOrder_SouldNotAllowBadRequest()
        {
            
            // Assing
            var order = new Order
            {
                OrderDate = DateTime.UtcNow

            };
            
            //Act
            
            var response = await _client.PostAsJsonAsync("/order", order);
            
            
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            Assert.That(content.Contains("The CustomerName field is required"), Is.True);
            
        }
        
       
        
        
        
    }