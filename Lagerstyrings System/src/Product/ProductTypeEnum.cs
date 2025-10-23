namespace Lagerstyrings_System {
    public enum ProductTypeEnum {
        Foodstuffs, //  Food items. Possibly perishable. Falls under certain rules. TBD.
        Medical,    //  Medical products. Possibly perishable. Falls under certain rules. TBD.
        Electronics,    //  Electronic products. Likely shock-sensitive in both senses of the word?
        Hazardous_Goods,    //  Flammable, toxic or otherwise requiring special handling and training.
        Other   //  Generic / catch-all classification.
    }
}
