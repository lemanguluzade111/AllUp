using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace AllUp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double? OldPrice { get; set; }
        public double Rate { get; set; }
        public bool IsDeactive { get; set; }
        public int BrandId { get; set; }
        [NotMapped]
        public List<IFormFile> Photos { get; set; }
        public Brand Brand { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public List<ProductTag> ProductTags { get; set; }
        public ProductDetail ProductDetail { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
    }
}
