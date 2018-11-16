using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace eWamLauncher
{

   [DataContract(Name = "Ewam", Namespace = "http://www.wyde.com")]
   public class Ewam : ICloneable, INotifyPropertyChanged
   {
      private string _name;
      [DataMember()] public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      private string _basePath;
      [DataMember()] public string basePath { get { return _basePath; } set { _basePath = value; NotifyPropertyChanged(); } }

      private ObservableCollection<BinariesSet> _binariesSets;
      [DataMember()] public ObservableCollection<BinariesSet> binariesSets { get { return _binariesSets; } set { _binariesSets = value; NotifyPropertyChanged(); } }

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

      public Ewam(string basePath = "")
      {
         this.binariesSets = new ObservableCollection<BinariesSet>();
         this.basePath = basePath;
      }

      public object Clone()
      {
         Ewam clone = (Ewam)this.MemberwiseClone();
         clone.binariesSets = new ObservableCollection<BinariesSet>();

         foreach (BinariesSet binariesSet in this.binariesSets)
         {
            clone.binariesSets.Add((BinariesSet)binariesSet.Clone());
         }

         return clone;
      }

   }
}
