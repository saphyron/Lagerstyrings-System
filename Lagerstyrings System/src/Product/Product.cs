namespace Lagerstyrings_System {
    /// <summary>
    /// Generic product with no special rules implementations.
    /// </summary>
    public class Product : IProduct {
        private static readonly ProductTypeEnum DEFAULT_PRODUCT_TYPE = ProductTypeEnum.Other;
        public long Id { get; }
        public string? Name { get; set; }
        public ProductTypeEnum ProductType { get; set; }

        public Product() : this(null, DEFAULT_PRODUCT_TYPE) {
        }

        public Product(string? name, ProductTypeEnum productType) {
            Name = name;
            ProductType = productType;
            //  Connect with database and make new item there; get auto-generated ID from database for object's ID field
        }

        public Product(long id) {
            //  Get existing item from database
            Id = id;
            throw new NotImplementedException();
        }
    }
}
