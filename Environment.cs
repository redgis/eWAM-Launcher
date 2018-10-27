﻿using System;
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
   [DataContract(Name = "Environment", Namespace = "http://www.wyde.com")]
   public class Environment : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _envRoot;
      public string envRoot { get { return _envRoot; } set { _envRoot = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _wfRoot;
      public string wfRoot { get { return _wfRoot; } set { _wfRoot = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _tgvSubPath;
      public string tgvSubPath { get { return _tgvSubPath; } set { _tgvSubPath = value; NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<EnvironmentVariable> _environmentVariables;
      public ObservableCollection<EnvironmentVariable> environmentVariables { get { return _environmentVariables; } set { _environmentVariables = value; NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<Launcher> _launchers;
      public ObservableCollection<Launcher> launchers { get { return _launchers; } set { _launchers = value; NotifyPropertyChanged(); } }

      //TODO: add some validation in NotifyPropertyChanged : mainly change selected binariesSet in each launcher !
      [DataMember()] private Ewam _ewam;
      public Ewam ewam { get { return _ewam; } set { _ewam = value; this.NotifyPropertyChanged(); } }

      [DataMember()] private BinariesSet _binariesSet;
      [DataMember()] public BinariesSet binariesSet { get { return _binariesSet; } set { _binariesSet = value; NotifyPropertyChanged(); } }


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

   }
}