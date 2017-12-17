using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace eWamLauncher
{
   [DataContract(Name = "wEnvironmentVariable", Namespace = "http://www.wyde.com")]
   public class wEnvironmentVariable : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _value;
      public string value { get { return _value; } set { _value = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _result;
      public string result { get { return _result; } set { _result = value;  NotifyPropertyChanged(); } }

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         if (this.PropertyChanged != null)
         {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public wEnvironmentVariable(string name = "", string value = "")
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
