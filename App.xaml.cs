using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;

namespace eWamLauncher
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public List<wEnvironment> environments;

        public App()
        {
            this.environments = new List<wEnvironment>();
            this.environments.Add(new wEnvironment("D:\\Desktop\\Work\\eWAM_Tools\\Dummy_eWAM\\Wynsure57-3\\Dev"));
        }
    }

    public class wBinaries
    {
        public string name;

        public List<string> exePathes;
        public List<string> dllPathes;
        public List<string> cppdllPathes;
    }

    public class wLauncher
    {
        public string program;
        public string arguments;
        public wBinaries binariesSet;
    }

    public class wEnvironment
    {
        public string name;

        public string tgvPath;
        public Dictionary<string, string> environmentVariables;
        public List<wBinaries> binariesSets;
        public List<wLauncher> launchers;

        public wEnvironment()
        {
            this.environmentVariables = new Dictionary<string, string>();
            this.binariesSets = new List<wBinaries>();
            this.launchers = new List<wLauncher>();
        }

        public wEnvironment(string path) : this()
        {
            this.importFromPath(path);
        }

        public void importFromPath(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);


            // Look for TGVs
            if (!File.Exists(path + "\\tgv\\W001001.TGV") || !File.Exists(path + "\\tgv\\W003001.TGV"))
            {
                throw new FileNotFoundException("W001001.TGV or W003001.TGV");
            }

            if (File.Exists(path + "\\tgv\\Prevoyance.TGV") &&
                File.Exists(path + "\\tgv\\WydePolicyAdminSolution.TGV"))
            {
                this.name = "Wynsure";
            }
            else
            {
                this.name = "eWAM";
            }

            // Look for batches
            try
            {
                this.importBatches(path + "\\bin");
            }
            catch (IOException)
            {
                this.importBatches(path + "\\batches");
            }

        }

        private void importBatches(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

            //Try importing env variables
            string[] batches = Directory.GetFiles(path, "*Set Env.bat");
            if (batches.Length <= 0) throw new FileNotFoundException(path + "*Set Env.bat");

            foreach (string batch in batches)
            {
                StreamReader sr = new StreamReader(batch);
                string input;
                string pattern = @"set[\t\s]+(?<key>[a-zA-Z0-9\-_\+]+)[\t\s]*=[\t\s]*(?<value>.+)";
                while (sr.Peek() >= 0)
                {
                    input = sr.ReadLine();
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(input);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Groups["comment"] == null)
                            {
                                // Add entries to environment variables list
                                this.environmentVariables.Add(match.Groups["key"].Value.ToUpper(), match.Groups["value"].Value);
                            }
                        }
                    }
                }
            }

            // Try importing launchers
            batches = Directory.GetFiles(path, "*.bat");
            if (batches.Length <= 0) throw new FileNotFoundException(path + "*.bat");

            foreach (string batch in batches)
            {
                StreamReader sr = new StreamReader(batch);
                string input;
                string pattern = @"(?<command>ewam.exe|ewamconsole.exe|wyseman.exe|wydeweb.exe)[\t\s]*(?<value>.+)";
                while (sr.Peek() >= 0)
                {
                    input = sr.ReadLine();
                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(input);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Groups["comment"] == null)
                            {
                                // Add entries to environment variables list
                                wLauncher launcher = new wLauncher();
                                launcher.program = match.Groups["command"].Value;
                                launcher.arguments = match.Groups["value"].Value;
                                launcher.binariesSet = this.binariesSets[0];
                                this.launchers.Add(launcher);
                            }
                        }
                    }
                }
            }
        }

        private void appendPathIfFoundFiles(string path, string files, List<string> list)
        {
            string[] foundFiles = Directory.GetFiles(path, files);

            if (foundFiles.Length > 0)
            {
                list.Add(path);
            }
        }

        private void importBinaries(string path)
        {
            string wydeRoot = this.environmentVariables["WYDE-ROOT"];

            if (Directory.Exists(wydeRoot))
            {
                wBinaries releaseBinaries = new wBinaries();
                releaseBinaries.name = "Release";
                wBinaries debugBinaries = new wBinaries();
                debugBinaries.name = "Debug";

                this.appendPathIfFoundFiles(wydeRoot + "\\bin", "*.exe", releaseBinaries.exePathes);
                this.appendPathIfFoundFiles(wydeRoot + "\\dll", "*.dll", releaseBinaries.dllPathes);
                this.appendPathIfFoundFiles(wydeRoot + "\\cppdll", "*.dll", releaseBinaries.cppdllPathes);

                this.appendPathIfFoundFiles(wydeRoot + "\\bin", "*.exe", debugBinaries.exePathes);
                this.appendPathIfFoundFiles(wydeRoot + "\\dll.debug", "*.dll", debugBinaries.dllPathes);
                this.appendPathIfFoundFiles(wydeRoot + "\\cppdll.debug", "*.dll", debugBinaries.cppdllPathes);

                this.binariesSets.Add(releaseBinaries);
                this.binariesSets.Add(debugBinaries);
            }
            else
            {
                throw new DirectoryNotFoundException("WYDE-ROOT : " + wydeRoot);
            }
        }

    }
}
