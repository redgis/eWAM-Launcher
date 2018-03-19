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
using Squirrel;
using System.Threading.Tasks;

namespace eWamLauncher
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   [DataContract(Name = "eWamLauncher", Namespace = "http://www.wyde.com")]
   public partial class MainWindow : MetroWindow, INotifyPropertyChanged
   {
      private wProfile _profile;
      public wProfile profile { get { return _profile; } set { _profile = value; this.NotifyPropertyChanged(); } }

      private string _assemblyVersion { get; set; }
      public string assemblyVersion { get { return _assemblyVersion; } set { _assemblyVersion = value; this.NotifyPropertyChanged(); } }

      private string _assemblyUpdateInfo { get; set; }
      public string assemblyUpdateInfo { get { return _assemblyUpdateInfo; } set { _assemblyUpdateInfo = value; this.NotifyPropertyChanged(); } }



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
         this.assemblyVersion = Assembly.GetEntryAssembly().GetName().Name;
         this.assemblyVersion += " - " + Assembly.GetEntryAssembly().GetName().Version;
         this.assemblyVersion += " - (c) Mphasis Wyde";
         this.assemblyUpdateInfo = "";

         this.profile = new wProfile();

         string defaultXMLSettings = Environment.ExpandEnvironmentVariables("%APPDATA%\\ewamLauncher.config.xml");
         string defaultJSONSettings = Environment.ExpandEnvironmentVariables("%APPDATA%\\ewamLauncher.config.json");

         try
         { LoadFromXML(defaultXMLSettings); }
         catch
         {
            try
            { LoadFromJSON(defaultJSONSettings); }
            catch
            { }
         }

         InitializeComponent();
         this.DataContext = this;

         StartUpdater();
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

      protected override void OnClosing(CancelEventArgs e)
      {
         string defaultXMLSettings = Environment.ExpandEnvironmentVariables("%APPDATA%\\ewamLauncher.config.xml");
         string defaultJSONSettings = Environment.ExpandEnvironmentVariables("%APPDATA%\\ewamLauncher.config.json");
         SaveToXML(defaultXMLSettings);
         SaveToJSON(defaultJSONSettings);
      }

      public void LoadFromXML(string fileName)
      {
         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractSerializer xmlDeserializer = new DataContractSerializer(typeof(ObservableCollection<wEnvironment>));
            wProfile tmpProfile =
                (wProfile)xmlDeserializer.ReadObject(reader);
            reader.Close();

            if (tmpProfile != null)
               this.profile = tmpProfile;

            foreach (wEnvironment env in this.profile.environments)
            {
               env.RestoreReferenceEwam(this.profile.ewams);
            }
         }
         catch (FileNotFoundException)
         {
         }
      }

      public void SaveToXML(string fileName)
      {
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(wProfile));
         xmlSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      public void LoadFromJSON(string fileName)
      {
         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractJsonSerializer jsonDeserializer = new DataContractJsonSerializer(typeof(wProfile));
            wProfile tmpProfile =
                (wProfile)jsonDeserializer.ReadObject(reader);
            reader.Close();

            if (tmpProfile != null)
               this.profile = tmpProfile;


            foreach (wEnvironment env in this.profile.environments)
            {
               env.RestoreReferenceEwam(this.profile.ewams);
            }
         }
         catch (FileNotFoundException)
         {
         }
      }

      public void SaveToJSON(string fileName)
      {
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(wProfile));
         jsonSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      public static string NormalizePath(string path)
      {
         return Path.GetFullPath(new Uri(path).LocalPath)
                  .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                  .ToUpperInvariant();
      }

      #region Environments actions

      public void OnNewEnvironment(object sender, RoutedEventArgs e)
      {
         wEnvironment environment = new wEnvironment();
         environment.name = "eWAM " + this.profile.environments.Count().ToString();
         ((ObservableCollection<wEnvironment>)lbEnvList.ItemsSource).Add(environment);
         lbEnvList.SelectedItem = environment;

      }

      public void OnDuplicateEnvironment(object sender, RoutedEventArgs e)
      {
         if (lbEnvList.SelectedItem == null)
         {
            return;
         }

         wEnvironment environment = (wEnvironment)((wEnvironment)lbEnvList.SelectedItem).Clone();
         environment.name += " (clone)";
         this.profile.environments.Add(environment);
         lbEnvList.SelectedItem = environment;
      }

      public void OnDeleteEnvironment(object sender, RoutedEventArgs e)
      {
         int curSelection = lbEnvList.SelectedIndex;

         if (lbEnvList.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<wEnvironment>)lbEnvList.ItemsSource).Remove((wEnvironment)lbEnvList.SelectedItem);
         lbEnvList.SelectedIndex = curSelection;
         if (lbEnvList.SelectedIndex == -1)
            lbEnvList.SelectedIndex = curSelection - 1;
      }

      public void OnImportEnvironment(object sender, RoutedEventArgs e)
      {

         //FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
         //folderBrowser.Description = "Select root folder of your environment (i.e. the folder containing tgv/ "

         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "TGV Base1|W001001.tgv|Any TGV|*.tgv";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            wEnvironmentImporter importer = new wEnvironmentImporter(this.profile);

            //Get Parent of Tgv/ folder containing W001001.TGV
            string envPath = Path.GetDirectoryName(Path.GetDirectoryName(fileBrowser.FileName));
            importer.GetEnvironment().tgvPath = Path.GetDirectoryName(fileBrowser.FileName);
            wEnvironment environment = importer.ImportFromPath(envPath);
            ((ObservableCollection<wEnvironment>)lbEnvList.ItemsSource).Add(environment);
            lbEnvList.SelectedItem = environment;

            //Import the environment associated with this new ewam
            importer = new wEnvironmentImporter(this.profile);
            wEnvironment newEnv = importer.ImportFromPath(environment.ewam.basePath);
            newEnv.name = environment.ewam.name;
            this.profile.environments.Add(newEnv);
         }
      }

      #endregion

      #region Launchers actions

      public void OnNewLauncher(object sender, RoutedEventArgs e)
      {
         wLauncher launcher = new wLauncher();
         launcher.name = "new launcher " + ((ObservableCollection<wLauncher>)lbLauncherList.ItemsSource).Count.ToString();
         ((ObservableCollection<wLauncher>)lbLauncherList.ItemsSource).Add(launcher);
         lbLauncherList.SelectedItem = launcher;
      }

      public void OnDuplicateLauncher(object sender, RoutedEventArgs e)
      {
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         wLauncher launcher = (wLauncher)((wLauncher)lbLauncherList.SelectedItem).Clone();
         launcher.name += "(clone)";
         ((ObservableCollection<wLauncher>)lbLauncherList.ItemsSource).Add(launcher);
         lbLauncherList.SelectedItem = launcher;
      }

      public void OnDeleteLauncher(object sender, RoutedEventArgs e)
      {
         int curSelection = lbLauncherList.SelectedIndex;
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<wLauncher>)lbLauncherList.ItemsSource).Remove((wLauncher)lbLauncherList.SelectedItem);
         lbLauncherList.SelectedIndex = curSelection;
      }

      public void OnImportLaunchers(object sender, RoutedEventArgs e)
      {
         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "Batch file|*.bat";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {

            string launchersPath = Path.GetDirectoryName(fileBrowser.FileName);

            wEnvironmentImporter importer = new wEnvironmentImporter((wProfile)this.profile, (wEnvironment)lbEnvList.SelectedItem);
            importer.ImportLaunchers(launchersPath);
         }
      }

      public void OnExecuteLauncher(object sender, RoutedEventArgs e)
      {
         // Before anything, make sure all env. variable are up to date !
         ((wEnvironment)lbEnvList.SelectedItem).ExpandAllEnvVariables();

         wLauncher launcher = (wLauncher)lbLauncherList.SelectedItem;

         ProcessStartInfo startInfo = new ProcessStartInfo();

         // Set %PATH%
         string pathVariable = "" + Environment.GetEnvironmentVariable("PATH");
         pathVariable = launcher.binariesSet.exePathes.Replace('\n', ';') + ";" +
            launcher.binariesSet.dllPathes.Replace('\n', ';') + ";" +
            launcher.binariesSet.cppdllPathes.Replace('\n', ';') + ";" + pathVariable;

         if (startInfo.EnvironmentVariables.ContainsKey("PATH"))
         {
            startInfo.EnvironmentVariables["PATH"] = pathVariable;
         }
         else
         {
            startInfo.EnvironmentVariables.Add("PATH", pathVariable);
         }

         // Put CppDll Folders in WYDE-DLL
         char[] delimiters = { '\n', ';' };
         string[] cppdlls = launcher.binariesSet.cppdllPathes.Split(delimiters);
         startInfo.EnvironmentVariables.Add("WYDE-DLL", cppdlls[0]);

         // TODO : to use when WYDE-DLL support ';' seperated list of pathes.
         //string cppdlls = launcher.binariesSet.cppdllPathes.Replace('\n', ';');
         //startInfo.EnvironmentVariables.Add("WYDE-DLL", cppdlls);

         // Set all other environment variables
         foreach (wEnvironmentVariable variable in
            ((wEnvironment)lbEnvList.SelectedItem).environmentVariables)
         {
            startInfo.EnvironmentVariables.Add(variable.name, variable.result);
         }

         // Find the path to our exe
         string command = "";
         foreach (string path in startInfo.EnvironmentVariables["PATH"].Split(';'))
         {
            if (File.Exists(path + "\\" + launcher.program))
            {
               command = path;
               break;
            }
         }

         // Set the command and arguments
         startInfo.WorkingDirectory = command;
         startInfo.FileName = command + "\\" + launcher.program;
         startInfo.Arguments = launcher.arguments;

         // We're all set, ready to launch.
         startInfo.UseShellExecute = false;
         try
         {
            Process newProcess = Process.Start(startInfo);
         }
         catch
         {
            // Display message showing the exception message
         }
      }

      private void processExited(object sender, EventArgs e)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Environment variables actions

      private void OnReevaluateEnvVariables(object sender, RoutedEventArgs e)
      {
         ((wEnvironment)lbEnvList.SelectedItem).ExpandAllEnvVariables();
      }

      private void OnReevaluateEnvVariables(object sender, EventArgs e)
      {
         ((wEnvironment)lbEnvList.SelectedItem).ExpandAllEnvVariables();
      }

      #endregion

      #region Ewam actions

      public void OnNewEwam(object sender, RoutedEventArgs e)
      {
         wEwam ewam = new wEwam();
         ewam.name = "eWAM " + this.profile.environments.Count().ToString();
         ((ObservableCollection<wEwam>)lbEwamList.ItemsSource).Add(ewam);
         lbEwamList.SelectedItem = ewam;
      }

      public void OnDuplicateEwam(object sender, RoutedEventArgs e)
      {
         if (lbEwamList.SelectedItem == null)
         {
            return;
         }

         wEwam ewam = (wEwam)((wEwam)lbEwamList.SelectedItem).Clone();
         ewam.name += " (clone)";
         this.profile.ewams.Add(ewam);
         lbEwamList.SelectedItem = ewam;
      }

      public void OnDeleteEwam(object sender, RoutedEventArgs e)
      {
         int curSelection = lbEwamList.SelectedIndex;

         if (lbEwamList.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<wEwam>)lbEwamList.ItemsSource).Remove((wEwam)lbEwamList.SelectedItem);

         lbEwamList.SelectedIndex = curSelection;
         if (lbEwamList.SelectedIndex == -1)
            lbEwamList.SelectedIndex = curSelection - 1;
      }

      public void OnImportEwam(object sender, RoutedEventArgs e)
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
            wEwamImporter wamImporter = new wEwamImporter(this.profile);

            string envPath = MainWindow.NormalizePath(Path.GetDirectoryName(fileBrowser.FileName));
            wEwam ewam = wamImporter.ImportFromPath(envPath);
            ((ObservableCollection<wEwam>)lbEwamList.ItemsSource).Add(ewam);
            lbEwamList.SelectedItem = ewam;

            //Import the environment associated with this new ewam
            wEnvironmentImporter envImporter = new wEnvironmentImporter(this.profile);
            wEnvironment newEnv = envImporter.ImportFromPath(ewam.basePath);
            newEnv.name = ewam.name;
            this.profile.environments.Add(newEnv);
         }
      }

      #endregion

      #region binariesSets actions

      public void OnNewBinariesSet(object sender, RoutedEventArgs e)
      {
         wBinariesSet binariesSet = new wBinariesSet();
         binariesSet.name = "new set of binaries " + ((ObservableCollection<wBinariesSet>)lbBinariesSets.ItemsSource).Count.ToString();
         ((ObservableCollection<wBinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
         lbBinariesSets.SelectedItem = binariesSet;
      }

      public void OnDuplicateBinariesSet(object sender, RoutedEventArgs e)
      {
         if (lbBinariesSets.SelectedItem == null)
         {
            return;
         }

         wBinariesSet binariesSet = (wBinariesSet)((wBinariesSet)lbBinariesSets.SelectedItem).Clone();
         binariesSet.name += " (clone)";
         ((ObservableCollection<wBinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
         lbBinariesSets.SelectedItem = binariesSet;
      }

      public void OnDeleteBinariesSet(object sender, RoutedEventArgs e)
      {
         int curSelection = lbBinariesSets.SelectedIndex;
         if (lbBinariesSets.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<wBinariesSet>)lbBinariesSets.ItemsSource).Remove((wBinariesSet)lbBinariesSets.SelectedItem);
         lbBinariesSets.SelectedIndex = curSelection;
         if (lbBinariesSets.SelectedIndex == -1)
            lbBinariesSets.SelectedIndex = curSelection - 1;
      }

      //public void OnImportBinariesSets(object sender, RoutedEventArgs e)
      //{
      //   OpenFileDialog fileBrowser = new OpenFileDialog();

      //   fileBrowser.Filter = "Any file|*.exe;*.dll";
      //   fileBrowser.FilterIndex = 1;
      //   fileBrowser.RestoreDirectory = true;

      //   if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      //   {

      //      string envPath = Path.GetDirectoryName(Path.GetDirectoryName(fileBrowser.FileName));

      //      wEwamImporter importer = new wEwamImporter((wProfile)this.profile);
      //      importer.ImportFromPath(envPath);
      //   }
      //}

      #endregion

      #region Update actions

      public void OnCheckUpdate(object sender, RoutedEventArgs e)
      {
         Task.Run( async () =>
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
                  this.assemblyUpdateInfo = "Update installed, please restart the app !";
               }
               else
               {
                  this.assemblyUpdateInfo = "You are up to date.";
               }
            }
         });
      }

      #endregion
   }
}
