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

      private string _exePaths;
      /// <summary>
      /// Paths to executables
      /// </summary>
      [DataMember()] public string exePaths { get { return _exePaths; } set { _exePaths = value;  NotifyPropertyChanged(); } }

      private string _dllPaths;
      /// <summary>
      /// Paths to Dlls
      /// </summary>
      [DataMember()] public string dllPaths { get { return _dllPaths; } set { _dllPaths = value;  NotifyPropertyChanged(); } }

      private string _cppdllPaths;
      /// <summary>
      /// Paths to CPPDLLs
      /// </summary>
      [DataMember()] public string cppdllPaths { get { return _cppdllPaths; } set { _cppdllPaths = value;  NotifyPropertyChanged(); } }

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
         this.exePaths = "";
         this.dllPaths = "";
         this.cppdllPaths = "";
      }

      public object Clone()
      {
         return (BinariesSet)this.MemberwiseClone();
      }

   }
}
