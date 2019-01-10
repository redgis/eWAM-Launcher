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
using log4net;


namespace eWamLauncher.Views
{
   /// <summary>
   /// Interaction logic for PackageDownloadView.xaml
   /// This view presents a PackageDownloadInfo object
   /// </summary>
   public partial class PackageDownloadView : UserControl
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(PackageDownloadView));

      public PackageDownloadView()
      {
         InitializeComponent();
      }

      /// <summary>
      /// When the download is cancelled (top right cross is clicked), cancel any ongoing download or
      /// install.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnCancel(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            PackageDownloadInfo packageDownloadInfo = (PackageDownloadInfo)this.DataContext;

            packageDownloadInfo.downloadWorker.CancelAsync();
            if (packageDownloadInfo.installWorker.WorkerSupportsCancellation)
            {
               // Cancel the asynchronous operation.
               packageDownloadInfo.installWorker.CancelAsync();
            }
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
