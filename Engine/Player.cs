using System.Collections.Generic;

namespace Engine
{
    public class Player : LivingCreature
    {
        // Constructor
        public Player(int currentHitPoints, int maximumHitPoints, int gold,
            int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            // There is no required item for this location
            if (location.ItemRequiredToEnter == null)
            {
                return true;
            }

            // See if the player has the required item in inventory
            foreach (InventoryItem item in Inventory)
            {
                if (item.Details.Id == location.ItemRequiredToEnter.Id)
                {
                    // Found the required item
                    return true;
                }
            }
            // Didnt find the required item in inventory
            return false;
        }

        public bool HasThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.Id == quest.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.Id == quest.Id)
                {
                    return playerQuest.IsCompleted;
                }
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
            {
                bool itemInInventory = false;

                // Check items in player inventory
                foreach (InventoryItem item in Inventory)
                {
                    // Item is in inventory
                    if (item.Details.Id == questItem.Details.Id)
                    {
                        itemInInventory = true;

                        // Not enough
                        if (item.Quantity < questItem.Quantity) 
                        {
                            return false;
                        }
                    }
                }

                // Doesnt have item in inventory
                if (!itemInInventory)
                {
                    return false;
                }
            }

            // Didnt return false anywhere
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
            {
                foreach (InventoryItem item in Inventory)
                {
                    if (item.Details.Id == questItem.Details.Id)
                    {
                        // Subtract the quantity from the player's inventory that was needed to complete the quest
                        item.Quantity -= questItem.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach (InventoryItem item in Inventory)
            {
                if (item.Details.Id == itemToAdd.Id)
                {
                    // Has item in inventory
                    item.Quantity++;

                    // Done
                    return;
                }
            }

            // No item, so add it
            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        public void MarkQuestCompleted(Quest quest)
        {
            // Search quest
            foreach (PlayerQuest playerQuest in Quests)
            {
                if (playerQuest.Details.Id == quest.Id)
                {
                    // Mark as completed
                    playerQuest.IsCompleted = true;

                    return;
                }
            }
        }

        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        
        public int Level
        {
            get { return ((ExperiencePoints / 100) + 1); }
        }

        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }
    }
}
