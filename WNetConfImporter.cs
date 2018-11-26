using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Xml.Serialization;
using System.IO;

namespace eWamLauncher
{


   [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
   [DataContract(Name = "eAuthenticate")]
   public enum eAuthenticate
   {
      [XmlEnum("")]
      [EnumMember(Value = "-1")]
      Default = -1,

      [EnumMember(Value = "0")]
      [XmlEnum("0")]
      None = 0,

      [EnumMember(Value = "1")]
      [XmlEnum("1")]
      Anonymous = 1,

      [EnumMember(Value = "2")]
      [XmlEnum("2")]
      Logged = 2,

      [EnumMember(Value = "3")]
      [XmlEnum("3")]
      Prompt = 3,

      [EnumMember(Value = "4")]
      [XmlEnum("4")]
      Selected = 4
   }


   [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
   [DataContract(Name = "eYesNo")]
   public enum eYesNo
   {
      [XmlEnum("")]
      [EnumMember(Value = "-1")]
      Default = -1,

      [EnumMember(Value = "0")]
      [XmlEnum("0")]
      No = 0,

      [EnumMember(Value = "1")]
      [XmlEnum("1")]
      Yes = 1
   }

   [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
   [DataContract(Name = "eDayOfWeek")]
   public enum eDayOfWeek
   {
      [EnumMember(Value = "-1")]
      [XmlEnum("-1")]
      Never = -1,

      [EnumMember(Value = "0")]
      [XmlEnum("0")]
      EveryDay = 0,

      [EnumMember(Value = "1")]
      [XmlEnum("1")]
      Monday = 1,

      [EnumMember(Value = "2")]
      [XmlEnum("2")]
      Tuesday = 2,

      [EnumMember(Value = "3")]
      [XmlEnum("3")]
      Wednesday = 3,

      [EnumMember(Value = "4")]
      [XmlEnum("4")]
      Thursday = 4,

      [EnumMember(Value = "5")]
      [XmlEnum("5")]
      Friday = 5,

      [EnumMember(Value = "6")]
      [XmlEnum("6")]
      Saturday = 6,

      [EnumMember(Value = "7")]
      [XmlEnum("7")]
      Sunday = 7
   }

   [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
   [DataContract(Name = "eStopMode")]
   public enum eStopMode
   {
      [EnumMember(Value = "0")]
      [XmlEnum("0")]
      TimeToWaitForLastSession = 0,

      [EnumMember(Value = "1")]
      [XmlEnum("1")]
      InfiniteWaitForLastSession = 1,

      [EnumMember(Value = "2")]
      [XmlEnum("2")]
      InfiniteWaitForLastRequest = 2,

      [EnumMember(Value = "3")]
      [XmlEnum("3")]
      AtOnce = 3
   }

   [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
   [DataContract(Name = "eCompression")]
   public enum eCompression
   {
      [EnumMember(Value = "0")]
      [XmlEnum("0")]
      Uncompressed = 0,

      [EnumMember(Value = "1")]
      [XmlEnum("1")]
      Level1 = 1,

      [EnumMember(Value = "2")]
      [XmlEnum("2")]
      Level2 = 2,

      [EnumMember(Value = "3")]
      [XmlEnum("3")]
      Level3 = 3,

      [EnumMember(Value = "4")]
      [XmlEnum("4")]
      Level4 = 4,

      [EnumMember(Value = "5")]
      [XmlEnum("5")]
      Level5 = 5,

      [EnumMember(Value = "6")]
      [XmlEnum("6")]
      Level6 = 6,

      [EnumMember(Value = "7")]
      [XmlEnum("7")]
      Level7 = 7,

      [EnumMember(Value = "8")]
      [XmlEnum("8")]
      Level8 = 8,

      [EnumMember(Value = "9")]
      [XmlEnum("9")]
      Level9 = 9


   }

   // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
   public partial class WydeNetWorkConfiguration : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         WydeNetWorkConfiguration clone = (WydeNetWorkConfiguration)this.MemberwiseClone();

         return clone;
      }

      public static WydeNetWorkConfiguration CreateFromWNetConfIni(string wNetConfFilename)
      {
         FileStream reader = new FileStream(wNetConfFilename, FileMode.Open);
         XmlSerializer serializer = new XmlSerializer(typeof(WydeNetWorkConfiguration));
         WydeNetWorkConfiguration wNetConf = (WydeNetWorkConfiguration)serializer.Deserialize(reader);
         reader.Close();

         return wNetConf;
      }

      private ClientConfiguration clientConfigurationField;

      private ServerConfiguration servicesManagerField;

      [Description("The client configuration has two main parts, the services and the overall security.")]
      public ClientConfiguration ClientConfiguration
      {
         get
         {
            return this.clientConfigurationField;
         }
         set
         {
            this.clientConfigurationField = value;
         }
      }

      [Description("The server configuration has two main parts, the services and the overall security.")]
      public ServerConfiguration ServicesManager
      {
         get
         {
            return this.servicesManagerField;
         }
         set
         {
            this.servicesManagerField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ClientConfiguration : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ClientConfiguration clone = (ClientConfiguration) this.MemberwiseClone();
         clone.Services = new ObservableCollection<ClientConfigurationService>();

         foreach (ClientConfigurationService service in this.Services)
         {
            clone.Services.Add((ClientConfigurationService)service.Clone());
         }

         return clone;
      }

      public ClientConfiguration()
      {
         this.Services = new ObservableCollection<ClientConfigurationService>();
      }

      private ObservableCollection<ClientConfigurationService> servicesField;

      private ConfigurationSecurity securityField;

      [System.Xml.Serialization.XmlArrayItemAttribute("Service", IsNullable = false)]
      [Description("The client services are the list of declared services that you can invoke.")]
      public ObservableCollection<ClientConfigurationService> Services
      {
         get
         {
            return this.servicesField;
         }
         set
         {
            this.servicesField = value;
         }
      }

      [Description("The client global security parameters.")]
      public ConfigurationSecurity Security
      {
         get
         {
            return this.securityField;
         }
         set
         {
            this.securityField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ClientConfigurationService : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ClientConfigurationService clone = (ClientConfigurationService)this.MemberwiseClone();
         clone.ServicesManagers = new ObservableCollection<ClientConfigurationServicesManager>();

         foreach (ClientConfigurationServicesManager serviceManagers in this.ServicesManagers)
         {
            clone.ServicesManagers.Add((ClientConfigurationServicesManager)serviceManagers.Clone());
         }

         return clone;
      }

      public ClientConfigurationService()
      {
         this.ServicesManagers = new ObservableCollection<ClientConfigurationServicesManager>();
      }

      private string nameField;

      private ObservableCollection<ClientConfigurationServicesManager> servicesManagersField;

      private ConfigurationSecurity securityField;

      private string aliasesField;

      [Description("The name of the service.  This name is the one used to invoke a given service.")]
      public string Name
      {
         get
         {
            return this.nameField;
         }
         set
         {
            this.nameField = value;
         }
      }

      [Description("List of remote WSM that can handle the request for the service. By " +
         "double-clicking on this node, you can declare one of two kinds of remote service " +
         "managers, either a standard manager with which communication occurs via standard " +
         "TCP/IP or an HTTP manager with which communication occurs via an HTTP server. The " +
         "two types of service manager have slightly different parameters.")]
      [System.Xml.Serialization.XmlArrayItemAttribute("ServicesManager", IsNullable = false)]
      public ObservableCollection<ClientConfigurationServicesManager> ServicesManagers
      {
         get
         {
            return this.servicesManagersField;
         }
         set
         {
            this.servicesManagersField = value;
         }
      }

      [Description("The service security parameters overriding client global parameter.")]
      [ExpandableObject]
      public ConfigurationSecurity Security
      {
         get
         {
            return this.securityField;
         }
         set
         {
            this.securityField = value;
         }
      }

      [Description("Name of the service on the server machine.If this field is blank, then the " +
         "Name field is used for the service name on the server machine.")]
      public string Aliases
      {
         get
         {
            return this.aliasesField;
         }
         set
         {
            this.aliasesField = value;
         }
      }
   }

   //
   //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
   //public partial class ClientConfigurationServiceServicesManagers
   //   {

   //    private ClientConfigurationServicesManager servicesManagerField;

   //    
   //    public ClientConfigurationServicesManager ServicesManager {
   //        get {
   //            return this.servicesManagerField;
   //        }
   //        set {
   //            this.servicesManagerField = value;
   //        }
   //    }
   //}

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ClientConfigurationServicesManager : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ClientConfigurationServicesManager clone = (ClientConfigurationServicesManager)this.MemberwiseClone();

         return clone;
      }

      private string httpHostField;

      private int httpPortField;

      private string proxyHostField;

      private int proxyPortField;

      private string httpAdditionalHeadersField;

      private string extensionField;

      private string hostField;

      private int portField;

      private int nbMaxConcurrentRequestsField;

      private string aliasField;

      private int timeBeforePollingField;

      private int emergencyField;

      private eCompression compressionField;

      private ConfigurationSecurity securityField;

      private int connectTimeoutField;
      [DefaultValue("")]      [Description("Name of host computer of the HTTP server.")]      public string HttpHost
      {
         get
         {
            return this.httpHostField;
         }
         set
         {
            this.httpHostField = value;
         }
      }
      [XmlIgnore]
      public bool HttpHostSpecified { get { return HttpHost != null && HttpHost != ""; } }

      [DefaultValue(0)]
      [Description("Port on host computer of the HTTP server.")]
      public int HttpPort
      {
         get
         {
            return this.httpPortField;
         }
         set
         {
            this.httpPortField = value;
         }
      }
      [XmlIgnore]
      public bool HttpPortSpecified { get { return HttpPort > 0; } }

      [DefaultValue("")]
      [Description("Name of any proxy computer used to attain the HTTP server.")]
      public string ProxyHost
      {
         get
         {
            return this.proxyHostField;
         }
         set
         {
            this.proxyHostField = value;
         }
      }
      [XmlIgnore]
      public bool ProxyHostSpecified { get { return ProxyHost != null && ProxyHost != ""; } }
      
      [DefaultValue(0)]
      [Description("Port on proxy computer used to attain the HTTP server.")]
      public int ProxyPort
      {
         get
         {
            return this.proxyPortField;
         }
         set
         {
            this.proxyPortField = value;
         }
      }
      [XmlIgnore]
      public bool ProxyPortSpecified { get { return ProxyPort > 0; } }

      [DefaultValue("")]
      [Description("By default set to false (value 0). Set to true (value 1) means wydeweb's " +
         "requests will contain the Content-Type header and the Cookie header (that contains " +
         "the session id).")]
      public string HttpAdditionalHeaders
      {
         get
         {
            return this.httpAdditionalHeadersField;
         }
         set
         {
            this.httpAdditionalHeadersField = value;
         }
      }
      [XmlIgnore]
      public bool HttpAdditionalHeadersSpecified { get { return HttpAdditionalHeaders != null && HttpAdditionalHeaders != ""; } }

      [DefaultValue("")]
      [Description("DLL on HTTP server that the HTTP server calls to communicate with WSM on " +
         "the server.  By default, this DLL is installed on the server in the directory " +
         "'scripts/wyseman/wsmisapi.dll'.")]
      public string Extension
      {
         get
         {
            return this.extensionField;
         }
         set
         {
            this.extensionField = value;
         }
      }
      [XmlIgnore]
      public bool ExtensionSpecified { get { return Extension != null && Extension != ""; } }

      [DefaultValue("")]
      [Description("Name of host computer where WSM executes.")]
      public string Host
      {
         get
         {
            return this.hostField;
         }
         set
         {
            this.hostField = value;
         }
      }
      [XmlIgnore]
      public bool HostSpecified { get {return Host != null && Host != "";} }

      [DefaultValue(0)]
      [Description("Port on host computer through which communication occurs.")]
      public int Port
      {
         get
         {
            return this.portField;
         }
         set
         {
            this.portField = value;
         }
      }
      [XmlIgnore]
      public bool PortSpecified { get { return Port > 0; } }

      [Description("This value is used to load-balance between WSM in the case where you have specified several Service Managers for a given service.  If this value is 100 for one Service Manager and 200 for another, then 2 times as many requests will directed to the second as the first.")]
      public int NbMaxConcurrentRequests
      {
         get
         {
            return this.nbMaxConcurrentRequestsField;
         }
         set
         {
            this.nbMaxConcurrentRequestsField = value;
         }
      }

      [Description("Name of the service on the server machine.  If this field is blank, then the Name field is used for the service name on the server machine.")]
      public string Alias
      {
         get
         {
            return this.aliasField;
         }
         set
         {
            this.aliasField = value;
         }
      }

      [Description("")]
      public int TimeBeforePolling
      {
         get
         {
            return this.timeBeforePollingField;
         }
         set
         {
            this.timeBeforePollingField = value;
         }
      }

      [Description("")]
      public int Emergency
      {
         get
         {
            return this.emergencyField;
         }
         set
         {
            this.emergencyField = value;
         }
      }

      [Description("Specifies the level of compression to use when sending information to the server.  The compression level ranges from 0 (no compression) to 9 (maximal compression).  Experience has shown that a compression level of 1 yields the best results for most cases, hardly slowing down processing, yet having a substantial impact on the traffic.  The actual compression used is the higher level between that specified here and that specified on the server.")]
      public eCompression Compression
      {
         get
         {
            return this.compressionField;
         }
         set
         {
            this.compressionField = value;
         }
      }

      [Description("The service security parameters overriding client global parameter.")]
      [ExpandableObject]
      public ConfigurationSecurity Security
      {
         get
         {
            return this.securityField;
         }
         set
         {
            this.securityField = value;
         }
      }

      [Description("Time of inactivity after which the connection will be closed by the server.")]
      public int ConnectTimeout
      {
         get
         {
            return this.connectTimeoutField;
         }
         set
         {
            this.connectTimeoutField = value;
         }
      }
   }

   //
   //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   //public partial class ClientConfigurationSecurity
   //{

   //   private eYesNo encryptionField;

   //   private eAuthenticate authenticateField;

   //   private string userNameField;

   //   private string userPasswordField;

   //   private string userDomainField;

   //   
   //   public eYesNo Encryption
   //   {
   //      get
   //      {
   //         return this.encryptionField;
   //      }
   //      set
   //      {
   //         this.encryptionField = value;
   //      }
   //   }

   //   
   //   public eAuthenticate Authenticate
   //   {
   //      get
   //      {
   //         return this.authenticateField;
   //      }
   //      set
   //      {
   //         this.authenticateField = value;
   //      }
   //   }

   //   
   //   public string UserName
   //   {
   //      get
   //      {
   //         return this.userNameField;
   //      }
   //      set
   //      {
   //         this.userNameField = value;
   //      }
   //   }

   //   
   //   public string UserPassword
   //   {
   //      get
   //      {
   //         return this.userPasswordField;
   //      }
   //      set
   //      {
   //         this.userPasswordField = value;
   //      }
   //   }

   //   
   //   public string UserDomain
   //   {
   //      get
   //      {
   //         return this.userDomainField;
   //      }
   //      set
   //      {
   //         this.userDomainField = value;
   //      }
   //   }
   //}

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfiguration : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfiguration clone = (ServerConfiguration)this.MemberwiseClone();
         clone.Services = new ObservableCollection<ServerConfigurationService>();

         foreach (ServerConfigurationService service in this.Services)
         {
            clone.Services.Add((ServerConfigurationService)service.Clone());
         }

         return clone;
      }

      public ServerConfiguration()
      {
         this.Services = new ObservableCollection<ServerConfigurationService>();
      }

      private ObservableCollection<ServerConfigurationService> servicesField;

      private ConfigurationSecurity securityField;

      private ServerConfigurationTraceConfig traceConfigField;

      [Description("The server services are the list of declared services that clients can invoke in this WSM.")]
      [System.Xml.Serialization.XmlArrayItemAttribute("Service", IsNullable = false)]
      public ObservableCollection<ServerConfigurationService> Services
      {
         get
         {
            return this.servicesField;
         }
         set
         {
            this.servicesField = value;
         }
      }

      [Description("The server global security parameters")]
      public ConfigurationSecurity Security
      {
         get
         {
            return this.securityField;
         }
         set
         {
            this.securityField = value;
         }
      }

      [Description("If you need to know how the wyde service manager statistics evolve, you can trace (log) these information.")]
      public ServerConfigurationTraceConfig TraceConfig
      {
         get
         {
            return this.traceConfigField;
         }
         set
         {
            this.traceConfigField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfigurationService : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfigurationService clone = (ServerConfigurationService)this.MemberwiseClone();
         clone.EnvironmentVariables = new ObservableCollection<NetConfEnvironmentVariable>();

         foreach (NetConfEnvironmentVariable variable in this.EnvironmentVariables)
         {
            clone.EnvironmentVariables.Add((NetConfEnvironmentVariable)variable.Clone());
         }

         return clone;
      }

      public ServerConfigurationService()
      {
         this.EnvironmentVariables = new ObservableCollection<NetConfEnvironmentVariable>();
      }

      private string nameField;

      private eCompression compressionField;

      private eYesNo encryptionField;

      private string aliasesField;

      private ObservableCollection<NetConfEnvironmentVariable> environmentVariablesField;

      private ServerConfigurationProcess processField;

      [Description("The name of the service.  This name is the one that clients use (taken from " +
         "the client 'alias' parameter if not blank, and otherwise taken from the client service " +
         "name) to invoke the service.")]
      public string Name
      {
         get
         {
            return this.nameField;
         }
         set
         {
            this.nameField = value;
         }
      }

      [Description("Data is exchanged between the clients and the WSM servers. This data can " +
         "be compressed to minimize network traffic.  You can specify either at the client side " +
         "or the server side the level of compression to use (0 for no encryption and 9 for " +
         "maximum encryption). For a given client executing the process, the highest level of " +
         "compression, either that specified by the client or that specified by the server, is " +
         "used. Experience has proven that for typical applications, a compression level of 1 " +
         "suits well. This compression level offers a good balance between network traffic " +
         "reduction and performance, with very little time consumed in compressing and " +
         "decompressing.More data intensive services may opt for a higher compression level. " +
         "WSM uses ZLIB compression.")]
      public eCompression Compression
      {
         get
         {
            return this.compressionField;
         }
         set
         {
            this.compressionField = value;
         }
      }

      [Description("Data is exchanged between the clients and the WSM servers. This data can " +
         "be encrypted. You specify at the client side whether to activate encryption or not, and " +
         "at the server end whether encryption is required or not. WSM uses the SSPI encryption " +
         "technique, which has minimal impact on performance. For data encryption, the client " +
         "might need to identify himself with the server.")]
      public eYesNo Encryption
      {
         get
         {
            return this.encryptionField;
         }
         set
         {
            this.encryptionField = value;
         }
      }

      [Description("Global list of aliases by which this service can be invoked.")]
      public string Aliases
      {
         get
         {
            return this.aliasesField;
         }
         set
         {
            this.aliasesField = value;
         }
      }

      [Description("Environment Variables embeded in this process.")]
      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      public ObservableCollection<NetConfEnvironmentVariable> EnvironmentVariables
      {
         get
         {
            return this.environmentVariablesField;
         }
         set
         {
            this.environmentVariablesField = value;
         }
      }

      [Description("")]
      [ExpandableObject]
      public ServerConfigurationProcess Process
      {
         get
         {
            return this.processField;
         }
         set
         {
            this.processField = value;
         }
      }

   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfigurationProcess : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfigurationProcess clone = (ServerConfigurationProcess)this.MemberwiseClone();
         clone.EnvironmentVariables = new ObservableCollection<NetConfEnvironmentVariable>();

         foreach (NetConfEnvironmentVariable variable in this.EnvironmentVariables)
         {
            clone.EnvironmentVariables.Add((NetConfEnvironmentVariable)variable.Clone());
         }

         return clone;
      }

      public ServerConfigurationProcess()
      {
         this.EnvironmentVariables = new ObservableCollection<NetConfEnvironmentVariable>();
      }

      private string commandLineField;

      private int nbMaxProcessesField;

      private int nbMaxConcurrentRequestsField;

      private eYesNo useResponsiveProcessesOnlyField;

      private string currentDirectoryField;

      private string userNameField;

      private string userDomainField;

      private string userPasswordField;

      private ObservableCollection<NetConfEnvironmentVariable> environmentVariablesField;

      private ServerConfigurationProcessManagement processManagementField;

      private ServerConfigurationLoadBalancing loadBalancingField;

      [Description("Command line to launch when process is invoked. This command line includes the executable and any parameters, just as if you were launching the service from the DOS command line.")]
      public string CommandLine
      {
         get
         {
            return this.commandLineField;
         }
         set
         {
            this.commandLineField = value;
         }
      }

      [Description("It can be advisable to separate execution of client's requests into several processes. This way, not all client execution runs in the same Windows process, and any problem such as a crash will have limited damage on the executing sessions. This number is the maximum number of processes that this service will have running at one time.")]
      public int NbMaxProcesses
      {
         get
         {
            return this.nbMaxProcessesField;
         }
         set
         {
            this.nbMaxProcessesField = value;
         }
      }

      [Description("A given client request is assigned to a thread. Multiple simultaneous requests can thus be processed.  Note that the number of threads isn't necessarily equal to the number of current sessions, but rather to the number of requests that at any given time are being executed simultaneously.")]
      public int NbMaxConcurrentRequests
      {
         get
         {
            return this.nbMaxConcurrentRequestsField;
         }
         set
         {
            this.nbMaxConcurrentRequestsField = value;
         }
      }

      [Description("")]
      public eYesNo UseResponsiveProcessesOnly
      {
         get
         {
            return this.useResponsiveProcessesOnlyField;
         }
         set
         {
            this.useResponsiveProcessesOnlyField = value;
         }
      }

      [Description("The directory the service is to be switched to before launching it.")]
      public string CurrentDirectory
      {
         get
         {
            return this.currentDirectoryField;
         }
         set
         {
            this.currentDirectoryField = value;
         }
      }

      [Description("User name of system account authorized to launch the service.")]
      public string UserName
      {
         get
         {
            return this.userNameField;
         }
         set
         {
            this.userNameField = value;
         }
      }

      [Description("Reserved for future use.")]
      public string UserDomain
      {
         get
         {
            return this.userDomainField;
         }
         set
         {
            this.userDomainField = value;
         }
      }

      [Description("Password of system account authorized to launch the service.")]
      public string UserPassword
      {
         get
         {
            return this.userPasswordField;
         }
         set
         {
            this.userPasswordField = value;
         }
      }

      [Description("Environment Variables embeded in this process.")]
      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      public ObservableCollection<NetConfEnvironmentVariable> EnvironmentVariables
      {
         get
         {
            return this.environmentVariablesField;
         }
         set
         {
            this.environmentVariablesField = value;
         }
      }

      [Description("Handles how and when the process is stopped.")]
      [ExpandableObject]
      public ServerConfigurationProcessManagement ProcessManagement
      {
         get
         {
            return this.processManagementField;
         }
         set
         {
            this.processManagementField = value;
         }
      }

      [Description("Defines how to divide sessions among processes.")]
      [ExpandableObject]
      public ServerConfigurationLoadBalancing LoadBalancing
      {
         get
         {
            return this.loadBalancingField;
         }
         set
         {
            this.loadBalancingField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfigurationLoadBalancing : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfigurationLoadBalancing clone = (ServerConfigurationLoadBalancing)this.MemberwiseClone();

         return clone;
      }

      private eYesNo forceRandomField;

      private int loadBalancingByNbSessionsField;

      private int loadBalancingByCPUField;

      private int loadBalancingByMemoryField;

      [Description("By default to true. Sessions are dividing randomly. This is corresponding of the previous behavior of Wyseman.")]
      public eYesNo ForceRandom
      {
         get
         {
            return this.forceRandomField;
         }
         set
         {
            this.forceRandomField = value;
         }
      }

      [Description("If force random is set to false. Specifies the weight (percentage) for a dividing by number of sessions.")]
      public int LoadBalancingByNbSessions
      {
         get
         {
            return this.loadBalancingByNbSessionsField;
         }
         set
         {
            this.loadBalancingByNbSessionsField = value;
         }
      }

      [Description("If force random is set to false. Specifies the weight (percentage) for a dividing by CPU.")]
      public int LoadBalancingByCPU
      {
         get
         {
            return this.loadBalancingByCPUField;
         }
         set
         {
            this.loadBalancingByCPUField = value;
         }
      }

      [Description("If force random is set to false. Specifies the weight (percentage) for a dividing by memory.")]
      public int LoadBalancingByMemory
      {
         get
         {
            return this.loadBalancingByMemoryField;
         }
         set
         {
            this.loadBalancingByMemoryField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfigurationProcessManagement : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfigurationProcessManagement clone = (ServerConfigurationProcessManagement)this.MemberwiseClone();

         return clone;
      }

      private int nbMaxSimultaneousSessionsField;

      private int requestOnMaxSimultaneousSessionsQueuedField;

      private int maxMemoryUsageField;

      private int requestOnMaxMemoryUsageQueuedField;

      private eYesNo stopAfterLastSessionField;

      private int stopAfterRunningTimeField;

      private int stopAfterInactivityTimeoutField;

      private int stopAfterMemoryLimitField;

      private int stopAfterNbSessionsField;

      private PeriodicalStop periodicalStopField;

      private AutomaticStart automaticStartField;

      [Description("")]
      public int NbMaxSimultaneousSessions
      {
         get
         {
            return this.nbMaxSimultaneousSessionsField;
         }
         set
         {
            this.nbMaxSimultaneousSessionsField = value;
         }
      }

      [Description("")]
      public int RequestOnMaxSimultaneousSessionsQueued
      {
         get
         {
            return this.requestOnMaxSimultaneousSessionsQueuedField;
         }
         set
         {
            this.requestOnMaxSimultaneousSessionsQueuedField = value;
         }
      }

      [Description("")]
      public int MaxMemoryUsage
      {
         get
         {
            return this.maxMemoryUsageField;
         }
         set
         {
            this.maxMemoryUsageField = value;
         }
      }

      [Description("")]
      public int RequestOnMaxMemoryUsageQueued
      {
         get
         {
            return this.requestOnMaxMemoryUsageQueuedField;
         }
         set
         {
            this.requestOnMaxMemoryUsageQueuedField = value;
         }
      }

      [Description("If 'Yes', the process will be stopped when the last session using it quits. When the next client requests the service, a new process is launched.")]
      public eYesNo StopAfterLastSession
      {
         get
         {
            return this.stopAfterLastSessionField;
         }
         set
         {
            this.stopAfterLastSessionField = value;
         }
      }

      [Description("Duration in minutes. 0 for unlimited. Specifies the amount of time that a process can stay alive. After this time has counted down, the process will be stopped.")]
      public int StopAfterRunningTime
      {
         get
         {
            return this.stopAfterRunningTimeField;
         }
         set
         {
            this.stopAfterRunningTimeField = value;
         }
      }

      [Description("Duration in minutes. 0 for unlimited. Specifies the amount of inactive time (since the last client request was processed) until the process is stopped.")]
      public int StopAfterInactivityTimeout
      {
         get
         {
            return this.stopAfterInactivityTimeoutField;
         }
         set
         {
            this.stopAfterInactivityTimeoutField = value;
         }
      }

      [Description("Integer amount in MB. 0 for unlimited. Specifies the amount of total memory allocated before the process is stopped.")]
      public int StopAfterMemoryLimit
      {
         get
         {
            return this.stopAfterMemoryLimitField;
         }
         set
         {
            this.stopAfterMemoryLimitField = value;
         }
      }

      [Description("0 for unlimited. The process is stopped after the specified number of sessions have been created.")]
      public int StopAfterNbSessions
      {
         get
         {
            return this.stopAfterNbSessionsField;
         }
         set
         {
            this.stopAfterNbSessionsField = value;
         }
      }

      [Description("Specifies with what periodicity the process is automatically stopped.")]
      [ExpandableObject]
      public PeriodicalStop PeriodicalStop
      {
         get
         {
            return this.periodicalStopField;
         }
         set
         {
            this.periodicalStopField = value;
         }
      }

      [Description("For certain systems, it might be necessary to launch processes automatically. WSM allows you to choose a period and the number of processes that will be running during this period. It means that during this period, whatever it happens, there will be at least the chosen number of processes.")]
      [ExpandableObject]
      public AutomaticStart AutomaticStart
      {
         get
         {
            return this.automaticStartField;
         }
         set
         {
            this.automaticStartField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class PeriodicalStop : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         PeriodicalStop clone = (PeriodicalStop)this.MemberwiseClone();

         return clone;
      }

      private int stopAtTimeField;

      private eDayOfWeek stopEveryField;

      private int cannotBeLaunchedDuringTimeField;

      private eStopMode stoppingModeField;

      private int timeToEndSessionsField;

      [Description("Time of day in seconds. Specifies in military time at what time of day the process should be stopped. This option is useful when you need to free up a system to run a regular batch.")]
      public int StopAtTime
      {
         get
         {
            return this.stopAtTimeField;
         }
         set
         {
            this.stopAtTimeField = value;
         }
      }

      [Description("Specifies the day of the week when the process should be stopped. This option is useful when you need to free up a system to run a regular batch.")]
      public eDayOfWeek StopEvery
      {
         get
         {
            return this.stopEveryField;
         }
         set
         {
            this.stopEveryField = value;
         }
      }

      [Description("Duration in seconds. Once the process is flagged to stop, then no user can request the service for the specified amount of time. This option is useful to hold off users until a batch terminates.")]
      public int CannotBeLaunchedDuringTime
      {
         get
         {
            return this.cannotBeLaunchedDuringTimeField;
         }
         set
         {
            this.cannotBeLaunchedDuringTimeField = value;
         }
      }

      [Description("Specifies when the session is to stop once it is flagged to stop by one of the above periodical stop parameters.")]
      public eStopMode StoppingMode
      {
         get
         {
            return this.stoppingModeField;
         }
         set
         {
            this.stoppingModeField = value;
         }
      }

      [Description("Amount of time to wait if the 'Stopping mode' is set to 'Time to wait for last session'.")]
      public int TimeToEndSessions
      {
         get
         {
            return this.timeToEndSessionsField;
         }
         set
         {
            this.timeToEndSessionsField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class AutomaticStart : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         AutomaticStart clone = (AutomaticStart)this.MemberwiseClone();

         return clone;
      }

      private int nbProcessesField;

      private eDayOfWeek fromDayField;

      private eDayOfWeek toDayField;

      private int fromTimeField;

      private int toTimeField;

      private eYesNo preloadField;

      [Description("Specifies number of processes that have to run during the period that you will choose.")]
      public int NbProcesses
      {
         get
         {
            return this.nbProcessesField;
         }
         set
         {
            this.nbProcessesField = value;
         }
      }

      [Description("Specify start of period of the week that need this automatic start.")]
      public eDayOfWeek FromDay
      {
         get
         {
            return this.fromDayField;
         }
         set
         {
            this.fromDayField = value;
         }
      }

      [Description("Specify end of period of the week that need this automatic start.")]
      public eDayOfWeek ToDay
      {
         get
         {
            return this.toDayField;
         }
         set
         {
            this.toDayField = value;
         }
      }

      [Description("Time of day in seconds. Specify start of period of the day that need this automatic start.")]
      public int FromTime
      {
         get
         {
            return this.fromTimeField;
         }
         set
         {
            this.fromTimeField = value;
         }
      }

      [Description("Time of day in seconds. Specify end of period of the day that need this automatic start.")]
      public int ToTime
      {
         get
         {
            return this.toTimeField;
         }
         set
         {
            this.toTimeField = value;
         }
      }

      [Description("yes/no : when processes are automatically started by WySeMan, CPP dlls are preloaded (WedRPCServer.exe is started with command parameter /PRELOAD). With option /PRELOADMETHOD You can also add your own code to preload any other objects from database such as products, workflow configuration...")]
      public eYesNo Preload
      {
         get
         {
            return this.preloadField;
         }
         set
         {
            this.preloadField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ConfigurationSecurity : ICloneable, INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ConfigurationSecurity clone = (ConfigurationSecurity)this.MemberwiseClone();

         return clone;
      }

      private eYesNo encryptionField;

      private eAuthenticate authenticateField;

      private string userNameField;

      private string userPasswordField;

      private string userDomainField;

      private string loginsField;

      [Description("Specifies whether or not to use encryption between the client and the server.")]
      public eYesNo Encryption
      {
         get
         {
            return this.encryptionField;
         }
         set
         {
            this.encryptionField = value;
         }
      }

      [Description("Determines how the client identifies himself to the server.")]
      public eAuthenticate Authenticate
      {
         get
         {
            return this.authenticateField;
         }
         set
         {
            this.authenticateField = value;
         }
      }

      [Description("Name of user if the authentication level is 'selected'.")]
      public string UserName
      {
         get
         {
            return this.userNameField;
         }
         set
         {
            this.userNameField = value;
         }
      }

      [Description("Id of user if the authentication level is 'selected'.")]
      public string UserPassword
      {
         get
         {
            return this.userPasswordField;
         }
         set
         {
            this.userPasswordField = value;
         }
      }

      [Description("Reserved for future use.")]
      public string UserDomain
      {
         get
         {
            return this.userDomainField;
         }
         set
         {
            this.userDomainField = value;
         }
      }

      [Description("")]
      public string Logins
      {
         get
         {
            return this.loginsField;
         }
         set
         {
            this.loginsField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class ServerConfigurationTraceConfig : INotifyPropertyChanged
   {
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

      public object Clone()
      {
         ServerConfigurationTraceConfig clone = (ServerConfigurationTraceConfig)this.MemberwiseClone();

         return clone;
      }

      private eYesNo startRunningField;

      private int timestepField;

      private eYesNo servicesField;

      private eYesNo processesField;

      [Description("Set this option to yes if you want to start tracing when you launch wyde service manager.")]
      public eYesNo StartRunning
      {
         get
         {
            return this.startRunningField;
         }
         set
         {
            this.startRunningField = value;
         }
      }

      [Description("You can define how often you want a snapshot of the datas to be taken.")]
      public int Timestep
      {
         get
         {
            return this.timestepField;
         }
         set
         {
            this.timestepField = value;
         }
      }

      [Description("Set to yes if you want to trace services datas.")]
      public eYesNo Services
      {
         get
         {
            return this.servicesField;
         }
         set
         {
            this.servicesField = value;
         }
      }

      [Description("Set to yes if you want to trace processes datas.")]
      public eYesNo Processes
      {
         get
         {
            return this.processesField;
         }
         set
         {
            this.processesField = value;
         }
      }
   }

   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class NetConfEnvironmentVariable : INotifyPropertyChanged
   {
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

      public object Clone()
      {
         NetConfEnvironmentVariable clone = (NetConfEnvironmentVariable)this.MemberwiseClone();

         return clone;
      }

      private string nameField;

      private string valueField;

      [Description("Environment variable name.")]
      public string Name
      {
         get
         {
            return this.nameField;
         }
         set
         {
            this.nameField = value;
         }
      }

      [Description("Environment variable value.")]
      public string Value
      {
         get
         {
            return this.valueField;
         }
         set
         {
            this.valueField = value;
         }
      }
   }
}
