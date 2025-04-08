using System.Collections.Generic;
using System.Linq;

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
            return Inventory.Exists(item => item.Details.Id == location.ItemRequiredToEnter.Id);
        }

        public bool HasThisQuest(Quest questToFind)
        {
            return Quests.Exists(quest => quest.Details.Id == questToFind.Id);
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
                // Dont have the item or quantity not enough
                if (!Inventory.Exists(item => item.Details.Id == questItem.Details.Id
                && item.Quantity >= questItem.Quantity))
                {
                    return false;
                }
            }

            
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
            {
                InventoryItem itemToRemove = Inventory.SingleOrDefault(item =>
                item.Details.Id == questItem.Details.Id);

                if (itemToRemove != null)
                {
                    itemToRemove.Quantity -= questItem.Quantity;
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.SingleOrDefault(i => i.Details.Id == itemToAdd.Id);
            if (item == null)
            {
                // No item, so add it
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                item.Quantity++;
            }


        }

        public void MarkQuestCompleted(Quest questToComplete)
        {
            PlayerQuest quest = Quests.SingleOrDefault(q => q.Details.Id == questToComplete.Id);
            if (quest != null)
            {
                quest.IsCompleted = true;
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
