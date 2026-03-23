using ClassLibrary1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Controller;

[ApiController]
public class OrderController: ControllerBase
{

    private ExampleContext _context;
    
    public OrderController (ExampleContext context)
    {
        _context= context;
    }

    [HttpGet("/order/{order}")]
    public async  Task<ActionResult<Order>> GetOrder(string order)
    {
        var dbOrder = await _context.Orders.FirstOrDefaultAsync(x=>x.CustomerName==order);

        if (dbOrder == null)
        {
            return NotFound();
        }
        

        return dbOrder;
    }
    
    [HttpPost("/order")]
    public async Task<ActionResult<Order>> PostOrder([FromBody]  Order postOrder)
    {

    
        await _context.Orders.AddAsync(postOrder);
        await _context.SaveChangesAsync();
        
        return Created();
        
        
    }
    


    [HttpPut("/order/{order}")]
    public async Task<ActionResult<Order>> EditOrder([FromBody]  Order updatedOrder, string order)
    {
        var dbOrder = await _context.Orders.FirstOrDefaultAsync(x=>x.CustomerName==order);

        if (dbOrder == null)
        {
            return NotFound();
        }
        dbOrder.CustomerName = updatedOrder.CustomerName;
        dbOrder.OrderDate = updatedOrder.OrderDate;
        await _context.SaveChangesAsync();
        
        return dbOrder;
        
        
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("/order/{order}")]
    public async Task<ActionResult> DeleteOrder(string order)
    {
        var dbOrder = await _context.Orders.FirstOrDefaultAsync(x=>x.CustomerName==order);
        if (dbOrder == null)
        {
            return NotFound();
        }
        _context.Remove(dbOrder);
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }
        
        return Ok($"Order {order} has been deleted successfully");
        
    }
    
}