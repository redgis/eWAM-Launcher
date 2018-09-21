using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace eWamLauncher
{
   public class wEnvironmentImporter
   {
      private wEnvironment environment;
      private ObservableCollection<wEwam> ewams;
      private wWAMLauncherSettings settings;


      public wEnvironmentImporter(wWAMLauncherSettings settings, ObservableCollection<wEwam> ewams, wEnvironment environment = null)
      {
         this.ewams = ewams;
         this.settings = settings;

         if (environment == null)
         {
            this.environment = new wEnvironment();
         }
         else
         {
            this.environment = environment;
         }
      }

      public wEnvironmentImporter(wProfile profile, wEnvironment environment = null) : this(profile.settings, profile.ewams, environment)
      { }

      public wEnvironment GetEnvironment()
      {
         return this.environment;
      }

      public wEnvironment ImportFromPath(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         path = MainWindow.NormalizePath(path);

         char[] delimiters = { ';', '\n' };

         // Look for TGVs
         if (this.environment.tgvPath == null || this.environment.tgvPath == "")
         {
            foreach (string subPath in this.settings.tgvSearchPathes.Split(delimiters))
            {
               if (File.Exists(path + "\\" + subPath + "\\W001001.TGV") && File.Exists(path + "\\" + subPath + "\\W003001.TGV"))
               {
                  this.environment.tgvPath = MainWindow.NormalizePath(path + "\\" + subPath);
                  break;
               }
            }
         }
         else
         {
            this.environment.tgvPath = MainWindow.NormalizePath(this.environment.tgvPath);
         }

         // Find out if it looks like a simple ewam environment or a wynsure environment
         if (this.environment.tgvPath != null && this.environment.tgvPath != "")
         {
            if (File.Exists(this.environment.tgvPath + "\\Prevoyance.TGV") && 
               File.Exists(this.environment.tgvPath + "\\WydePolicyAdminSolution.TGV"))
            {
               this.environment.name = "Wynsure";
            }
            else
            {
               this.environment.name = "eWAM";
            }
         }

         // Look for env vars
         foreach (string subPath in this.settings.launcherSearchPathes.Split(delimiters))
         {
            try
            {
               this.ImportEnvironmentVariables(path + "\\" + subPath);
            }
            catch (IOException)
            { }
         }

         // Resolve env. vars
         this.environment.ExpandAllEnvVariables();

         // Look for binaries
         try
         {
            wEnvironmentVariable wydeRootEnvVar = this.environment.GetEnvironmentVariable("WYDE-ROOT");
            string wydeRoot = "";
            if (wydeRootEnvVar != null)
               wydeRoot = MainWindow.NormalizePath(wydeRootEnvVar.value);

            // See if we can guess the corresponding eWAM environment, if any exists.
            // If not, maybe try importing a new eWAM from the newly found WYDE-ROOT ... ?
            foreach (wEwam ew in this.ewams)
            {
               if (ew.basePath ==  wydeRoot)
               {
                  this.environment.ewam = ew;
                  break;
               }
            }

            // if no corresponding ewam found, try to import it
            if (this.environment.ewam == null)
            {
               wEwamImporter ewamImporter = new wEwamImporter(this.settings);
               this.environment.ewam = ewamImporter.ImportFromPath(wydeRoot);

               if (this.environment.ewam != null)
               {
                  this.ewams.Add(this.environment.ewam);
               }
            }


         }
         catch (DirectoryNotFoundException)
         {

         }
         catch (FileNotFoundException)
         {

         }

         // Look for launchers, load launcher-specific env. variables
         foreach (string subPath in this.settings.launcherSearchPathes.Split(delimiters))
         { 
            try
            {
               this.ImportLaunchers(path + "\\" + subPath);
            }
            catch (IOException)
            {
            }
         }

         return this.environment;
      }

      private string ReplaceDPandCDwithPath(string value, string path)
      {
         value = value.Replace("%~dp0", path);
         value = value.Replace("%~Dp0", path);
         value = value.Replace("%~dP0", path);
         value = value.Replace("%~DP0", path);
         value = value.Replace("%cd%", path);
         value = value.Replace("%Cd%", path);
         value = value.Replace("%cD%", path);
         value = value.Replace("%CD%", path);

         return value;
      }

      public ObservableCollection<wEnvironmentVariable> ImportEnvironmentVariables(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         path = MainWindow.NormalizePath(path);

         //Try importing env variables
         string[] batches = Directory.GetFiles(path, "*Set Env*.bat");
         if (batches.Length <= 0) throw new FileNotFoundException(path + "*Set Env*.bat");

         foreach (string batch in batches)
         {
            StreamReader sr = new StreamReader(batch);

            string pattern = @"(?<comment>^[\@\t\s]*(?:[rR][eE][mM]|:)+)?.*set[\t\s]+[""]?(?<key>[^=%]+)[\t\s]*=[\t\s]*(?<value>[^""]+)";
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
                        if (newKey != "WYDE-DLL")
                        {
                           this.environment.environmentVariables.Add(
                              new wEnvironmentVariable(newKey, match.Groups["value"].Value));
                        }
                     }
                  }
               }
            }
         }

         foreach(wEnvironmentVariable envVar in this.environment.environmentVariables)
         {
            envVar.value = this.ReplaceDPandCDwithPath(envVar.value, path);
         }

         return this.environment.environmentVariables;
      }

      public ObservableCollection<wLauncher> ImportLaunchers(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         path = MainWindow.NormalizePath(path);
         
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
                        if (this.environment.ewam != null && this.environment.ewam.binariesSets.Count > 0)
                        {
                           if (launcherName.ToLower().Contains("debug"))
                           {
                              foreach(wBinariesSet bs in this.environment.ewam.binariesSets)
                              {
                                 if (bs.name.ToLower().Contains("debug"))
                                 {
                                    launcher.binariesSet = bs;
                                    break;
                                 }
                              }
                           }
                           else
                           {
                              launcher.binariesSet = this.environment.ewam.binariesSets[0];
                           }
                        }
                        this.environment.launchers.Add(launcher);
                     }
                  }
               }
            }
         }

         return this.environment.launchers;
      }
   }
}
