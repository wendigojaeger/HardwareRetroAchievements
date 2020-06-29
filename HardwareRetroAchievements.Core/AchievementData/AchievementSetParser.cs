using System;
using System.IO;
using System.Text.Json;

namespace HardwareRetroAchievements.Core.AchievementData
{
    public class AchievementSetParser
    {
        public static AchievementSet ParseFile(string path)
        {
            return ParseContents(File.ReadAllText(path));
        }

        public static AchievementSet ParseContents(string jsonString)
        {
            return JsonSerializer.Deserialize<AchievementSet>(jsonString);
        }

        public static AchievementSet ParseFromBytes(ReadOnlySpan<byte> bytes)
        {
            return JsonSerializer.Deserialize<AchievementSet>(bytes);
        }
    }
}
