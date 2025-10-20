namespace Lagerstyrings_System {
    /// <summary>
    /// Interface for product class, the instantiated object of which represents 
    /// a type of product. 
    /// Not instantiated for every item unit.
    /// </summary>
    public interface IProduct {
        /// <summary>
        /// ID-field. 
        /// Cannot be set manually; auto-generated upon creation in database.
        /// </summary>
        long Id { get; }
        /// <summary>
        /// Human-legible name of warehouse.
        /// Not strictly necessary? Recommended to set anyway.
        /// </summary>
        String? Name { get; set; }
        /// <summary>
        /// Sub-classification of product. 
        /// May be used to implement rules of handling, such as FIFO-principles or 
        /// required refrigeration.
        /// </summary>
        ProductTypeEnum ProductType { get; set; }
    }
}
