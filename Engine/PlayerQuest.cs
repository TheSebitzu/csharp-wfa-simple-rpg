namespace Engine
{
    public class PlayerQuest
    {
        // Constructor
        public PlayerQuest(Quest details)
        {
            Details = details;
            IsCompleted = false;
        }
        public Quest Details { get; set; }
        public bool IsCompleted { get; set; }
    }
}
