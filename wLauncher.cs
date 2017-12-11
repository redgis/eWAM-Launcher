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
   public class wLauncher : ICloneable
   {
      [DataMember()]
      public string name { get; set; }

      [DataMember()]
      public string program { get; set; }

      [DataMember()]
      public string arguments { get; set; }

      [DataMember()]
      public string binariesSet { get; set; }

      public wLauncher()
      {
         this.binariesSet = "";
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }


   }
}
