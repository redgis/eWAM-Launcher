using Hardcodet.Wpf.TaskbarNotification;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using log4net;


namespace eWamLauncher
{
   /// <summary>
   /// Download manager class. It simply holds the observable list of downloads, and allows for 
   /// new package download addition
   /// </summary>
   public class PackageDownloadManager : INotifyPropertyChanged
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(PackageDownloadManager));

      /// <summary>
      /// Reference to the Profile, holding for instance, the server URL from which to 
      /// download things. Passed to the PackageDownloadInfo class.
      /// </summary>
      private Profile _profile = null;

      private ObservableCollection<PackageDownloadInfo> _downloads;
      public ObservableCollection<PackageDownloadInfo> downloads { get { return _downloads; } set { _downloads = value; this.NotifyPropertyChanged(); } }


      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public PackageDownloadManager(Profile profile)
      {
         this._profile = profile;
         this.downloads = new ObservableCollection<PackageDownloadInfo>();
      }

      /// <summary>
      /// Adds and starts a new package download
      /// </summary>
      /// <param name="package">the package to be downloaded</param>
      /// <param name="targetDir">destination folder where the download must be deployed</param>
      /// <param name="packageDownloadCompletedHandler">download completion handler that will be triggered (optional)</param>
      public void AddDownloadTask(Package package, string targetDir, PackageDownloadCompletedHandler packageDownloadCompletedHandler = null)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString() + ": adding download of " + package.Description + " to " + targetDir);

         try
         {
            // Create new PackageDownloadInfo holding the download information for this package
            PackageDownloadInfo packageDownloadInfo = new PackageDownloadInfo(package, targetDir, this._profile, packageDownloadCompletedHandler);
            
            this.downloads.Add(packageDownloadInfo);
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

      //private void OnCancel(object sender, EventArgs e)
      //{
      //   BackgroundWorker worker = sender as BackgroundWorker;

      //   if (worker.WorkerSupportsCancellation)
      //   {
      //      // Cancel the asynchronous operation.
      //      worker.CancelAsync();
      //   }
      //}

      //private void OnPause(object sender, EventArgs e)
      //{
      //   if (this.downloadWorker.WorkerSupportsCancellation)
      //   {
      //      // Cancel the asynchronous operation.
      //      this.downloadWorker.CancelAsync();
      //   }
      //}

      //private void OnResume(object sender, EventArgs e)
      //{
      //   if (this.downloadWorker.Worker)
      //   {
      //      // Cancel the asynchronous operation.
      //      this.downloadWorker.CancelAsync();
      //   }
      //}


   }
}
