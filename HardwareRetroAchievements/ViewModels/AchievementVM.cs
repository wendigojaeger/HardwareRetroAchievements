using HardwareRetroAchievements.Core.AchievementData;
using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareRetroAchievements.ViewModels
{
    public class AchievementVM : ViewModelBase
    {
        public Achievement Achievement { get; }

        private bool _isUnlocked;
        public bool IsUnlocked
        {
            get => _isUnlocked;
            set
            {
                if (SetValue(ref _isUnlocked, value))
                {
                    RaisePropertyChanged(nameof(BadgeUrl));
                }
            }
        }

        public string BadgeUrl
        {
            get
            {
                return $"http://i.retroachievements.org/Badge/{Achievement.BadgeName}{(_isUnlocked ? string.Empty : "_lock")}.png";
            }
        }

        public AchievementVM(Achievement achievement)
        {
            if (achievement == null)
            {
                throw new ArgumentNullException(nameof(achievement));
            }

            Achievement = achievement;
        }
    }
}
