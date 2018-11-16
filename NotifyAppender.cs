using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using log4net;
using log4net.Core;

namespace eWamLauncher
{
   public class NotifyAppender : AppenderSkeleton, INotifyPropertyChanged
   {
      private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      #region Members and events
      private static string _notification;
      private event PropertyChangedEventHandler _propertyChanged;

      public event PropertyChangedEventHandler PropertyChanged
      {
         add { _propertyChanged += value; }
         remove { _propertyChanged -= value; }
      }
      #endregion

      /// <summary>
      /// Get or set the notification message.
      /// </summary>
      public string Notification
      {
         get
         {
            return _notification; ;
         }
         set
         {
            if (_notification != value)
            {
               _notification = value;
               OnChange();
            }
         }
      }

      /// <summary>
      /// Raise the change notification.
      /// </summary>
      private void OnChange()
      {
         PropertyChangedEventHandler handler = _propertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(string.Empty));
         }
      }

      public static NotifyAppender GetAppender()
      {
         foreach (ILog log in LogManager.GetCurrentLoggers())
         {
            foreach (IAppender appender in log.Logger.Repository.GetAppenders())
            {
               if (appender is NotifyAppender)
               {
                  return appender as NotifyAppender;
               }
            }
         }
         return null;
      }

      /// <summary>
      /// Get a reference to the log instance.
      /// </summary>
      public NotifyAppender Appender
      {
         get
         {
            return NotifyAppender.GetAppender();
         }

      }

      /// <summary>
      /// Append the log information to the notification.
      /// </summary>
      /// <param name="loggingEvent">The log event.</param>
      protected override void Append(LoggingEvent loggingEvent)
      {
         StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
         Layout.Format(writer, loggingEvent);
         Notification += writer.ToString();
      }
   }
}
