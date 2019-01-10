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
   /// <summary>
   /// Class representing a launcher. A launcher is basically a command line : a n executable program, 
   /// and a set of parameters (arguments).
   /// </summary>
   [DataContract(Name = "Launcher", Namespace = "http://www.wyde.com")]
   public class Launcher : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      private string _program;
      [DataMember()] public string program { get { return _program; } set { _program = value;  NotifyPropertyChanged(); } }

      private string _arguments;
      [DataMember()] public string arguments { get { return _arguments; } set { _arguments = value;  NotifyPropertyChanged(); } }

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
         return this.MemberwiseClone();
      }

      /// <summary>
      /// Generate the batch command line for this launcher. This is be called / executed in the 
      /// context of the environment this launcher belongs to : using the same environment variables
      /// provided by "ewamSetEnvFilename" batch file.
      /// </summary>
      /// <param name="ewamSetEnvFilename">Batch file called before executing the launcher. Usually 
      /// contains the necessary environment variables.</param>
      /// <returns></returns>
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
