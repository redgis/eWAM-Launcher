using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eWamLauncher
{
    interface IEwamImporter
    {
        void importFromPath(string path, wEnvironment environment);

        void importEnvironmentVariables(string path, wEnvironment environment);

        void importLaunches(string path, wEnvironment environment);

        void importBinaries(string path, wEnvironment environment);
    }
}
