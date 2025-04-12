using System.ComponentModel;
using System.Linq;

namespace Engine
{
    public class Vendor : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BindingList<InventoryItem> Inventory { get; private set; }
        public Vendor(string name)
        {
            Name = name;
            Inventory = new BindingList<InventoryItem>();
        }
        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(i => i.Details.Id == itemToAdd.Id);
            if (item == null)
            {
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                item.Quantity += quantity;
            }
            OnPropertyChanged("Inventory");
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(i => i.Details.Id == itemToRemove.Id);
            if (item == null)
            {
                // No item so ignore
            }
            else
            {
                item.Quantity -= quantity;

                // Dont allow negative quantity
                if (item.Quantity < 0)
                {
                    item.Quantity = 0;
                }
                // If its 0, remove the item
                if (item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }
                OnPropertyChanged("Inventory");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
