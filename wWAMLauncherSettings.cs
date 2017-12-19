using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   [DataContract(Name = "wWAMLauncherSettings", Namespace = "http://www.wyde.com")]
   public class wWAMLauncherSettings : ICloneable, INotifyPropertyChanged
   {
      // TODO : when setting or getting, transform "\n" seperated list to ";" seperated list, and vice versa.
      [DataMember()] private string _exeSearchPathes;
      public string exeSearchPathes { get { return _exeSearchPathes; } set { _exeSearchPathes= value; NotifyPropertyChanged(); } }
      [DataMember()] private string _dllSearchPathes;
      public string dllSearchPathes { get { return _dllSearchPathes; } set { _dllSearchPathes = value; NotifyPropertyChanged(); } }
      [DataMember()] private string _cppdllSearchPathes;
      public string cppdllSearchPathes { get { return _cppdllSearchPathes; } set { _cppdllSearchPathes = value; NotifyPropertyChanged(); } }
      [DataMember()] private string _launcherSearchPathes;
      public string launcherSearchPathes { get { return _launcherSearchPathes; } set { _launcherSearchPathes = value; NotifyPropertyChanged(); } }
      [DataMember()] private string _batchSearchPathes;
      public string batchSearchPathes { get { return _batchSearchPathes; } set { _batchSearchPathes = value; NotifyPropertyChanged(); } }
      [DataMember()] private string _tgvSearchPathes;
      public string tgvSearchPathes { get { return _tgvSearchPathes; } set { _tgvSearchPathes = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _launcherUpdateServerURL;
      public string launcherUpdateServerURL { get { return _launcherUpdateServerURL; } set { _launcherUpdateServerURL = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _ewamUpdateServerURL;
      public string ewamUpdateServerURL { get { return _ewamUpdateServerURL; } set { _ewamUpdateServerURL = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _ewamexes;
      public string ewamexes { get { return _ewamexes; } set { _ewamexes = value; NotifyPropertyChanged(); } }


      //[DataMember()] private string _ewamdlls;
      //public string ewamdlls { get { return _ewamdlls; } set { _ewamdlls = value; NotifyPropertyChanged(); } }

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
         wWAMLauncherSettings clone = (wWAMLauncherSettings)this.MemberwiseClone();

         return clone;
      }


      public wWAMLauncherSettings()
      {
         this.exeSearchPathes = @"bin";
         this.dllSearchPathes = @"dll;dll.debug";
         this.cppdllSearchPathes = @"cppdll;cppdll.debug";
         this.launcherSearchPathes = @"bin";
         this.batchSearchPathes = @"batches";
         this.tgvSearchPathes = @"tgv";

         this.ewamexes = "ewam.exe;ewamconsole.exe;wyseman.exe;wydeweb.exe;remoteewam.exe;wedrpcserver.exe;wsmadmin.exe;RemoteWAM.exe";

         this.launcherUpdateServerURL = @"http://regismt440p.wyde.paris.local/wLauncherUpdate/";
         this.ewamUpdateServerURL = @"http://regismt440p.wyde.paris.local/eWamUpdate/";
      }
   }
}
