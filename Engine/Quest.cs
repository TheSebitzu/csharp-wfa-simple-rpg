﻿using System.Collections.Generic;

namespace Engine
{
    public class Quest
    {
        // Constructor
        public Quest(int id, string name, string description,
            int rewardExperiencePoints, int rewardGold, Item rewardItem = null)
        {
            Id = id;
            Name = name;
            Description = description;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = rewardGold;
            RewardItem = rewardItem;
            QuestCompletionItems = new List<QuestCompletionItem>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }
        public Item RewardItem { get; set; }
        public List<QuestCompletionItem> QuestCompletionItems { get; set; }
    }
}
