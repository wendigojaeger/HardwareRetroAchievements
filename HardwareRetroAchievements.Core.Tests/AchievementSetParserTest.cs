using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;

namespace HardwareRetroAchievements.Core.Tests
{
    public class AchievementSetParserTest
    {
        [Fact]
        public void ShouldParseContraAchievementSet()
        {
            string fixturesDir = getFixturesDirectory();
            string contraSetPath = Path.Combine(fixturesDir, "1447.json");

            var achievementSet = AchievementSetParser.ParseFile(contraSetPath);

            Assert.Equal("Contra", achievementSet.Title);
            Assert.Equal("NES", achievementSet.ConsoleName);
            Assert.Equal(ConsoleID.Nintendo, achievementSet.ConsoleID);
            Assert.Equal(283 , achievementSet.ForumTopicID);
            Assert.Equal(46, achievementSet.Achievements.Count);

            var achievement = achievementSet.Achievements[0];

            Assert.Equal(71880, achievement.ID);
            Assert.Equal("R:0xH001c!=0_R:0xH0030!=1_R:0x 0064!=5_0xH05c7=16.1._0xH05c7=15.1._0xH05c7=14.1._0xH05c7=13.1._0xH05c7=12.1._0xH05c7=11.1._0xH05c7=10.1._0xH05c7=9.1._0xH05c7=8.1._0xH05c7=7.1._0xH05c7=6.1._0xH05c7=5.1._0xH05c7=4.1._0xH05c7=3.1._0xH05c7=2.1._0xH05c7=1.1._0xH05c7=0_d0xH05a7=12_R:0xH010a=25_R:0xH04c0=7_R:0xH04c1=7_R:0xH04c2=7_R:0xH04c3=7_R:0xH04c4=7_R:0xH04c5=7_R:0xH04c6=7_R:0xH04c7=7_R:0xH002c=6", achievement.MemAddr);
            Assert.Equal("Oculus Drift (backup)", achievement.Title);
            Assert.Equal("backup of this achievment before rework (for bug fix)", achievement.Description);
            Assert.Equal(10, achievement.Points);
            Assert.Equal("kdecks", achievement.Author);
            Assert.Equal(1550020792, achievement.Modified);
            Assert.Equal(1550020792, achievement.Created);
            Assert.Equal("52131", achievement.BadgeName);
            Assert.Equal(5, achievement.Flags);
        }

        [Fact]
        public void ShouldParseMegaManXAchievementSet()
        {
            string fixturesDir = getFixturesDirectory();
            string contraSetPath = Path.Combine(fixturesDir, "637.json");

            var achievementSet = AchievementSetParser.ParseFile(contraSetPath);

            Assert.Equal("Mega Man X", achievementSet.Title);
            Assert.Equal("SNES", achievementSet.ConsoleName);
            Assert.Equal(ConsoleID.SuperNintendo, achievementSet.ConsoleID);
            Assert.Equal(244, achievementSet.ForumTopicID);
            Assert.Equal(68, achievementSet.Achievements.Count);

            var achievement = achievementSet.Achievements[0];

            Assert.Equal(89630, achievement.ID);
            Assert.Equal("N:0xH001f7a=5_N:0xO001f9c>d0xO001f9c_C:0xO001f9c=1.1._N:0xH001f7a=4_N:0xQ001f9c>d0xQ001f9c_C:0xQ001f9c=1.1._N:0xH001f7a=7_N:0xR001f9c>d0xR001f9c_C:0xR001f9c=1.1._0!=0.3.", achievement.MemAddr);
            Assert.Equal("Testing the Measure Creature with my heart", achievement.Title);
            Assert.Equal("Collect the heart from Storm Eagle, Flame Mamoth and Boomer Kuwanger stage", achievement.Description);
            Assert.Equal(0, achievement.Points);
            Assert.Equal("matheus2653", achievement.Author);
            Assert.Equal(1576107822, achievement.Modified);
            Assert.Equal(1575153424, achievement.Created);
            Assert.Equal("00080", achievement.BadgeName);
            Assert.Equal(5, achievement.Flags);
        }

        private string getFixturesDirectory()
        {
            string cwd = Directory.GetCurrentDirectory();
            return Path.Combine(Path.GetFullPath(cwd.Substring(0, cwd.IndexOf("bin") - 1)), "fixtures");
        }
    }
}
