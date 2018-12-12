using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using log4net;


namespace eWamLauncher
{

   public class PackageListCompletedEventArgs : EventArgs
   {
      public PackageListCompletedEventArgs(WideIndex index)
      {
         this.PackageIndex = index;
      }

      public Exception Error;
      public bool Cancelled;

      public WideIndex PackageIndex { get; }
   }

   public delegate void PackageListCompletedHandler(object sender, PackageListCompletedEventArgs e);

   public class PackageIndexGetter
   {
      private static readonly ILog log = LogManager.GetLogger(typeof(PackageIndexGetter));

      private string url;
      private WideIndex packageIndex;

      public event PackageListCompletedHandler PackageListCompleted;


      public PackageIndexGetter(string url)
      {
         this.url = url;
      }

      public void GetPackages()
      {
         WebClient wc = new WebClient();
         wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(this.OnPackageIndexDownloaded);
         wc.DownloadDataAsync(new Uri(this.url));
      }


      private void OnPackageIndexDownloaded(object sender, DownloadDataCompletedEventArgs e)
      {
         log.Info(System.Reflection.MethodBase.GetCurrentMethod().ToString());

         try
         {
            if (e.Cancelled == true)
            {
               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : package-index.xml download cancelled");
            }
            else if (e.Error != null)
            {
               log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + e.Error.Message);
            }
            else
            {
               byte[] raw = e.Result;

               String webData = System.Text.Encoding.UTF8.GetString(raw);
               XmlSerializer serializer = new XmlSerializer(typeof(WideIndex));
               MemoryStream memStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(webData));
               this.packageIndex = (WideIndex)serializer.Deserialize(memStream);
            }

            PackageListCompletedEventArgs args = new PackageListCompletedEventArgs(packageIndex);
            args.Cancelled = e.Cancelled;
            args.Error = e.Error;

            this.PackageListCompleted(this, args);
         }
         catch (Exception exception)
         {
            log.Error(System.Reflection.MethodBase.GetCurrentMethod().ToString() + " : " + exception.Message);
         }
      }


   }
}
