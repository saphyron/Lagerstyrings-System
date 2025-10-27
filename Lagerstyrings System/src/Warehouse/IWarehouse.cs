namespace Lagerstyrings_System {
    public interface IWarehouse {
        /// <summary>
        /// ID-field. Cannot be set manually; auto-generated upon creation in database.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Human-legible name of warehouse.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Add an existing stock-keeping unit tied to this warehouse, or create 
        /// a new stock-keeping unit, depending on implementation.
        /// </summary>
        void AddSku();
    }
}