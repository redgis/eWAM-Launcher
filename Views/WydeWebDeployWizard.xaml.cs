using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace eWamLauncher.Views
{

   public class WizardReturnEventArgs<ResultType>
   {
      public WizardReturnEventArgs(ResultType result)
      {
         Result = result;
      }

      public ResultType Result { get; }
   }

   public delegate void WizardReturnEventHandler<ResultType>(object sender, WizardReturnEventArgs<ResultType> e);

   /// <summary>
   /// Interaction logic for WydeWebDeployWizard.xaml
   /// This wizard is meant to allow for selection of a WydeWen service and a 
   /// launcher (to have the client parmeters), selection of a package (activex or clickonce), 
   /// and deploy the appropriately configured package to a local IIS folder, for instance.
   /// This wizard is composed of the steps :
   /// - WydeWebDeployServiceSelector     : select a service to deploy
   /// - WydeWebDeployLauncherSelector    : select a launcher to use (e.g. wydeweb_wynsure, wydeweb_wynsureFr, etc.)
   /// - WydeWebDeployPackageSelector     : select a package to deploy this service / launcher
   /// - WydeWebDeployFinish              : show info, and do the deploy
   /// </summary>
   public partial class WydeWebDeployWizard : NavigationWindow
   {
      private WWService service;
      private Launcher launcher;
      private Package package;

      private WydeWebDeployServiceSelector serviceSelector;
      private WydeWebDeployLauncherSelector optionSelector;
      private WydeWebDeployPackageSelector packageSelector;
      private WydeWebDeployFinish finishPage;

      /// <summary>
      /// Initialize Wizard, create all steps, start first step
      /// </summary>
      /// <param name="environment"></param>
      public WydeWebDeployWizard(Environment environment)
      {
         InitializeComponent();

         serviceSelector = new WydeWebDeployServiceSelector(environment.wNetConf);
         serviceSelector.WizardReturn += this.ServiceSelector_Return;

         optionSelector = new WydeWebDeployLauncherSelector(environment.launchers.ToList());
         optionSelector.WizardReturn += OptionSelector_Return;

         packageSelector = new WydeWebDeployPackageSelector();
         packageSelector.WizardReturn += PackageSelector_Return;

         finishPage = new WydeWebDeployFinish();
         finishPage.WizardReturn += FinishPage_WizardReturn;

         this.Navigate(serviceSelector);
      }

      /// <summary>
      /// Treatement of result of service selector
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ServiceSelector_Return(object sender, WizardReturnEventArgs<WWService> e)
      {
         if(e.Result == null)
         {
            this.Close();
         }

         this.service = e.Result;
         this.Navigate(optionSelector);
      }

      /// <summary>
      /// Treatement of result of option selector
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OptionSelector_Return(object sender, WizardReturnEventArgs<Launcher> e)
      {
         if (e.Result == null)
         {
            this.Close();
         }

         this.launcher = e.Result;
         this.Navigate(packageSelector);
      }

      /// <summary>
      /// Treatement of result of package selector
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void PackageSelector_Return(object sender, WizardReturnEventArgs<Package> e)
      {
         if (e.Result == null)
         {
            this.Close();
         }

         this.package = e.Result;

         this.finishPage.chunk = this.service.GetWNetClientChunk();

         this.finishPage.options = this.launcher.arguments;
         this.finishPage.options = this.finishPage.options.Replace("%*", "");
         this.finishPage.options = Regex.Replace(this.finishPage.options, 
            @"/service:[a-zA-Z0-9]*", "", RegexOptions.IgnoreCase);
         this.finishPage.options = this.finishPage.options.Trim();
         this.finishPage.options += " /HtmlErrorPage:WydeWebErrors.html";

         if (this.package.Type == "clickonce")
         {
            this.finishPage.options += " /SERVICE:" + this.service.Name;
         }

         this.finishPage.package = this.package;

         this.Navigate(this.finishPage);
      }

      /// <summary>
      /// On finish wizard, download the selected package and close the wizard window
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void FinishPage_WizardReturn(object sender, WizardReturnEventArgs<bool> e)
      {
         if (e.Result)
         {
            ((MainWindow)Application.Current.MainWindow).packageDownloadManager.AddDownloadTask(
               this.package, this.finishPage.path, this.OnDownloadCompleted);
         }
         else
         {

         }

         this.Close();
      }

      /// <summary>
      /// Handle completion of the download of the selected package
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnDownloadCompleted(object sender, PackageDownloadCompletedEventArgs e)
      {
         if (e.Cancelled == true)
         {

         }
         else if (e.Error != null)
         {

         }
         else
         {
            //Do things with files

            //Setup the ActiveX deployment
            if (this.package.Type == "activex")
            {
               //For each HTML file, setup its PARAM tags appropriately
               List<string> htmlFiles = Directory.GetFiles(this.finishPage.path, "*.html").ToList();
               foreach (string filename in htmlFiles)
               {
                  this.SetupActiveXLauncher(filename);
               }

            }
            //Setup the ClickOnce deployment
            else if (this.package.Type == "clickonce")
            {
               //Write the selected launcher's options (modified by user in finishPage, see 
               // PackageSelector_Return), in options.txt 
               File.WriteAllText(
                  Path.Combine(this.finishPage.path, "options.txt"),
                  this.finishPage.options);

               //Write wnetconf.xml using the chunk previously generated (see PackageSelector_Return)
               File.WriteAllText(
                  Path.Combine(this.finishPage.path, "wnetconf.xml"),
                  this.finishPage.chunk);
            }
         }
      }

      /// <summary>
      /// Generate HTML code to be setup in the HTML files of the ActiveX Package
      /// </summary>
      /// <param name="filename"></param>
      private void SetupActiveXLauncher(string filename)
      {
         string content = File.ReadAllText(filename);

         content = Regex.Replace(content,
            "<PARAM NAME=\"Caption\" Value=\".*\">",
            "<PARAM NAME=\"Caption\" Value=\"\">", 
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"ClassName\" Value=\".*\">",
            "<PARAM NAME=\"ClassName\" Value=\"\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"MethodName\" Value=\".*\">",
            "<PARAM NAME=\"MethodName\" Value=\"\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"Parameters\" Value=\".*\">",
            "<PARAM NAME=\"Parameters\" Value=\"\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"Options\" Value=\".*\">",
            "<PARAM NAME=\"Options\" Value=\"" + this.finishPage.options + "\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"HostName\" Value=\".*\">",
            "<PARAM NAME=\"HostName\" Value=\"\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"PortNumber\" Value=\".*\">",
            "<PARAM NAME=\"PortNumber\" Value=\"\">",
            RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<PARAM NAME=\"ServiceName\" Value=\".*\">",
            "<PARAM NAME=\"ServiceName\" Value=\"" + this.service.Name + "\">",
            RegexOptions.IgnoreCase);

         //content = Regex.Replace(content,
         //   "<PARAM NAME=\"Configuration\" Value=\".*\">",
         //   "<PARAM NAME=\"Configuration\" Value=\"\">",
         //   RegexOptions.IgnoreCase);

         content = Regex.Replace(content,
            "<services>.*</services>",
            this.finishPage.chunk,
            RegexOptions.IgnoreCase|RegexOptions.Singleline);

         File.WriteAllText(filename, content);
      }
   }
}
