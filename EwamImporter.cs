using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using log4net;

namespace eWamLauncher
{
   /// <summary>
   /// Factory class used to import an eWAM from a given path.
   /// </summary>
   public class EwamImporter
   {
      private Ewam ewam;
      private Settings settings;

      private static readonly ILog log = LogManager.GetLogger(typeof(EwamImporter));

      /// <summary>
      /// Importer needs to have access to parameters in order to help it configure the importing 
      /// (sub folders to lookup).
      /// </summary>
      /// <param name="settings">settings used during importing</param>
      /// <param name="ewam">A pre-built ewam object may be provided, and will be filled 
      /// with imported information</param>
      public EwamImporter(Settings settings, Ewam ewam = null)
      {
         this.settings = settings;

         if (ewam != null)
         {
            this.ewam = ewam;
         }
         else
         {
            this.ewam = new Ewam();
         }
      }

      /// <summary>
      /// Importer needs to have access to parameters in order to help it configure the importing 
      /// (sub folders to lookup).
      /// </summary>
      /// <param name="profile">profile containing settings to be used during importing</param>
      /// <param name="ewam">A pre-built ewam object may be provided, and will be filled 
      /// with imported information</param>
      public EwamImporter(Profile profile, Ewam ewam = null) : this(profile.settings, ewam)
      { }

      /// <summary>
      /// Get the imported ewam
      /// </summary>
      /// <returns>the imported ewam</returns>
      public Ewam GetEwam()
      {
         return this.ewam;
      }

      /// <summary>
      /// Import an ewam from given path.
      /// </summary>
      /// <param name="path">path from which to import the ewam instance</param>
      /// <returns>imported ewam</returns>
      public Ewam ImportFromPath(string path)
      {
         if (!Directory.Exists(path)) throw new DirectoryNotFoundException("WYDE-ROOT : " + path);

         path = MainWindow.NormalizePath(path);
         
         this.ewam = new Ewam(path);

         // dictionaries to store (key:binaries set name) => (value:paths)
         Dictionary<string, string> exePathes = new Dictionary<string, string>();
         Dictionary<string, string> dllPathes = new Dictionary<string, string>();
         Dictionary<string, string> cppdllPathes = new Dictionary<string, string>();

         char[] delimiters = { ';', '\n' };

         // Retrieve paths for each binaries set (release, debug, etc), for exe, dlls, cppdlls, based on settings search paths.
         this.FindPathes(path, this.settings.exeSearchPathes.Split(delimiters), "*.exe", exePathes);
         this.FindPathes(path, this.settings.dllSearchPathes.Split(delimiters), "*.dll", dllPathes);
         this.FindPathes(path, this.settings.cppdllSearchPathes.Split(delimiters), "*.dll", cppdllPathes);
         
         //Recombine the binaries sets collected paths into a single BinariesSet object.
         Dictionary<string, BinariesSet> binariesSets = new Dictionary<string, BinariesSet>();

         foreach (KeyValuePair<string, string> binSetPathes in exePathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new BinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].exePathes = binSetPathes.Value;
         }

         foreach (KeyValuePair<string, string> binSetPathes in dllPathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new BinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].dllPathes = binSetPathes.Value;
         }

         foreach (KeyValuePair<string, string> binSetPathes in cppdllPathes)
         {
            if (!binariesSets.ContainsKey(binSetPathes.Key))
            {
               binariesSets.Add(binSetPathes.Key, new BinariesSet());
            }

            binariesSets[binSetPathes.Key].name = binSetPathes.Key;
            binariesSets[binSetPathes.Key].cppdllPathes = binSetPathes.Value;
         }

         // store binaries sets in current ewam
         foreach (KeyValuePair<string, BinariesSet> binariesSet in binariesSets)
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

      /// <summary>
      /// Internal method used to find the set of sub paths used by this ewam instance for a specified file type.
      /// </summary>
      /// <param name="basePath">root path to look into</param>
      /// <param name="subPathes">list of sub paths in which to lookup</param>
      /// <param name="fileType">file types to lookup</param>
      /// <param name="foundPathes">paths containing given file types</param>
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

                  string normalizedFullPath = Path.GetFullPath(new Uri(fullPath).LocalPath)
                     .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                  //Remove prefix base path
                  string foundSubPath = normalizedFullPath.Substring(basePath.Length + 1)
                     .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                  if (!foundPathes[setName].Contains(foundSubPath))
                  {
                     if (normalizedFullPath.StartsWith(basePath))
                     {
                        foundPathes[setName] += foundSubPath + "\n";
                     }
                  }
               }
            }
            catch (DirectoryNotFoundException exception)
            {
               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
            }
         }
      }

   }
}
