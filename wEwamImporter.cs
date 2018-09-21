using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace eWamLauncher
{
   public class wEwamImporter
   {
      private wEwam ewam;
      private wWAMLauncherSettings settings;
      
      public wEwamImporter(wWAMLauncherSettings settings, wEwam ewam = null)
      {
         this.settings = settings;

         if (ewam != null)
         {
            this.ewam = ewam;
         }
         else
         {
            this.ewam = new wEwam();
         }
      }

      public wEwamImporter(wProfile profile, wEwam ewam = null) : this(profile.settings, ewam)
      { }

      public wEwam GetEwam()
      {
         return this.ewam;
      }

      public wEwam ImportFromPath(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException("WYDE-ROOT : " + path);

         path = MainWindow.NormalizePath(path);
         
         this.ewam = new wEwam(path);

         // dictionaries to store (key:binaries set name) => (value:pathes)
         Dictionary<string, string> exePathes = new Dictionary<string, string>();
         Dictionary<string, string> dllPathes = new Dictionary<string, string>();
         Dictionary<string, string> cppdllPathes = new Dictionary<string, string>();

         char[] delimiters = { ';', '\n' };

         // Retrieve pathes for each binaries set (release, debug, etc), for exe, dlls, cppdlls, based on settings search pathes.
         this.FindPathes(path, this.settings.exeSearchPathes.Split(delimiters), "*.exe", exePathes);
         this.FindPathes(path, this.settings.dllSearchPathes.Split(delimiters), "*.dll", dllPathes);
         this.FindPathes(path, this.settings.cppdllSearchPathes.Split(delimiters), "*.dll", cppdllPathes);


         //Recombine the binaries sets collected pathes into a single wBinariesSet object.
         Dictionary<string, wBinariesSet> binariesSets = new Dictionary<string, wBinariesSet>();

         foreach (KeyValuePair<string, string> binSetPathes in exePathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new wBinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].exePathes = binSetPathes.Value;
         }

         foreach (KeyValuePair<string, string> binSetPathes in dllPathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new wBinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].dllPathes = binSetPathes.Value;
         }

         foreach (KeyValuePair<string, string> binSetPathes in cppdllPathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new wBinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].cppdllPathes = binSetPathes.Value;
         }

         // store binaries sets in current ewam
         foreach (KeyValuePair<string, wBinariesSet> binariesSet in binariesSets)
         {
            this.ewam.binariesSets.Add(binariesSet.Value);
         }

         this.ewam.name = "eWAM";

         string[] ewamExes = Directory.GetFiles(path, "ewam.exe", SearchOption.AllDirectories);
         if (ewamExes.Length > 0)
         {
            this.ewam.name += " " + FileVersionInfo.GetVersionInfo(ewamExes[0]).ProductVersion.Replace(",", ".").Replace(" ", "");
         }

         return this.ewam;
      }

      private void FindPathes(string basePath, string[] subPathes, string fileType,
         Dictionary<string, string> foundPathes)
      {
         basePath = MainWindow.NormalizePath(basePath);

         foreach (string subSearchPath in subPathes)
         {
            string defaultBinariesSetName = "release";

            if (subSearchPath.ToLower().Contains("debug"))
            {
               defaultBinariesSetName = "debug";
            }

            string searchPath = MainWindow.NormalizePath(basePath + "\\" + subSearchPath);

            try
            {
               string[] pathes = Directory.GetFiles(searchPath, fileType, SearchOption.AllDirectories);
               foreach (string fullFilename in pathes)
               {
                  string fullPath = Path.GetDirectoryName(fullFilename);

                  string setName = defaultBinariesSetName;

                  if (fullPath != searchPath)
                  {
                     setName = fullPath.Substring(searchPath.Length + 1).ToLower();
                  }

                  if (!foundPathes.ContainsKey(setName))
                  {
                     foundPathes.Add(setName, "");
                  }

                  if (!foundPathes[setName].Contains(fullPath))
                  {
                     foundPathes[setName] += fullPath + "\n";
                  }
               }
            }
            catch (DirectoryNotFoundException)
            {

            }
         }
      }

   }
}
