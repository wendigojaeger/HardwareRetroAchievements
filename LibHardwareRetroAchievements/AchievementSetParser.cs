using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace HardwareRetroAchievements.Core
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
