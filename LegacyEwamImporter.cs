using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

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
            string wydeRoot = "";
            foreach (wEnvironmentVariable variable in environment.environmentVariables)
            {
               if (variable.name == "WYDE-ROOT")
               {
                  wydeRoot = variable.value;
               }
            }
            //string wydeRoot = "D:\\Desktop\\Work\\Tiny Projects\\eWAM_Tools\\Dummy_eWAM\\eWAM_6.1.5.11_x64";
            //string wydeRoot = "D:\\wyde\\eWAM\\eWAM-Developper-6.1.5.x64\\6.1.5.11";
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

      private void expandEnvVariables(wEnvironment environment)
      {
         foreach (wEnvironmentVariable variable in environment.environmentVariables)
         {

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
            string input;
            string pattern = @"(?<comment>^[\@\t\s]*(?:REM|:)+)?.*set[\t\s]+(?<key>[a-zA-Z0-9\-_\+]+)[\t\s]*=[\t\s]*(?<value>.+)";
            while (sr.Peek() >= 0)
            {
               input = sr.ReadLine();
               Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
               MatchCollection matches = rgx.Matches(input);
               if (matches.Count > 0)
               {
                  foreach (Match match in matches)
                  {
                     if (match.Groups["comment"].Value == "")
                     {
                        // Add entries to environment variables list
                        environment.environmentVariables.Add(
                           new wEnvironmentVariable(
                              match.Groups["key"].Value.ToUpper(), match.Groups["value"].Value));
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
            string input;
            string pattern = @"(?<comment>^[\@\t\s]*(?:REM|:)+)?.*(?<command>ewam\.exe|ewamconsole\.exe|wyseman\.exe|wydeweb\.exe)[""\t\s]*(?<value>.+)";
            while (sr.Peek() >= 0)
            {
               input = sr.ReadLine();
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
