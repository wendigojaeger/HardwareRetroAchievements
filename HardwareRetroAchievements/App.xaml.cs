using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HardwareRetroAchievements
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //kboily : Generic.xaml should be automatically loaded by the framework but for some reason it's not. Loading manually for the moment
            Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri("Themes/Generic.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
        }
    }
}
