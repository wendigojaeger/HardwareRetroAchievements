using HardwareRetroAchievements.Core.AchievementData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HardwareRetroAchievements.Core.Provider
{
    public interface IAchievementsProvider
    {
        Task<IEnumerable<AchievementSet>> GetAchievements(ConsoleID consoleID);
    }
}
