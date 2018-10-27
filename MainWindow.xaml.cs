﻿using System;
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
using System.Windows.Input;
using System.Windows.Controls;
using System.IO.Compression;

namespace eWamLauncher
{
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

      [DataMember()] private ObservableCollection<Package> _packages;
      public ObservableCollection<Package> packages { get { return _packages; } set { _packages = value; this.NotifyPropertyChanged(); } }
      

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

         this.profile = new Profile();
         this.packages = new ObservableCollection<Package>();

         string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
         string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");

         try
         { LoadCfgFromXML(defaultXMLSettings); }
         catch
         {
            try
            { LoadCfgFromJSON(defaultJSONSettings); }
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

      private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
      {
         this.Close();
      }

      protected override void OnClosing(CancelEventArgs e)
      {
         string defaultXMLSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.xml");
         string defaultJSONSettings = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\ewamLauncher\\ewamLauncher.config.json");
         SaveCfgToXML(defaultXMLSettings);
         SaveCfgToJSON(defaultJSONSettings);
      }

      #region Configuration commands

      private void OpenConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void OpenConfiguration_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "XML configuration file|*.xml|JSON configuration file|*.json";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;
         fileBrowser.Title = "Open Configuration file";

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            try
            {
               if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".xml")
               {
                  LoadCfgFromXML(fileBrowser.FileName);
               }
               else if (Path.GetExtension(fileBrowser.FileName).ToLower() == ".json")
               {
                  LoadCfgFromJSON(fileBrowser.FileName);
               }
            } catch (Exception)
            {

            }
         }
      }

      private void SaveConfiguration_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      private void SaveConfiguration_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "XML configuration file|*.xml|JSON configuration file|*.json";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;
         fileBrowser.CheckFileExists = false;
         fileBrowser.Title = "Save Configuration file";

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            try
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
            catch (Exception)
            {

            }
         }
      }
      
      #endregion

      #region Configuration actions

      public void LoadCfgFromXML(string fileName)
      {
         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            DataContractSerializer xmlDeserializer = new DataContractSerializer(typeof(ObservableCollection<Environment>));
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
         catch (FileNotFoundException)
         {
         }
      }

      public void SaveCfgToXML(string fileName)
      {
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(Profile));
         xmlSerializer.WriteObject(writer, this.profile);
         writer.Close();
      }

      public void LoadCfgFromJSON(string fileName)
      {
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
         catch (FileNotFoundException)
         {
         }
      }

      public void SaveCfgToJSON(string fileName)
      {
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
            catch (Exception e)
            {
               return path;
            }
         }
         else
         {
            return path;
         }
      }

      private void OnChangePath(object sender, RoutedEventArgs e)
      {
         //OpenFileDialog fileBrowser = new OpenFileDialog();

         //fileBrowser.Filter = "eWAM TGV Files|*.tgv";
         //fileBrowser.FilterIndex = 1;
         //fileBrowser.RestoreDirectory = true;
         //fileBrowser.FileName = "Select a .tgv from the environment.";

         //if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         //{
         //   EwamImporter wamImporter = new EwamImporter(this.profile);

         //   ((Environment)lbEnvList.SelectedItem).tgvPath = MainWindow.NormalizePath(Path.GetDirectoryName(fileBrowser.FileName));
         //}

         FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
         folderBrowser.Description = "Select TGV folder for your environment (i.e. the folder containing tgv/)";
         folderBrowser.SelectedPath = (string)((System.Windows.Controls.Button)sender).Tag;
         if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            ((System.Windows.Controls.Button)sender).Tag = folderBrowser.SelectedPath;
         }

      }

      private void OnExplorePath(object sender, RoutedEventArgs e)
      {
         Process.Start("explorer.exe", (string)((System.Windows.Controls.Button)sender).Tag);
      }

      #endregion

      #region Environments actions

      public void OnNeEnvironment(object sender, RoutedEventArgs e)
      {
         Environment environment = new Environment();
         environment.name = "eWAM " + this.profile.environments.Count().ToString();
         ((ObservableCollection<Environment>)lbEnvList.ItemsSource).Add(environment);
         lbEnvList.SelectedItem = environment;

      }

      public void OnDuplicateEnvironment(object sender, RoutedEventArgs e)
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

      public void OnDeleteEnvironment(object sender, RoutedEventArgs e)
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

      public void OnImportEnvironment(object sender, RoutedEventArgs e)
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

      public void OnMoveUpEnvironment(object sender, RoutedEventArgs e)
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

      public void OnMoveDownEnvironment(object sender, RoutedEventArgs e)
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

      public void OnFileExportEnvironment(object sender, RoutedEventArgs e)
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
            try
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
            catch (Exception)
            {

            }
         }
      }

      public void OnFileImportEnvironment(object sender, RoutedEventArgs e)
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
            try
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
            catch (Exception execption)
            {

            }
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
         catch (FileNotFoundException)
         {
            return null;
         }
         catch (Exception e)
         {
            return null;
         }
      }

      public void SaveEnvironmentToXML(string fileName)
      {
         Environment envCopy = (Environment) ((Environment)lbEnvList.SelectedItem).Clone();
         envCopy.binariesSet = null;
         envCopy.ewam = null;
         envCopy.envRoot = "";
         envCopy.wfRoot = "";
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
         catch (FileNotFoundException)
         {
            return null;
         }
         catch (Exception e)
         {
            return null;
         }
      }

      public void SaveEnvironmentToJSON(string fileName)
      {
         Environment envCopy = (Environment)((Environment)lbEnvList.SelectedItem).Clone();
         envCopy.binariesSet = null;
         envCopy.ewam = null;
         envCopy.envRoot = "";
         envCopy.wfRoot = "";
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(Environment));
         jsonSerializer.WriteObject(writer, envCopy);
         writer.Close();
      }

      #endregion

      #region Environment variables actions

      private void OnReevaluateEnvVariables(object sender, RoutedEventArgs e)
      {
         ((Environment)lbEnvList.SelectedItem).ExpandAllEnvVariables();
      }

      private void OnReevaluateEnvVariables(object sender, EventArgs e)
      {
         ((Environment)lbEnvList.SelectedItem).ExpandAllEnvVariables();
      }

      public void OnMoveUpVariable(object sender, RoutedEventArgs e)
      {
         if (lbEnvList.SelectedItem == null || dgVarList.SelectedItem == null)
         {
            return;
         }

         int varIndex = ((Environment)lbEnvList.SelectedItem).environmentVariables.IndexOf(
            (EnvironmentVariable)dgVarList.SelectedItem);

         if (varIndex > 0)
         {
            ((Environment)lbEnvList.SelectedItem).environmentVariables.Move(varIndex, varIndex - 1);
         }
      }

      public void OnMoveDownVariable(object sender, RoutedEventArgs e)
      {
         if (lbEnvList.SelectedItem == null || dgVarList.SelectedItem == null)
         {
            return;
         }

         int varIndex = ((Environment)lbEnvList.SelectedItem).environmentVariables.IndexOf(
            (EnvironmentVariable)dgVarList.SelectedItem);

         if (varIndex < ((Environment)lbEnvList.SelectedItem).environmentVariables.Count() - 1)
         {
            ((Environment)lbEnvList.SelectedItem).environmentVariables.Move(varIndex, varIndex + 1);
         }
      }

      #endregion

      #region Launchers actions

      public void OnNewLauncher(object sender, RoutedEventArgs e)
      {
         Launcher launcher = new Launcher();
         launcher.name = "new launcher " + ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Count.ToString();
         ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Add(launcher);
         lbLauncherList.SelectedItem = launcher;
      }

      public void OnDuplicateLauncher(object sender, RoutedEventArgs e)
      {
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         Launcher launcher = (Launcher)((Launcher)lbLauncherList.SelectedItem).Clone();
         launcher.name += "(clone)";
         ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Add(launcher);
         lbLauncherList.SelectedItem = launcher;
      }

      public void OnDeleteLauncher(object sender, RoutedEventArgs e)
      {
         int curSelection = lbLauncherList.SelectedIndex;
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Remove((Launcher)lbLauncherList.SelectedItem);
         lbLauncherList.SelectedIndex = curSelection;
         if (lbLauncherList.SelectedIndex == -1)
            lbLauncherList.SelectedIndex = curSelection - 1;
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

            EnvironmentImporter importer = new EnvironmentImporter((Profile)this.profile, (Environment)lbEnvList.SelectedItem);
            importer.ImportLaunchers(launchersPath);
         }
      }

      public void OnConsoleExecuteLauncher(object sender, RoutedEventArgs e)
      {
         // Before anything, make sure all env. variable are up to date !
         Environment environment = (Environment)lbEnvList.SelectedItem;
         environment.ExpandAllEnvVariables();

         Launcher launcher = (Launcher)lbLauncherList.SelectedItem;

         ProcessStartInfo startInfo = new ProcessStartInfo();
         
         if (environment.binariesSet == null)
         {
            System.Windows.MessageBox.Show(
               "Warning : no binaries selected.",
               "No eWAM binaries selected",
               System.Windows.MessageBoxButton.OK);
            return;
         }

         char[] delimiters = { '\n', ';', '\r', '\b' };

         // Set %PATH%
         List<string> binSubPathes = new List<string>();
         binSubPathes.AddRange(environment.binariesSet.dllPathes.Split(delimiters));
         binSubPathes.AddRange(environment.binariesSet.cppdllPathes.Split(delimiters));
         binSubPathes.AddRange(environment.binariesSet.exePathes.Split(delimiters));

         string pathVariable = "";
         foreach (string subBinPath in binSubPathes)
         {
            if (subBinPath != "")
            {
               pathVariable += environment.ewam.basePath + "\\" + subBinPath + ";";
            }
         }

         if (((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("PATH") != null)
         {
            pathVariable += ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("PATH").result + ";";
         }

         pathVariable += System.Environment.GetEnvironmentVariable("PATH");

         if (startInfo.EnvironmentVariables.ContainsKey("PATH"))
         {
            startInfo.EnvironmentVariables["PATH"] = pathVariable;
         }
         else
         {
            startInfo.EnvironmentVariables.Add("PATH", pathVariable);
         }

         // Put CppDll Folders in WYDE-DLL
         string[] cppdlls = environment.binariesSet.cppdllPathes.Split(delimiters);
         startInfo.EnvironmentVariables.Add("WYDE-DLL", environment.ewam.basePath + "\\" + cppdlls[0]);

         // TODO : to use when WYDE-DLL support ';' seperated list of pathes.
         //string cppdlls = launcher.binariesSet.cppdllPathes.Replace('\n', ';');
         //startInfo.EnvironmentVariables.Add("WYDE-DLL", cppdlls);

         //startInfo.EnvironmentVariables.Add("WYDE-ROOT", ((Environment)lbEnvList.SelectedItem).ewam.basePath);
         startInfo.EnvironmentVariables.Add("WYDE-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WYDE-ROOT").value);
         startInfo.EnvironmentVariables.Add("WF-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WF-ROOT").value);
         startInfo.EnvironmentVariables.Add("ENV-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("ENV-ROOT").value);
         startInfo.EnvironmentVariables.Add("WYDE-TGV", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WYDE-TGV").value);


         // Set all other environment variables
         foreach (EnvironmentVariable variable in
            ((Environment)lbEnvList.SelectedItem).environmentVariables)
         {
            if (variable.name == "PATH") continue;

            if (startInfo.EnvironmentVariables.ContainsKey(variable.name))
            {
               startInfo.EnvironmentVariables[variable.name] += variable.result;
            }
            else
            {
               startInfo.EnvironmentVariables.Add(variable.name, variable.result);
            }
         }

         // Find the path to our exe
         string commandPath = "";
         foreach (string path in startInfo.EnvironmentVariables["PATH"].Split(';'))
         {
            if (File.Exists(path + "\\cmd.exe"))
            {
               commandPath = path;
               break;
            }
         }

         // Set the command and arguments
         startInfo.WorkingDirectory = environment.ewam.basePath;
         startInfo.FileName = commandPath + "\\cmd.exe";
         startInfo.Arguments = "/K \"" + launcher.program + " " + environment.ExpandString(launcher.arguments) + "\"";

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

      public void OnExecuteLauncher(object sender, RoutedEventArgs e)
      {
         // Before anything, make sure all env. variable are up to date !
         Environment environment = (Environment)lbEnvList.SelectedItem;
         environment.ExpandAllEnvVariables();

         Launcher launcher = (Launcher)lbLauncherList.SelectedItem;

         ProcessStartInfo startInfo = new ProcessStartInfo();

         if (environment.binariesSet == null)
         {
            System.Windows.MessageBox.Show(
               "Warning : no binaries selected.",
               "No eWAM binaries selected",
               System.Windows.MessageBoxButton.OK);
            return;
         }

         char[] delimiters = { '\n', ';', '\r', '\b' };

         // Set %PATH%
         List<string> binSubPathes = new List<string>();
         binSubPathes.AddRange(environment.binariesSet.dllPathes.Split(delimiters));
         binSubPathes.AddRange(environment.binariesSet.cppdllPathes.Split(delimiters));
         binSubPathes.AddRange(environment.binariesSet.exePathes.Split(delimiters));

         string pathVariable = "";
         foreach (string subBinPath in binSubPathes)
         {
            if (subBinPath != "")
            {
               pathVariable += environment.ewam.basePath + "\\" + subBinPath + ";";
            }
         }

         if (((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("PATH") != null)
         {
            pathVariable += ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("PATH").result + ";";
         }
         
         pathVariable += System.Environment.GetEnvironmentVariable("PATH");


         if (startInfo.EnvironmentVariables.ContainsKey("PATH"))
         {
            startInfo.EnvironmentVariables["PATH"] = pathVariable;
         }
         else
         {
            startInfo.EnvironmentVariables.Add("PATH", pathVariable);
         }

         // Put CppDll Folders in WYDE-DLL
         string[] cppdlls = environment.binariesSet.cppdllPathes.Split(delimiters);
         startInfo.EnvironmentVariables.Add("WYDE-DLL", environment.ewam.basePath + "\\" + cppdlls[0]);

         // TODO : to use when WYDE-DLL support ';' seperated list of pathes.
         //string cppdlls = launcher.binariesSet.cppdllPathes.Replace('\n', ';');
         //startInfo.EnvironmentVariables.Add("WYDE-DLL", cppdlls);

         //startInfo.EnvironmentVariables.Add("WYDE-ROOT", ((Environment)lbEnvList.SelectedItem).ewam.basePath);
         startInfo.EnvironmentVariables.Add("WYDE-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WYDE-ROOT").value);
         startInfo.EnvironmentVariables.Add("WF-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WF-ROOT").value);
         startInfo.EnvironmentVariables.Add("ENV-ROOT", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("ENV-ROOT").value);
         startInfo.EnvironmentVariables.Add("WYDE-TGV", 
            ((Environment)lbEnvList.SelectedItem).GetEnvironmentVariable("WYDE-TGV").value);

         // Set all other environment variables
         foreach (EnvironmentVariable variable in
            ((Environment)lbEnvList.SelectedItem).environmentVariables)
         {
            if (variable.name == "PATH") continue;

            if (startInfo.EnvironmentVariables.ContainsKey(variable.name))
            {
               startInfo.EnvironmentVariables[variable.name] += variable.result;
            }
            else
            {
               startInfo.EnvironmentVariables.Add(variable.name, variable.result);
            }
         }

         // Find the path to our exe
         string commandPath = "";
         foreach (string path in startInfo.EnvironmentVariables["PATH"].Split(';'))
         {
            if (File.Exists(path + "\\" + launcher.program))
            {
               commandPath = path;
               break;
            }
         }

         // Set the command and arguments
         startInfo.WorkingDirectory = commandPath;
         startInfo.FileName = commandPath + "\\" + launcher.program;
         startInfo.Arguments = environment.ExpandString(launcher.arguments);

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

      public void OnMoveUpLauncher(object sender, RoutedEventArgs e)
      {
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         int launcherIndex = ((Environment)lbEnvList.SelectedItem).launchers.IndexOf(
            (Launcher)lbLauncherList.SelectedItem);

         if (launcherIndex > 0)
         {
            ((Environment)lbEnvList.SelectedItem).launchers.Move(launcherIndex, launcherIndex - 1);
         }
      }

      public void OnMoveDownLauncher(object sender, RoutedEventArgs e)
      {
         if (lbLauncherList.SelectedItem == null)
         {
            return;
         }

         int launcherIndex = ((Environment)lbEnvList.SelectedItem).launchers.IndexOf(
            (Launcher)lbLauncherList.SelectedItem);

         if (launcherIndex < ((Environment)lbEnvList.SelectedItem).launchers.Count() - 1)
         {
            ((Environment)lbEnvList.SelectedItem).launchers.Move(launcherIndex, launcherIndex + 1);
         }
      }

      #endregion

      #region Ewam actions

      public void OnNewEwam(object sender, RoutedEventArgs e)
      {
         Ewam ewam = new Ewam();
         ewam.name = "eWAM " + this.profile.environments.Count().ToString();
         ((ObservableCollection<Ewam>)lbEwamList.ItemsSource).Add(ewam);
         lbEwamList.SelectedItem = ewam;
      }

      public void OnDuplicateEwam(object sender, RoutedEventArgs e)
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

      public void OnDeleteEwam(object sender, RoutedEventArgs e)
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

      public void OnFileExportEwam(object sender, RoutedEventArgs e)
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
            catch (Exception)
            {

            }
         }
      }

      public void OnFileImportEwam(object sender, RoutedEventArgs e)
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
            catch (Exception execption)
            {

            }
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
         catch (FileNotFoundException)
         {
            return null;
         }
         catch (Exception e)
         {
            return null;
         }
      }

      public void SaveEwamToXML(string fileName)
      {
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
         catch (FileNotFoundException)
         {
            return null;
         }
         catch (Exception e)
         {
            return null;
         }
      }

      public void SaveEwamToJSON(string fileName)
      {
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

      public void OnMoveDownEwam(object sender, RoutedEventArgs e)
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

      #endregion

      #region binariesSets actions

      public void OnNewBinariesSet(object sender, RoutedEventArgs e)
      {
         BinariesSet binariesSet = new BinariesSet();
         binariesSet.name = "new set of binaries " + ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Count.ToString();
         ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
         lbBinariesSets.SelectedItem = binariesSet;
      }

      public void OnDuplicateBinariesSet(object sender, RoutedEventArgs e)
      {
         if (lbBinariesSets.SelectedItem == null)
         {
            return;
         }

         BinariesSet binariesSet = (BinariesSet)((BinariesSet)lbBinariesSets.SelectedItem).Clone();
         binariesSet.name += " (clone)";
         ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Add(binariesSet);
         lbBinariesSets.SelectedItem = binariesSet;
      }

      public void OnDeleteBinariesSet(object sender, RoutedEventArgs e)
      {
         int curSelection = lbBinariesSets.SelectedIndex;
         if (lbBinariesSets.SelectedItem == null)
         {
            return;
         }

         ((ObservableCollection<BinariesSet>)lbBinariesSets.ItemsSource).Remove((BinariesSet)lbBinariesSets.SelectedItem);
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

      //      EwamImporter importer = new EwamImporter((Profile)this.profile);
      //      importer.ImportFromPath(envPath);
      //   }
      //}

      #endregion
      
      #region Package actions

      public void OnRefreshPackages(object sender, RoutedEventArgs e)
      {
         this.packages.Clear();

         System.Net.WebClient wc = new System.Net.WebClient();
         byte[] raw;
         try
         {
            raw = wc.DownloadData(this.profile.settings.ewamUpdateServerURL + "//package-index.xml");
         } 
         catch (Exception except)
         {
            System.Windows.MessageBox.Show(
               "Could not download package-index.xml from " + 
                  this.profile.settings.ewamUpdateServerURL + 
                  "//package-index.xml\n\n" + except.Message, 
               "Package index error",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
            return;
         }
         String webData = System.Text.Encoding.UTF8.GetString(raw);
         XmlSerializer serializer = new XmlSerializer(typeof(WideIndex));
         MemoryStream memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(webData));
         WideIndex packageIndex = (WideIndex)serializer.Deserialize(memStream);
         foreach (Package package in packageIndex.packages)
         {
            this.packages.Add(package);
         }
      }

      public async void OnImportSelectedPackage(object sender, RoutedEventArgs e)
      {
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

         //Create ProgressBar
         System.Windows.Controls.Label label = new System.Windows.Controls.Label();
         label.Content = "Downloading " + package.Description + "...";
         label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
         label.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
         stkpProgressBars.Children.Add(label);

         System.Windows.Controls.ProgressBar progressBar = new System.Windows.Controls.ProgressBar();
         progressBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         stkpProgressBars.Children.Add(progressBar);

         await Task.Run(() => PullPackage(this, targetDir, package, progressBar));


         label.Content = "Configuring...";

         // the package is an eWAM binaries set
         if (package.Type == "ewam")
         {
            Ewam importedEwam = null;

            // Get BinariesSet and import associated eWAM
            foreach (PackageComponent component in package.Components)
            {
               if (component.Name == "BinariesSets")
               {
                  string filename = NormalizePath(targetDir + "\\" + component.Files[0].Path);
                  string extension = Path.GetExtension(filename);

                  if (extension == ".xwam")
                  {
                     importedEwam = LoadEwamFromXML(filename);
                  }
                  else if (extension == ".jswam")
                  {
                     importedEwam = LoadEwamFromJSON(filename);
                  }

                  importedEwam.basePath = targetDir;
                  importedEwam.name = package.Description;

                  profile.ewams.Add(importedEwam);
               }
            }

            // If exists, also create associated environment using Launchers
            foreach (PackageComponent component in package.Components)
            {
               if (component.Name == "Launchers")
               {
                  string filename = NormalizePath(targetDir + "\\" + component.Files[0].Path);
                  string extension = Path.GetExtension(filename);

                  Environment importedEnv = null;

                  if (extension == ".xenv")
                  {
                     importedEnv = LoadEnvironmentFromXML(filename);
                  }
                  else if (extension == ".jsenv")
                  {
                     importedEnv = LoadEnvironmentFromJSON(filename);
                  }

                  importedEnv.envRoot = targetDir;
                  importedEnv.ewam = importedEwam;
                  importedEnv.name = package.Description;

                  profile.environments.Add(importedEnv);
               }
            }
         }
         // the package is an environment
         else if (package.Type == "environment")
         {
            // If exists, also create associated environment using Launchers
            foreach (PackageComponent component in package.Components)
            {
               if (component.Name == "Launchers")
               {
                  string filename = NormalizePath(targetDir + "\\" + component.Files[0].Path);
                  string extension = Path.GetExtension(filename);

                  Environment importedEnv = null;

                  if (extension == ".xenv")
                  {
                     importedEnv = LoadEnvironmentFromXML(filename);
                  }
                  else if (extension == ".jsenv")
                  {
                     importedEnv = LoadEnvironmentFromJSON(filename);
                  }

                  // fill envRoot and wfRoot by adding the provided path as prefix
                  importedEnv.envRoot = targetDir + "\\" + importedEnv.envRoot;
                  importedEnv.wfRoot = targetDir + "\\" + importedEnv.wfRoot;

                  importedEnv.name = package.Description;

                  profile.environments.Add(importedEnv);
               }
            }
         }


         //Remove Progress bar
         stkpProgressBars.Children.Remove(progressBar);
         stkpProgressBars.Children.Remove(label);

      }

      private void PullPackage(MainWindow gui, string targetDir, Package package, System.Windows.Controls.ProgressBar progressBar)
      {

         int progressAmount = 0;
         foreach (PackageComponent component in package.Components)
         {
            progressAmount += component.Files.Count();
         }
         int currentProgress = 0;

         //Download
         System.Net.WebClient wc = new System.Net.WebClient();
         foreach (PackageComponent component in package.Components)
         {
            foreach (ComponentFile file in component.Files)
            {
               string convertedPath = file.Path.Replace("\\", "/");
               Directory.CreateDirectory(Path.GetDirectoryName(targetDir + "\\" + file.Path));
               string url = this.profile.settings.ewamUpdateServerURL + "/" +
                  package.Id + "/" +
                  convertedPath;
               wc.DownloadFile(
                  url,
                  targetDir + "\\" + file.Path);

               //Decompress the things if needed
               if (file.Compression != null && file.Compression == "zip")
               {
                  ZipFile.ExtractToDirectory(targetDir + "\\" + file.Path, targetDir);
                  File.Delete(targetDir + "\\" + file.Path);
               }

               currentProgress++;
               gui.Dispatcher.BeginInvoke(new Action(() => progressBar.Value = ((double)currentProgress / (double)progressAmount) * 100.0));
            }
         }

      }

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
