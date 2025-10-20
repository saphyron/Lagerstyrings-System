namespace Lagerstyrings_System {
    /// <summary>
    /// Stock-keeping unit.
    /// Represents a container holding one specific type of product.
    /// Must always exist within a warehouse.
    /// </summary>
    public class SKU {
        /// <summary>
        /// ID-field. 
        /// Cannot be set manually; auto-generated upon creation in database.
        /// </summary>
        public long Id { get; }
        /// <summary>
        /// Warehouse within which the stock-keeping unit exists.
        /// </summary>
        public IWarehouse warehouse { get; set; }
        /// <summary>
        /// Type of product contained within the stock-keeping unit.
        /// </summary>
        public IProduct? product { get; set; }
        /// <summary>
        /// Number of products within the stock-keeping unit.
        /// Cannot be negative.
        /// </summary>
        public ulong count { get; set; }

        /// <summary>
        /// Create new stock-keeping unit within context of given warehouse.
        /// </summary>
        /// <param name="warehouse">Warehouse within which the SKU should exist</param>
        public SKU(IWarehouse warehouse) {
            this.warehouse = warehouse;
        }

        /// <summary>
        /// Create SKU object from existing data in database.
        /// </summary>
        /// <param name="id">ID of SKU</param>
        public SKU(long id) {
            //  Get existing SKU from database, and populate
            this.Id = id;
            warehouse = null; throw new NotImplementedException();
        }
    }
}
