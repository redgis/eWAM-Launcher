using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{
   public class wEnvVariableValue
   {
      public string value { set; get; }
      public string result { set; get; }

      public wEnvVariableValue()
      {
      }

      public wEnvVariableValue(string value)
      {
         this.value = value;
      }

      public wEnvVariableValue(string value, string result)
      {
         this.value = value;
         this.result = result;
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }
   }
}
