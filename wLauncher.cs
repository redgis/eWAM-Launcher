using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Windows;
using System.ComponentModel;

namespace eWamLauncher
{
   [DataContract(Name = "wLauncher", Namespace = "http://www.wyde.com")]
   public class wLauncher : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _program;
      [DataMember()] public string program { get { return _program; } set { _program = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _arguments;
      [DataMember()] public string arguments { get { return _arguments; } set { _arguments = value;  NotifyPropertyChanged(); } }

      //[DataMember()] private string _binariesSet;
      //[DataMember()] public string binariesSet { get { return _binariesSet; } set { _binariesSet = value;  NotifyPropertyChanged(); } }

      [DataMember()] private wBinariesSet _binariesSet;
      [DataMember()] public wBinariesSet binariesSet { get { return _binariesSet; } set { _binariesSet = value; NotifyPropertyChanged(); } }

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

      public wLauncher()
      {
         this.name = "";
         this.program = "";
         this.arguments = "";
         this.binariesSet = null;
      }

      public object Clone()
      {
         wLauncher clone = (wLauncher)this.MemberwiseClone();

         //No need to clone binariesSet because it is a reference!
         //clone.binariesSet = this.binariesSet.Clone();

         return this.MemberwiseClone();
      }

      public void RestoreReferenceBinariesSet(wEwam ewam)
      {
         foreach (wBinariesSet binariesSet in ewam.binariesSets)
         {
            if (this.binariesSet != null && binariesSet.name == this.binariesSet.name)
            {
               this.binariesSet = binariesSet;
            }
         }
      }
   }
}
