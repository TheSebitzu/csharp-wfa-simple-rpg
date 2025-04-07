using Engine;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace SimpleRPG
{
    public partial class SimpleRPG : Form
    {
        private Player _player;
        private Monster _currentMonster;

        public SimpleRPG()
        {
            InitializeComponent();

            // Create player
            _player = new Player(10, 10, 20, 0, 1);

            // Start at home
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));

            // Give the player a sword
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            // Populate the labels with info
            RefreshAll();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void btnUseWeapons_Click(object sender, EventArgs e)
        {
            // Get the selected weapon
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            // How much damage to deal
            int damageToDeal = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            // Apply damage
            _currentMonster.CurrentHitPoints -= damageToDeal;

            // Display message
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToDeal.ToString() + " points." + Environment.NewLine;

            // Check monster is dead
            if (_currentMonster.CurrentHitPoints <= 0)
            {
                // Display message
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;

                // Give gold and xp
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                _player.Gold += _currentMonster.RewardGold;

                // Write in rtb
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold" + Environment.NewLine;

                // Looted items
                List<InventoryItem> lootedItems = new List<InventoryItem>();
                foreach(LootItem item in _currentMonster.LootTable)
                {
                    if (RandomNumberGenerator.NumberBetween(1, 100) <= item.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(item.Details, 1));
                    }
                }

                // If no random items, give default drop
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        if (lootItem.IsDefaultItem)
                        {
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                // Add items to inventory
                foreach(InventoryItem item in  lootedItems)
                {
                    _player.AddItemToInventory(item.Details);
                    var name = item.Quantity == 1 ? item.Details.Name : item.Details.NamePlural;
                    rtbMessages.Text += "You loot " + item.Quantity.ToString() + " " + name + Environment.NewLine;
                }    

                RefreshAll();

                // Enter so it looks good
                rtbMessages.Text += Environment.NewLine;

                // Doesnt actually move the player
                // Heals the player and creates new monster
                MoveTo(_player.CurrentLocation);
            }
            // Monster is not dead
            else
            {
                CurrentMonsterAttack();
            }
        }

        private void btnUsePotions_Click(object sender, EventArgs e)
        {
            // Get current potion
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            // Heal player
            _player.CurrentHitPoints = Math.Min(_player.CurrentHitPoints + potion.AmountToHeal, _player.MaximumHitPoints);

            // Remove potion
            foreach(InventoryItem item in _player.Inventory)
            {
                if(item.Details.Id == potion.Id)
                {
                    item.Quantity--;
                    break;
                }

            }
            // Display mesage
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            CurrentMonsterAttack();


        }

        private void MoveTo(Location newLocation)
        {
            //Does the location have any required items
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                return;
            }

            // Update the player's current location
            _player.CurrentLocation = newLocation;

            // Update available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // Heal the player to full
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            RefreshAll();

            // Update HP in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            Quest questInCurrentLocation = newLocation.QuestAvailableHere;

            // Location has quest
            if (questInCurrentLocation != null)
            {
                // Player has quest
                if (_player.HasThisQuest(questInCurrentLocation))
                {
                    // Quest not completed
                    if (!_player.CompletedThisQuest(questInCurrentLocation))
                    {

                        // Player has all items to complete the quest
                        if (_player.HasAllQuestCompletionItems(questInCurrentLocation))
                        {
                            // Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the '" + questInCurrentLocation.Name + "' quest." + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(questInCurrentLocation);

                            // Give quest rewards (gold and xp)
                            GiveQuestRewards(questInCurrentLocation);

                            // Add the reward item to the player's inventory
                            _player.AddItemToInventory(questInCurrentLocation.RewardItem);

                            // Mark the quest as completed
                            _player.MarkQuestCompleted( questInCurrentLocation);
                        }
                    }
                }
                else
                {
                    // The player does not already have the quest

                    // Display the messages
                    rtbMessages.Text += "You receive the " + questInCurrentLocation.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += questInCurrentLocation.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with:" + Environment.NewLine;
                    foreach (QuestCompletionItem questItem in questInCurrentLocation.QuestCompletionItems)
                    {
                        string name = questItem.Quantity == 1 ? questItem.Details.Name : questItem.Details.NamePlural;
                        rtbMessages.Text += questItem.Quantity.ToString() + " " + name + Environment.NewLine;
                        
                    }
                    rtbMessages.Text += Environment.NewLine;

                    // Add the quest to the player's quest list
                    _player.Quests.Add(new PlayerQuest(questInCurrentLocation));
                }
            }

            // Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                // Make a new monster, using the values from the standard monster in the World.Monster list
                Monster monster = World.MonsterByID(newLocation.MonsterLivingHere.Id);

                _currentMonster = new Monster(monster.Id, monster.Name, monster.MaximumDamage,monster.RewardExperiencePoints,
                    monster.RewardGold, monster.CurrentHitPoints, monster.MaximumHitPoints);

                foreach (LootItem lootItem in monster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                ShowCombat(true);
            }
            else
            {
                _currentMonster = null;

                ShowCombat(false);
            }

            RefreshAll();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // No weapons avalaible
                cboWeapons.Visible = false;
                btnUseWeapons.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotions.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void GiveQuestRewards(Quest quest)
        {
            rtbMessages.Text += "You receive: " + Environment.NewLine;
            rtbMessages.Text += quest.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
            rtbMessages.Text += quest.RewardGold.ToString() + " gold" + Environment.NewLine;
            rtbMessages.Text += quest.RewardItem.Name + Environment.NewLine;
            rtbMessages.Text += Environment.NewLine;

            _player.ExperiencePoints += quest.RewardExperiencePoints;
            _player.Gold += quest.RewardGold;
        }

        private void ShowCombat(bool state)
        {
            cboWeapons.Visible = state;
            cboPotions.Visible = state;
            btnUseWeapons.Visible = state;
            btnUsePotions.Visible = state;
        }

        private void RefreshAll()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();

            UpdateInventoryListInUI();
            UpdateWeaponListInUI();
            UpdatePotionListInUI();
            UpdateQuestListInUI();
        }

        private void CurrentMonsterAttack()
        {
            // Damage monster will deal
            int monsterDamage = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            // Display message
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + monsterDamage.ToString() + " points of damage." + Environment.NewLine;

            // Subtract damage from player
            _player.CurrentHitPoints -= monsterDamage;


            // Check if player is dead
            if (_player.CurrentHitPoints <= 0)
            {
                // Display message
                rtbMessages.Text = "The " + _currentMonster.Name + " did " + monsterDamage.ToString() + " points of damage." + Environment.NewLine;
                rtbMessages.Text = "The " + _currentMonster.Name + " killed you." + Environment.NewLine + "You died" + Environment.NewLine;

                // Move player to "Home"
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            // Update UI
            RefreshAll();
        }
    }
}
