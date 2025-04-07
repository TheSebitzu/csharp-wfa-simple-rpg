namespace Engine
{
    public class InventoryItem
    {
        // Constructor
        public InventoryItem(Item details, int quantity)
        {
            Details = details;
            Quantity = quantity;
        }
        public Item Details { get; set; }
        public int Quantity { get; set; }

    }
}
