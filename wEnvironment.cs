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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;



namespace eWamLauncher
{
   [DataContract(Name = "wEnvironment", Namespace = "http://www.wyde.com")]
   public class wEnvironment : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _tgvPath;
      public string tgvPath { get { return _tgvPath; } set { _tgvPath = value; NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<wEnvironmentVariable> _environmentVariables;
      public ObservableCollection<wEnvironmentVariable> environmentVariables { get { return _environmentVariables; } set { _environmentVariables = value; NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<wLauncher> _launchers;
      public ObservableCollection<wLauncher> launchers { get { return _launchers; } set { _launchers = value; NotifyPropertyChanged(); } }

      //TODO: add some validation in NotifyPropertyChanged : mainly change selected binariesSet in each launcher !
      [DataMember()] private wEwam _ewam;
      public wEwam ewam { get { return _ewam; } set { _ewam = value; this.NotifyPropertyChanged(); } }

      public ObservableCollection<Process> processes { get; set; }


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

      public wEnvironment()
      {
         this.environmentVariables = new ObservableCollection<wEnvironmentVariable>();
         this.launchers = new ObservableCollection<wLauncher>();
         this.ewam = null;
      }

      public object Clone()
      {
         wEnvironment clone = (wEnvironment)this.MemberwiseClone();
         clone.environmentVariables = new ObservableCollection<wEnvironmentVariable>();
         clone.launchers = new ObservableCollection<wLauncher>();

         foreach (wEnvironmentVariable variable in this.environmentVariables)
         {
            clone.environmentVariables.Add((wEnvironmentVariable)variable.Clone());
         }

         foreach (wLauncher launcher in this.launchers)
         {
            clone.launchers.Add((wLauncher)launcher.Clone());
         }

         return clone;
      }


      public void RestoreReferenceEwam(IEnumerable<wEwam> referenceEwams)
      {
         if (this.ewam == null)
            return;

         foreach (wEwam ew in referenceEwams)
         {
            if (ew.basePath == this.ewam.basePath)
            {
               this.ewam = ew;
            }
         }

         foreach(wLauncher launcher in this.launchers)
         {
            launcher.RestoreReferenceBinariesSet(this.ewam);
         }
      }

      public wEnvironmentVariable GetEnvironmentVariable(string name)
      {
         wEnvironmentVariable result = null;

         foreach (wEnvironmentVariable variable in this.environmentVariables)
         {
            if (variable.name == name)
            {
               result = variable;
               break;
            }
         }

         return result;
      }

      private string ExpandEnvVariableMatch(Match match)
      {
         return this.ResolveVariable(match.Groups["variable"].Value);
      }

      public string ResolveVariable(string name)
      {
         string result = null;

         string variableValue = "";

         bool variableIsInEnvironment = false;

         // Look for variable in current wEnvironment (i.e. this)
         wEnvironmentVariable localEnvVar = this.GetEnvironmentVariable(name);
         if (localEnvVar != null)
         {
            variableValue = localEnvVar.value;
            variableIsInEnvironment = true;
         }
         else
         {
            // If not found, look in provided environment variables, look in process env. 
            // variables
            variableValue = Environment.GetEnvironmentVariable(name);
         }

         if (variableValue == null)
         {
            result = "";
         }
         else
         {
            // Expand env variable value by looking up all %....% and expanding each of these matches
            Regex rgx = new Regex(@"%(?<variable>[^=%\s\t]+)%");
            result = rgx.Replace(
               variableValue,
               new MatchEvaluator(this.ExpandEnvVariableMatch));

            if (variableIsInEnvironment)
            {
               localEnvVar.result = result;
            }
         }

         return result;
      }

      public void ExpandAllEnvVariables()
      {

         //Get rid of WYDE-DLL variable : set up in binaries set !
         List<wEnvironmentVariable> toRemove = new List<wEnvironmentVariable>();
         foreach (wEnvironmentVariable envVariable in this.environmentVariables)
         {
            if (envVariable.name == "WYDE-DLL")
            {
               toRemove.Add(envVariable);
            }
         }
         foreach (wEnvironmentVariable envVariable in toRemove)
         {
            this.environmentVariables.Remove(envVariable);
         }


         // Expand all variables
         foreach (wEnvironmentVariable envVariable in this.environmentVariables)
         {
            envVariable.result = this.ResolveVariable(envVariable.name);
         }
      }

   }
}
