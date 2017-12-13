using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace eWamLauncher
{
   interface IEwamImporter
   {
      void SetInitialEnvironment(wEnvironment environment);

      wEnvironment ImportFromPath(string path);

      ObservableCollection<wEnvironmentVariable> ImportEnvironmentVariables(string path);

      ObservableCollection<wLauncher> ImportLaunchers(string path);

      ObservableCollection<wBinariesSet> ImportBinaries(string path);

      wEnvironment GetEnvironment();
   }
}
