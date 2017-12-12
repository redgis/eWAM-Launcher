using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace eWamLauncher
{
   public class LegacyEwamImporter : IEwamImporter
   {

      public void importFromPath(string path, wEnvironment environment)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         // Look for TGVs
         if (!File.Exists(path + "\\tgv\\W001001.TGV") || !File.Exists(path + "\\tgv\\W003001.TGV"))
         {
            throw new FileNotFoundException("W001001.TGV or W003001.TGV");
         }

         environment.tgvPath = path + "\\tgv";

         if (File.Exists(path + "\\tgv\\Prevoyance.TGV") &&
             File.Exists(path + "\\tgv\\WydePolicyAdminSolution.TGV"))
         {
            environment.name = "Wynsure";
         }
         else
         {
            environment.name = "eWAM";
         }

         // Look for env vars
         try
         {
            this.importEnvironmentVariables(path + "\\bin", environment);
         }
         catch (IOException)
         {
            this.importEnvironmentVariables(path + "\\batches", environment);
         }

         // Resolve env. vars
         this.expandEnvVariables(environment);

         // Look for binaries
         try
         {
            string wydeRoot = environment.environmentVariables["WYDE-ROOT"].value;
            this.importBinaries(wydeRoot, environment);
         }
         catch (DirectoryNotFoundException)
         {

         }
         catch (FileNotFoundException)
         {

         }

         // Look for launchers, load launcher-specific env. variables
         try
         {
            this.importLaunchers(path + "\\bin", environment);
         }
         catch (IOException)
         {
            this.importLaunchers(path + "\\batches", environment);
         }

      }

      private string resolveVariable(string name, ObservableDictionary<string, wEnvVariableValue> variables)
      {
         string result = Environment.GetEnvironmentVariable(name);

         // If not found, look in provided environment variables
         if (result == null)
         {
            if (variables.ContainsKey(name))
            {
               string pattern = @"%(?<variable>[^=%]+)%";
               string input = variables[name].value;

               Regex rgx = new Regex(pattern);
               MatchCollection matches = rgx.Matches(input);
               if (matches.Count > 0)
               {
                  foreach (Match match in matches)
                  {
                     if (match.Groups["variable"].Value != "")
                     {

                     }
                  }
               }
            }
         }

         if (result == null) 
            result = "";

         return result;
      }

      private void expandEnvVariables(wEnvironment environment)
      {
         // Expand all variables
         foreach (KeyValuePair<string, wEnvVariableValue> envVariable in 
            environment.environmentVariables)
         {
            resolveVariable(envVariable.Value.value, environment.environmentVariables);
         }
      }

      public void importEnvironmentVariables(string path, wEnvironment environment)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         //Try importing env variables
         string[] batches = Directory.GetFiles(path, "*Set Env.bat");
         if (batches.Length <= 0) throw new FileNotFoundException(path + "*Set Env.bat");

         foreach (string batch in batches)
         {
            StreamReader sr = new StreamReader(batch);
            string pattern = @"(?<comment>^[\@\t\s]*(?:REM|:)+)?.*set[\t\s]+(?<key>[^=%]+)[\t\s]*=[\t\s]*(?<value>.+)";
            while (sr.Peek() >= 0)
            {
               string input = sr.ReadLine();
               Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
               MatchCollection matches = rgx.Matches(input);
               if (matches.Count > 0)
               {
                  foreach (Match match in matches)
                  {
                     if (match.Groups["comment"].Value == "")
                     {
                        

                        string newKey = match.Groups["key"].Value.ToUpper();

                        // If this variable already exists, AND has a different value, we still 
                        // want to keep this different value, so that the user can make a clean up,
                        // and choose the right value by himself. We thus increment the variable
                        // name, before adding it to the dictionary
                        if (environment.environmentVariables.ContainsKey(newKey))
                        {
                           if (environment.environmentVariables[newKey].value == match.Groups["value"].Value)
                           {
                              // if it's same value, just ignore this match, move on to next one.
                              continue;
                           }
                           else
                           {
                              int increment = 0;
                              while (environment.environmentVariables.ContainsKey(newKey))
                              {
                                 increment++;
                                 newKey = match.Groups["key"].Value.ToUpper() + "_" + increment.ToString();
                              }
                           }
                        }

                        // Add entries to environment variables list
                        environment.environmentVariables.Add(
                           newKey,
                           new wEnvVariableValue(match.Groups["value"].Value));
                     }
                  }
               }
            }
         }
      }

      public void importLaunchers(string path, wEnvironment environment)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         // Try importing launchers
         string[] batches = Directory.GetFiles(path, "*.bat");
         if (batches.Length <= 0) throw new FileNotFoundException(path + "*.bat");

         foreach (string batch in batches)
         {
            string launcherName = Path.GetFileNameWithoutExtension(batch);
            StreamReader sr = new StreamReader(batch);
            string pattern = @"(?<comment>^[\@\t\s]*(?:REM|:)+)?.*(?<command>ewam\.exe|ewamconsole\.exe|wyseman\.exe|wydeweb\.exe)[""\t\s]*(?<value>.+)";
            while (sr.Peek() >= 0)
            {
               string input = sr.ReadLine();
               Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
               MatchCollection matches = rgx.Matches(input);
               if (matches.Count > 0)
               {
                  foreach (Match match in matches)
                  {
                     if (match.Groups["comment"].Value == "")
                     {
                        // Add entries to environment variables list
                        wLauncher launcher = new wLauncher();
                        launcher.name = launcherName;
                        launcher.program = match.Groups["command"].Value;
                        launcher.arguments = match.Groups["value"].Value;
                        if (environment.binariesSets.Count > 0)
                        {
                           launcher.binariesSet = environment.binariesSets[0].name;
                        }
                        environment.launchers.Add(launcher);
                     }
                  }
               }
            }
         }
      }

      private void appendPathIfFoundFiles(string path, string files, ICollection<string> list)
      {
         try
         {
            string[] foundFiles = Directory.GetFiles(path, files);

            if (foundFiles.Length > 0)
            {
               list.Add(path);
            }
         }
         catch (DirectoryNotFoundException)
         { }
      }

      public void importBinaries(string path, wEnvironment environment)
      {
         if (Directory.Exists(path))
         {
            wBinariesSet releaseBinaries = new wBinariesSet();
            releaseBinaries.name = "Release";
            wBinariesSet debugBinaries = new wBinariesSet();
            debugBinaries.name = "Debug";

            this.appendPathIfFoundFiles(path + "\\bin", "*.exe", releaseBinaries.exePathes);
            this.appendPathIfFoundFiles(path + "\\dll", "*.dll", releaseBinaries.dllPathes);
            this.appendPathIfFoundFiles(path + "\\cppdll", "*.dll", releaseBinaries.cppdllPathes);

            this.appendPathIfFoundFiles(path + "\\bin", "*.exe", debugBinaries.exePathes);
            this.appendPathIfFoundFiles(path + "\\dll.debug", "*.dll", debugBinaries.dllPathes);
            this.appendPathIfFoundFiles(path + "\\cppdll.debug", "*.dll", debugBinaries.cppdllPathes);

            environment.binariesSets.Add(releaseBinaries);
            environment.binariesSets.Add(debugBinaries);
         }
         else
         {
            throw new DirectoryNotFoundException("WYDE-ROOT : " + path);
         }
      }

   }
}
