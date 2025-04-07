namespace Engine
{
    public class HealingPotion : Item
    {
        // Constructor
        public HealingPotion(int id, string name, string namePlural, int amountToHeal)
            : base(id, name, namePlural)
        {
            AmountToHeal = amountToHeal;
        }

        public int AmountToHeal { get; set; }
    }
}
