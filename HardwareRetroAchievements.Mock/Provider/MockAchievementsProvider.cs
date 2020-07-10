using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HardwareRetroAchievements.Core.AchievementData;
using HardwareRetroAchievements.Core.Provider;

namespace HardwareRetroAchievements.Mock.Provider
{
    public class MockAchievementsProvider : IAchievementsProvider
    {
        public Task<IEnumerable<AchievementSet>> GetAchievements(ConsoleID consoleID)
        {
            return Task.FromResult(ReadLocalAchievements(new DirectoryInfo($"AchievementSets/{(int)consoleID}")));
        }

        public IEnumerable<AchievementSet> ReadLocalAchievements(DirectoryInfo directory)
        {
            var achievements = new List<AchievementSet>();
            foreach(var jsonFile in directory.EnumerateFiles())
            {
                try
                {
                    achievements.Add(AchievementSetParser.ParseFile(jsonFile.FullName));
                }
                catch
                {
                    //skip
                }
            }
            return achievements;
        }
    }
}
