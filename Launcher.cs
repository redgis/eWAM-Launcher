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
   [DataContract(Name = "Launcher", Namespace = "http://www.wyde.com")]
   public class Launcher : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      private string _program;
      [DataMember()] public string program { get { return _program; } set { _program = value;  NotifyPropertyChanged(); } }

      private string _arguments;
      [DataMember()] public string arguments { get { return _arguments; } set { _arguments = value;  NotifyPropertyChanged(); } }

      //private string _binariesSet;
      //[DataMember()] public string binariesSet { get { return _binariesSet; } set { _binariesSet = value;  NotifyPropertyChanged(); } }

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

      public Launcher()
      {
         this.name = "";
         this.program = "";
         this.arguments = "";
      }

      public object Clone()
      {
         //No need to clone binariesSet because it is a reference!
         //clone.binariesSet = this.binariesSet.Clone();

         return this.MemberwiseClone();
      }

      public string GenerateBatch(string ewamSetEnvFilename)
      {
         string output = "";
         output += "@echo off\n";
         output += "@call \"%~dp0" + ewamSetEnvFilename + "\"\n";
         output += "@echo Starting " + this.name + "...\n";
         output += "@call \"" + this.program + "\" " + this.arguments + "\n";
         return output;
      }
   }
}
