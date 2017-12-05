using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace eWamLauncher
{
    [DataContract(Name = "Launcher", Namespace = "http://www.wyde.com/")]
    public class wLauncher
    {
        [DataMember()]
        public string name;

        [DataMember()]
        public string program;

        [DataMember()]
        public string arguments;

        [DataMember()]
        public string binariesSet;

        public wLauncher()
        {
            this.binariesSet = "";
        }
    }
}
