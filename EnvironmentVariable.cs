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
   [DataContract(Name = "EnvironmentVariable", Namespace = "http://www.wyde.com")]
   public class EnvironmentVariable : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name.ToUpper(); } set { _name = value.ToUpper();  NotifyPropertyChanged(); } }

      [DataMember()] private string _value;
      public string value { get { return _value; } set { _value = value;  NotifyPropertyChanged(); } }

      [DataMember()] private string _result;
      public string result { get { return _result; } set { _result = value;  NotifyPropertyChanged(); } }

      public Boolean isBeingResolved;

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

      //Needed to be able to add lines in the Env Variables datagrid

      public EnvironmentVariable()
      {
         this.name = "WYDE-DUMMY";
         this.value = "";
         this.isBeingResolved = false;
   }

      public EnvironmentVariable(string name = "", string value = "")
      {
         this.name = name;
         this.value = value;
         this.isBeingResolved = false;
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }
   }
}
