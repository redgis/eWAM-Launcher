﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using log4net;
using System.Diagnostics;
using System.Xml.Serialization;

namespace eWamLauncher.Views
{

   /// <summary>
   /// Interaction logic for EnvironmentView.xaml
   /// View code-behind for an Environment (datacontext is a Environment object)
   /// </summary>
   public partial class EnvironmentView : System.Windows.Controls.UserControl
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(EnvironmentView));

      public static readonly RoutedUICommand ChangeTgvPath =
         new RoutedUICommand("Change TGV Path", "ChangeTgvPath", typeof(EnvironmentView));

      public static readonly RoutedUICommand ExploreTgvPath =
         new RoutedUICommand("Explore TGV Path", "ExploreTgvPath", typeof(EnvironmentView));



      public EnvironmentView()
      {
         InitializeComponent();
      }

      #region Path actions


      /// <summary>
      /// Command Handler to change TGV path of a TextBox in the UI
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnChangeTgvPath(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string tgvPath = 
               ((Environment)this.DataContext).envRoot + "\\" + ((Environment)this.DataContext).tgvSubPath;

            if (MainWindow.ChangePath(ref tgvPath))
            {
               if (((Environment)this.DataContext).envRoot !=
                  MainWindow.FindLongestCommonPath(tgvPath, ((Environment)this.DataContext).envRoot))
               {
                  throw new Exception("TGV path must be a sub folder of envRoot");
               }
               else
               {
                  ((Environment)this.DataContext).tgvSubPath = 
                     tgvPath.Substring(((Environment)this.DataContext).envRoot.Length + 1);
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

      private void OnChangeTgvPath_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      /// <summary>
      /// Command handler to navigate (i.e. open an explorer window), to a TGV path specified in the UI
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnExploreTgvPath(object sender, ExecutedRoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string tgvPath = ((Environment)this.DataContext).envRoot + "\\" + ((Environment)this.DataContext).tgvSubPath;
            MainWindow.ExplorePath(tgvPath);
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

      private void OnExploreTgvPath_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = true;
      }

      #endregion

      #region Environment variables actions

      /// <summary>
      /// Command handler to import environment variable from batch files from a given path
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnImportEnvVariables(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "Batch file|*.bat";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

               string envVarsPath = System.IO.Path.GetDirectoryName(fileBrowser.FileName);

               EnvironmentImporter importer =
                  new EnvironmentImporter(
                     ((MainWindow)System.Windows.Application.Current.MainWindow).profile,
                     (Environment)this.DataContext);
               importer.ImportEnvironmentVariables(envVarsPath);
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
      /// Command handler to force evaluation of all environment variable of the environment
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnReevaluateEnvVariables(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            ((Environment)this.DataContext).ExpandAllEnvVariables();
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
      /// Command handler to force evaluation of all environment variable of the environment
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnReevaluateEnvVariables(object sender, EventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            ((Environment)this.DataContext).ExpandAllEnvVariables();
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
      /// Command handler to move up selected variable in the list of env. variables
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpVariable(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (DataContext == null || dgVarList.SelectedItem == null)
            {
               return;
            }

            int varIndex = ((Environment)this.DataContext).environmentVariables.IndexOf(
               (EnvironmentVariable)dgVarList.SelectedItem);

            if (varIndex > 0)
            {
               ((Environment)this.DataContext).environmentVariables.Move(varIndex, varIndex - 1);
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
      /// Command handler to move down selected variable in the list of env. variables
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownVariable(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (this.DataContext == null || dgVarList.SelectedItem == null)
            {
               return;
            }

            int varIndex = ((Environment)this.DataContext).environmentVariables.IndexOf(
               (EnvironmentVariable)dgVarList.SelectedItem);

            if (varIndex < ((Environment)this.DataContext).environmentVariables.Count() - 1)
            {
               ((Environment)this.DataContext).environmentVariables.Move(varIndex, varIndex + 1);
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

      #region Launchers actions


      /// <summary>
      /// Creates and adds a new empty launcher in the launchers list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnNewLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Launcher launcher = new Launcher();
            launcher.name = "new launcher " + ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Count.ToString();
            ((ObservableCollection<Launcher>)lbLauncherList.ItemsSource).Add(launcher);
            lbLauncherList.SelectedItem = launcher;
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
      /// Duplicates the selected launcher and adds the duplicate to the launchers list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDuplicateLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
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
      /// Delete selected launcher
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeleteLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
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
      /// Imports existing launchers from batch files from a local folder
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnImportLaunchers(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "Batch file|*.bat";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

               string launchersPath = System.IO.Path.GetDirectoryName(fileBrowser.FileName);

               EnvironmentImporter importer = 
                  new EnvironmentImporter(
                     ((MainWindow)System.Windows.Application.Current.MainWindow).profile, 
                     (Environment)this.DataContext);
               importer.ImportLaunchers(launchersPath);
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
      /// Internal method to set a given env. variables in a ProcessStartInfo (used to launch 
      /// a process). This methods replace any preexisting env. var. with the new value.
      /// </summary>
      /// <param name="startInfo"></param>
      /// <param name="VarName"></param>
      /// <param name="VarValue"></param>
      private void SetEnvVarToProcessInfo(ProcessStartInfo startInfo, 
         string VarName, string VarValue)
      {
         if (startInfo.EnvironmentVariables.ContainsKey(VarName))
         {
            startInfo.EnvironmentVariables[VarName] = VarValue;
         }
         else
         {
            startInfo.EnvironmentVariables.Add(VarName, VarValue);
         }
      }

      /// <summary>
      /// Internal method to set a given env. variables in a ProcessStartInfo (used to launch 
      /// a process). This methods appends any preexisting env. var. with the new value.
      /// </summary>
      /// <param name="startInfo"></param>
      /// <param name="VarName"></param>
      /// <param name="VarValue"></param>
      private void AppendEnvVarToProcessInfo(ProcessStartInfo startInfo,
         string VarName, string VarValue)
      {
         if (startInfo.EnvironmentVariables.ContainsKey(VarName))
         {
            startInfo.EnvironmentVariables[VarName] += VarValue;
         }
         else
         {
            startInfo.EnvironmentVariables.Add(VarName, VarValue);
         }
      }

      /// <summary>
      /// Internal method to prepare a process to be ready to start, within the context 
      /// of this environment (using its env. variable mainly).
      /// </summary>
      /// <param name="launcher"></param>
      /// <returns></returns>
      private ProcessStartInfo GetPreparedProcessInfo(Launcher launcher)
      {
         // Before anything, make sure all env. variable are up to date !
         Environment environment = (Environment)this.DataContext;
         environment.ExpandAllEnvVariables();

         if (environment.binariesSet == null)
         {
            throw new Exception("No eWAM binaries selected");
         }

         ProcessStartInfo startInfo = new ProcessStartInfo();

         char[] delimiters = { '\n', ';', '\r', '\b' };

         // Set %PATH%
         List<string> binSubPaths = new List<string>();
         binSubPaths.AddRange(environment.binariesSet.dllPathes.Split(delimiters));
         binSubPaths.AddRange(environment.binariesSet.cppdllPathes.Split(delimiters));
         binSubPaths.AddRange(environment.binariesSet.exePathes.Split(delimiters));

         string pathVariable = "";
         foreach (string subBinPath in binSubPaths)
         {
            if (subBinPath != "")
            {
               pathVariable += environment.ewam.basePath + "\\" + subBinPath + ";";
            }
         }

         if (((Environment)this.DataContext).GetEnvironmentVariable("PATH") != null)
         {
            pathVariable += ((Environment)this.DataContext).additionalPath + ";";
         }

         pathVariable += System.Environment.GetEnvironmentVariable("PATH");
         SetEnvVarToProcessInfo(startInfo, "PATH", pathVariable);

         // Put CppDll Folders in WYDE-DLL
         string[] cppdlls = environment.binariesSet.cppdllPathes.Split(delimiters);
         SetEnvVarToProcessInfo(startInfo, "WYDE-DLL", environment.ewam.basePath + "\\" + cppdlls[0]);

         // TODO : to use when WYDE-DLL support ';' seperated list of paths.
         //string cppdlls = launcher.binariesSet.cppdllPaths.Replace('\n', ';');
         //startInfo.EnvironmentVariables.Add("WYDE-DLL", cppdlls);
         //startInfo.EnvironmentVariables.Add("WYDE-ROOT", ((Environment)this.DataContext).ewam.basePath);

         SetEnvVarToProcessInfo(startInfo, "WYDE-ROOT",
            ((Environment)this.DataContext).GetEnvironmentVariable("WYDE-ROOT").value);
         SetEnvVarToProcessInfo(startInfo, "WF-ROOT",
            ((Environment)this.DataContext).GetEnvironmentVariable("WF-ROOT").value);
         SetEnvVarToProcessInfo(startInfo, "ENV-ROOT",
            ((Environment)this.DataContext).GetEnvironmentVariable("ENV-ROOT").value);
         SetEnvVarToProcessInfo(startInfo, "WYDE-TGV",
            ((Environment)this.DataContext).GetEnvironmentVariable("WYDE-TGV").value);

         // Set all other environment variables
         foreach (EnvironmentVariable variable in
            ((Environment)this.DataContext).environmentVariables)
         {
            AppendEnvVarToProcessInfo(startInfo, variable.name, variable.result);
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

         return startInfo;
      }

      /// <summary>
      /// Start the selected launcher from a console, using VS context and environment variables
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnConsoleExecuteLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment env = (Environment)this.DataContext;

            Launcher launcher = (Launcher)lbLauncherList.SelectedItem;
            Launcher dummyLauncher = new Launcher();

            dummyLauncher.name = launcher.name;
            dummyLauncher.program = "cmd.exe";

            if (env.useVS)
            {
               string vsCmd = "";

               switch (env.VsPlateform)
               {
                  case eVsPlateform.x86:
                     vsCmd = Path.Combine(env.associatedVS.basePath, env.associatedVS.vcvarSubPath32);
                     break;
                  case eVsPlateform.x64:
                     vsCmd = Path.Combine(env.associatedVS.basePath, env.associatedVS.vcvarSubPath64);
                     break;
                  default:
                     break;
               }

               dummyLauncher.arguments = "/K \"\"" + vsCmd + "\" && " + launcher.program + " " +
                  ((Environment)this.DataContext).ExpandString(launcher.arguments) + "\"";
            }
            else
            {
               dummyLauncher.arguments = "/K \"" + launcher.program + " " +
                  ((Environment)this.DataContext).ExpandString(launcher.arguments) + "\"";
            }

            ProcessStartInfo startInfo = this.GetPreparedProcessInfo(dummyLauncher);
            startInfo.WorkingDirectory = env.envRoot;

            Process newProcess = Process.Start(startInfo);
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
      /// Start the selected launcher
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnExecuteLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Launcher launcher = (Launcher)lbLauncherList.SelectedItem;
            ProcessStartInfo startInfo = this.GetPreparedProcessInfo(launcher);
            Process newProcess = Process.Start(startInfo);
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
      /// Unimplemented event handler for process exit
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnProcessExited(object sender, EventArgs e)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Move selected launcher up in the launchers list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbLauncherList.SelectedItem == null)
            {
               return;
            }

            int launcherIndex = ((Environment)this.DataContext).launchers.IndexOf(
               (Launcher)lbLauncherList.SelectedItem);

            if (launcherIndex > 0)
            {
               ((Environment)this.DataContext).launchers.Move(launcherIndex, launcherIndex - 1);
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
      /// Move selected launcher down in the launchers list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownLauncher(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbLauncherList.SelectedItem == null)
            {
               return;
            }

            int launcherIndex = ((Environment)this.DataContext).launchers.IndexOf(
               (Launcher)lbLauncherList.SelectedItem);

            if (launcherIndex < ((Environment)this.DataContext).launchers.Count() - 1)
            {
               ((Environment)this.DataContext).launchers.Move(launcherIndex, launcherIndex + 1);
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

      /// <summary>
      /// Export all launchers to batch files, in the specified folder, including the
      /// eWAM Set Env.bat to set the environment variables
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportAllLaunchers(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;


            char[] delimiters = { '\n', ';', '\r', '\b' };

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select output folder";
            folderBrowser.SelectedPath = environment.envRoot;

            if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
               return;
            }

            environment.GenerateBatchFiles(folderBrowser.SelectedPath);
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
      /// Open a console for this environment, with envirionment and VS context set up
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnOpenEnvironmentConsole(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;

            Launcher dummyLauncher = new Launcher();

            dummyLauncher.name = environment.name;
            dummyLauncher.program = "cmd.exe";
            dummyLauncher.arguments = "";

            if (environment.useVS)
            {
               string vsCmd = "";

               switch (environment.VsPlateform)
               {
                  case eVsPlateform.x86:
                     vsCmd = Path.Combine(environment.associatedVS.basePath, environment.associatedVS.vcvarSubPath32);
                     break;
                  case eVsPlateform.x64:
                     vsCmd = Path.Combine(environment.associatedVS.basePath, environment.associatedVS.vcvarSubPath64);
                     break;
                  default:
                     break;
               }

               dummyLauncher.arguments = "/K \"\"" + vsCmd + "\"\"";
            }

            ProcessStartInfo startInfo = this.GetPreparedProcessInfo(dummyLauncher);
            startInfo.WorkingDirectory = environment.envRoot;

            Process newProcess = Process.Start(startInfo);
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

      #region WydeWeb services actions

      /// <summary>
      /// Creates and adds a new empty WydeWeb service in the services list in the 
      /// WydeWeb configuration of the environment
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnNewWWService(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            WWService service = new WWService();
            service.Name = "new service " + ((ObservableCollection<WWService>)lbServices.ItemsSource).Count.ToString();
            ((ObservableCollection<WWService>)lbServices.ItemsSource).Add(service);
            lbServices.SelectedItem = service;
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
      /// Duplicates the selected WydeWeb service and adds the duplicate to the services list in the 
      /// WydeWeb configuration of the environment
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDuplicateWWService(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbServices.SelectedItem == null)
            {
               return;
            }

            WWService service = (WWService)((WWService)lbServices.SelectedItem).Clone();
            service.Name += " (clone)";
            ((ObservableCollection<WWService>)lbServices.ItemsSource).Add(service);
            lbServices.SelectedItem = service;
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
      /// Delete selected WydeWeb service
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeleteWWService(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            int curSelection = lbServices.SelectedIndex;
            if (lbServices.SelectedItem == null)
            {
               return;
            }

         ((ObservableCollection<WWService>)lbServices.ItemsSource).Remove((WWService)lbServices.SelectedItem);
            lbServices.SelectedIndex = curSelection;
            if (lbServices.SelectedIndex == -1)
               lbServices.SelectedIndex = curSelection - 1;
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
      /// Move selected WydeWeb service up in the services list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveUpWWService(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbServices.SelectedItem == null)
            {
               return;
            }

            int serviceIndex = ((Environment)this.DataContext).wNetConf.services.IndexOf(
               (WWService)lbServices.SelectedItem);

            if (serviceIndex > 0)
            {
               ((Environment)this.DataContext).wNetConf.services.Move(serviceIndex, serviceIndex - 1);
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
      /// Move selected WydeWeb service down in the services list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnMoveDownWWService(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (lbServices.SelectedItem == null)
            {
               return;
            }

            int serviceIndex = ((Environment)this.DataContext).wNetConf.services.IndexOf(
               (WWService)lbServices.SelectedItem);

            if (serviceIndex < ((Environment)this.DataContext).wNetConf.services.Count() - 1)
            {
               ((Environment)this.DataContext).wNetConf.services.Move(serviceIndex, serviceIndex + 1);
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

      #region WydeWeb actions

      /// <summary>
      /// Load WydeWeb configuration from wNetConf.ini file
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public WydeNetWorkConfiguration LoadWNetConf(string fileName)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            FileStream reader = new FileStream(fileName, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
            WydeNetWorkConfiguration wNetConf = (WydeNetWorkConfiguration)serializer.Deserialize(reader);
            reader.Close();

            return wNetConf;
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }

         return null;
      }

      /// <summary>
      /// Extract a WNetConf XML content (ready for storing in a wNetConf.ini or wNetConf.xml, or a
      /// WydeWebAsAuto.html) from WydeWeb configurationof the environment
      /// </summary>
      /// <param name="outputType">Specifies for what purpose this WNetConf is to be built : server side, client side, WSMISAPI, full configuration, client side single service</param>
      /// <param name="service">If single service requested, the service for which to generate the client side WNetConf</param>
      /// <returns></returns>
      private string GetWNetConf(WNetConf.eNetConf outputType, WWService service)
      {
         Environment environment = (Environment)this.DataContext;
         if (environment == null) throw new Exception("Select a valid environment first.");
         if (environment.wNetConf == null) throw new Exception("This environment doesn't have a WydeWeb configuration.");

         WydeNetWorkConfiguration wNetConf = environment.wNetConf.GetWydeNetConf(outputType, service);

         StringWriter writer = new StringWriter();
         XmlSerializer xmlSerializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
         XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
         namespaces.Add("", "");
         xmlSerializer.Serialize(writer, wNetConf, namespaces);

         return writer.ToString();
      }

      /// <summary>
      /// Import environment WydeWeb configuration from wNetConf.ini file
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileImportWNetConfIni(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (System.Windows.MessageBox.Show(
               "This will erase any WydeWeb configuration you have for this environment !\n" +
               "\n\n    Are you sure ?",
               "Overwrite current WydeWeb configuration ?",
               System.Windows.MessageBoxButton.YesNo,
               System.Windows.MessageBoxImage.Question) != MessageBoxResult.Yes)
               return;

            Environment environment = (Environment)this.DataContext;
            if (environment == null) throw new Exception("Select a valid environment first.");

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "WNetConf file|*.ini|Any file|*.*";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Export";

            environment.ExpandAllEnvVariables();
            EnvironmentVariable wNetConfVar = environment.GetEnvironmentVariable("WYDE-NETCONF");
            if (wNetConfVar != null)
            {
               fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(wNetConfVar.result);
               fileBrowser.FileName = "wnetconf.ini";
            }

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               if (System.IO.Path.GetFileName(fileBrowser.FileName).ToLower() == "wnetconf.ini")
               {
                  WydeNetWorkConfiguration wNetConf = LoadWNetConf(fileBrowser.FileName);

                  environment.wNetConf = new WNetConf(wNetConf);
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
      /// Export WydeWeb configuration to wNetConf.ini : only client side configuration
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportWNetConfIni_Client(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;
            if (environment == null) throw new Exception("Select a valid environment first.");
            if (environment.wNetConf == null) throw new Exception("This environment doesn't have a WydeWeb configuration.");

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "WNetConf file|wNetConf.ini";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Export";

            environment.ExpandAllEnvVariables();
            fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(
               environment.GetEnvironmentVariable("WYDE-NETCONF").result);
            fileBrowser.FileName = "wnetconf.ini";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileBrowser.FileName));

               string output = GetWNetConf(WNetConf.eNetConf.Client, null);
               File.WriteAllText(fileBrowser.FileName, output);

               //WydeNetWorkConfiguration wNetConf = environment.wNetConf.GetWydeNetConf(WNetConf.eNetConf.Client, null);
               //FileStream writer = new FileStream(fileBrowser.FileName, FileMode.Create);
               //XmlSerializer xmlSerializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
               //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
               //namespaces.Add("", "");
               //xmlSerializer.Serialize(writer, wNetConf, namespaces);
               //writer.Close();
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
      /// Export WydeWeb configuration to wNetConf.ini : only IIS WSMISAPI configuration
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportWNetConfIni_WSMISAPI(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;
            if (environment == null) throw new Exception("Select a valid environment first.");
            if (environment.wNetConf == null) throw new Exception("This environment doesn't have a WydeWeb configuration.");

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "WNetConf file|wNetConf.ini";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Export";

            environment.ExpandAllEnvVariables();
            fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(
               environment.GetEnvironmentVariable("WYDE-NETCONF").result);
            fileBrowser.FileName = "wnetconf.ini";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

               Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileBrowser.FileName));

               string output = GetWNetConf(WNetConf.eNetConf.WSMISAPI, null);
               File.WriteAllText(fileBrowser.FileName, output);

               //WydeNetWorkConfiguration wNetConf = environment.wNetConf.GetWydeNetConf(WNetConf.eNetConf.WSMISAPI, null);
               //FileStream writer = new FileStream(fileBrowser.FileName, FileMode.Create);
               //XmlSerializer xmlSerializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
               //xmlSerializer.Serialize(writer, wNetConf);
               //writer.Close();
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
      /// Export WydeWeb configuration to wNetConf.ini : only server side configuration
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportWNetConfIni_Server(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;
            if (environment == null) throw new Exception("Select a valid environment first.");
            if (environment.wNetConf == null) throw new Exception("This environment doesn't have a WydeWeb configuration.");

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "WNetConf file|wNetConf.ini";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Export";

            environment.ExpandAllEnvVariables();
            fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(
               environment.GetEnvironmentVariable("WYDE-NETCONF").result);
            fileBrowser.FileName = "wnetconf.ini";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileBrowser.FileName));

               string output = GetWNetConf(WNetConf.eNetConf.Server, null);
               File.WriteAllText(fileBrowser.FileName, output);

               //WydeNetWorkConfiguration wNetConf = environment.wNetConf.GetWydeNetConf(WNetConf.eNetConf.Server, null);
               //FileStream writer = new FileStream(fileBrowser.FileName, FileMode.Create);
               //XmlSerializer xmlSerializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
               //xmlSerializer.Serialize(writer, wNetConf);
               //writer.Close();
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
      /// Export WydeWeb configuration to wNetConf.ini : full (client and server side) configuration
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnFileExportWNetConfIni_Full(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            Environment environment = (Environment)this.DataContext;
            if (environment == null) throw new Exception("Select a valid environment first.");
            if (environment.wNetConf == null) throw new Exception("This environment doesn't have a WydeWeb configuration.");

            OpenFileDialog fileBrowser = new OpenFileDialog();

            fileBrowser.Filter = "WNetConf file|wNetConf.ini";
            fileBrowser.FilterIndex = 1;
            fileBrowser.RestoreDirectory = true;
            fileBrowser.CheckFileExists = false;
            fileBrowser.Title = "Export";

            environment.ExpandAllEnvVariables();
            fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(
               environment.GetEnvironmentVariable("WYDE-NETCONF").result);
            fileBrowser.FileName = "wnetconf.ini";

            if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
               Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileBrowser.FileName));

               string output = GetWNetConf(WNetConf.eNetConf.Full, null);
               File.WriteAllText(fileBrowser.FileName, output);

               //WydeNetWorkConfiguration wNetConf = environment.wNetConf.GetWydeNetConf(WNetConf.eNetConf.Full, null);
               //FileStream writer = new FileStream(fileBrowser.FileName, FileMode.Create);
               //XmlSerializer xmlSerializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
               //xmlSerializer.Serialize(writer, wNetConf);
               //writer.Close();
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
      /// Launche WydeWeb deployment wizard
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void OnDeployWydeWeb(object sender, RoutedEventArgs e)
      {
         WydeWebDeployWizard deployWizard = new WydeWebDeployWizard((Environment)this.DataContext);
         bool? wizardResult = deployWizard.ShowDialog();
      }

      #endregion

   }
}
