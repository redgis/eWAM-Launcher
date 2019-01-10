using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using System.Net;
using log4net;
using System.IO;
using SharpCompress.Archives;

namespace eWamLauncher
{
   public class PackageDownloadCompletedEventArgs : EventArgs
   {
      public Exception Error;
      public bool Cancelled;
   }

   public delegate void PackageDownloadCompletedHandler(object sender, PackageDownloadCompletedEventArgs e);


   /// <summary>
   /// Class containing all the information about an ongoing package download
   /// </summary>
   public class PackageDownloadInfo : INotifyPropertyChanged
   {
      /// <summary>
      /// Event triggered when the package download and deployment is completed
      /// </summary>
      public event PackageDownloadCompletedHandler PackageDownloadCompleted;

      private static readonly ILog log = LogManager.GetLogger(typeof(PackageDownloadInfo));

      /// <summary>
      /// Async download worker
      /// </summary>
      private WebClient _downloadWorker;
      public WebClient downloadWorker { get { return _downloadWorker; } set { _downloadWorker = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Async worker for copy / extract tasks
      /// </summary>
      private BackgroundWorker _installWorker;
      public BackgroundWorker installWorker { get { return _installWorker; } set { _installWorker = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// the package being downloaded / deployed
      /// </summary>
      public Package _package;
      public Package package { get { return _package; } set { _package = value; this.NotifyPropertyChanged(); } }

      public string _description = "";
      public string description { get { return _description; } set { _description = value; this.NotifyPropertyChanged(); } }
      
      /// <summary>
      /// Remaining tasks before the package download is complete. The package download is decomposed in 
      /// download and extract of several files, called "subtaskes". 
      /// </summary>
      Queue<SubTask> remainingTasks;

      /// <summary>
      /// States of a package download
      /// </summary>
      public enum ePackageDownloadState
      {
         Running,
         Cancelled,
         Failed,
         Done
      }

      /// <summary>
      /// Current state of the package download
      /// </summary>
      private ePackageDownloadState _state = ePackageDownloadState.Running;
      public ePackageDownloadState state { get { return _state; } set { _state = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// estimated amount of work for this package download
      /// </summary>
      private int _totalWorkAmount = 0;

      /// <summary>
      /// estimated amount of work DONE so far for this package download
      /// </summary>
      private int _doneWorkAmount = 0;

      /// <summary>
      /// estimated percentage of work done so far for this package download
      /// </summary>
      public int _overallProgress = 0;
      public int overallprogress { get { return _overallProgress; } set { _overallProgress = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Description of the current download SubTask being processed
      /// </summary>
      public string _currentDownloadDescription = "";
      public string currentDownloadDescription { get { return _currentDownloadDescription; } set { _currentDownloadDescription = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Progress of the current download SubTask being processed
      /// </summary>
      public int _currentDownloadProgress = 0;
      public int currentDownloadProgress { get { return _currentDownloadProgress; } set { _currentDownloadProgress = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Description of the current install SubTask being processed
      /// </summary>
      public string _currentInstallDescription = "";
      public string currentInstallDescription { get { return _currentInstallDescription; } set { _currentInstallDescription = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Progress of the current install SubTask being processed
      /// </summary>
      public int _currentInstallProgress = 0;
      public int currentInstallProgress { get { return _currentInstallProgress; } set { _currentInstallProgress = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Destination folder of the package
      /// </summary>
      public string _targetDir = "";
      public string targetDir { get { return _targetDir; } set { _targetDir = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Reference to the profile, used for adding the package to the list of environments 
      /// or ewams once the download is complete
      /// </summary>
      private Profile _profile;
      public Profile profile { get { return _profile; } set { _profile = value; this.NotifyPropertyChanged(); } }


      public event PropertyChangedEventHandler PropertyChanged;
      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         if (this.PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }


      public PackageDownloadInfo(Package package, string targetDir, Profile profile,
         PackageDownloadCompletedHandler packageDownloadCompletedHandler)
      {
         this.package = (Package)package.Clone();
         this.targetDir = targetDir;
         this.profile = profile;
         this.description = this.package.Description;

         this.remainingTasks = new Queue<SubTask>();

         this.PopulateTasks();

         this.downloadWorker = new WebClient();
         this.downloadWorker.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
         this.downloadWorker.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadCompleted);

         this.installWorker = new BackgroundWorker();
         this.installWorker.WorkerReportsProgress = true;
         this.installWorker.WorkerSupportsCancellation = true;


         this.installWorker.DoWork += new DoWorkEventHandler(OnInstall);
         this.installWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnInstallCompleted);
         this.installWorker.ProgressChanged += new ProgressChangedEventHandler(OnOnInstallProgressChanged);

         this.PackageDownloadCompleted += packageDownloadCompletedHandler;

         ((MainWindow)Application.Current.MainWindow).eWAMLauncherNotifyIcon.ShowBalloonTip(
            "Download started",
            this.package.Description, BalloonIcon.Info);

         this.TreatNextTasks();
      }

      /// <summary>
      /// Creates all the subtasks that will need to be completed in order to finish 
      /// the download of this package.
      /// </summary>
      private void PopulateTasks()
      {
         //Populate tasks
         foreach (var component in this.package.Components)
         {
            //If it is compressed, add download + uncrompress tasks for the component
            if (component.IsCompressed())
            {
               string url = this._profile.settings.ewamUpdateServerURL + "/" +
                     this.package.Id + "/" +
                     component.Name + ".zip";

               string zipFullPath = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".zip";

               SubTask dlTask = new SubTask(SubTask.eTaskTypes.Download, url, zipFullPath, component.Name,
                  component.Files.Count());
               this.remainingTasks.Enqueue(dlTask);
               this.remainingTasks.Enqueue(new SubTask(SubTask.eTaskTypes.Extract, zipFullPath,
                  this.targetDir, component.Name, component.Files.Count(), dlTask));
            }
            else // just add a download task for each file of the component
            {
               foreach (ComponentFile componentFile in component.Files)
               {
                  string convertedPath = componentFile.Path.Replace("\\", "/");

                  string url = this._profile.settings.ewamUpdateServerURL + "/" +
                           this.package.Id + "/" +
                           convertedPath;

                  string destination = this.targetDir + "\\" + componentFile.Path;

                  this.remainingTasks.Enqueue(new SubTask(SubTask.eTaskTypes.Download, url, destination,
                     Path.GetFileName(componentFile.Path), 20));
               }
            }
         }

         foreach (var task in this.remainingTasks)
         {
            this._totalWorkAmount += task.workAmount;
         }
      }


      /// <summary>
      /// Get next treatable task (download or extract, whatever), and start treating it
      /// </summary>
      /// <returns></returns>
      private bool TreatNextTasks()
      {
         try
         {
            bool result = false;

            // No more task ! We're finished, configure the environment and finish up.
            if (this.remainingTasks.Count == 0)
            {
               this.OnDone(new AsyncCompletedEventArgs(null, false, null));
               return result;
            }

            bool canProcessTasks = true;

            while (this.remainingTasks.Count > 0 && canProcessTasks)
            {
               canProcessTasks = false;

               SubTask nextTask = this.remainingTasks.Peek();

               if (nextTask.prerequisite == null ||
                  (nextTask.prerequisite != null && nextTask.prerequisite.completed))
               {

                  switch (nextTask.taskType)
                  {
                     case SubTask.eTaskTypes.Download:
                        if (!downloadWorker.IsBusy)
                        {
                           this.remainingTasks.Dequeue();
                           this.currentDownloadDescription = "Downloading " + nextTask.description + "...";
                           this.currentDownloadProgress = 0;
                           Directory.CreateDirectory(Path.GetDirectoryName(nextTask.target));
                           downloadWorker.DownloadFileAsync(new Uri(nextTask.source), nextTask.target, nextTask);
                           canProcessTasks = true;
                           result = true;
                        }
                        break;
                     case SubTask.eTaskTypes.Extract:
                        if (!installWorker.IsBusy)
                        {
                           this.remainingTasks.Dequeue();
                           this.currentInstallDescription = "Installing " + nextTask.description + "...";
                           this.currentInstallProgress = 0;
                           Directory.CreateDirectory(Path.GetDirectoryName(nextTask.target));
                           installWorker.RunWorkerAsync(nextTask);
                           canProcessTasks = true;
                           result = true;
                        }
                        break;
                     default:
                        break;
                  }
               }
            }

            return result;
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
         return false;
      }

      /// <summary>
      /// Handler for the download worker : gets notified when a download SubTask progress has changed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
      {
         SubTask currentTask = (SubTask)e.UserState;
         this.currentDownloadProgress = e.ProgressPercentage;
         this.OnOverallProgressChanged((currentTask.workAmount * e.ProgressPercentage) / 100);
      }

      /// <summary>
      /// Handler for the download worker : gets notified when a download SubTask is completed (or cancelled, or failed)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
      {
         try
         { 
            SubTask currentTask = (SubTask)e.UserState;

            if (e.Cancelled == true)
            {
               this.currentDownloadDescription = "Cancelled.";

               log.Info("Download canceled : " + currentTask.description +
                  " from " + currentTask.source + " to " + currentTask.target);

               this.OnDone(e);
            }
            else if (e.Error != null)
            {
               this.currentDownloadDescription = "Error !";

               log.Info("Download failed : " + currentTask.description +
                  " from " + currentTask.source + " to " + currentTask.target +
                  " with error : " + e.Error.Message);

               this.OnDone(e);
            }
            else
            {
               currentTask.completed = true;
               this.currentDownloadProgress = 100;
               this.currentDownloadDescription = currentTask.description + " - Download finished.";
               this._doneWorkAmount += currentTask.workAmount;
               this.OnOverallProgressChanged(0);
               this.TreatNextTasks();
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

      /// <summary>
      /// This is the 'install' worker actual method, doing the job
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnInstall(object sender, DoWorkEventArgs e)
      {
         try
         {
            BackgroundWorker worker = sender as BackgroundWorker;
            SubTask currentTask = (SubTask)e.Argument;
            e.Result = currentTask;
         

            int currentTaskWorkAmountDone = 0;

            //Do extract of the given zip
            //ZipFile.ExtractToDirectory(targetDir + "\\" + component.Name + ".zip", targetDir);
            using (SharpCompress.Archives.Zip.ZipArchive archive = SharpCompress.Archives.Zip.ZipArchive.Open(currentTask.source))
            {
               foreach (SharpCompress.Archives.Zip.ZipArchiveEntry entry in archive.Entries)
               {
                  if (worker.CancellationPending == true)
                  {
                     e.Cancel = true;
                     break;
                  }
                  else
                  {
                     //if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                     Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(this.targetDir, entry.ToString())));
                     entry.WriteToFile(Path.Combine(this.targetDir, entry.ToString()));
                     currentTaskWorkAmountDone++;

                     worker.ReportProgress((int)(((double)currentTaskWorkAmountDone / (double)currentTask.workAmount) * 100), currentTask);
                  }
               }
            }


            File.Delete(currentTask.source);
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

      /// <summary>
      /// Handler for the install worker : gets notified when a install SubTask progress has changed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnOnInstallProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         SubTask currentTask = (SubTask)e.UserState;
         this.currentInstallProgress = e.ProgressPercentage;
         this.OnOverallProgressChanged((currentTask.workAmount * e.ProgressPercentage) / 100);
      }

      /// <summary>
      /// Handler for the install worker : gets notified when a install SubTask is completed (or cancelled, or failed)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnInstallCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         try
         {
            if (e.Cancelled == true)
            {
               this.currentInstallDescription = "Cancelled.";

               log.Info("Install canceled : " + this.description
                  /*+ " from " + currentTask.source + " to " + currentTask.target*/);

               this.OnDone(e);
            }
            else if (e.Error != null)
            {
               SubTask currentTask = (SubTask)e.Result;

               this.currentInstallDescription = "Error !";

               log.Info("Install failed : " + currentTask.description +
                  " from " + currentTask.source + " to " + currentTask.target +
                  " with error : " + e.Error.Message);

               this.OnDone(e);
            }
            else
            {
               SubTask currentTask = (SubTask)e.Result;

               currentTask.completed = true;
               this.currentInstallProgress = 100;
               this.currentInstallDescription = currentTask.description + " - Install finished.";
               this._doneWorkAmount += currentTask.workAmount;
               this.OnOverallProgressChanged(0);
               this.TreatNextTasks();
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

      /// <summary>
      /// Handler for the overrall progress : gets notified every time there's a change or completion in any SubTask
      /// </summary>
      /// <param name="currentTaskEstimatedWorkMountDone"></param>
      private void OnOverallProgressChanged(int currentTaskEstimatedWorkMountDone)
      {
         this.overallprogress = (int)
            (((double)(this._doneWorkAmount + currentTaskEstimatedWorkMountDone)*100.0) / (double)this._totalWorkAmount);
      }

      /// <summary>
      /// Handler for the package download/deploy completion
      /// </summary>
      /// <param name="e"></param>
      private void OnDone(AsyncCompletedEventArgs e)
      {
         try
         {
            if (this.state != ePackageDownloadState.Running)
            {
               return;
            }

            if (e.Cancelled == true)
            {
               this.state = ePackageDownloadState.Cancelled;

               ((MainWindow)Application.Current.MainWindow).eWAMLauncherNotifyIcon.ShowBalloonTip(
                  "Download canceled", this.package.Description, BalloonIcon.Warning);

               log.Info("Package pull canceled : " + this.package.Description);

            }
            else if (e.Error != null)
            {
               this.state = ePackageDownloadState.Failed;

               ((MainWindow)Application.Current.MainWindow).eWAMLauncherNotifyIcon.ShowBalloonTip(
                     "Download failed",
                     this.package.Description + "\n" + e.Error.Message, BalloonIcon.Error);

               log.Info("Package pull canceled : " + this.package.Description);

            }
            else
            {
               this.state = ePackageDownloadState.Done;

               ((MainWindow)Application.Current.MainWindow).eWAMLauncherNotifyIcon.ShowBalloonTip(
               "Download completed",
               this.package.Description, BalloonIcon.Info);

               log.Info("Download completed of " + this.package.Description +
                  " (while " + this.description + ") " +
                  ", at " + this.overallprogress + ", to " + this.targetDir);

               this.ConfigureEnvironments();
            }

            PackageDownloadCompletedEventArgs evtArgs = new PackageDownloadCompletedEventArgs();
            evtArgs.Cancelled = e.Cancelled;
            evtArgs.Error = e.Error;
            
            this.PackageDownloadCompleted(this, evtArgs);

            ((MainWindow)Application.Current.MainWindow).packageDownloadManager.downloads.Remove(this);
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

      /// <summary>
      /// Adds and configures the environment or ewam package downloaded, so it appears in the 
      /// Profile's list of ewams and environments (see "Profile" class)
      /// </summary>
      private void ConfigureEnvironments()
      {
         try
         {
            this.description = this.package.Description + " - Configuring ... ";
            //Look for BinariesSets
            // Get BinariesSet and import associated eWAM
            Ewam importedEwam = null;
            Boolean found = false;
            foreach (PackageComponent component in this.package.Components)
            {
               if (component.Files != null)
               {
                  foreach (ComponentFile file in component.Files)
                  {
                     string filename = MainWindow.NormalizePath(
                        this.targetDir + "\\" + file.Path);
                     string extension = Path.GetExtension(filename);

                     if (extension == ".xwam")
                     {
                        importedEwam = ((MainWindow)Application.Current.MainWindow).LoadEwamFromXML(filename);
                        found = true;
                        break;
                     }
                     else if (extension == ".jswam")
                     {
                        importedEwam = ((MainWindow)Application.Current.MainWindow).LoadEwamFromJSON(filename);
                        found = true;
                        break;
                     }

                  }
               }

               if (found)
               {
                  importedEwam.basePath = this.targetDir;
                  importedEwam.name = this.package.Name;

                  this._profile.ewams.Add(importedEwam);
                  break;
               }
            }

            //Look for environment definition / launchers
            // If exists, create associated environment using Launchers
            found = false;
            Environment importedEnv = null;
            foreach (PackageComponent component in this.package.Components)
            {
               if (component.Files != null)
               {
                  foreach (ComponentFile file in component.Files)
                  {
                     string filename = MainWindow.NormalizePath(this.targetDir + "\\" + file.Path);
                     string extension = Path.GetExtension(filename);

                     if (extension == ".xenv")
                     {
                        importedEnv = ((MainWindow)Application.Current.MainWindow).LoadEnvironmentFromXML(filename);
                        found = true;
                        break;
                     }
                     else if (extension == ".jsenv")
                     {
                        importedEnv = ((MainWindow)Application.Current.MainWindow).LoadEnvironmentFromJSON(filename);
                        found = true;
                        break;
                     }
                  }
               }

               if (found)
               {
                  // Fill envRoot and wfRoot by adding the provided path as prefix
                  importedEnv.envRoot = this.targetDir + "\\" + importedEnv.envRoot;
                  importedEnv.wfRoot = this.targetDir + "\\" + importedEnv.wfRoot;
                  importedEnv.ewam = importedEwam;
                  importedEnv.name = this.package.Name;

                  this._profile.environments.Add(importedEnv);
                  break;
               }
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
