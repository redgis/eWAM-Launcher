using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MahApps.Metro.Controls;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Diagnostics;
using System.Windows.Navigation;
using Squirrel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO.Compression;
using SharpCompress.Archives;
using log4net;
using Hardcodet.Wpf.TaskbarNotification;
using System.Runtime.InteropServices;
using System.Net;

namespace eWamLauncher
{
   public static class Commands
   {

      public static readonly RoutedUICommand ChangePath =
         new RoutedUICommand("Change Path", "ChangePath", typeof(MainWindow));

      public static readonly RoutedUICommand ExplorePath =
         new RoutedUICommand("Explore Path", "ExplorePath", typeof(MainWindow));

   }

   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   [DataContract(Name = "eWamLauncher", Namespace = "http://www.wyde.com")]
   public partial class MainWindow : MetroWindow, INotifyPropertyChanged
   {
      private Profile _profile;
      /// <summary>
      /// Current profile (containing all configurations: application settings, list of ewams, list of environments, etc.)
      /// </summary>
      public Profile profile { get { return _profile; } set { _profile = value; this.NotifyPropertyChanged(); } }

      private string _assemblyDescription { get; set; }
      /// <summary>
      /// Used to display application version on main window
      /// </summary>
      public string assemblyDescription { get { return _assemblyDescription; } set { _assemblyDescription = value; this.NotifyPropertyChanged(); } }

      private string _assemblyVersion { get; set; }
      /// <summary>
      /// Used to display application version on main window
      /// </summary>
      public string assemblyVersion { get { return _assemblyVersion; } set { _assemblyVersion = value; this.NotifyPropertyChanged(); } }

      private string _assemblyUpdateInfo { get; set; }
      /// <summary>
      /// Used to display up-to-date information on the main window
      /// </summary>
      public string assemblyUpdateInfo { get { return _assemblyUpdateInfo; } set { _assemblyUpdateInfo = value; this.NotifyPropertyChanged(); } }

      private PackageDownloadManager _packageDownloadManager;
      /// <summary>
      /// List of package downloads being currently processed
      /// </summary>
      public PackageDownloadManager packageDownloadManager { get { return _packageDownloadManager; } set { _packageDownloadManager = value; this.NotifyPropertyChanged(); } }

      private WideIndex _WideIndex;
      /// <summary>
      /// Local image of downloaded package-index.xml. This is used to generate the list of available packages for download
      /// </summary>
      public WideIndex WideIndex { get { return _WideIndex; } set { _WideIndex = value; this.NotifyPropertyChanged(); } }

      private ObservableDictionary<string, ObservableCollection<Package>> _productsPackages;
      /// <summary>
      /// Stored the list of online packages available, sorted by product (i.e., Package.Type : clickonce, activex, ewam, wynsure ...)
      /// </summary>
      public ObservableDictionary<string, ObservableCollection<Package>> productsPackages { get { return _productsPackages; } set { _productsPackages = value; this.NotifyPropertyChanged(); } }

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public MainWindow()
      {
         //Initialize logging system
         log4net.Config.XmlConfigurator.Configure();

         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.assemblyVersion = Assembly.GetEntryAssembly().GetName().Name + " - by Régis Martinez";
            this.assemblyVersion += " - " + Assembly.GetEntryAssembly().GetName().Version;
            this.assemblyVersion += " - (c) Mphasis Wyde";
            this.assemblyUpdateInfo = "";

            string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
            string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");

            this.profile = new Profile();
            this.productsPackages = new ObservableDictionary<string, ObservableCollection<Package>>();

            try
            { LoadCfgFromXML(defaultXMLSettings); }
            catch
            {
               try
               { LoadCfgFromJSON(defaultJSONSettings); }
               catch (Exception exception)
               {
                  this.profile = new Profile();
                  this.productsPackages = new ObservableDictionary<string, ObservableCollection<Package>>();
                  log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
               }
            }

            this.packageDownloadManager = new PackageDownloadManager(this.profile);

            InitializeComponent();
            this.DataContext = this;

            this.LoadPackagesAsync();

            StartUpdater();
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

      #region Logger

      //Log Appender used to show log in UI.
      public static NotifyAppender logAppender
      {
         get
         {
            return NotifyAppender.GetAppender();
         }
      }

      private static readonly ILog log = 
         LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      #endregion

      #region Close action

      /// <summary>
      /// This Close command is used specifically when clicking "close" from the systray 
      /// notification icon context menu.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void CloseApplication(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.Close();
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
      /// Handler for the application close command
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());
         this.Close();
      }

      /// <summary>
      /// Implementation of the Closing event, in order to save configuration when closing the application
      /// </summary>
      /// <param name="e"></param>
      protected override void OnClosing(CancelEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
         string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");
         SaveCfgToXML(defaultXMLSettings);
         SaveCfgToJSON(defaultJSONSettings);
      }

      /// <summary>
      /// Used to make window come to front when dbl-click the systray notification icon
      /// </summary>
      /// <param name="hWnd"></param>
      /// <returns></returns>
      [DllImport("user32.dll")]
      static extern bool SetForegroundWindow(IntPtr hWnd);

      /// <summary>
      /// Used to restore window when dbl-click the systray notification icon
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void RestoreCommandHandler(object sender, RoutedEventArgs e)
      {
         this.Show();
         this.Focus();

         SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);

         if (this.WindowState != WindowState.Normal && this.WindowState != WindowState.Maximized)
         {
            this.WindowState = WindowState.Normal;
         }
      }

      /// <summary>
      /// Used to "Minimize to systray" (i.e. only keep systray icon and hide window from the taskbar)
      /// when minimizing... if parametered to do so in settings.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnStateChanged(EventArgs e)
      {
         if (WindowState == System.Windows.WindowState.Minimized &&
             this.profile.settings.minimizeToTray)
         {
            this.Hide();
         }

         base.OnStateChanged(e);
      }

      #endregion

      #region Configuration commands

      /// <summary>
      /// Command to browse to, and open a configuration file
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void LoadConfiguration(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML configuration file|*.xml|JSON configuration file|*.json";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.Title = "Open Configuration file";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xml")
               {
                  LoadCfgFromXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".json")
               {
                  LoadCfgFromJSON(fileBrowser.FileName);
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

      private void LoadConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      /// <summary>
      /// Command to save configuration into default location (%localappdata%\ewamLauncher\*)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void SaveConfiguration(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
            string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");
            SaveCfgToXML(defaultXMLSettings);
            SaveCfgToJSON(defaultJSONSettings);
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

      private void SaveConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      /// <summary>
      /// Command to browse to, and save a configuration file
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void SaveConfigurationAs(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML configuration file|*.xml|JSON configuration file|*.json";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Save Configuration file";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xml")
               {
                  SaveCfgToXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".json")
               {
                  SaveCfgToJSON(fileBrowser.FileName);
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

      private void SaveConfigurationAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }


      #endregion

      #region Configuration actions

      /// <summary>
      /// Internal method used backup existing file by prefixing it with date, and only keeping 
      /// a limited number of exising backups.
      /// </summary>
      /// <param name="fileName">name of file to be backed up if existing</param>
      /// <param name="keepOnlyXBackups">limit of number of backup prefixed with date to be kept</param>
      private void BackupToDatedFilename(string fileName, int keepOnlyXBackups)
      {
         if (File.Exists(fileName))
         {
            string path = Path.GetDirectoryName(fileName);
            string prefix = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss - ");
            string suffix = Path.GetFileName(fileName);
            File.Move(fileName, path + "\\" + prefix + suffix);

            List<string> existingBackups = Directory.GetFiles(path, "*" + suffix).ToList();
            existingBackups.Sort();

            if (existingBackups.Count() > keepOnlyXBackups)
            {
               existingBackups.RemoveRange(existingBackups.Count() - keepOnlyXBackups, keepOnlyXBackups);

               foreach (string backupFile in existingBackups)
               {
                  File.Delete(backupFile);
               }
            }
         }

         
      }

      /// <summary>
      /// Loads Profile from XML configuration file
      /// </summary>
      /// <param name="fileName">file from which to load configuration</param>
      public void LoadCfgFromXML(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractSerializer xmlDeserializer = new DataContractSerializer(typeof(Profile));
            Profile tmpProfile =
                (Profile)xmlDeserializer.ReadObject(reader);
            reader.Close();

            if (tmpProfile != null)
            {
               this.profile = tmpProfile;
            }

            foreach (Environment env in this.profile.environments)
            {
               env.RestoreReferenceEwam(this.profile.ewams);
               env.RestoreReferenceVS(this.profile.settings.visualStudios);
            }
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      /// <summary>
      /// Saves Profile to XML configuration file
      /// </summary>
      /// <param name="fileName">file to which to save configuration</param>
      public void SaveCfgToXML(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         if (!Directory.Exists(Path.GetDirectoryName(fileName)))
         {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
         }
         else
         {
            this.BackupToDatedFilename(fileName, this.profile.settings.numberOfBackups);
         }
         
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Profile));
         xmlSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      /// <summary>
      /// Loads Profile from JSON configuration file
      /// </summary>
      /// <param name="fileName">file from which to load configuration</param>
      public void LoadCfgFromJSON(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(Profile));
            Profile tmpProfile =
                (Profile)jsonDeserializer.ReadObject(reader);
            reader.Close();

            if (tmpProfile != null)
               this.profile = tmpProfile;


            foreach (Environment env in this.profile.environments)
            {
               env.RestoreReferenceEwam(this.profile.ewams);
               env.RestoreReferenceVS(this.profile.settings.visualStudios);
            }
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      /// <summary>
      /// Saves Profile to JSON configuration file
      /// </summary>
      /// <param name="fileName">file to which to save configuration</param>
      public void SaveCfgToJSON(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         if (!Directory.Exists(Path.GetDirectoryName(fileName)))
         {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
         }
         else
         {
            this.BackupToDatedFilename(fileName, this.profile.settings.numberOfBackups);
         }

         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(Profile));
         jsonSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      #endregion

      #region Path actions

      /// <summary>
      /// Normalizes a path name. This is necessary, for instance, when comparing two paths.
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string NormalizePath(string path)
      {
         if (path != null && path != "")
         {
            try
            {
               return Path.GetFullPath(new Uri(path).LocalPath)
                     .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
               //.ToUpperInvariant();
            }
            catch (Exception exception)
            {
               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
               return path;
            }
         }
         else
         {
            return path;
         }
      }

      /// <summary>
      /// Returns the longest common prefix of to paths. Used to extract the sub folders 
      /// relative to a common parent folder.
      /// </summary>
      /// <param name="path1"></param>
      /// <param name="path2"></param>
      /// <returns></returns>
      public static string FindLongestCommonPath(string path1, string path2)
      {
         string result = "";

         string absPath1 = Path.GetFullPath(path1);
         string absPath2 = Path.GetFullPath(path2);

         char[] delimiters = { '\\' };

         string[] pathChunks1 = absPath1.Split(delimiters);
         string[] pathChunks2 = absPath2.Split(delimiters);

         
         for (int index = 0; index < Math.Min(pathChunks1.Count(), pathChunks2.Count()); index ++)
         {
            if (pathChunks1[index].ToUpperInvariant() == pathChunks2[index].ToUpperInvariant())
            {
               if (index > 0)
               {
                  result += "\\";
               }

               result += pathChunks1[index];
            }
            else
            {
               break;
            }

         }

         return result;
      }

      /// <summary>
      /// Command Handler to change path of a TextBox in the UI
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnChangePath(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string newvalue = (string)e.OriginalSource.GetType().GetField("Tag").GetValue(e.OriginalSource);

            if (MainWindow.ChangePath(ref newvalue))
            {
               e.OriginalSource.GetType().GetProperty("Tag").SetValue(e.OriginalSource, newvalue);
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

      private void OnChangePath_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      ///// <summary>
      ///// Command handler to navigate (i.e. open an explorer window), to a path specified in the UI
      ///// </summary>
      ///// <param name="sender"></param>
      ///// <param name="e"></param>
      //private void OnExplorePath(object sender, RoutedEventArgs e)
      //{
      //   log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

      //   try
      //   {
      //      MainWindow.ExplorePath((string)((System.Windows.FrameworkContentElement)e.OriginalSource).Tag);
      //   }
      //   catch (Exception exception)
      //   {
      //      log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

      //      System.Windows.MessageBox.Show(
      //         "Something went wrong ! \n\n" + exception.Message,
      //         "Oops",
      //         System.Windows.MessageBoxButton.OK,
      //         System.Windows.MessageBoxImage.Error);
      //   }
      //}

      /// <summary>
      /// Command handler to navigate (i.e. open an explorer window), to a path specified in the UI
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnExplorePath(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            MainWindow.ExplorePath((string)e.OriginalSource.GetType().GetProperty("Tag").GetValue(e.OriginalSource));
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

      private void OnExplorePath_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }
      
      /// <summary>
      /// Static generic method to modify a referenced path
      /// </summary>
      /// <param name="oldPath"></param>
      /// <returns></returns>
      public static bool ChangePath(ref string oldPath)
      {
         bool changed = false;
         FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
         folderBrowser.Description = "Select TGV folder for your environment (i.e. the folder containing tgv/)";
         folderBrowser.SelectedPath = oldPath;
         if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            changed = true;
            oldPath = folderBrowser.SelectedPath;
         }

         return changed;
      }

      /// <summary>
      /// Static generic method to navigate to a given path
      /// </summary>
      /// <param name="path"></param>
      public static void ExplorePath(string path)
      {
         Process.Start("explorer.exe", System.Environment.ExpandEnvironmentVariables(path));
      }

      #endregion

      #region Environments actions

      /// <summary>
      /// Creates and adds a new empty environment in the env. list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnNewEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = new Environment();
            environment.name = "eWAM " + this.profile.environments.Count().ToString();
            ((ObservableCollection<Environment>)lbEnvList.ItemsSource).Add(environment);
            lbEnvList.SelectedItem = environment;
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
      /// Duplicates the selected environment and adds the duplicate to the env. list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDuplicateEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEnvList.SelectedItem == null)
            {
               return;
            }

            Environment environment = (Environment)((Environment)lbEnvList.SelectedItem).Clone();
            environment.name += " (clone)";
            this.profile.environments.Add(environment);
            lbEnvList.SelectedItem = environment;
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
      /// Delete selected environment
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeleteEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            int curSelection = lbEnvList.SelectedIndex;

            if (lbEnvList.SelectedItem == null)
            {
               return;
            }

            ((ObservableCollection<Environment>)lbEnvList.ItemsSource).Remove((Environment)lbEnvList.SelectedItem);
            lbEnvList.SelectedIndex = curSelection;
            if (lbEnvList.SelectedIndex == -1)
               lbEnvList.SelectedIndex = curSelection - 1;
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
      /// Import an existing environment from a local folder
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnImportEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select root folder of your environment (Parent folder of the TGV folder)";

            //OpenFileDialog fileBrowser = new OpenFileDialog();
            //fileBrowser.Filter = "TGV Base1|W001001.tgv|Any TGV|*.tgv";
            //fileBrowser.FilterIndex = 1;
            //fileBrowser.RestoreDirectory = true;

            //if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               EnvironmentImporter importer = new EnvironmentImporter(this.profile);
               Environment environment = importer.ImportFromPath(folderBrowser.SelectedPath);

               Boolean addEnvironment = true;
               foreach (Environment env in this.profile.environments)
               {
                  if (MainWindow.NormalizePath(env.envRoot) == MainWindow.NormalizePath(environment.envRoot))
                  {
                     addEnvironment = false;

                     if (System.Windows.MessageBox.Show(
                           "An environment with same path already exists : \"" +
                              env.name + "\"\nAdd anyway ?",
                           "Environment already exists",
                           MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                     {
                        addEnvironment = true;
                     }
                     break;
                  }
               }

               if (addEnvironment)
               {
                  ((ObservableCollection<Environment>)lbEnvList.ItemsSource).Add(environment);
                  lbEnvList.SelectedItem = environment;
               }

               //Import the environment associated with this new ewam, if it doesn't already exist !
               if (environment.ewam == null)
               {
                  System.Windows.MessageBox.Show(
                     "Warning no corresponding eWAM binaries were found associated with this environment. You will need to set it manualy.",
                     "No corresponding binaies found !",
                     MessageBoxButton.OK);
               }
               else if (System.Windows.MessageBox.Show(
                  "Associated eWAM don't have an environment. \n" +
                  "Do you want to import the environment corresponding the eWAM binaries ? \n\n" +
                  environment.ewam.name + " : " + environment.ewam.basePath,
                  "Corresponding eWAM binaies found !",
                  MessageBoxButton.YesNo) == MessageBoxResult.Yes)
               {
                  importer = new EnvironmentImporter(this.profile);
                  Environment newEnv = importer.ImportFromPath(environment.ewam.basePath);
                  newEnv.name = environment.ewam.name;

                  foreach (Environment env in this.profile.environments)
                  {
                     if (MainWindow.NormalizePath(env.envRoot) == MainWindow.NormalizePath(newEnv.envRoot))
                     {
                        addEnvironment = false;

                        //if (System.Windows.MessageBox.Show(
                        //      "An environment with same path already exists : \"" + 
                        //         env.name + "\"\nAdd anyway ?",
                        //      "Environment already exists",
                        //      MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        //{
                        //   addEnvironment = true;
                        //}
                        break;
                     }
                  }

                  if (addEnvironment)
                  {
                     this.profile.environments.Add(newEnv);
                  }
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

      /// <summary>
      /// Move selected environment up in the env. list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEnvList.SelectedItem == null)
            {
               return;
            }

            int envIndex = this.profile.environments.IndexOf((Environment)lbEnvList.SelectedItem);

            if (envIndex > 0)
            {
               this.profile.environments.Move(envIndex, envIndex - 1);
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
      /// Move selected environment down in the env. list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEnvList.SelectedItem == null)
            {
               return;
            }

            int envIndex = this.profile.environments.IndexOf((Environment)lbEnvList.SelectedItem);

            if (envIndex < this.profile.environments.Count() - 1)
            {
               this.profile.environments.Move(envIndex, envIndex + 1);
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
      /// Export selected environment to an xml (.xenv) or json (.jsenv)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML environment file|*.xenv|JSON environment file|*.jsenv";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Save";
            fileBrowser.InitialDirectory = ((Environment)lbEnvList.SelectedItem).envRoot;

            fileBrowser.FileName = ((Environment)lbEnvList.SelectedItem).name;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xenv")
               {
                  ((Environment)lbEnvList.SelectedItem).SaveEnvironmentToXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jsenv")
               {
                  ((Environment)lbEnvList.SelectedItem).SaveEnvironmentToJSON(fileBrowser.FileName);
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

      /// <summary>
      /// Import environment from an xml (.xenv) or json (.jsenv)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileImportEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML environment file|*.xenv|JSON environment file|*.jsenv";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Open";

            Environment importedEnv = null;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xenv")
               {
                  importedEnv = LoadEnvironmentFromXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jsenv")
               {
                  importedEnv = LoadEnvironmentFromJSON(fileBrowser.FileName);
               }

               importedEnv.envRoot = "";
               importedEnv.wfRoot = "";

               FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
               folderBrowser.SelectedPath = Path.GetDirectoryName(fileBrowser.FileName);

               folderBrowser.Description = "Select Environment Root";
               if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                  importedEnv.envRoot = folderBrowser.SelectedPath;
               }

               folderBrowser.Description = "Select WF Root (Wynsure resource root path)";
               if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
               {
                  importedEnv.wfRoot = folderBrowser.SelectedPath;
               }

               this.profile.environments.Add(importedEnv);
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
      /// Generic method to load an environment from an XML file
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public Environment LoadEnvironmentFromXML(string fileName)
      {
         try
         {
            fileName = NormalizePath(fileName);
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractSerializer xmlDeserializer = new DataContractSerializer(typeof(Environment));
            Environment tmpProfile =
                (Environment)xmlDeserializer.ReadObject(reader);
            reader.Close();

            return tmpProfile;
            //foreach (Environment env in this.profile.environments)
            //{
            //   env.RestoreReferenceEwam(this.profile.ewams);
            //}
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
      }

      /// <summary>
      /// Generic method to load an environment from an JSON file
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public Environment LoadEnvironmentFromJSON(string fileName)
      {
         try
         {
            fileName = NormalizePath(fileName);
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(Environment));
            Environment tmpEnv =
                (Environment)jsonDeserializer.ReadObject(reader);
            reader.Close();

            return tmpEnv;

            //foreach (Environment env in this.profile.environments)
            //{
            //   env.RestoreReferenceEwam(this.profile.ewams);
            //}
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
      }

      /// <summary>
      /// Export all environments to %wf-root%\Launchers\<name>.jsenv and .xenv.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnFileExportAllEnvironments(object sender, RoutedEventArgs e)
      {
         try
         {
            System.Windows.MessageBox.Show(@"Each environment will be saved to %wf-root%\Launchers\<name>.jsenv and .xenv", "Export All Environment Definitions", MessageBoxButton.OK);

            this.profile.ExportAllEnvironments(@"%wf-root%\Launchers");
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }

      }

      #endregion

      #region Ewam actions

      /// <summary>
      /// Creates and adds a new empty ewam in the ewam list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnNewEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Ewam ewam = new Ewam();
            ewam.name = "eWAM " + this.profile.environments.Count().ToString();
            ((ObservableCollection<Ewam>)lbEwamList.ItemsSource).Add(ewam);
            lbEwamList.SelectedItem = ewam;
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
      /// Duplicates the selected ewam and adds the duplicate to the ewam list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDuplicateEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEwamList.SelectedItem == null)
            {
               return;
            }

            Ewam ewam = (Ewam)((Ewam)lbEwamList.SelectedItem).Clone();
            ewam.name += " (clone)";
            this.profile.ewams.Add(ewam);
            lbEwamList.SelectedItem = ewam;
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
      /// Delete selected ewam
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeleteEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            int curSelection = lbEwamList.SelectedIndex;

            if (lbEwamList.SelectedItem == null)
            {
               return;
            }

         ((ObservableCollection<Ewam>)lbEwamList.ItemsSource).Remove((Ewam)lbEwamList.SelectedItem);

            lbEwamList.SelectedIndex = curSelection;
            if (lbEwamList.SelectedIndex == -1)
               lbEwamList.SelectedIndex = curSelection - 1;
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
      /// Import an existing ewam from a local folder
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnImportEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            //FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            //folderBrowser.Description = "Select root folder of an eWAM installation.";

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "eWAM Maps Files|*.Map";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.FileName = "Select a .map file in eWAM root folder.";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               EwamImporter wamImporter = new EwamImporter(this.profile);

               string envPath = MainWindow.NormalizePath(Path.GetDirectoryName(fileBrowser.FileName));
               Ewam ewam = wamImporter.ImportFromPath(envPath);
               ((ObservableCollection<Ewam>)lbEwamList.ItemsSource).Add(ewam);
               lbEwamList.SelectedItem = ewam;

               //Import the environment associated with this new ewam
               Environment newEnv = new Environment();
               newEnv.ewam = ewam;
               newEnv.name = ewam.name;
               EnvironmentImporter envImporter = new EnvironmentImporter(this.profile, newEnv);
               envImporter.ImportFromPath(ewam.basePath);
               this.profile.environments.Add(newEnv);
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
      /// Export selected ewam to an xml (.xenv) or json (.jsenv)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML ewam file|*.xwam|JSON ewam file|*.jswam";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Save";
            fileBrowser.InitialDirectory = ((Ewam)lbEwamList.SelectedItem).basePath;

            fileBrowser.FileName = ((Ewam)lbEwamList.SelectedItem).name;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               try
               {
                  if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xwam")
                  {
                     ((Ewam)lbEwamList.SelectedItem).SaveEwamToXML(fileBrowser.FileName);
                  }
                  else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jswam")
                  {
                     ((Ewam)lbEwamList.SelectedItem).SaveEwamToJSON(fileBrowser.FileName);
                  }
               }
               catch (Exception exception)
               {
                  log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
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

      /// <summary>
      /// Import ewam from an xml (.xenv) or json (.jsenv)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileImportEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "XML ewam file|*.xwam|JSON ewam file|*.jswam";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Open";

            Ewam importedWam = null;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               try
               {
                  if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xwam")
                  {
                     importedWam = LoadEwamFromXML(fileBrowser.FileName);
                  }
                  else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jswam")
                  {
                     importedWam = LoadEwamFromJSON(fileBrowser.FileName);
                  }

                  importedWam.basePath = "";

                  FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                  folderBrowser.SelectedPath = Path.GetDirectoryName(fileBrowser.FileName);

                  folderBrowser.Description = "Select Ewam Root";
                  if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                  {
                     importedWam.basePath = folderBrowser.SelectedPath;
                  }

                  this.profile.ewams.Add(importedWam);
               }
               catch (Exception exception)
               {
                  log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
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

      /// <summary>
      /// Generic method to load an ewam from an XML file
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public Ewam LoadEwamFromXML(string fileName)
      {
         try
         {
            fileName = NormalizePath(fileName);
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractSerializer xmlDeserializer = new DataContractSerializer(typeof(Ewam));
            Ewam tmpProfile =
                (Ewam)xmlDeserializer.ReadObject(reader);
            reader.Close();

            return tmpProfile;
            //foreach (Environment env in this.profile.environments)
            //{
            //   env.RestoreReferenceEwam(this.profile.ewams);
            //}
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
      }

      /// <summary>
      /// Generic method to load an ewam from an JSON file
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public Ewam LoadEwamFromJSON(string fileName)
      {
         try
         {
            fileName = NormalizePath(fileName);
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(Ewam));
            Ewam tmpEnv =
                (Ewam)jsonDeserializer.ReadObject(reader);
            reader.Close();

            return tmpEnv;

            //foreach (Environment env in this.profile.environments)
            //{
            //   env.RestoreReferenceEwam(this.profile.ewams);
            //}
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            return null;
         }
      }

      /// <summary>
      /// Move selected ewam up in the ewam list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEwamList.SelectedItem == null)
            {
               return;
            }

            int wamIndex = this.profile.ewams.IndexOf((Ewam)lbEwamList.SelectedItem);

            if (wamIndex > 0)
            {
               this.profile.ewams.Move(wamIndex, wamIndex - 1);
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
      /// Move selected ewam down in the ewam list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownEwam(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbEwamList.SelectedItem == null)
            {
               return;
            }

            int wamIndex = this.profile.ewams.IndexOf((Ewam)lbEwamList.SelectedItem);

            if (wamIndex < this.profile.ewams.Count() - 1)
            {
               this.profile.ewams.Move(wamIndex, wamIndex + 1);
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
      /// Export all ewams to %wyde-root%\Launchers\<name>.jswam and .xwam
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportAllEwams(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            System.Windows.MessageBox.Show(@"Each ewam will be saved to %wyde-root%\Launchers\<name>.jswam and .xwam", "Export All eWAM Definitions", MessageBoxButton.OK);

            this.profile.ExportAllEwams(@"%wyde-root%\Launchers");
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

      #endregion

      #region Package actions

      /// <summary>
      /// Internal method that asynchronously downloads package-index.xml
      /// </summary>
      private void LoadPackagesAsync()
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            PackageIndexGetter packageGetter = new PackageIndexGetter(this.profile.settings.ewamUpdateServerURL + "//package-index.xml");
            packageGetter.PackageListCompleted += this.OnPackageIndexDownloaded;
            packageGetter.GetPackages();
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      /// <summary>
      /// Even handler called when asynchronous package-index.xml download is completed, sets the 
      /// result in local variable, and indexed byt product (package.type)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnPackageIndexDownloaded(object sender, PackageListCompletedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (e.Cancelled == true)
            {
               eWAMLauncherNotifyIcon.ShowBalloonTip("package-index.xml download cancelled",
                  e.Error.Message, BalloonIcon.Error);

               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : package-index.xml download cancelled");
            }
            else if (e.Error != null)
            {
               eWAMLauncherNotifyIcon.ShowBalloonTip("package-index.xml download failed",
                  e.Error.Message, BalloonIcon.Error);

               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + e.Error.Message);
            }
            else
            {
               this.WideIndex = e.PackageIndex;

               this.productsPackages.Clear();

               foreach (Package package in e.PackageIndex.Packages)
               {
                  if (!this.productsPackages.ContainsKey(package.Type))
                  {
                     this.productsPackages.Add(package.Type, new ObservableCollection<Package>());
                  }

                  this.productsPackages[package.Type].Add(package);
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

      /// <summary>
      /// Command handler used to asynchronously load the online package-index.xml content, only if not downloaded already
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnLoadPackagesIfNeeded(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (this.WideIndex == null)
            {
               this.OnRefreshPackages(sender, e);
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
      /// Command handler to force asynchronously load the online package-index.xml content
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnRefreshPackages(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.productsPackages.Clear();

            PackageIndexGetter packageGetter = new PackageIndexGetter(this.profile.settings.ewamUpdateServerURL + "//package-index.xml");

            packageGetter.PackageListCompleted += this.OnPackageIndexRefreshed;
            packageGetter.GetPackages();
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
      /// Even handler called when asynchronous package-index.xml download is completed, sets the 
      /// result in local variable, and indexed byt product (package.type)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnPackageIndexRefreshed(object sender, PackageListCompletedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (e.Cancelled == true)
            {
               eWAMLauncherNotifyIcon.ShowBalloonTip("package-index.xml download cancelled",
                  e.Error.Message, BalloonIcon.Error);

               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : package-index.xml download cancelled");
            }
            else if (e.Error != null)
            {
               eWAMLauncherNotifyIcon.ShowBalloonTip("package-index.xml download failed",
                  e.Error.Message, BalloonIcon.Error);

               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + e.Error.Message);
            }
            else
            {
               this.WideIndex = e.PackageIndex;

               this.productsPackages.Clear();

               foreach (Package package in e.PackageIndex.Packages)
               {
                  if (!this.productsPackages.ContainsKey(package.Type))
                  {
                     this.productsPackages.Add(package.Type, new ObservableCollection<Package>());
                  }

                  this.productsPackages[package.Type].Add(package);
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

      /// <summary>
      /// Command handler to download the selected package in a selected folder
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnImportSelectedPackage(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbPackageList.SelectedItem == null)
            {
               return;
            }

            string targetDir = "";
            //Select destination folder
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select target directory";
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               targetDir = NormalizePath(folderBrowser.SelectedPath);
            }
            else
            {
               return;
            }

            Package package = (Package)lbPackageList.SelectedItem;

            this.packageDownloadManager.AddDownloadTask(package, targetDir);
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
      /// Command handler to download the selected components of the selected package in a selected folder
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnImportSelectedComponents(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbComponentList.SelectedItems.Count == 0)
            {
               return;
            }

            string targetDir = "";
            //Select destination folder
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select target directory";
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               targetDir = NormalizePath(folderBrowser.SelectedPath);
            }
            else
            {
               return;
            }

            Package selectedPackage = (Package)lbPackageList.SelectedItem;
            Package package = new Package();
            package.Description = selectedPackage.Description;
            package.Id = selectedPackage.Id;
            package.Name = selectedPackage.Name;
            package.Type = selectedPackage.Type;
            package.Version = selectedPackage.Version;

            package.Components = new PackageComponent[lbComponentList.SelectedItems.Count];
            lbComponentList.SelectedItems.CopyTo(package.Components, 0);

            this.packageDownloadManager.AddDownloadTask(package, targetDir);
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

      #endregion

      #region Update actions

      /// <summary>
      /// Squirrel async automatic update process
      /// </summary>
      private void StartUpdater()
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Task.Run(async () =>
            {
               try
               {
                  using (var mgr = new UpdateManager(this.profile.settings.launcherUpdateServerURL))
                  {
                     UpdateInfo updateInfo = await mgr.CheckForUpdate();

                     this.assemblyUpdateInfo = "Latest version: " + updateInfo.FutureReleaseEntry.Version;

                     if (updateInfo.CurrentlyInstalledVersion.Version != updateInfo.FutureReleaseEntry.Version)
                     {
                        //this.assemblyUpdateInfo = "New version detected, downloading...";
                        //await mgr.DownloadReleases(updateInfo.ReleasesToApply);
                        //this.assemblyUpdateInfo = "Download finished, Ready to apply the update.";
                        //string resultPath = await mgr.ApplyReleases(updateInfo);  //Error on this line, with message : "The system cannot find the file specified"
                        //this.assemblyUpdateInfo = "Update applied. You now need to restart !";
                        //mgr.KillAllExecutablesBelongingToPackage();

                        await mgr.UpdateApp();
                        this.assemblyUpdateInfo = "eWamLauncher updated :) ! Please restart the application !";
                     }
                     else
                     {
                        this.assemblyUpdateInfo = "You are up to date.";
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
            });
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
      /// Checks if the current binary is up-to-date, displays message accordingly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnCheckUpdate(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.assemblyUpdateInfo = "Checking latest version, a moment please... ";

            Task.Run(async () =>
            {
               try
               {
                  using (var mgr = new UpdateManager(this.profile.settings.launcherUpdateServerURL))
                  {
                     UpdateInfo updateInfo = await mgr.CheckForUpdate();

                     this.assemblyUpdateInfo = "Latest version: " + updateInfo.FutureReleaseEntry.Version;

                     if (updateInfo.CurrentlyInstalledVersion.Version != updateInfo.FutureReleaseEntry.Version)
                     {
                        //this.assemblyUpdateInfo = "New version detected, downloading...";
                        //await mgr.DownloadReleases(updateInfo.ReleasesToApply);
                        //this.assemblyUpdateInfo = "Download finished, Ready to apply the update.";
                        //string resultPath = await mgr.ApplyReleases(updateInfo); //Error on this line : "The system cannot find the file specified"
                        //this.assemblyUpdateInfo = "Update applied. You now need to restart !";
                        //mgr.KillAllExecutablesBelongingToPackage();

                        await mgr.UpdateApp();
                        this.assemblyUpdateInfo = "eWamLauncher updated :) ! Please restart the application !";
                     }
                     else
                     {
                        this.assemblyUpdateInfo = "You are up to date.";
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
            });
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

      #endregion

   }
}
