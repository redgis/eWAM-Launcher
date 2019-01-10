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
   /// Interaction logic for WydeWebDeployLauncherSelector.xaml
   /// Step for selection of the launcher from which to get command line parameter for WydeWeb client
   /// </summary>
   public partial class WydeWebDeployLauncherSelector : PageFunction<Launcher>, INotifyPropertyChanged
   {
      private List<Launcher> _launchers= new List<Launcher>();
      public List<Launcher> launchers { get { return _launchers; } set { _launchers = value; this.NotifyPropertyChanged(); } }

      public event WizardReturnEventHandler<Launcher> WizardReturn;

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

      public WydeWebDeployLauncherSelector(List<Launcher> launchers)
      {
         InitializeComponent();
         this.DataContext = this;

         //We want to keep only the launchers uing WydeWeb.exe
         foreach(var launcher in launchers)
         {
            if (launcher.program.ToLower() == "wydeweb.exe")
            {
               this.launchers.Add(launcher);
            }
         }
      }

      private void OnSelect(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<Launcher>((Launcher)lbLaunchers.SelectedItem));
         //OnReturn(new ReturnEventArgs<Launcher>((Launcher)lbLaunchers.SelectedItem));
      }

      private void OnCancel(object sender, RoutedEventArgs e)
      {
         this.WizardReturn?.Invoke(this, new WizardReturnEventArgs<Launcher>(null));
         //OnReturn(new ReturnEventArgs<Launcher>(null));
      }
   }
}
