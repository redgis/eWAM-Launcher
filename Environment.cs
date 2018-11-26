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
using System.Windows.Forms;

namespace eWamLauncher
{
   [DataContract(Name = "Environment", Namespace = "http://www.wyde.com")]
   public class Environment : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      private string _envRoot;
      [DataMember()] public string envRoot { get { return _envRoot; } set { _envRoot = value; NotifyPropertyChanged(); } }

      private string _wfRoot;
      [DataMember()] public string wfRoot { get { return _wfRoot; } set { _wfRoot = value; NotifyPropertyChanged(); } }

      private string _tgvSubPath;
      [DataMember()] public string tgvSubPath { get { return _tgvSubPath; } set { _tgvSubPath = value; NotifyPropertyChanged(); } }

      private ObservableCollection<EnvironmentVariable> _environmentVariables;
      [DataMember()] public ObservableCollection<EnvironmentVariable> environmentVariables { get { return _environmentVariables; } set { _environmentVariables = value; NotifyPropertyChanged(); } }

      private ObservableCollection<Launcher> _launchers;
      [DataMember()] public ObservableCollection<Launcher> launchers { get { return _launchers; } set { _launchers = value; NotifyPropertyChanged(); } }

      private Ewam _ewam;
      [DataMember()] public Ewam ewam { get { return _ewam; } set { _ewam = value; this.NotifyPropertyChanged(); } }

      private BinariesSet _binariesSet;
      [DataMember()] public BinariesSet binariesSet { get { return _binariesSet; } set { _binariesSet = value; NotifyPropertyChanged(); } }

      private WNetConf _wNetConf;
      [DataMember()] public WNetConf wNetConf { get { return _wNetConf; } set { _wNetConf = value; NotifyPropertyChanged(); } }

      //private WydeNetWorkConfiguration _wydeNetConf;
      //[DataMember()] public WydeNetWorkConfiguration wydeNetConf { get { return _wydeNetConf; } set { _wydeNetConf = value; NotifyPropertyChanged(); } }


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

      public Environment()
      {
         this.environmentVariables = new ObservableCollection<EnvironmentVariable>();
         this.launchers = new ObservableCollection<Launcher>();
         this.ewam = null;
         this.binariesSet = null;
      }

      public object Clone()
      {
         Environment clone = (Environment)this.MemberwiseClone();
         clone.environmentVariables = new ObservableCollection<EnvironmentVariable>();
         clone.launchers = new ObservableCollection<Launcher>();

         foreach (EnvironmentVariable variable in this.environmentVariables)
         {
            clone.environmentVariables.Add((EnvironmentVariable)variable.Clone());
         }

         foreach (Launcher launcher in this.launchers)
         {
            clone.launchers.Add((Launcher)launcher.Clone());
         }

         return clone;
      }
      
      public void RestoreReferenceEwam(IEnumerable<Ewam> referenceEwams)
      {
         if (this.ewam == null)
            return;

         //Restore ewam reference
         foreach (Ewam ew in referenceEwams)
         {
            if (ew.basePath == this.ewam.basePath)
            {
               this.ewam = ew;
            }
         }

         //Restore binary set
         if (this.binariesSet != null)
         {
            foreach (BinariesSet binariesSet in ewam.binariesSets)
            {
               if (binariesSet.name == this.binariesSet.name)
               {
                  this.binariesSet = binariesSet;
               }
            }
         }
         else
         {
            if (this.ewam != null)
            {
               this.binariesSet = this.ewam.binariesSets[0];
            }
         }
      }

      public EnvironmentVariable GetEnvironmentVariable(string name)
      {
         EnvironmentVariable result = null;

         if (name.ToUpper() == "WYDE-ROOT" && this.ewam != null)
         {
            result = new EnvironmentVariable("WYDE-ROOT", ExpandString(this.ewam.basePath));
         }

         if (name.ToUpper() == "ENV-ROOT" && this.envRoot != null)
         {
            result = new EnvironmentVariable("ENV-ROOT", ExpandString(this.envRoot));
         }

         if (name.ToUpper() == "WF-ROOT")
         {
            if (this.wfRoot != null && this.wfRoot != "")
            {
               result = new EnvironmentVariable("WF-ROOT", ExpandString(this.wfRoot));
            }
            else if (this.envRoot != null)
            {
               result = new EnvironmentVariable("WF-ROOT", ExpandString(this.envRoot));
            }
            
         }

         if (name.ToUpper() == "WYDE-TGV" && this.ewam != null)
         {
            result = new EnvironmentVariable("WYDE-TGV", ExpandString(this.envRoot + "\\" + this.tgvSubPath));
         }

         foreach (EnvironmentVariable variable in this.environmentVariables)
         {
            if (variable.name.ToUpper() == name.ToUpper())
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

         // Look for variable in current Environment (i.e. this)
         EnvironmentVariable localEnvVar = this.GetEnvironmentVariable(name);

         if (localEnvVar != null)
         {
            variableValue = localEnvVar.value;
            variableIsInEnvironment = true;
         }
         else
         {
            // If not found, look in provided environment variables, look in process env. 
            // variables
            variableValue = System.Environment.GetEnvironmentVariable(name);
         }

         if (variableValue == null)
         {
            result = "";
         }
         else
         {
            if (!localEnvVar.isBeingResolved)
            {
               localEnvVar.isBeingResolved = true;
               {
                  // Expand env variable value by looking up all %....% and expanding each of these matches
                  Regex rgx = new Regex(@"%(?<variable>[^=%\s\t]+)%");
                  result = rgx.Replace(
                     variableValue,
                     new MatchEvaluator(this.ExpandEnvVariableMatch));
               }
               localEnvVar.isBeingResolved = false;

               if (variableIsInEnvironment)
               {
                  localEnvVar.result = result;
               }
               
            }
         }

         return result;
      }

      public string ExpandString(string str)
      {
         if (str == null || str == "") return "";

         // Expand env variable value by looking up all %....% and expanding each of these matches
         Regex rgx = new Regex(@"%(?<variable>[^=%\s\t]+)%");
         string result = rgx.Replace(
            str,
            new MatchEvaluator(this.ExpandEnvVariableMatch));
         return result;
      }

      public void ExpandAllEnvVariables()
      {
         //Get rid of WYDE-DLL variable : set up in binaries set !
         List<EnvironmentVariable> toRemove = new List<EnvironmentVariable>();
         foreach (EnvironmentVariable envVariable in this.environmentVariables)
         {
            if (envVariable.name.ToUpper() == "WYDE-DLL" || 
                envVariable.name.ToUpper() == "WYDE-ROOT" ||
                envVariable.name.ToUpper() == "WF-ROOT" ||
                envVariable.name.ToUpper() == "ENV-ROOT" ||
                envVariable.name.ToUpper() == "WYDE-TGV")
            {
               toRemove.Add(envVariable);
            }
         }
         foreach (EnvironmentVariable envVariable in toRemove)
         {
            this.environmentVariables.Remove(envVariable);
         }


         // Expand all variables
         foreach (EnvironmentVariable envVariable in this.environmentVariables)
         {
            envVariable.result = this.ResolveVariable(envVariable.name);
         }
      }

      public string GetEnvVarBatch()
      {
         Dictionary<string, string> variables = new Dictionary<string, string>();

         char[] delimiters = { '\n', ';', '\r', '\b' };

         variables.Add("WYDE-ROOT", this.ewam.basePath);
         string[] cppdlls = this.binariesSet.cppdllPathes.Split(delimiters);
         variables.Add("WYDE-DLL", "%WYDE-ROOT%" + "\\" + cppdlls[0]);

         variables.Add("ENV-ROOT", this.envRoot);

         if (this.wfRoot == null || this.wfRoot == "")
            variables.Add("WF-ROOT", this.envRoot);
         else
            variables.Add("WF-ROOT", this.wfRoot);

         variables.Add("WYDE-TGV", "%ENV-ROOT%" + "\\" + this.tgvSubPath);


         // Set all other environment variables
         foreach (EnvironmentVariable variable in this.environmentVariables)
         {
            if (variable.name == "PATH") continue;

            variables.Add(variable.name, variable.value);
         }


         // Add raw PATHs
         List<string> binSubPathes = new List<string>();
         binSubPathes.AddRange(this.binariesSet.dllPathes.Split(delimiters));
         binSubPathes.AddRange(this.binariesSet.cppdllPathes.Split(delimiters));
         binSubPathes.AddRange(this.binariesSet.exePathes.Split(delimiters));

         string pathVariable = "";
         foreach (string subBinPath in binSubPathes)
         {
            if (subBinPath != "")
            {
               pathVariable += "%WYDE-ROOT%" + "\\" + subBinPath + ";";
            }
         }

         if (this.GetEnvironmentVariable("PATH") != null)
         {
            pathVariable += this.GetEnvironmentVariable("PATH").value + ";";
         }

         pathVariable += "%PATH%";

         variables.Add("PATH", pathVariable);


         string output = "";
         foreach (KeyValuePair<string, string> envVar in variables)
         {
            output += "set " + envVar.Key + "=" + envVar.Value + "\n";
         }

         return output;
      }

      public void GenerateBatchFiles(string path)
      {
         string eWamSetEnv = "eWAM Set Env.bat";

         System.IO.File.WriteAllText(path + "\\" + eWamSetEnv, this.GetEnvVarBatch());

         foreach (Launcher launcher in this.launchers)
         {
            System.IO.File.WriteAllText(path + "\\" + launcher.name + ".bat", launcher.GenerateBatch(eWamSetEnv));
         }
      }

   }
}
