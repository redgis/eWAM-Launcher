using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace eWamLauncher
{
    [DataContract(Name = "Environment", Namespace = "http://www.wyde.com/")]
    public class wEnvironment
    {
        [DataMember()]
        public string name;

        [DataMember()]
        public string tgvPath;

        [DataMember()]
        public List<KeyValuePair<string, string>> environmentVariables;

        [DataMember()]
        public List<wBinaries> binariesSets;

        [DataMember()]
        public List<wLauncher> launchers;

        public wEnvironment()
        {
            this.environmentVariables = new List<KeyValuePair<string, string>>();
            this.binariesSets = new List<wBinaries>();
            this.launchers = new List<wLauncher>();
        }
    }
}
