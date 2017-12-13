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

namespace eWamLauncher
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   [DataContract(Name = "eWamLauncher", Namespace = "http://www.wyde.com")]
   public partial class MainWindow : MetroWindow, INotifyPropertyChanged
   {
      [DataMember()] private ObservableCollection<wEnvironment> _environments;
      public ObservableCollection<wEnvironment> environments { get { return _environments; } set { _environments = value;  this.NotifyPropertyChanged(); } }

      [DataMember()] public string Settings { get; set; }


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
         Version version = Assembly.GetEntryAssembly().GetName().Version;
         Assembly curAssembly = Assembly.GetEntryAssembly();

         this.environments = new ObservableCollection<wEnvironment>();

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
            this.environments =
                (ObservableCollection<wEnvironment>)xmlDeserializer.ReadObject(reader);
            reader.Close();
         }
         catch (FileNotFoundException)
         {
         }

      }

      public void SaveToXML(string fileName)
      {
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<wEnvironment>));
         xmlSerializer.WriteObject(writer, this.environments);
         writer.Close();
      }

      public void LoadFromJSON(string fileName)
      {

      }

      public void SaveToJSON(string fileName)
      {
         FileStream writer = new FileStream(fileName, FileMode.Create);
         DataContractJsonSerializer jsonSerializer =
             new DataContractJsonSerializer(typeof(ObservableCollection<wEnvironment>));
         jsonSerializer.WriteObject(writer, this.environments);
         writer.Close();
      }


      #region Environments actions

      public void OnNewEnvironment(object sender, RoutedEventArgs e)
      {
         wEnvironment environment = new wEnvironment();
         environment.name = "eWAM " + this.environments.Count().ToString();
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
         this.environments.Add(environment);
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
      }

      public void OnImportEnvironment(object sender, RoutedEventArgs e)
      {
         if (this.environments == null)
         {
            this.environments = new ObservableCollection<wEnvironment>();
         }

         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "TGV Base1|W001001.tgv|Any TGV|*.tgv";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            IEwamImporter importer = new LegacyEwamImporter();

            // Get Parent of Tgv/ folder containing W001001.TGV
            string envPath = Path.GetDirectoryName(Path.GetDirectoryName(fileBrowser.FileName));
            wEnvironment environment = importer.ImportFromPath(envPath);
            ((ObservableCollection<wEnvironment>)lbEnvList.ItemsSource).Add(environment);
            lbEnvList.SelectedItem = environment;
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
      }

      public void OnImportBinariesSets(object sender, RoutedEventArgs e)
      {
         OpenFileDialog fileBrowser = new OpenFileDialog();

         fileBrowser.Filter = "Any file|*.exe;*.dll";
         fileBrowser.FilterIndex = 1;
         fileBrowser.RestoreDirectory = true;

         if (fileBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {

            string envPath = Path.GetDirectoryName(Path.GetDirectoryName(fileBrowser.FileName));

            IEwamImporter importer = new LegacyEwamImporter((wEnvironment)lbEnvList.SelectedItem);
            importer.ImportBinaries(envPath);
         }
      }

      #endregion


      #region launchers actions


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

            string envPath = Path.GetDirectoryName(fileBrowser.FileName);

            IEwamImporter importer = new LegacyEwamImporter((wEnvironment)lbEnvList.SelectedItem);
            importer.ImportLaunchers(envPath);
         }
      }

      #endregion

   }
}
