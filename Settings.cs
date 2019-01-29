using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace eWamLauncher
{
   /// <summary>
   /// This class contains the application settings with there default values.
   /// The member attributes "DisplayName", "Category" and "Description" are used by propertygrids : 
   /// https://github.com/xceedsoftware/wpftoolkit/wiki/PropertyGrid
   /// </summary>
   [DataContract(Name = "Settings", Namespace = "http://www.wyde.com")]
   public class Settings : ICloneable, INotifyPropertyChanged
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(EwamImporter));

      // TODO : when setting or getting, transform "\n" seperated list to ";" seperated list, and vice versa.
      private string _exeSearchPathes = @"bin;bin64;WydeServer\wsmServer;WydeServer\wwClient";
      [Category("eWAM import settings")]
      [DisplayName("Search pathes for eWam binaries")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string exeSearchPathes { get { return _exeSearchPathes; } set { _exeSearchPathes= value; NotifyPropertyChanged(); } }

      private string _dllSearchPathes = @"dll;dll.debug;EjbDll;dll64;dll64.debug;EjbDll64";
      [Category("eWAM import settings")]
      [DisplayName("Search pathes for eWam dlls")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string dllSearchPathes { get { return _dllSearchPathes; } set { _dllSearchPathes = value; NotifyPropertyChanged(); } }

      private string _cppdllSearchPathes = @"cppdll;cppdll.debug;cppdll64;cppdll64.debug";
      [Category("eWAM import settings")]
      [DisplayName("Search pathes for CppDlls")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string cppdllSearchPathes { get { return _cppdllSearchPathes; } set { _cppdllSearchPathes = value; NotifyPropertyChanged(); } }

      private string _launcherSearchPathes = @"bin;batch;batches;launchers;environments";
      [Category("Environment import settings")]
      [DisplayName("Search pathes for launchers")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string launcherSearchPathes { get { return _launcherSearchPathes; } set { _launcherSearchPathes = value; NotifyPropertyChanged(); } }

      private string _batchSearchPathes = @"bin;batch;batches";
      [Category("Environment import settings")]
      [DisplayName("Search pathes for batch files")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string batchSearchPathes { get { return _batchSearchPathes; } set { _batchSearchPathes = value; NotifyPropertyChanged(); } }

      private string _tgvSearchPathes = @"tgv";
      [Category("Environment import settings")]
      [DisplayName("Search pathes for TGV")]
      [Description("Comma seperated list of sub folder to be looked into.")]
      [DataMember()] public string tgvSearchPathes { get { return _tgvSearchPathes; } set { _tgvSearchPathes = value; NotifyPropertyChanged(); } }
      
      private string _ewamexes = "ewam.exe;ewamconsole.exe;wyseman.exe;wydeweb.exe;remoteewam.exe;wedrpcserver.exe;wsmadmin.exe;RemoteWAM.exe";
      [Category("Unused settings")]
      [DisplayName("eWAM common executables")]
      [Description("Comma seperated list of eWam executables. This settings is unused for now.")]
      [DataMember()] public string ewamexes { get { return _ewamexes; } set { _ewamexes = value; NotifyPropertyChanged(); } }

      private string _launcherUpdateServerURL = @"http://formationt430s3.wyde.paris.local/wLauncherUpdate/";
      [Category("Update settings")]
      [DisplayName("Update URL")]
      [Description("URL to self update server (for eWamLauncher to update itself).")]
      [DataMember()] public string launcherUpdateServerURL { get { return _launcherUpdateServerURL; } set { _launcherUpdateServerURL = value; NotifyPropertyChanged(); } }

      private string _ewamUpdateServerURL = @"http://formationt430s3.wyde.paris.local/eWamUpdate/";
      [Category("Package repository settings")]
      [DisplayName("Package server URL")]
      [Description("URL of the package repository containing the package-index.xml file.")]
      [DataMember()] public string ewamUpdateServerURL { get { return _ewamUpdateServerURL; } set { _ewamUpdateServerURL = value; NotifyPropertyChanged(); } }

      private bool _minimizeToTray = true;
      [Category("eWamLauncher settings")]
      [DisplayName("Minimize to systray")]
      [Description("When enabled, the application will minimize itself to systray. Double click on icon to restore.")]
      [DataMember()] public bool minimizeToTray { get { return _minimizeToTray; } set { _minimizeToTray = value; NotifyPropertyChanged(); } }

      private int _numberOfBackups = 5;
      [Category("eWamLauncher settings")]
      [DisplayName("Number of backups")]
      [Description("Keep only X last backups of settings history.")]
      [DataMember()] public int numberOfBackups { get { return _numberOfBackups; } set { _numberOfBackups = value; NotifyPropertyChanged(); } }

      private List<VisualStudioDefinition> _visualStudios = new List<VisualStudioDefinition>();
      [Category("Visual Studio settings")]
      [DisplayName("Visual Studios")]
      [Description("List of Visual Studio installed on your system.")]
      [DataMember()] public List<VisualStudioDefinition> visualStudios { get { return _visualStudios; } set { _visualStudios = value; NotifyPropertyChanged(); } }
      

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public object Clone()
      {
         Settings clone = (Settings)this.MemberwiseClone();

         return clone;
      }

      /// <summary>
      /// List of Visual Studio versions. To be used when inventoring available VS on the 
      /// system (see AutoDetectVisualStudios())
      /// </summary>
      private static Dictionary<string, string> vsNames = new Dictionary<string, string>()
      {
         { "6.0", "6.0" },
         { "9.0", "2008" },
         { "10.0", "2010" },
         { "11.0", "2012" },
         { "12.0", "2013" },
         { "14.0", "2015" },
         { "15.0", "2017" },
         { "16.0", "2019" }
      };

      public Settings()
      {
         this.exeSearchPathes = @"bin;bin64;WydeServer\wsmServer;WydeServer\wwClient";
         this.dllSearchPathes = @"dll;dll.debug;EjbDll;dll64;dll64.debug;EjbDll64";
         this.cppdllSearchPathes = @"cppdll;cppdll.debug;cppdll64;cppdll64.debug";
         this.launcherSearchPathes = @"bin;batch;batches;launchers;environments";
         this.batchSearchPathes = @"bin;batch;batches";
         this.tgvSearchPathes = @"tgv";

         this.ewamexes = "ewam.exe;ewamconsole.exe;wyseman.exe;wydeweb.exe;remoteewam.exe;wedrpcserver.exe;wsmadmin.exe;RemoteWAM.exe";

         this.launcherUpdateServerURL = @"http://formationt430s3.wyde.paris.local/wLauncherUpdate/";
         this.ewamUpdateServerURL = @"http://formationt430s3.wyde.paris.local/eWamUpdate/";

         this.minimizeToTray = true;
         this.numberOfBackups = 5;

         this.visualStudios = new List<VisualStudioDefinition>();
         AutoDetectVisualStudios();
      }

      /// <summary>
      /// Look for Visual Studio versions installed on the system
      /// </summary>
      public void AutoDetectVisualStudios()
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {

            this.visualStudios.Clear();

            // All Visual Studio versions are registered under this registry key
            RegistryKey VSVersionPathsKey =
               Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7");

            // for each entry found (each entry should be a different visual studio instance
            foreach (string valueName in VSVersionPathsKey.GetValueNames())
            {
               string vsPath = (string)Registry.GetValue(VSVersionPathsKey.Name, valueName, "");

               if (vsPath != null && vsPath != "")
               {
                  // create the object that will hold all the information of this VS instance
                  VisualStudioDefinition vsDef =
                     new VisualStudioDefinition() { basePath = vsPath, version = valueName, name = vsNames[valueName] };

                  // Look for vcvars32.bat
                  List<String> vcvars32 =
                     StripPrefix(vsDef.basePath, Directory.GetFiles(vsDef.basePath, "vcvars32.bat", SearchOption.AllDirectories));

                  // Look for vcvarsamd64.bat or vcvars64.bat
                  List<String> vcvars64 =
                     StripPrefix(vsDef.basePath, Directory.GetFiles(vsDef.basePath, "vcvarsamd64.bat", SearchOption.AllDirectories));
                  if (vcvars64.Count() == 0)
                  {
                     vcvars64 =
                        StripPrefix(vsDef.basePath, Directory.GetFiles(vsDef.basePath, "vcvars64.bat", SearchOption.AllDirectories));
                  }

                  if (vcvars32.Count() != 0)
                     vsDef.vcvarSubPath32 = vcvars32[0];

                  if (vcvars64.Count() != 0)
                     vsDef.vcvarSubPath64 = vcvars64[0];

                  this.visualStudios.Add(vsDef);
               }
            }
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }

      /// <summary>
      /// Removes a prefix path from larger pathes (used to retrieve only the remaining subpath)
      /// </summary>
      /// <param name="prefix">the prefix to be remove</param>
      /// <param name="files">the list of files from which the prefix must be removed</param>
      /// <returns></returns>
      static List<string> StripPrefix(string prefix, string[] files)
      {
         List<string> result = new List<string>();

         //normalize and remove trailing '/' or '\' from prefix
         string normalizePrefix = Path.GetFullPath(new Uri(prefix).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

         foreach (string file in files)
         {
            //normalize and remove trailing '/' or '\'
            string normalizedFile = Path.GetFullPath(new Uri(file).LocalPath)
               .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);


            if (file.StartsWith(normalizePrefix))
            {
               result.Add(normalizedFile.Substring(normalizePrefix.Length + 1));
            }
         }

         return result;
      }

   }
}
