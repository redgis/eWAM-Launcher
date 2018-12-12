using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace eWamLauncher.Views
{
   /// <summary>
   /// Interaction logic for WydeWebDeployPackageSelector.xaml
   /// </summary>
   public partial class WydeWebDeployPackageSelector : PageFunction<Package>, INotifyPropertyChanged
   {
      public List<Package> _packages = new List<Package>();
      public List<Package> packages { get { return _packages; } set { _packages = value; this.NotifyPropertyChanged(); } }

      public event WizardReturnEventHandler<Package> WizardReturn;

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
   
      public WydeWebDeployPackageSelector()
      {
         InitializeComponent();
         this.DataContext = this;

         lbPackages.Visibility = Visibility.Hidden;
         txtStatus.Visibility = Visibility.Visible;

         if (((MainWindow)System.Windows.Application.Current.MainWindow).WideIndex == null)
         {
            string serverURL = ((MainWindow)System.Windows.Application.Current.MainWindow)
               .profile.settings.ewamUpdateServerURL + "//package-index.xml";
            PackageIndexGetter packageGetter = new PackageIndexGetter(serverURL);
            packageGetter.PackageListCompleted += this.OnPackageIndexDownloaded;
            packageGetter.GetPackages();
         }
         else
         {
            //Do treatment
            this.ComputePackageList();
         }
      }

      private void OnPackageIndexDownloaded(object sender, PackageListCompletedEventArgs e)
      {
         if (e.Cancelled == true)
         {
            txtStatus.Content = "Cancelled";
         }
         else if (e.Error != null)
         {
            txtStatus.Content = "Error : " + e.Error.Message;
         }
         else
         {
            ((MainWindow)System.Windows.Application.Current.MainWindow).WideIndex = e.PackageIndex;

            //Do treatment
            this.ComputePackageList();
         }
      }

      private void ComputePackageList()
      {
         foreach (var package in ((MainWindow)System.Windows.Application.Current.MainWindow).WideIndex.Packages)
         {
            if (package.Type == "activex" || package.Type == "clickonce")
            {
               this.packages.Add(package);
            }
         }

         lbPackages.Visibility = Visibility.Visible;
         txtStatus.Visibility = Visibility.Hidden;
      }

      private void OnSelect(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<Package>((Package)lbPackages.SelectedItem));
         //OnReturn(new ReturnEventArgs<Package>((Package)lbPackages.SelectedItem));
      }

      private void OnCancel(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<Package>(null));
         //OnReturn(new ReturnEventArgs<Package>(null));
      }

   }
}