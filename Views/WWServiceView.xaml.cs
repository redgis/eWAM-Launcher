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
   /// </summary>
   public partial class WWServiceView : UserControl
   {
      public WWServiceView()
      {
         InitializeComponent();
      }

      private static readonly ILog log = LogManager.GetLogger(typeof(WWServiceView));

      private void OnGetWNetClientChunk(object sender, RoutedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            WWService simplifiedService = (WWService)this.DataContext;

            ClientConfigurationService service = simplifiedService.GetClientService(WNetConf.eNetConf.SingleService);

            XmlAttributes overrideAttributes = new XmlAttributes();
            overrideAttributes.XmlRoot = new XmlRootAttribute("service");
         
            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
            overrides.Add(typeof(ClientConfigurationService), overrideAttributes);

            StringWriter strWriter = new StringWriter();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }; 

            XmlWriter writer = XmlWriter.Create(strWriter, xmlWriterSettings);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ClientConfigurationService), overrides);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            xmlSerializer.Serialize(writer, service, namespaces);

            tbWNetClientChunk.Text = "<services>\n" + strWriter.ToString() + "\n</services>\n";
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
