using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace eWamLauncher.Views
{
   /// <summary>
   /// Interaction logic for SettingsView.xaml
   /// This view presents a Settings object
   /// </summary>
   public partial class SettingsView : UserControl
   {
      public SettingsView()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Command handler used to look for all Visual Studio instances installed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnAutoDetectVisualStudios(object sender, RoutedEventArgs e)
      {
         Settings _settings = (Settings)this.DataContext;
         _settings.AutoDetectVisualStudios();
      }
   }
}
