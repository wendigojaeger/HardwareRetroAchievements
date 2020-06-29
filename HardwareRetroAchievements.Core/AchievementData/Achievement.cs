namespace HardwareRetroAchievements.Core.AchievementData
{
    public class Achievement
    {
        public int ID { get; set; }
        public string MemAddr { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public string Author { get; set; }
        public int Modified { get; set; } // UNIX timestamp ?
        public int Created { get; set; } // UNIX timestamp ?
        public string BadgeName { get; set; }
        public int Flags { get; set; } // TODO: Find documentation
    }
}
