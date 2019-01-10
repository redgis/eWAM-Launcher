using System;
using System.Collections.Generic;
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
using System.Xml;
using System.Xml.Serialization;
using log4net;

namespace eWamLauncher.Views
{
   /// <summary>
   /// Interaction logic for WWServiceView.xaml
   /// This view presents a WWService object, allowing management of the configuration 
   /// </summary>
   public partial class WWServiceView : UserControl
   {
      public WWServiceView()
      {
         InitializeComponent();
      }

      private static readonly ILog log = LogManager.GetLogger(typeof(WWServiceView));

      /// <summary>
      /// Command Handler to generate XML chunk of wNetConf, of the client-side WydeWeb configuration 
      /// for a particular service
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnGetWNetClientChunk(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            WWService simplifiedService = (WWService)this.DataContext;
            tbWNetClientChunk.Text = simplifiedService.GetWNetClientChunk();
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
   }
}
