using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace eWamLauncher
{
   [DataContract(Name = "BinariesSet", Namespace = "http://www.wyde.com/")]
   public class wBinariesSet : ICloneable
   {
      [DataMember()]
      public string name { get; set; }

      [DataMember()]
      public ObservableCollection<string> exePathes { get; set; }

      [DataMember()]
      public ObservableCollection<string> dllPathes { get; set; }

      [DataMember()]
      public ObservableCollection<string> cppdllPathes { get; set; }

      public wBinariesSet()
      {
         this.name = "";
         this.exePathes = new ObservableCollection<string>();
         this.dllPathes = new ObservableCollection<string>();
         this.cppdllPathes = new ObservableCollection<string>();
      }

      public object Clone()
      {
         wBinariesSet clone = (wBinariesSet)this.MemberwiseClone();
         clone.exePathes = new ObservableCollection<string>();
         clone.dllPathes = new ObservableCollection<string>();
         clone.cppdllPathes = new ObservableCollection<string>();

         foreach (string exePath in this.exePathes)
         {
            clone.exePathes.Add(exePath);
         }

         foreach (string dllPath in this.dllPathes)
         {
            clone.dllPathes.Add(dllPath);
         }

         foreach (string cppDllPath in this.cppdllPathes)
         {
            clone.cppdllPathes.Add(cppDllPath);
         }

         return clone;
      }

   }
}
