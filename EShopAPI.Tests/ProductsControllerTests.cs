using Xunit;
using EshopAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EshopAPI.Models;

namespace EShopAPI.Tests;
public class ProductsControllerTests
{
    private readonly DbContextOptions<AppDbContext> _contextOptions;

public ProductsControllerTests()
{
    _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("ProductsControllerTest")
        .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    using var context = new AppDbContext(_contextOptions);

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    context.AddRange(
        new Product { Title = "Product1", Price = 10.99m, Description = "Description1" },
        new Product { Title = "Product2", Price = 20.99m, Description = "Description2" }
    );

    context.SaveChanges();
}}