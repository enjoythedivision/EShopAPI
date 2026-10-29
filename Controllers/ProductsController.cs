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

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return NotFound();
            return Ok(product);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductByCategory(Product.Category category)
        {
            var products = await _context.Products.Where(p => p.ProductCategory == category).ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto productDto)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
        {
            var productToEdit = await _context.Products.FindAsync(id);

            if (productToEdit is null) return NotFound();

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var productToDelete = await _context.Products.FindAsync(id);

            if (productToDelete is null) return NotFound();

            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
