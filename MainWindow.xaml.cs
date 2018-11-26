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
      public Profile profile { get { return _profile; } set { _profile = value; this.NotifyPropertyChanged(); } }

      private string _assemblyVersion { get; set; }
      public string assemblyVersion { get { return _assemblyVersion; } set { _assemblyVersion = value; this.NotifyPropertyChanged(); } }

      private string _assemblyUpdateInfo { get; set; }
      public string assemblyUpdateInfo { get { return _assemblyUpdateInfo; } set { _assemblyUpdateInfo = value; this.NotifyPropertyChanged(); } }

      private ObservableCollection<Package> _packages;
      public ObservableCollection<Package> packages { get { return _packages; } set { _packages = value; this.NotifyPropertyChanged(); } }

      private PackageDownloadManager _packageDownloadManager;
      public PackageDownloadManager packageDownloadManager { get { return _packageDownloadManager; } set { _packageDownloadManager = value; this.NotifyPropertyChanged(); } }

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

      public MainWindow()
      {
         //Initialize logging system
         log4net.Config.XmlConfigurator.Configure();

         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.assemblyVersion = Assembly.GetEntryAssembly().GetName().Name;
            this.assemblyVersion += " - " + Assembly.GetEntryAssembly().GetName().Version;
            this.assemblyVersion += " - (c) Mphasis Wyde";
            this.assemblyUpdateInfo = "";

            string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
            string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");

            this.profile = new Profile();
            this.packages = new ObservableCollection<Package>();

            try
            { LoadCfgFromXML(defaultXMLSettings); }
            catch
            {
               try
               { LoadCfgFromJSON(defaultJSONSettings); }
               catch (Exception exception)
               {
                  this.profile = new Profile();
                  this.packages = new ObservableCollection<Package>();
                  log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
               }
            }

            this.packageDownloadManager = new PackageDownloadManager(this.profile);

            InitializeComponent();
            this.DataContext = this;

            //this.notifier.MouseDown += new System.Windows.Forms.MouseEventHandler(OnNotifyIconClicked);
            //this.notifier.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            //this.notifier.Visible = true;

            //this.menu = (System.Windows.Controls.ContextMenu)this.FindResource("NotifierContextMenu");

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

      private void StartUpdater()
         {
            Task.Run(async () =>
            {
               using (var mgr = new UpdateManager(this.profile.settings.launcherUpdateServerURL))
               {
                 // Note, in most of these scenarios, the app exits after this method
                 // completes!
                 //SquirrelAwareApp.HandleEvents(
                 //   onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                 //   onAppUpdate: v =>
                 //   {
                 //      mgr.CreateShortcutForThisExe();
                 //      System.Windows.MessageBox.Show("Updated", "Update detected!", System.Windows.MessageBoxButton.OK);
                 //   },
                 //   onAppUninstall: v => mgr.RemoveShortcutForThisExe(),
                 //   onFirstRun: () =>
                 //   {
                 //      System.Windows.MessageBox.Show("First run", "First run!", System.Windows.MessageBoxButton.OK);
                 //   },
                 //   onAppObsoleted: v =>
                 //   {
                 //      System.Windows.MessageBox.Show("Obsoleted", "App obsolete!", System.Windows.MessageBoxButton.OK);
                 //   }
                 //);

                  await mgr.UpdateApp();
               }
            });
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

      private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      #endregion

      #region Close action

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

      private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());
         this.Close();
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
         string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");
         SaveCfgToXML(defaultXMLSettings);
         SaveCfgToJSON(defaultJSONSettings);
      }

      [DllImport("user32.dll")]
      static extern bool SetForegroundWindow(IntPtr hWnd);

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

      protected override void OnStateChanged(EventArgs e)
      {
         if (WindowState == System.Windows.WindowState.Minimized)
            this.Hide();

         base.OnStateChanged(e);
      }

      #endregion

      #region Configuration commands

      private void OpenConfiguration(object sender, ExecutedRoutedEventArgs e)
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

      private void OpenConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void SaveConfiguration(object sender, ExecutedRoutedEventArgs e)
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

      private void SaveConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      #endregion

      #region Configuration actions

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
            }
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      public void SaveCfgToXML(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Profile));
         xmlSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

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
            }
         }
         catch (FileNotFoundException exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      public void SaveCfgToJSON(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(Profile));
         jsonSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      #endregion

      #region Path actions

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

      private void OnChangePath(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string newvalue = (string)((System.Windows.Controls.Button)e.OriginalSource).Tag;

            if (MainWindow.ChangePath(ref newvalue))
            {
               ((System.Windows.Controls.Button)e.OriginalSource).Tag = newvalue;
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

      private void OnExplorePath(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            MainWindow.ExplorePath((string)((System.Windows.Controls.Button)e.OriginalSource).Tag);
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

      public static void ExplorePath(string path)
      {
         Process.Start("explorer.exe", path);
      }

      #endregion

      #region Environments actions

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

      public void OnImportEnvironment(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select root folder of your environment";

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
                  this.SaveEnvironmentToXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jsenv")
               {
                  this.SaveEnvironmentToJSON(fileBrowser.FileName);
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

      public void SaveEnvironmentToXML(string fileName)
      {
         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         Environment envCopy = (Environment)((Environment)lbEnvList.SelectedItem).Clone();
         envCopy.binariesSet = null;
         envCopy.ewam = null;

         if (envCopy.envRoot != null && envCopy.envRoot != "" && 
             envCopy.wfRoot != null && envCopy.wfRoot != "")
         {
            string commonPath = FindLongestCommonPath(envCopy.envRoot, envCopy.wfRoot);
            envCopy.envRoot = envCopy.envRoot.Substring(commonPath.Length + 1);
            envCopy.wfRoot = envCopy.wfRoot.Substring(commonPath.Length + 1);
         }
         else
         {
            envCopy.envRoot = "";
            envCopy.wfRoot = "";
         }

         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Environment));
         xmlSerializer.WriteObject(writer, envCopy);
         writer.Close();
      }

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

      public void SaveEnvironmentToJSON(string fileName)
      {
         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         Environment envCopy = (Environment)((Environment)lbEnvList.SelectedItem).Clone();
         envCopy.binariesSet = null;
         envCopy.ewam = null;

         if (envCopy.envRoot != null && envCopy.envRoot != "" &&
             envCopy.wfRoot != null && envCopy.wfRoot != "")
         {
            string commonPath = FindLongestCommonPath(envCopy.envRoot, envCopy.wfRoot);
            envCopy.envRoot = envCopy.envRoot.Substring(commonPath.Length + 1);
            envCopy.wfRoot = envCopy.wfRoot.Substring(commonPath.Length + 1);
         }
         else
         {
            envCopy.envRoot = "";
            envCopy.wfRoot = "";
         }

         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(Environment));
         jsonSerializer.WriteObject(writer, envCopy);
         writer.Close();
      }

      #endregion

      #region Ewam actions

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
                     this.SaveEwamToXML(fileBrowser.FileName);
                  }
                  else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".jswam")
                  {
                     this.SaveEwamToJSON(fileBrowser.FileName);
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

      public void SaveEwamToXML(string fileName)
      {
         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         Ewam ewamCopy = (Ewam)((Ewam)lbEwamList.SelectedItem).Clone();
         ewamCopy.basePath = "";
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Ewam));
         xmlSerializer.WriteObject(writer, ewamCopy);
         writer.Close();
      }

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

      public void SaveEwamToJSON(string fileName)
      {
         Directory.CreateDirectory(Path.GetDirectoryName(fileName));

         Ewam ewamCopy = (Ewam)((Ewam)lbEnvList.SelectedItem).Clone();
         ewamCopy.basePath = "";
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(Ewam));
         jsonSerializer.WriteObject(writer, ewamCopy);
         writer.Close();
      }

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

      #endregion

      #region Package actions

      public void OnRefreshPackages(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            this.packages.Clear();

            WebClient wc = new WebClient();
            wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.OnPackageIndexDownloaded);
            wc.DownloadDataAsync(new Uri(this.profile.settings.ewamUpdateServerURL + "//package-index.xml"));
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

      private void OnPackageIndexDownloaded(object sender, DownloadDataCompletedEventArgs e)
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
               byte[] raw = e.Result;

               String webData = System.Text.Encoding.UTF8.GetString(raw);
               XmlSerializer serializer = new XmlSerializer(typeof(WideIndex));
               MemoryStream memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(webData));
               WideIndex packageIndex = (WideIndex)serializer.Deserialize(memStream);
               foreach (Package package in packageIndex.Packages)
               {
                  this.packages.Add(package);
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

      public void OnCheckUpdate(object sender, RoutedEventArgs e)
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
                        //this.assemblyUpdateInfo = "New version available, downloading...";

                        //await mgr.DownloadReleases(updateInfo.ReleasesToApply);

                        //this.assemblyUpdateInfo = "Download finished, Ready to apply the update.";

                        //string resultPath = await mgr.ApplyReleases(updateInfo);

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
