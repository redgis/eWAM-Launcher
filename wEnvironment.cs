using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace eWamLauncher
{
   [DataContract(Name = "Environment", Namespace = "http://www.wyde.com/")]
   public class wEnvironment : INotifyPropertyChanged, ICloneable
   {
      [DataMember()]
      public string name { get; set; }

      [DataMember()]
      public string tgvPath { get; set; }

      [DataMember()]
      public ObservableCollection<wEnvironmentVariable> environmentVariables { get; set; }

      [DataMember()]
      public ObservableCollection<wBinariesSet> binariesSets { get; set; }

      [DataMember()]
      public ObservableCollection<wLauncher> launchers { get; set; }

      public wEnvironment()
      {
         this.environmentVariables = new ObservableCollection<wEnvironmentVariable>();
         this.binariesSets = new ObservableCollection<wBinariesSet>();
         this.launchers = new ObservableCollection<wLauncher>();
      }

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
      {
         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public object Clone()
      {
         wEnvironment clone = (wEnvironment)this.MemberwiseClone();
         clone.environmentVariables = new ObservableCollection<wEnvironmentVariable>();
         clone.binariesSets = new ObservableCollection<wBinariesSet>();
         clone.launchers = new ObservableCollection<wLauncher>();

         foreach (wEnvironmentVariable variable in this.environmentVariables)
         {
            clone.environmentVariables.Add((wEnvironmentVariable)variable.Clone());
         }

         foreach (wBinariesSet binariesSet in this.binariesSets)
         {
            clone.binariesSets.Add((wBinariesSet)binariesSet.Clone());
         }

         foreach (wLauncher launcher in this.launchers)
         {
            clone.launchers.Add((wLauncher)launcher.Clone());
         }

         return clone;
      }

   }
}
