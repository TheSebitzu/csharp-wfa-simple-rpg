using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Engine
{
    public class Player : LivingCreature
    {
        // Constructor
        private Player(int currentHitPoints, int maximumHitPoints, int gold,
            int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);
            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            // If it exists
            try
            {
                // Load the XML
                XmlDocument playerDataDocument = new XmlDocument();
                playerDataDocument.LoadXml(xmlPlayerData);

                // Get currentHP, maxHP, gold, xp
                int currentHitPoints = Convert.ToInt32(playerDataDocument.
                    SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                int maximumHitPoints = Convert.ToInt32(playerDataDocument
                    .SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                int gold = Convert.ToInt32(playerDataDocument.
                    SelectSingleNode("/Player/Stats/Gold").InnerText);
                int experiencePoints = Convert.ToInt32(playerDataDocument.
                    SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

                // Create a player with those stats
                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints);

                // Get location and send player there
                int currentLocationID = Convert.ToInt32(playerDataDocument.
                    SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                // Add weapon
                if (playerDataDocument.SelectSingleNode("/Player/Stats/CurrentWeapon") != null)
                {
                    int currentWeaponId = Convert.ToInt32(playerDataDocument.
                        SelectSingleNode("/Player/Stats/CurrentWeapon").InnerText);
                    player.CurrentWeapon = (Weapon)World.ItemByID(currentWeaponId);
                }

                // Add items to inventoy
                foreach (XmlNode node in playerDataDocument.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["Id"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                // Add quests
                foreach (XmlNode node in playerDataDocument.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["Id"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);
                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;
                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            // If we cant read the XML, create a new player
            catch
            { 
                return Player.CreateDefaultPlayer();
            }
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

        public string ToXmlString()
        {
            XmlDocument playerDataDocument = new XmlDocument();

            // Create the <player>
            XmlNode player = playerDataDocument.CreateElement("Player");
            playerDataDocument.AppendChild(player);

            // Create the <stats> as a child of <player>
            XmlNode stats = playerDataDocument.CreateElement("Stats");
            player.AppendChild(stats);

            // Current HP
            XmlNode currentHP = playerDataDocument.CreateElement("CurrentHitPoints");
            currentHP.AppendChild(playerDataDocument.CreateTextNode(this.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHP);

            // Maximum HP
            XmlNode maximumHP = playerDataDocument.CreateElement("MaximumHitPoints");
            maximumHP.AppendChild(playerDataDocument.CreateTextNode(this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHP);

            // Gold
            XmlNode gold = playerDataDocument.CreateElement("Gold");
            gold.AppendChild(playerDataDocument.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            // Experience
            XmlNode experiencePoints = playerDataDocument.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerDataDocument.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);

            // Current location
            XmlNode currentLocation = playerDataDocument.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerDataDocument.CreateTextNode(this.CurrentLocation.Id.ToString()));
            stats.AppendChild(currentLocation);

            // Create the <inventoryItems> as a child of <player>
            XmlNode inventoryItems = playerDataDocument.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            // Create <item> children to <items>
            foreach (InventoryItem item in this.Inventory)
            {
                XmlNode inventoryItem = playerDataDocument.CreateElement("InventoryItem");

                // Id atribute
                XmlAttribute id = playerDataDocument.CreateAttribute("Id");
                id.Value = item.Details.Id.ToString();
                inventoryItem.Attributes.Append(id);

                // Quantity atribute
                XmlAttribute quantity = playerDataDocument.CreateAttribute("Quantity");
                quantity.Value = item.Quantity.ToString();
                inventoryItem.Attributes.Append(quantity);

                // Add the child <item> to <items>
                inventoryItems.AppendChild(inventoryItem);
            }

            // Create the <playerQuests> as a child of <player>
            XmlNode playerQuests = playerDataDocument.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            // Create <quest> children to <quests>
            foreach (PlayerQuest quest in this.Quests)
            {
                XmlNode playerQuest = playerDataDocument.CreateElement("PlayerQuest");

                // Id atribute
                XmlAttribute idAttribute = playerDataDocument.CreateAttribute("Id");
                idAttribute.Value = quest.Details.Id.ToString();
                playerQuest.Attributes.Append(idAttribute);

                // IsCompleted atribute
                XmlAttribute isCompletedAttribute = playerDataDocument.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = quest.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);

                // Add the <quest> item to <quests>
                playerQuests.AppendChild(playerQuest);
            }

            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerDataDocument.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerDataDocument.CreateTextNode(this.CurrentWeapon.Id.ToString()));
                stats.AppendChild(currentWeapon);
            }

            return playerDataDocument.InnerXml;
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
        public Weapon CurrentWeapon { get; set; }
    }
}
