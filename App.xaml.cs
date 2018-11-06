using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Squirrel;
using log4net;

namespace eWamLauncher
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(App));

      protected override void OnStartup(StartupEventArgs e)
      {
         log4net.Config.XmlConfigurator.Configure();
         log.Info("        =============  Started Logging  =============        ");
         base.OnStartup(e);
      }

      public App()
      {
      }

   }

}
