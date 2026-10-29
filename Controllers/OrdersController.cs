using EshopAPI.Data;
using EshopAPI.DTOs;
using EshopAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EshopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();
            var ordersResponse = orders.Select(order => new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            }).ToList();
            return Ok(ordersResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (order is null) return NotFound();
            var orderResponse = new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
            return Ok(orderResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDto)
        {
            var order = new Order
            {
                CustomerName = orderDto.CustomerName,
                Email = orderDto.Email,
                ShippingAddress = orderDto.ShippingAddress,

            };

            var totalAmount = 0m;
            
            foreach (var orderItem in orderDto.Items)
            {
                var product = await _context.Products.FindAsync(orderItem.ProductId);
                if (product is null) return NotFound();
                var orderItemEntity = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = orderItem.Quantity,
                    UnitPrice = product.Price
                };
                order.OrderItems.Add(orderItemEntity);
                totalAmount += orderItemEntity.Quantity * orderItemEntity.UnitPrice;
                product.Stock -= orderItemEntity.Quantity;
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var orderResponse = new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var orderToDelete = await _context.Orders.FindAsync(id);
            if (orderToDelete is null) return NotFound();
            _context.Orders.Remove(orderToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
            
        }

    }
}
