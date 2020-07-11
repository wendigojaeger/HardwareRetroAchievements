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
            if (achievementSet == null)
                throw new ArgumentNullException(nameof(achievementSet));

            AchievementSet = achievementSet;
        }

        public AchievementSet AchievementSet { get; }

        public string ImageIconUrl => $"http://retroachievements.org{AchievementSet.ImageIcon}";
        public string ImageBoxArtUrl => $"http://retroachievements.org{AchievementSet.ImageBoxArt}";
        public string ImageTitleUrl => $"http://retroachievements.org{AchievementSet.ImageTitle}";
        public string ImageInGameUrl => $"http://retroachievements.org{AchievementSet.ImageIngame}";

    }
}
