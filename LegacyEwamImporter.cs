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
      private wEnvironment environment;


      public LegacyEwamImporter()
      {
         this.environment = new wEnvironment();
      }

      public LegacyEwamImporter(wEnvironment environment)
      {
         this.environment = environment;
      }

      public void SetInitialEnvironment(wEnvironment environment)
      {
         this.environment = environment;
      }

      public wEnvironment GetEnvironment()
      {
         return this.environment;
      }

      public wEnvironment ImportFromPath(string path) 
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         // Look for TGVs
         if (!File.Exists(path + "\\tgv\\W001001.TGV") || !File.Exists(path + "\\tgv\\W003001.TGV"))
         {
            throw new FileNotFoundException("W001001.TGV or W003001.TGV");
         }

         this.environment.tgvPath = path + "\\tgv";

         if (File.Exists(path + "\\tgv\\Prevoyance.TGV") &&
             File.Exists(path + "\\tgv\\WydePolicyAdminSolution.TGV"))
         {
            this.environment.name = "Wynsure";
         }
         else
         {
            this.environment.name = "eWAM";
         }

         // Look for env vars
         try
         {
            this.ImportEnvironmentVariables(path + "\\bin");
         }
         catch (IOException)
         {
            this.ImportEnvironmentVariables(path + "\\batches");
         }

         // Resolve env. vars
         this.environment.ExpandAllEnvVariables();

         // Look for binaries
         try
         {
            string wydeRoot = this.environment.GetEnvironmentVariable("WYDE-ROOT").value;
            ImportBinaries(wydeRoot);
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
            this.ImportLaunchers(path + "\\bin");
         }
         catch (IOException)
         {
            this.ImportLaunchers(path + "\\batches");
         }

         return this.environment;
      }

      public ObservableCollection<wEnvironmentVariable> ImportEnvironmentVariables(string path)
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
                        wEnvironmentVariable localEnVariable = this.environment.GetEnvironmentVariable(newKey);
                        if (localEnVariable != null)
                        {
                           if (localEnVariable.value == match.Groups["value"].Value)
                           {
                              // if it's same value, just ignore this match, move on to next one.
                              continue;
                           }
                           else
                           {
                              int increment = 0;
                              while (this.environment.GetEnvironmentVariable(newKey) != null)
                              {
                                 increment++;
                                 newKey = match.Groups["key"].Value.ToUpper() + "_" + increment.ToString();
                              }
                           }
                        }

                        // Add entries to environment variables list
                        this.environment.environmentVariables.Add(
                           new wEnvironmentVariable(newKey, match.Groups["value"].Value));
                     }
                  }
               }
            }
         }
         return this.environment.environmentVariables;
      }

      public ObservableCollection<wLauncher> ImportLaunchers(string path)
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
                        if (this.environment.binariesSets.Count > 0)
                        {
                           launcher.binariesSet = this.environment.binariesSets[0].name;
                        }
                        this.environment.launchers.Add(launcher);
                     }
                  }
               }
            }
         }

         return this.environment.launchers;
      }

      public ObservableCollection<wBinariesSet> ImportBinaries(string path)
      {
         if (Directory.Exists(path))
         {
            wBinariesSet releaseBinaries = new wBinariesSet();
            releaseBinaries.name = "Release";
            wBinariesSet debugBinaries = new wBinariesSet();
            debugBinaries.name = "Debug";

            if (Directory.GetFiles(path + "\\bin", "*.exe").Length > 0)
               releaseBinaries.exePathes += path + "\\bin" + "\n";

            if (Directory.GetFiles(path + "\\dll", "*.dll").Length > 0)
               releaseBinaries.dllPathes += path + "\\dll" + "\n";

            if (Directory.GetFiles(path + "\\cppdll", "*.dll").Length > 0)
               releaseBinaries.cppdllPathes += path + "\\cppdll" + "\n";


            if (Directory.GetFiles(path + "\\bin", "*.exe").Length > 0)
               debugBinaries.exePathes += path + "\\bin" + "\n";

            if (Directory.GetFiles(path + "\\dll.debug", "*.dll").Length > 0)
               debugBinaries.dllPathes += path + "\\dll.debug" + "\n";

            if (Directory.GetFiles(path + "\\cppdll.debug", "*.dll").Length > 0)
               debugBinaries.cppdllPathes += path + "\\cppdll.debug" + "\n";

            this.environment.binariesSets.Add(releaseBinaries);
            this.environment.binariesSets.Add(debugBinaries);
         }
         else
         {
            throw new DirectoryNotFoundException("WYDE-ROOT : " + path);
         }

         return this.environment.binariesSets;
      }

   }
}
