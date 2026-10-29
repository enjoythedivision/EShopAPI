namespace EshopAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public enum Category
        {
            Rock,
            Metal,
            Jazz,
            HipHop,
            Electronic
        }
        public Category ProductCategory { get; set; }
    }
}
