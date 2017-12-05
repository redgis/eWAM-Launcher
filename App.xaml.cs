using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

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
            IEwamImporter importer = new LegacyEwamImporter();
            this.environments = new List<wEnvironment>();
            wEnvironment environment = new wEnvironment();
            importer.importFromPath("D:\\Desktop\\Work\\Tiny Projects\\eWAM_Tools\\Dummy_eWAM\\Wynsure57-3\\Dev", environment);
            this.environments.Add(environment);

            FileStream writer = new FileStream("ewamLauncher.config.json", FileMode.Create);
            DataContractJsonSerializer jsonSerializer =
                new DataContractJsonSerializer(typeof(List<wEnvironment>));
            jsonSerializer.WriteObject(writer, this.environments);
            writer.Close();

            writer = new FileStream("ewamLauncher.config.xml", FileMode.Create);
            DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(List<wEnvironment>));
            xmlSerializer.WriteObject(writer, this.environments);
            writer.Close();
        }
    }

}
