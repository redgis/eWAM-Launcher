using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace eWamLauncher
{
    [DataContract(Name = "BinariesSet", Namespace = "http://www.wyde.com/")]
    public class wBinaries
    {
        [DataMember()]
        public string name;

        [DataMember()]
        public List<string> exePathes;

        [DataMember()]
        public List<string> dllPathes;

        [DataMember()]
        public List<string> cppdllPathes;

        public wBinaries()
        {
            this.name = "";
            this.exePathes = new List<string>();
            this.dllPathes = new List<string>();
            this.cppdllPathes = new List<string>();
        }
    }
}
