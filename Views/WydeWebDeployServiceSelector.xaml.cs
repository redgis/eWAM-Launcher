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
   /// Interaction logic for WydeWebDeployServiceSelector.xaml
   /// Step for selecting the service to be deployed
   /// </summary>
   public partial class WydeWebDeployServiceSelector : PageFunction<WWService>, INotifyPropertyChanged
   {
      public List<WWService> _services;
      public List<WWService> services { get { return _services; } set { _services = value; this.NotifyPropertyChanged(); } }

      public event WizardReturnEventHandler<WWService> WizardReturn;

      public event PropertyChangedEventHandler PropertyChanged;

      // This method is called by the Set accessor of each property.
      // The CallerMemberName attribute that is applied to the optional propertyName
      // parameter causes the property name of the caller to be substituted as an argument.
      private void NotifyPropertyChanged(string propertyName = "")
      {
         this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public WydeWebDeployServiceSelector(WNetConf wNetConf)
      {
         InitializeComponent();
         this.DataContext = this;

         this.services = wNetConf.services.ToList();
         //((WNetConf)this.DataContext).services.ToList();
      }

      private void OnSelect(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<WWService>((WWService)lbServices.SelectedItem));
         //OnReturn(new ReturnEventArgs<WWService>((WWService)lbServices.SelectedItem));
      }

      private void OnCancel(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<WWService>(null));
         //OnReturn(new ReturnEventArgs<WWService>(null));
      }
   }
}
