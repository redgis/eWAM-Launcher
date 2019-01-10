using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
      private static readonly ILog log = LogManager.GetLogger(typeof(SettingsView));

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

      /// <summary>
      /// Handle click on an hyper-text link
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnClickHLink(object sender, RequestNavigateEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

   }
}
