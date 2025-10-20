namespace Lagerstyrings_System {
    public class Warehouse : IWarehouse {
        public long Id { get; }
        public String? Name { get; set; }

        /// <summary>
        /// Create a new Warehouse object with a database parallel of given name.
        /// </summary>
        public Warehouse() : this(null) {
        }

        /// <summary>
        /// Create a new Warehouse object with a database parallel of given name.
        /// </summary>
        /// <param name="name">Human-legible name of Warehouse</param>
        public Warehouse(String? name) {
            //  Database create new warehouse
            this.Name = name;
        }

        /// <summary>
        /// Create Warehouse object populated from data in database.
        /// </summary>
        /// <param name="id">ID of Warehouse in database</param>
        public Warehouse(long id) {
            //  this object should populate from database
            this.Id = id;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new, empty SKU object assigned to this Warehouse.
        /// This SKU has no assigned Product, and holds no items.
        /// </summary>
        public void AddSku() {
            AddSku(null, 0);
        }

        /// <summary>
        /// Create new SKU object assigned to this Warehouse.
        /// This SKU has an assigned Product, but holds no items.
        /// </summary>
        /// <param name="product">Product object which the SKU will contain</param>
        public void AddSku(IProduct? product) {
            AddSku(product, 0);
        }

        /// <summary>
        /// Create new SKU object assigned to this Warehouse.
        /// </summary>
        /// <param name="product">Product object which the SKU will contain</param>
        /// <param name="productCount">Number of units of product which the SKU will contain</param>
        public void AddSku(IProduct? product, ulong productCount) {
            SKU sku = new SKU(this);
            sku.product = product;
            sku.count = productCount;
        }

        /// <summary>
        /// Add existing SKU object to Warehouse.
        /// Typically used for moving SKUs from one Warehouse to another.
        /// </summary>
        /// <param name="sku">Existing SKU object</param>
        public void AddSku(SKU sku) {
            sku.warehouse = this;
        }
    }
}
