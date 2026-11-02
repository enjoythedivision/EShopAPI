using EshopAPI.Data;
using EshopAPI.Models;
using EshopAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EshopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, "Unable to retrieve products.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product is null) return NotFound("Product not found.");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, "Unable to retrieve the product.");
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductByCategory(Product.Category category)
        {
            try
            {
                var products = await _context.Products.Where(p => p.ProductCategory == category).ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {Category}", category);
                return StatusCode(500, "Unable to retrieve products for this category.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto productDto)
        {
            try
            {
                var product = new Product
                {
                    Title = productDto.Title,
                    Artist = productDto.Artist,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    ImageUrl = productDto.ImageUrl,
                    ProductCategory = productDto.ProductCategory
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Unable to create the product.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
        {
            try
            {
                var productToEdit = await _context.Products.FindAsync(id);

                if (productToEdit is null) return NotFound("Product not found.");

                productToEdit.Title = productDto.Title;
                productToEdit.ImageUrl = productDto.ImageUrl;
                productToEdit.Stock = productDto.Stock;
                productToEdit.Artist = productDto.Artist;
                productToEdit.Description = productDto.Description;
                productToEdit.Price = productDto.Price;
                productToEdit.ProductCategory = productDto.ProductCategory;
                await _context.SaveChangesAsync();
                return Ok(productToEdit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "Unable to update the product.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var productToDelete = await _context.Products.FindAsync(id);

                if (productToDelete is null) return NotFound("Product not found.");

                _context.Products.Remove(productToDelete);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "Unable to delete the product.");
            }
        }
    }
}
