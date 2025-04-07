namespace Engine
{
    public class QuestCompletionItem
    {
        // Constructor
        public QuestCompletionItem(Item details, int quantity)
        {
            Details = details;
            Quantity = quantity;
        }
        public Item Details { get; set; }
        public int Quantity { get; set; }
    }
}
