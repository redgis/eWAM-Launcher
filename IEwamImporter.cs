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

      ObservableDictionary<string, wEnvVariableValue> ImportEnvironmentVariables(string path);

      wLauncher[] ImportLaunchers(string path);

      wBinariesSet[] ImportBinaries(string path);

      wEnvironment GetEnvironment();
   }
}
