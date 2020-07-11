using System.Collections.Generic;

namespace HardwareRetroAchievements.Core.AchievementData
{
    public class AchievementSet
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public ConsoleID ConsoleID { get; set; }
        public int ForumTopicID { get; set; }
        public int Flags { get; set; } // TODO: Find more documentation on this, 5 = Unofficial, 3 = Official
        public string ImageIcon { get; set; }
        public string ImageTitle { get; set; }
        public string ImageIngame { get; set; }
        public string ImageBoxArt { get; set; }
        public string Publisher { get; set; }
        public string Developer { get; set; }
        public string Genre { get; set; }
        public string Released { get; set; }
        public bool IsFinal { get; set; }
        public string ConsoleName { get; set; }
        public string RichPresencePatch { get; set; }
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
}
