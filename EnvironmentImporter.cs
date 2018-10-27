using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace eWamLauncher
{
   public class EnvironmentImporter
   {
      private Environment environment;
      private ObservableCollection<Ewam> ewams;
      private Settings settings;

      private String wydeRoot;

      public EnvironmentImporter(Settings settings, ObservableCollection<Ewam> ewams, Environment environment = null)
      {
         this.ewams = ewams;
         this.settings = settings;

         if (environment == null)
         {
            this.environment = new Environment();
         }
         else
         {
            this.environment = environment;
         }
      }

      public EnvironmentImporter(Profile profile, Environment environment = null) : this(profile.settings, profile.ewams, environment)
      { }

      public Environment GetEnvironment()
      {
         return this.environment;
      }

      public Environment ImportFromPath(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

         path = MainWindow.NormalizePath(path);

         char[] delimiters = { ';', '\n' };

         this.environment.envRoot = path;

         // Look for TGVs
         if (this.environment.tgvSubPath == null || this.environment.tgvSubPath == "")
         {
            foreach (string subPath in this.settings.tgvSearchPathes.Split(delimiters))
            {
               if (File.Exists(path + "\\" + subPath + "\\W001001.TGV") && File.Exists(path + "\\" + subPath + "\\W003001.TGV"))
               {
                  this.environment.tgvSubPath = subPath;
                  break;
               }
            }
         }

         // Find out if it looks like a simple ewam environment or a wynsure environment
         if ((this.environment.name == null || this.environment.name == "") && 
            this.environment.envRoot != null && this.environment.envRoot != "")
         {
            if (File.Exists(this.environment.envRoot + "\\" + this.environment.tgvSubPath + "\\Prevoyance.TGV") && 
               File.Exists(this.environment.envRoot + "\\" + this.environment.tgvSubPath + "\\WydePolicyAdminSolution.TGV"))
            {
               this.environment.name = "Wynsure";
            }
            else
            {
               this.environment.name = "eWAM";
            }
         }

         // Look for env vars
         foreach (string subPath in this.settings.batchSearchPathes.Split(delimiters))
         {
            try
            {
               this.ImportEnvironmentVariables(path + "\\" + subPath);
            }
            catch (IOException e)
            { }
         }

         // Resolve env. vars
         this.environment.ExpandAllEnvVariables();

         // Look for binaries ... if not already defined
         if (this.environment.ewam == null)
         {
            try
            {
               this.wydeRoot = MainWindow.NormalizePath(this.wydeRoot);

               // See if we can guess the corresponding eWAM environment, if any exists.
               // If not, maybe try importing a new eWAM from the newly found WYDE-ROOT ... ?
               foreach (Ewam ew in this.ewams)
               {
                  if (MainWindow.NormalizePath(ew.basePath) == this.wydeRoot)
                  {
                     this.environment.ewam = ew;
                     break;
                  }
               }

               // if no corresponding ewam found, try to import it
               if (this.environment.ewam == null)
               {
                  if (System.Windows.MessageBox.Show(
                     "eWAM for this environment hasn't been imported." + 
                     "\n\nDo you want to try importing it from WYDE-ROOT : " + this.wydeRoot,
                     "Import corresponding eWAM ?",
                     System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                  {
                     EwamImporter ewamImporter = new EwamImporter(this.settings);
                     this.environment.ewam = ewamImporter.ImportFromPath(this.wydeRoot);

                     if (this.environment.ewam != null)
                     {
                        this.ewams.Add(this.environment.ewam);
                     }
                  }
               }


            }
            catch (DirectoryNotFoundException)
            {

            }
            catch (FileNotFoundException)
            {

            }
         }

         // Look for launchers, load launcher-specific env. variables
         foreach (string subPath in this.settings.batchSearchPathes.Split(delimiters))
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

      public ObservableCollection<EnvironmentVariable> ImportEnvironmentVariables(string path)
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


                        if (newKey == "WF-ROOT" && (this.environment.wfRoot == null || this.environment.wfRoot == ""))
                        {

                           if (System.Windows.MessageBox.Show(
                                 "Use this for WF-ROOT ?\n\"" +
                                    match.Groups["value"].Value + "\"",
                                 "Choose WF-ROOT",
                                 System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                           {
                              this.environment.wfRoot = match.Groups["value"].Value;
                           }
                        }

                        // Add entries to environment variables list
                        if (newKey == "WYDE-ROOT" && (this.wydeRoot == null || this.wydeRoot == ""))
                        {
                           if (System.Windows.MessageBox.Show(
                                 "Use this for WYDE-ROOT ?\n\"" +
                                    match.Groups["value"].Value + "\"",
                                 "Choose WYDE-ROOT",
                                 System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
                           {
                              this.wydeRoot = match.Groups["value"].Value;
                           }
                        }

                        if (newKey != "WYDE-DLL" &&
                            newKey != "WYDE-ROOT" &&
                            newKey != "ENV-ROOT" &&
                            newKey != "WF-ROOT" &&
                            newKey != "WYDE-TGV")
                        {
                           // If this variable already exists, AND has a different value, we still 
                           // want to keep this different value, so that the user can make a clean up,
                           // and choose the right value by himself. We thus increment the variable
                           // name, before adding it to the dictionary
                           EnvironmentVariable localEnVariable = this.environment.GetEnvironmentVariable(newKey);
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

                           this.environment.environmentVariables.Add(
                              new EnvironmentVariable(newKey, match.Groups["value"].Value));
                        }
                     }
                  }
               }
            }
         }

         foreach(EnvironmentVariable envVar in this.environment.environmentVariables)
         {
            envVar.value = this.ReplaceDPandCDwithPath(envVar.value, path);
         }

         this.environment.ExpandAllEnvVariables();

         return this.environment.environmentVariables;
      }

      public ObservableCollection<Launcher> ImportLaunchers(string path)
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
                        // Add entries to environment launchers list
                        Launcher launcher = new Launcher();
                        launcher.name = launcherName;
                        launcher.program = match.Groups["command"].Value;
                        launcher.arguments = match.Groups["value"].Value;

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
