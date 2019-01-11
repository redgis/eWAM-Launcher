using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Win32;

namespace eWamLauncher
{
   /// <summary>
   /// This class the information of an instance of Visual Studio installed : version name, path and pathes to vcvarsXX.bat
   /// </summary>
   [DataContract(Name = "VisualStudioDefinition", Namespace = "http://www.wyde.com")]
   [DisplayName("Visual Studio definition")]
   public class VisualStudioDefinition : INotifyPropertyChanged
   {
      private string _version;
      [DisplayName("Version")]
      [Description("Visual Studio technical version.")]
      [DataMember()] public string version { get { return _version; } set { _version = value; NotifyPropertyChanged(); } }

      private string _name;
      [DisplayName("Name")]
      [Description("Visual Studio Commercial Name.")]
      [DataMember()] public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      private string _basePath;
      [DisplayName("Visual Studio Path")]
      [Description("Visual Studio Path.")]
      [DataMember()] public string basePath { get { return _basePath; } set { _basePath = value; NotifyPropertyChanged(); } }

      private string _vcvarSubPath32;
      [DisplayName("vcvars32")]
      [Description("Sub path to vcvars.bat for 32bit compilation.")]
      [DataMember()] public string vcvarSubPath32 { get { return _vcvarSubPath32; } set { _vcvarSubPath32 = value; NotifyPropertyChanged(); } }

      private string _vcvarSubPath64;
      [DisplayName("vcvars64")]
      [Description("Sub path to vcvars.bat for 64bit compilation.")]
      [DataMember()] public string vcvarSubPath64 { get { return _vcvarSubPath64; } set { _vcvarSubPath64 = value; NotifyPropertyChanged(); } }
      
      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

   }
}
