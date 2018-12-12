using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
   /// <summary>
   /// Interaction logic for WydeWebDeployFinish.xaml
   /// </summary>
   public partial class WydeWebDeployFinish : PageFunction<String>, INotifyPropertyChanged
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(WydeWebDeployFinish));

      public event WizardReturnEventHandler<bool> WizardReturn;

      private string _chunk;
      public string chunk { get { return _chunk; } set { _chunk = value; this.NotifyPropertyChanged(); } }

      private string _options;
      public string options { get { return _options; } set { _options = value; this.NotifyPropertyChanged(); } }

      private Package _package;
      public Package package { get { return _package; } set { _package = value; this.NotifyPropertyChanged(); } }

      private string _path;
      public string path { get { return _path; } set { _path = value; this.NotifyPropertyChanged(); } }

      private string _pathStatus;
      public string pathStatus { get { return _pathStatus; } set { _pathStatus = value; this.NotifyPropertyChanged(); } }

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

      public WydeWebDeployFinish()
      {
         InitializeComponent();
         this.DataContext = this;
      }

      private void OnFinish(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<bool>(true));
      }

      private void OnCancel(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<bool>(false));
      }

      #region Path actions

      private bool hasWriteAccessToFolder(string folderPath)
      {
         try
         {
            // Attempt to get a list of security permissions from the folder. 
            // This will raise an exception if the path is read only or do not have access to view the permissions. 
            //System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
            string filename = Path.Combine(folderPath, Guid.NewGuid().ToString());
            var tmpTestFile = File.Create(filename);
            tmpTestFile.Close();
            File.Delete(filename);
            btFinish.IsEnabled = true;
            return true;
         }
         catch (Exception e)
         {
            btFinish.IsEnabled = false;
            return false;
         }
      }

      private void OnChangePath(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            string tmpPath = this.path;
            MainWindow.ChangePath(ref tmpPath);
            this.path = tmpPath;

            if (!this.hasWriteAccessToFolder(this.path))
            {
               tbPathStatus.Content = "Warning : write access denied ! Change path or restart app as admin.";
            }
            else
            {
               tbPathStatus.Content = "";
            }

         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      private void OnExplorePath(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            MainWindow.ExplorePath(this.path);
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);

            System.Windows.MessageBox.Show(
               "Something went wrong ! \n\n" + exception.Message,
               "Oops",
               System.Windows.MessageBoxButton.OK,
               System.Windows.MessageBoxImage.Error);
         }
      }

      #endregion

      private void OnPathChanged(object sender, TextChangedEventArgs e)
      {
         if (!this.hasWriteAccessToFolder(tbPath.Text))
         {
            tbPathStatus.Content = "Warning : write access denied ! Change path or restart app as admin.";
         }
         else
         {
            tbPathStatus.Content = "";
         }
      }

   }
}
