using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   public class wEnvironmentVariable : ICloneable
   {
      public string name { set; get; }
      public string value { set; get; }
      public string result { set; get; }

      public wEnvironmentVariable()
      {
      }

      public wEnvironmentVariable(string name, string value) : this()
      {
         this.name = name;
         this.value = value;
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }
   }
}
