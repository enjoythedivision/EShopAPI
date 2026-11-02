using EshopAPI.Data;
using EshopAPI.DTOs;
using EshopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EshopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context, ILogger<OrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ToListAsync();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, "Unable to retrieve orders.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order is null) return NotFound("Order not found.");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, "Unable to retrieve the order.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto orderDto)
        {
            try
            {
                var order = new Order
                {
                    CustomerName = orderDto.CustomerName,
                    Email = orderDto.Email,
                    ShippingAddress = orderDto.ShippingAddress
                };

                var totalAmount = 0m;

                foreach (var orderItem in orderDto.Items)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product is null) return NotFound($"Product with ID {orderItem.ProductId} not found.");

                    if (orderItem.Quantity > product.Stock)
                    {
                        return BadRequest($"Not enough stock for {product.Title}.");
                    }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Unable to create the order.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var orderToDelete = await _context.Orders.FindAsync(id);
                if (orderToDelete is null) return NotFound("Order not found.");

                _context.Orders.Remove(orderToDelete);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", id);
                return StatusCode(500, "Unable to delete the order.");
            }
        }
    }
}
