using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Engine
{
    public class Player : LivingCreature
    {
        private int _gold;
        private int _experiencePoints;
        private Location _currentLocation;
        private Monster _currentMonster;

        public event EventHandler<MessageEventArgs> OnMessage;

        public int Gold
        {
            get { return _gold; }
            set
            {
                _gold = value;
                OnPropertyChanged("Gold");
            }
        }

        public int ExperiencePoints
        {
            get { return _experiencePoints; }
            private set
            {
                _experiencePoints = value;
                OnPropertyChanged("ExperiencePoints");
                OnPropertyChanged("Level");
            }
        }

        public int Level
        {
            get { return ((ExperiencePoints / 100) + 1); }
        }

        public Location CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                _currentLocation = value;
                OnPropertyChanged("CurrentLocation");
            }
        }

        public Weapon CurrentWeapon { get; set; }

        public List<int> LocationsVisited { get; set; }
        public BindingList<InventoryItem> Inventory { get; set; }

        public List<Weapon> Weapons
        {
            get
            {
                return Inventory.Where(x => x.Details is Weapon).Select(x =>
            x.Details as Weapon).ToList();
            }
        }

        public List<HealingPotion> Potions
        {
            get
            {
                return Inventory.Where(x => x.Details is HealingPotion).Select(x =>
            x.Details as HealingPotion).ToList();
            }
        }

        public BindingList<PlayerQuest> Quests { get; set; }

        private Player(int currentHitPoints, int maximumHitPoints, int gold,
            int experiencePoints) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;

            Inventory = new BindingList<InventoryItem>();
            Quests = new BindingList<PlayerQuest>();

            LocationsVisited = new List<int>();

        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);
            if (!player.LocationsVisited.Contains(player.CurrentLocation.Id))
            {
                player.LocationsVisited.Add(player.CurrentLocation.Id);
            }
            return player;
        }

        public void AddExperiencePoints(int experiencePointsToAdd)
        {
            ExperiencePoints += experiencePointsToAdd;
            MaximumHitPoints = 5 + Level * 5;
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

                foreach (XmlNode node in playerDataDocument.SelectNodes("/Player/LocationsVisited/LocationVisited"))
                {
                    int id = Convert.ToInt32(node.Attributes["Id"].Value);
                    player.LocationsVisited.Add(id);
                }
                if (!player.LocationsVisited.Contains(player.CurrentLocation.Id))
                {
                    player.LocationsVisited.Add(player.CurrentLocation.Id);
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
            return Inventory.Any(i => i.Details.Id == location.ItemRequiredToEnter.Id);
        }

        public bool HasThisQuest(Quest quest)
        {
            return Quests.Any(q => q.Details.Id == quest.Id);
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest playerQuest in Quests.Where(q => q.Details.Id == quest.Id))
            {
                return playerQuest.IsCompleted;
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            // See if the player has all the items needed to complete the quest here
            foreach (QuestCompletionItem questItem in quest.QuestCompletionItems)
            {
                // Dont have the item or quantity not enough
                if (!Inventory.Any(i => i.Details.Id == questItem.Details.Id
                && i.Quantity >= questItem.Quantity))
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
                InventoryItem item = Inventory.SingleOrDefault(i =>
                i.Details.Id == questItem.Details.Id);

                if (item != null)
                {
                    RemoveItemFromInventory(item.Details, questItem.Quantity);
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(i => i.Details.Id == itemToAdd.Id);
            if (item == null)
            {
                // No item, so add it
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else
            {
                item.Quantity += quantity;
            }

            RaiseIntentoryChangedEvent(itemToAdd);

        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(i => i.Details.Id == itemToRemove.Id);
            if (item == null)
            {
                // Ignore it
            }
            else
            {
                item.Quantity -= quantity;

                if (item.Quantity < 0)
                {
                    item.Quantity = 0;
                }
                if (item.Quantity == 0)
                {
                    Inventory.Remove(item);
                }

                RaiseIntentoryChangedEvent(itemToRemove);
            }
        }

        public void RaiseIntentoryChangedEvent(Item item)
        {
            if (item is Weapon)
            {
                OnPropertyChanged("Weapons");
            }
            if (item is HealingPotion)
            {
                OnPropertyChanged("Potions");
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

        private void RaiseMessage(string message, bool addExtraNewLine = false)
        {
            if (OnMessage != null)
            {
                OnMessage(this, new MessageEventArgs(message, addExtraNewLine));
            }
        }

        public void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!HasRequiredItemToEnterThisLocation(newLocation))
            {
                RaiseMessage("You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location.");
                return;
            }

            if (!LocationsVisited.Contains(CurrentLocation.Id))
            {
                LocationsVisited.Add(CurrentLocation.Id);
            }

            // Update the player's current location
            CurrentLocation = newLocation;

            // Heal the player to full
            CurrentHitPoints = MaximumHitPoints;

            Quest questInCurrentLocation = newLocation.QuestAvailableHere;

            // Location has quest
            if (questInCurrentLocation != null)
            {
                // Player has quest
                if (HasThisQuest(questInCurrentLocation))
                {
                    // Quest not completed
                    if (!CompletedThisQuest(questInCurrentLocation))
                    {

                        // Player has all items to complete the quest
                        if (HasAllQuestCompletionItems(questInCurrentLocation))
                        {
                            // Display message
                            RaiseMessage("");
                            RaiseMessage("You complete the '" + newLocation.QuestAvailableHere.Name + "' quest.");

                            // Remove quest items from inventory
                            RemoveQuestCompletionItems(questInCurrentLocation);

                            // Give quest rewards
                            RaiseMessage("You receive: ");
                            RaiseMessage(questInCurrentLocation.RewardExperiencePoints + " experience points");
                            RaiseMessage(questInCurrentLocation.RewardGold + " gold");
                            RaiseMessage(questInCurrentLocation.RewardItem.Name, true);

                            AddExperiencePoints(newLocation.QuestAvailableHere.RewardExperiencePoints);
                            Gold += questInCurrentLocation.RewardGold;

                            // Add the reward item to inventory
                            AddItemToInventory(questInCurrentLocation.RewardItem);

                            // Mark quest as completed
                            MarkQuestCompleted(questInCurrentLocation);
                        }
                    }
                }
                else
                {
                    // The player does not already have the quest

                    // Display the messages
                    RaiseMessage("You receive the " + questInCurrentLocation.Name + " quest.");
                    RaiseMessage(questInCurrentLocation.Description);
                    RaiseMessage("To complete it, return with:");
                    foreach (QuestCompletionItem questItem in questInCurrentLocation.QuestCompletionItems)
                    {
                        string name = questItem.Quantity == 1 ? questItem.Details.Name : questItem.Details.NamePlural;
                        RaiseMessage(questItem.Quantity.ToString() + " " + name);

                    }
                    RaiseMessage("");

                    // Add the quest to the player's quest list
                    Quests.Add(new PlayerQuest(questInCurrentLocation));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                RaiseMessage("You see a " + newLocation.MonsterLivingHere.Name);

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster monster = World.MonsterByID(newLocation.MonsterLivingHere.Id);

                _currentMonster = new Monster(monster.Id, monster.Name, monster.MaximumDamage, monster.RewardExperiencePoints,
                    monster.RewardGold, monster.CurrentHitPoints, monster.MaximumHitPoints);

                foreach (LootItem lootItem in monster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

            }
            else
            {
                _currentMonster = null;
            }
        }

        public void UseWeapon(Weapon weapon)
        {
            // How much damage to deal
            int damageToDeal = RandomNumberGenerator.NumberBetween(weapon.MinimumDamage, weapon.MaximumDamage);

            // Apply damage
            _currentMonster.CurrentHitPoints -= damageToDeal;

            // Display message
            RaiseMessage("You hit the " + _currentMonster.Name + " for " + damageToDeal.ToString() + " points.");

            // Check monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                // Display message
                RaiseMessage("");
                RaiseMessage("You defeated the " + _currentMonster.Name);

                // Give gold and xp
                AddExperiencePoints(_currentMonster.RewardExperiencePoints);
                Gold += _currentMonster.RewardGold;

                // Write in rtb
                RaiseMessage("You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points");
                RaiseMessage("You receive " + _currentMonster.RewardGold.ToString() + " gold");

                // Looted items
                List<InventoryItem> lootedItems = new List<InventoryItem>();
                foreach (LootItem item in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= item.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(item.Details, 1));
                    }
                }

                // If no random items, give default drop
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable.Where(i => i.IsDefaultItem))
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // Add items to inventory
                foreach (InventoryItem item in lootedItems)
                {
                    AddItemToInventory(item.Details);
                    var name = item.Quantity == 1 ? item.Details.Name : item.Details.NamePlural;
                    RaiseMessage("You loot " + item.Quantity.ToString() + " " + name);
                }

                // Enter so it looks good
                RaiseMessage("");

                // Doesnt actually move the player
                // Heals the player and creates new monster
                MoveTo(CurrentLocation);
            }
            else
            {
                // Monster is not dead

                // How much damage to deal
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                // Display message
                RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer + " points of damage.");

                // Damage the player
                CurrentHitPoints -= damageToPlayer;

                // Check if player is dead
                if (CurrentHitPoints <= 0)
                {
                    RaiseMessage("The " + _currentMonster.Name + " killed you.");

                    // Move player to HOME
                    MoveHome();
                }
            }
        }

        public void UsePotion(HealingPotion potion)
        {
            // Heal player
            CurrentHitPoints = Math.Min(CurrentHitPoints + potion.AmountToHeal, MaximumHitPoints);

            // Remove potion
            RemoveItemFromInventory(potion, 1);

            // Display message
            RaiseMessage("You drink a " + potion.Name);

            // Monster attacks

            // How much damage to deal
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Display message
            RaiseMessage("The " + _currentMonster.Name + " did " + damageToPlayer + " points of damage.");

            // Damage the player
            CurrentHitPoints -= damageToPlayer;

            // Check if player is dead
            if (CurrentHitPoints < 0)
            {
                RaiseMessage("The " + _currentMonster.Name + " killed you.");

                // Move player to HOME
                MoveHome();

            }
        }

        private void MoveHome()
        {
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
        }

        public void MoveNorth()
        {
            if (CurrentLocation.LocationToNorth != null)
            {
                MoveTo(CurrentLocation.LocationToNorth);
            }
        }

        public void MoveEast()
        {
            if (CurrentLocation.LocationToEast != null)
            {
                MoveTo(CurrentLocation.LocationToEast);
            }
        }

        public void MoveSouth()
        {
            if (CurrentLocation.LocationToSouth != null)
            {
                MoveTo(CurrentLocation.LocationToSouth);
            }
        }

        public void MoveWest()
        {
            if (CurrentLocation.LocationToWest != null)
            {
                MoveTo(CurrentLocation.LocationToWest);
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

            // Create the "LocationsVisited" child node to hold each LocationVisited node
            XmlNode locationsVisited = playerDataDocument.CreateElement("LocationsVisited");
            player.AppendChild(locationsVisited);
            // Create an "LocationVisited" node for each item in the player's inventory
            foreach (int locationID in LocationsVisited)
            {
                XmlNode locationVisited = playerDataDocument.CreateElement("LocationVisited");

                XmlAttribute id = playerDataDocument.CreateAttribute("Id");
                id.Value = locationID.ToString();
                locationVisited.Attributes.Append(id);

                locationsVisited.AppendChild(locationVisited);
            }

            // Add weapon to xml
            if (CurrentWeapon != null)
            {
                XmlNode currentWeapon = playerDataDocument.CreateElement("CurrentWeapon");
                currentWeapon.AppendChild(playerDataDocument.CreateTextNode(this.CurrentWeapon.Id.ToString()));
                stats.AppendChild(currentWeapon);
            }

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



            return playerDataDocument.InnerXml;
        }

    }
}
