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

   [DataContract(Name = "wEwam", Namespace = "http://www.wyde.com")]
   public class wEwam : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private string _name;
      public string name { get { return _name; } set { _name = value; NotifyPropertyChanged(); } }

      [DataMember()] private string _basePath;
      public string basePath { get { return _basePath; } set { _basePath = value; NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<wBinariesSet> _binariesSets;
      public ObservableCollection<wBinariesSet> binariesSets { get { return _binariesSets; } set { _binariesSets = value; NotifyPropertyChanged(); } }

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

      public wEwam(string basePath = "")
      {
         this.binariesSets = new ObservableCollection<wBinariesSet>();
         this.basePath = basePath;
      }

      public object Clone()
      {
         wEwam clone = (wEwam)this.MemberwiseClone();
         clone.binariesSets = new ObservableCollection<wBinariesSet>();

         foreach (wBinariesSet binariesSet in this.binariesSets)
         {
            clone.binariesSets.Add((wBinariesSet)binariesSet.Clone());
         }

         return clone;
      }

   }
}
