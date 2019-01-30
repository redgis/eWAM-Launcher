using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Windows;
using System.ComponentModel;

namespace eWamLauncher
{
   /// <summary>
   /// Set of binaries paths to be part of an eWAM instance. a Set of binaries includes a path
   /// to the .exe files, a path to dlls, and a path to cppdlls.
   /// </summary>
   [DataContract(Name = "BinariesSet", Namespace = "http://www.wyde.com")]
   public class BinariesSet : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      private string _exePathes;
      /// <summary>
      /// Paths to executables
      /// </summary>
      [DataMember()] public string exePathes { get { return _exePathes; } set { _exePathes = value;  NotifyPropertyChanged(); } }

      private string _dllPathes;
      /// <summary>
      /// Paths to Dlls
      /// </summary>
      [DataMember()] public string dllPathes { get { return _dllPathes; } set { _dllPathes = value;  NotifyPropertyChanged(); } }

      private string _cppdllPathes;
      /// <summary>
      /// Paths to CPPDLLs
      /// </summary>
      [DataMember()] public string cppdllPathes { get { return _cppdllPathes; } set { _cppdllPathes = value;  NotifyPropertyChanged(); } }

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public BinariesSet(string name = "")
      {
         this.name = name;
         this.exePathes = "";
         this.dllPathes = "";
         this.cppdllPathes = "";
      }

      public object Clone()
      {
         return (BinariesSet)this.MemberwiseClone();
      }

   }
}
