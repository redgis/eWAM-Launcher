using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Squirrel;
using log4net;
using System.Threading;

namespace eWamLauncher
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(App));

      bool tookMutex = false;

      /// <summary>
      /// mutex semaphore used to allow only one instance
      /// </summary>
      static Mutex mutex = new Mutex(true, "{A70F17F8-D109-4761-97F1-9D5B96D99D53}");
      

      protected override void OnStartup(StartupEventArgs e)
      {
         // Start log4net logging system
         log4net.Config.XmlConfigurator.Configure();
         log.Info("        =============  Started Logging  =============        ");

         // Check if application not open already by trying to get a lock on this system-wide mutex
         if (mutex.WaitOne(TimeSpan.Zero, true))
         {
            this.tookMutex = true;
            base.OnStartup(e);
         }
         else
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : instance already running, exiting...");

            MessageBox.Show(
               "Application is already running. Only one instance " +
               "allowed. Check notification bar for eWAM Launcher icon.", 
               "App already running", 
               MessageBoxButton.OK, MessageBoxImage.Warning);

            //System.Windows.Application.Current.Shutdown();
            this.Shutdown();
         }
      }

      protected override void OnExit(ExitEventArgs e)
      {
         // Release mutex to let other instances start... I think this is not mandatory because .NET will automatically 
         // clean the mutex if the process owning it crasshes or closes... I think. So better be safe.
         if (this.tookMutex)
         {
            mutex.ReleaseMutex();
         }

         base.OnExit(e);
         log.Info("        =============  Stopped Logging  =============        ");
      }

      public App()
      {
      }
   }
}
