using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareRetroAchievements.Core
{
    public class AchievementSet
    {
        private List<Achievement> _achievements = new List<Achievement>();

        public int ID { get; set; }
        public string Title { get; set; }
        public ConsoleID ConsoleID { get; set; }
        public int ForumTopicID { get; set; }
        public int Flags { get; set; } // TODO: Find documentation on this
        public string ImageIcon { get; set; }
        public string ImageTitle { get; set; }
        public string ImageInGame { get; set; }
        public string ImageBoxArt { get; set; }
        public string Publisher { get; set; }
        public string Developer { get; set; }
        public string Genre { get; set; }
        public string Released { get; set; }
        public bool IsFinal { get; set; }
        public string ConsoleName { get; set; }
        public string RichPresencePatch { get; set; }
        public List<Achievement> Achievements
        {
            get
            {
                return _achievements;
            }
            set
            {
                _achievements = value;
            }
        }
    }
}
