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
   /// <summary>
   /// Represents an environment variable : its name, its symbolic value (i.e. containing un resolved %...%), its expanded value (all %...% resolved too their value)
   /// </summary>
   [DataContract(Name = "EnvironmentVariable", Namespace = "http://www.wyde.com")]
   public class EnvironmentVariable : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name.ToUpper(); } set { _name = value.ToUpper();  NotifyPropertyChanged(); } }

      private string _value;
      /// <summary>
      /// Symbolic (unresolved) value of the environment variable
      /// </summary>
      [DataMember()] public string value { get { return _value; } set { _value = value;  NotifyPropertyChanged(); } }

      private string _result;
      /// <summary>
      /// Expanded (resolved) value of the environment variable
      /// </summary>
      [DataMember()] public string result { get { return _result; } set { _result = value;  NotifyPropertyChanged(); } }

      /// <summary>
      /// Boolean used to know the variable is being resolved, to avoid cycles due recursive env. variable definitions.
      /// </summary>
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
