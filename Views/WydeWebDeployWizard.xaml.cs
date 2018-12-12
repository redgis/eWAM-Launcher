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

      private void ServiceSelector_Return(object sender, WizardReturnEventArgs<WWService> e)
      {
         if(e.Result == null)
         {
            this.Close();
         }

         this.service = e.Result;
         this.Navigate(optionSelector);
      }

      private void OptionSelector_Return(object sender, WizardReturnEventArgs<Launcher> e)
      {
         if (e.Result == null)
         {
            this.Close();
         }

         this.launcher = e.Result;
         this.Navigate(packageSelector);
      }

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
            if (this.package.Type == "activex")
            {
               List<string> htmlFiles = Directory.GetFiles(this.finishPage.path, "*.html").ToList();

               foreach (string filename in htmlFiles)
               {
                  this.SetupActiveXLauncher(filename);
               }

            }
            else if (this.package.Type == "clickonce")
            {
               File.WriteAllText(
                  Path.Combine(this.finishPage.path, "options.txt"),
                  this.finishPage.options);

               File.WriteAllText(
                  Path.Combine(this.finishPage.path, "wnetconf.xml"),
                  this.finishPage.chunk);
            }
         }
      }

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
