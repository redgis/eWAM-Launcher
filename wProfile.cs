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
   [DataContract(Name = "wProfile", Namespace = "http://www.wyde.com")]
   public class wProfile : ICloneable, INotifyPropertyChanged
   {
      [DataMember()] private ObservableCollection<wEnvironment> _environments;
      public ObservableCollection<wEnvironment> environments { get { return _environments; } set { _environments = value; this.NotifyPropertyChanged(); } }

      [DataMember()] private ObservableCollection<wEwam> _ewams;
      public ObservableCollection<wEwam> ewams { get { return _ewams; } set { _ewams = value; this.NotifyPropertyChanged(); } }

      [DataMember()] private wWAMLauncherSettings _settings;
      public wWAMLauncherSettings settings { get { return _settings; } set { _settings = value; this.NotifyPropertyChanged(); } }


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

      public wProfile()
      {
         this.environments = new ObservableCollection<wEnvironment>();
         this.ewams = new ObservableCollection<wEwam>();
         this.settings = new wWAMLauncherSettings();
      }

      public object Clone()
      {
         wProfile clone = (wProfile)this.MemberwiseClone();

         clone.environments = new ObservableCollection<wEnvironment>();
         clone.ewams = new ObservableCollection<wEwam>();
         clone.settings = (wWAMLauncherSettings)settings.Clone();

         foreach (wEnvironment environment in this.environments)
         {
            clone.environments.Add((wEnvironment)environment.Clone());
         }

         foreach (wEwam ewam in this.ewams)
         {
            clone.ewams.Add((wEwam)ewam.Clone());
         }

         return clone;
      }

   }
}