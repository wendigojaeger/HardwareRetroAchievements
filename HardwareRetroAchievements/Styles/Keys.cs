using System.Windows;

namespace HardwareRetroAchievements.Styles
{
    public class Keys
    {
        #region Colors and Bruses
        public static ComponentResourceKey ControlVeryLightColorKey = new ComponentResourceKey(typeof(Keys), nameof(ControlVeryLightColorKey));
        public static ComponentResourceKey ControlVeryLightBrushKey = new ComponentResourceKey(typeof(Keys), nameof(ControlVeryLightBrushKey));

        public static ComponentResourceKey AltTextColorKey = new ComponentResourceKey(typeof(Keys), nameof(AltTextColorKey));
        public static ComponentResourceKey AltTextBrushKey = new ComponentResourceKey(typeof(Keys), nameof(AltTextBrushKey));
        #endregion

        #region Templates
        public static ComponentResourceKey AchievementSetPreviewTemplateKey = new ComponentResourceKey(typeof(Keys), nameof(AchievementSetPreviewTemplateKey));
        public static ComponentResourceKey AchievementTemplateKey = new ComponentResourceKey(typeof(Keys), nameof(AchievementTemplateKey));
        #endregion

        #region Converters
        public static ComponentResourceKey AlternateBackgroundConverterKey = new ComponentResourceKey(typeof(Keys), nameof(AlternateBackgroundConverterKey));
        #endregion

        #region Styles
        public static ComponentResourceKey AlternatingBackgroundItemStyleKey = new ComponentResourceKey(typeof(Keys), nameof(AlternatingBackgroundItemStyleKey));
        public static ComponentResourceKey NoSelectionListBoxStyleKey = new ComponentResourceKey(typeof(Keys), nameof(NoSelectionListBoxStyleKey));
        #endregion
    }
}
