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

      [DataMember()] private ObservableCollection<string> _exePathes;
      public ObservableCollection<string> exePathes { get { return _exePathes; } set { _exePathes = value;  NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<string> _dllPathes;
      public ObservableCollection<string> dllPathes { get { return _dllPathes; } set { _dllPathes = value;  NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<string> _cppdllPathes;
      public ObservableCollection<string> cppdllPathes { get { return _cppdllPathes; } set { _cppdllPathes = value;  NotifyPropertyChanged(); } }

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

      public wBinariesSet()
      {
         this.name = "";
         this.exePathes = new ObservableCollection<string>();
         this.dllPathes = new ObservableCollection<string>();
         this.cppdllPathes = new ObservableCollection<string>();
      }

      public object Clone()
      {
         wBinariesSet clone = (wBinariesSet)this.MemberwiseClone();
         clone.exePathes = new ObservableCollection<string>();
         clone.dllPathes = new ObservableCollection<string>();
         clone.cppdllPathes = new ObservableCollection<string>();

         foreach (string exePath in this.exePathes)
         {
            clone.exePathes.Add(exePath);
         }

         foreach (string dllPath in this.dllPathes)
         {
            clone.dllPathes.Add(dllPath);
         }

         foreach (string cppDllPath in this.cppdllPathes)
         {
            clone.cppdllPathes.Add(cppDllPath);
         }

         return clone;
      }

   }
}
