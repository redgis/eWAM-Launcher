using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eWamLauncher
{
   /// <summary>
   /// This class contains all the information of the local configuration : the environments, 
   /// the ewams,the settings, and allows for derialization and deserialization
   /// </summary>
   [DataContract(Name = "Profile", Namespace = "http://www.wyde.com")]
   public class Profile : ICloneable, INotifyPropertyChanged
   {
      /// <summary>
      /// List of environments
      /// </summary>
      private ObservableCollection<Environment> _environments;
      [DataMember()] public ObservableCollection<Environment> environments { get { return _environments; } set { _environments = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// List of ewams
      /// </summary>
      private ObservableCollection<Ewam> _ewams;
      [DataMember()] public ObservableCollection<Ewam> ewams { get { return _ewams; } set { _ewams = value; this.NotifyPropertyChanged(); } }

      /// <summary>
      /// Application settings
      /// </summary>
      private Settings _settings;
      [DataMember()] public Settings settings { get { return _settings; } set { _settings = value; this.NotifyPropertyChanged(); } }


      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public Profile()
      {
         this.environments = new ObservableCollection<Environment>();
         this.ewams = new ObservableCollection<Ewam>();
         this.settings = new Settings();
      }

      /// <summary>
      /// Export all environments to XML and JSON, in a specified path, that may 
      /// contain environment variables (e.g. like %env-root% or %wf-root%).
      /// </summary>
      /// <param name="path">path to which the files will be generated</param>
      public void ExportAllEnvironments(string path)
      {
         foreach (var env in this.environments)
         {
            env.SaveEnvironmentToJSON(env.ExpandString(path) + "\\" + env.name + ".jsenv");
            env.SaveEnvironmentToXML(env.ExpandString(path) + "\\" + env.name + ".xenv");
         }
      }

      /// <summary>
      /// Export all ewams to XML and JSON, in a specified path, that may 
      /// contain %wyde-root%.
      /// </summary>
      /// <param name="path">path to which the files will be generated</param>
      public void ExportAllEwams(string path)
      {
         foreach (var ewam in this.ewams)
         {
            ewam.SaveEwamToJSON(Regex.Replace(path, @"[%]wyde-root[%]", ewam.basePath) + "\\" + ewam.name + ".jswam");
            ewam.SaveEwamToXML(Regex.Replace(path, @"[%]wyde-root[%]", ewam.basePath) + "\\" + ewam.name + ".xwam");
         }
      }

      public object Clone()
      {
         Profile clone = (Profile)this.MemberwiseClone();

         clone.environments = new ObservableCollection<Environment>();
         clone.ewams = new ObservableCollection<Ewam>();
         clone.settings = (Settings)settings.Clone();

         foreach (Environment environment in this.environments)
         {
            clone.environments.Add((Environment)environment.Clone());
         }

         foreach (Ewam ewam in this.ewams)
         {
            clone.ewams.Add((Ewam)ewam.Clone());
         }

         return clone;
      }

   }
}