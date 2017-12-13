using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace eWamLauncher
{
   [DataContract(Name = "EnvironmentVariableValue", Namespace = "http://www.wyde.com/")]
   public class wEnvVariableValue : DependencyObject
   {
      [DataMember()]
      public string value
      {
         get { return (string)GetValue(valueProperty); }
         set { SetValue(valueProperty, value); }
      }
      public static readonly DependencyProperty valueProperty =
          DependencyProperty.Register("value", typeof(string), typeof(wEnvVariableValue), new PropertyMetadata(""));

      [DataMember()]
      public string result
      {
         get { return (string)GetValue(resultProperty); }
         set { SetValue(resultProperty, value); }
      }
      public static readonly DependencyProperty resultProperty =
          DependencyProperty.Register("result", typeof(string), typeof(wEnvVariableValue), new PropertyMetadata(""));

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
