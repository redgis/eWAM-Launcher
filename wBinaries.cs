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
   [DataContract(Name = "wBinariesSet", Namespace = "http://www.wyde.com")]
   public class wBinariesSet : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _exePathes;
      public string exePathes { get { return _exePathes; } set { _exePathes = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _dllPathes;
      public string dllPathes { get { return _dllPathes; } set { _dllPathes = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _cppdllPathes;
      public string cppdllPathes { get { return _cppdllPathes; } set { _cppdllPathes = value;  NotifyPropertyChanged(); } }

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

      public wBinariesSet(string name = "")
      {
         this.name = name;
         this.exePathes = "";
         this.dllPathes = "";
         this.cppdllPathes = "";
      }

      public object Clone()
      {
         return (wBinariesSet)this.MemberwiseClone();
      }

   }
}
