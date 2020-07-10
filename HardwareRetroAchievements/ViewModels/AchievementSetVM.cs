using HardwareRetroAchievements.Core.AchievementData;
using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareRetroAchievements.ViewModels
{
    public class AchievementSetVM : ViewModelBase
    {
        public AchievementSetVM(AchievementSet achievementSet)
        {
            AchievementSet = achievementSet;
        }

        public AchievementSet AchievementSet { get; }

        public string IconUrl => $"http://retroachievements.org{AchievementSet.ImageIcon}";
        public string BoxArtUrl => $"http://retroachievements.org{AchievementSet.ImageBoxArt}";

    }
}
