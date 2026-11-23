namespace ChudoObuv
{
    // Модель для отображения товара в каталоге
    internal class CatalogItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Category { get; set; } = "";
        public string Composition { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";

        public int Stock { get; set; }
        public string StockText => Stock > 5 ? "много" : "мало";
        public bool LowStock => Stock <= 3;

        public decimal Price { get; set; }
        public decimal PriceWithDiscount { get; set; }
        public bool HasDiscount { get; set; }
    }
}