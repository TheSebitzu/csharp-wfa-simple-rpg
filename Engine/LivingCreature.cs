namespace Engine
{
    public class LivingCreature
    {
        // Constructor
        public LivingCreature(int currentHitPoints, int maximumHitPoints)
        {
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
        }
        public int MaximumHitPoints { get; set; }
        public int CurrentHitPoints { get; set; }
    }
}
