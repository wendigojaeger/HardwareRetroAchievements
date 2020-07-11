using HardwareRetroAchievements.Core.AchievementData;
using HardwareRetroAchievements.Core.Provider;
using System.Collections.Generic;
using System.Linq;

namespace HardwareRetroAchievements.ViewModels
{
    public class AchievementsViewerVM : ViewModelBase
    {
        private ConsoleID _currentConsole = ConsoleID.SuperNintendo;
        public ConsoleID CurrentConsole
        {
            get => _currentConsole;
            set
            {
                if (SetValue(ref _currentConsole, value))
                {
                    RefreshAchievements();
                }
            }
        }

        private IAchievementsProvider _achievementsProvider;

        public IAchievementsProvider AchievementsProvider
        {
            get => _achievementsProvider;
            set
            {
                if (SetValue(ref _achievementsProvider, value))
                {
                    RefreshAchievements();
                }
            }
        }

        private List<AchievementSet> _allAchievements;
        public List<AchievementSet> AllAchievements
        {
            get => _allAchievements;
            private set => SetValue(ref _allAchievements, value);
        }

        private List<AchievementSetVM> _allAchievementsVMs;
        public List<AchievementSetVM> AllAchievementsVMs
        {
            get => _allAchievementsVMs;
            private set => SetValue(ref _allAchievementsVMs, value);
        }

        public void RefreshAchievements()
        {
            if(AchievementsProvider == null)
            {
                _allAchievements = null;
            }

            //todo call async
            _allAchievements = AchievementsProvider.GetAchievements(CurrentConsole).Result.ToList();
            _allAchievementsVMs = _allAchievements.Select(x => new AchievementSetVM(x)).ToList();

            SelectedAchievementSetVM = _allAchievementsVMs.FirstOrDefault(x => SelectedAchievementSetVM != null ? x.AchievementSet.ID == SelectedAchievementSetVM.AchievementSet.ID : true);
        }

        private AchievementSetVM _selectedAchievementSetVM;
        public AchievementSetVM SelectedAchievementSetVM
        {
            get => _selectedAchievementSetVM;
            set => SetValue(ref _selectedAchievementSetVM, value);
        }
    }
}
