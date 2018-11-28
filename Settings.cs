using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   [DataContract(Name = "Settings", Namespace = "http://www.wyde.com")]
   public class Settings : ICloneable, INotifyPropertyChanged
   {
      // TODO : when setting or getting, transform "\n" seperated list to ";" seperated list, and vice versa.
      private string _exeSearchPathes;
      [Category("Environment import settings")]
      [DisplayName("Search pathes for eWam binaries")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string exeSearchPathes { get { return _exeSearchPathes; } set { _exeSearchPathes= value; NotifyPropertyChanged(); } }

      private string _dllSearchPathes;
      [Category("eWAM import settings")]
      [DisplayName("Search pathes for eWam dlls")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string dllSearchPathes { get { return _dllSearchPathes; } set { _dllSearchPathes = value; NotifyPropertyChanged(); } }

      private string _cppdllSearchPathes;
      [Category("eWAM import settings")]
      [DisplayName("Search pathes for CppDlls")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string cppdllSearchPathes { get { return _cppdllSearchPathes; } set { _cppdllSearchPathes = value; NotifyPropertyChanged(); } }

      private string _launcherSearchPathes;
      [Category("Environment import settings")]
      [DisplayName("Search pathes for launchers")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string launcherSearchPathes { get { return _launcherSearchPathes; } set { _launcherSearchPathes = value; NotifyPropertyChanged(); } }

      private string _batchSearchPathes;
      [Category("Environment import settings")]
      [DisplayName("Search pathes for batch files")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string batchSearchPathes { get { return _batchSearchPathes; } set { _batchSearchPathes = value; NotifyPropertyChanged(); } }

      private string _tgvSearchPathes;
      [Category("Environment import settings")]
      [DisplayName("Search pathes for TGV")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string tgvSearchPathes { get { return _tgvSearchPathes; } set { _tgvSearchPathes = value; NotifyPropertyChanged(); } }
      
      private string _ewamexes;
      [Category("Unused settings")]
      [DisplayName("eWAM common executables")]
      [Description("Comma seperated list of eWam executables. This settings is unused for now.")]
      [DataMember()] public string ewamexes { get { return _ewamexes; } set { _ewamexes = value; NotifyPropertyChanged(); } }

      private string _launcherUpdateServerURL;
      [Category("Update settings")]
      [DisplayName("Update URL")]
      [Description("URL to self update server (for eWamLauncher to update itself).")]
      [DataMember()] public string launcherUpdateServerURL { get { return _launcherUpdateServerURL; } set { _launcherUpdateServerURL = value; NotifyPropertyChanged(); } }

      private string _ewamUpdateServerURL;
      [Category("Package repository settings")]
      [DisplayName("Package server URL")]
      [Description("URL of the package repository containing the package-index.xml file.")]
      [DataMember()] public string ewamUpdateServerURL { get { return _ewamUpdateServerURL; } set { _ewamUpdateServerURL = value; NotifyPropertyChanged(); } }

      private bool _minimizeToTray;
      [Category("eWamLauncher settings")]
      [DisplayName("Minimize to systray")]
      [Description("When enabled, the application will minimize itself to systray. Double click on icon to restore.")]
      [DataMember()] public bool minimizeToTray { get { return _minimizeToTray; } set { _minimizeToTray = value; NotifyPropertyChanged(); } }

      //private string _ewamdlls;
      //[DataMember()] public string ewamdlls { get { return _ewamdlls; } set { _ewamdlls = value; NotifyPropertyChanged(); } }

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

      public object Clone()
      {
         Settings clone = (Settings)this.MemberwiseClone();

         return clone;
      }


      public Settings()
      {
         this.exeSearchPathes = @"bin;WydeServer\wsmServer;WydeServer\wwClient";
         this.dllSearchPathes = @"dll;dll.debug";
         this.cppdllSearchPathes = @"cppdll;cppdll.debug";
         this.launcherSearchPathes = @"bin;batch;batches;launchers;environments";
         this.batchSearchPathes = @"bin;batch;batches";
         this.tgvSearchPathes = @"tgv";

         this.ewamexes = "ewam.exe;ewamconsole.exe;wyseman.exe;wydeweb.exe;remoteewam.exe;wedrpcserver.exe;wsmadmin.exe;RemoteWAM.exe";

         this.launcherUpdateServerURL = @"http://regismt470p.wyde.paris.local/wLauncherUpdate/";
         this.ewamUpdateServerURL = @"http://regismt470p.wyde.paris.local/eWamUpdate/";

         this.minimizeToTray = true;
      }
   }
}
