//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System;
using System.Xml.Serialization;

namespace eWamLauncher
{

   [DataContract(Name = "WNetConf", Namespace = "http://www.wyde.com")]
   public class WNetConf : ICloneable, INotifyPropertyChanged
   {
      public WNetConf(WydeNetWorkConfiguration wydeNetworkConf = null)
      {
         this.services = new ObservableCollection<WWService>();

         if (wydeNetworkConf == null)
         {
            this.serverSecurity = new ConfigurationSecurity();
            this.clientSecurity = new ConfigurationSecurity();
            this.traceConfig = new ServerConfigurationTraceConfig();
            return;
         }

         if (wydeNetworkConf.ClientConfiguration == null)
         {
            this.clientSecurity = new ConfigurationSecurity();
         }
         else
         {
            this.clientSecurity = wydeNetworkConf.ClientConfiguration.Security;
         }

         if (wydeNetworkConf.ServicesManager == null)
         {
            this.serverSecurity = new ConfigurationSecurity();
            this.traceConfig = new ServerConfigurationTraceConfig();
         }
         else
         {
            this.serverSecurity = wydeNetworkConf.ServicesManager.Security;
            this.traceConfig = wydeNetworkConf.ServicesManager.TraceConfig;
         }

         foreach (ServerConfigurationService serverService in wydeNetworkConf.ServicesManager.Services)
         {
            foreach (ClientConfigurationService clientService in wydeNetworkConf.ClientConfiguration.Services)
            {
               if (serverService.Name == clientService.Name)
               {
                  WWService wwService = new WWService(clientService, serverService);
                  this.services.Add(wwService);
                  break;
               }
            }
         }
      }

      public object Clone()
      {
         WNetConf clone = (WNetConf)this.MemberwiseClone();

         clone.services = new ObservableCollection<WWService>();

         foreach (WWService service in this.services)
         {
            clone.services.Add((WWService)service.Clone());
         }

         return clone;
      }

      private ObservableCollection<WWService> _services;
      [DataMember()] public ObservableCollection<WWService> services { get { return _services; } set { _services = value; this.NotifyPropertyChanged(); } }

      private ConfigurationSecurity _clientSecurity;
      [Description("Security Settings.")]
      [DisplayName("Security Settings.")]
      [DataMember()] public ConfigurationSecurity clientSecurity { get { return _clientSecurity; } set { _clientSecurity = value; this.NotifyPropertyChanged(); } }

      private ConfigurationSecurity _serverSecurity;
      [Description("Security Settings.")]
      [DisplayName("Security Settings.")]
      [DataMember()] public ConfigurationSecurity serverSecurity { get { return _serverSecurity; } set { _serverSecurity = value; this.NotifyPropertyChanged(); } }

      private ServerConfigurationTraceConfig _traceConfig;
      [Description("Global Trace Settings.")]
      [DisplayName("Global Trace Settings.")]
      [DataMember()] public ServerConfigurationTraceConfig traceConfig { get { return _traceConfig; } set { _traceConfig = value; this.NotifyPropertyChanged(); } }


      public enum eNetConf
      {
         Full,
         Client,
         WSMISAPI,
         Server,
         SingleService
      }

      public WydeNetWorkConfiguration GetWydeNetConf(eNetConf outputType, WWService service)
      {
         WydeNetWorkConfiguration result = new WydeNetWorkConfiguration();


         switch (outputType)
         {
            case eNetConf.Full:
               result.ClientConfiguration = this.GetClientConfiguration(outputType, service);
               result.ServicesManager = this.GetServerConfiguration();
               break;

            case eNetConf.SingleService:
            case eNetConf.Client:
            case eNetConf.WSMISAPI:
               result.ClientConfiguration = this.GetClientConfiguration(outputType, service);
               break;

            case eNetConf.Server:
               result.ServicesManager = this.GetServerConfiguration();
               break;

            default:
               break;
         }

         return result;
      }

      public ClientConfiguration GetClientConfiguration(eNetConf outputType, WWService service)
      {
         ClientConfiguration result = new ClientConfiguration();

         result.Security = this.clientSecurity;

         switch (outputType)
         {
            case eNetConf.SingleService:
               result.Services.Add(service.GetClientService(outputType));
               break;

            case eNetConf.Full:
            case eNetConf.Client:
               foreach (WWService simplifiedService in this.services)
               {
                  result.Services.Add(simplifiedService.GetClientService(outputType));
               }
               break;

            case eNetConf.WSMISAPI:
               foreach (WWService simplifiedService in this.services)
               {
                  if (simplifiedService.clientService != null && simplifiedService.clientService.HttpHost != null &&
                     simplifiedService.clientService.HttpHost != "")
                  {
                     result.Services.Add(simplifiedService.GetClientService(outputType));
                  }
               }
               break;

            case eNetConf.Server:
            default:
               break;
         }
         

         return result;
      }

      public ServerConfiguration GetServerConfiguration()
      {
         ServerConfiguration result = new ServerConfiguration();

         result.Security = this.serverSecurity;
         result.TraceConfig = this.traceConfig;

         foreach (WWService simplifiedService in this.services)
         {
            result.Services.Add(simplifiedService.GetServerService());
         }

         return result;
      }

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
   }

   //[DataContract(Name = "WWService", Namespace = "http://www.wyde.com")]
   //public class WWService : ICloneable, INotifyPropertyChanged
   //{
   //   public event PropertyChangedEventHandler PropertyChanged;

   //   // This method is called by the Set accessor of each property.
   //   // The CallerMemberName attribute that is applied to the optional propertyName
   //   // parameter causes the property name of the caller to be substituted as an argument.
   //   private void NotifyPropertyChanged(string propertyName = "")
   //   {
   //      if (this.PropertyChanged != null)
   //      {
   //         this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
   //      }
   //   }

   //   public object Clone()
   //   {
   //      WWService clone = (WWService)this.MemberwiseClone();

   //      return clone;
   //   }


   //   private ClientConfigurationService _clientService;

   //   [ExpandableObject]
   //   [DataMember()] public ClientConfigurationService clientService { get { return _clientService; } set { _clientService = value; this.NotifyPropertyChanged(); } }

   //   private ServerConfigurationService _serverService;

   //   [ExpandableObject]
   //   [DataMember()] public ServerConfigurationService serverService { get { return _serverService; } set { _serverService = value; this.NotifyPropertyChanged(); } }


   //   public WWService(ClientConfigurationService clientService = null, ServerConfigurationService serverService = null)
   //   {
   //      this.clientService = clientService;
   //      this.serverService = serverService;
   //   }

   //}


   [DataContract(Name = "WWService", Namespace = "http://www.wyde.com")]
   public class WWService : ICloneable, INotifyPropertyChanged
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

      public WWService(ClientConfigurationService clientService = null, ServerConfigurationService serverService = null)
      {
         if (clientService != null)
            this.clientService = new WWClientService(clientService);

         if (serverService != null)
         {
            this.serverService = new WWServerService(serverService);
            this.Name = serverService.Name;
            this.Alias = serverService.Aliases;
         }
      }

      public object Clone()
      {
         WWService clone = (WWService)this.MemberwiseClone();

         return clone;
      }

      private WWClientService _clientService;
      [DataMember()] public WWClientService clientService { get { return _clientService; } set { _clientService = value; this.NotifyPropertyChanged(); } }

      private WWServerService _serverService;
      [DataMember()] public WWServerService serverService { get { return _serverService; } set { _serverService = value; this.NotifyPropertyChanged(); } }



      public ClientConfigurationService GetClientService(WNetConf.eNetConf outputType)
      {
         if (this.clientService == null) return null;

         ClientConfigurationService result = this.clientService.GetClientService(outputType);
         result.Name = this.Name;
         result.Aliases = this.Alias;
         return result;
      }

      public ServerConfigurationService GetServerService()
      {
         if (this.serverService == null) return null;

         ServerConfigurationService result = this.serverService.GetServerService();
         result.Name = this.Name;
         result.Aliases = this.Alias;
         return result;
      }


      //private ClientConfigurationService _clientService;

      //[ExpandableObject]
      //[DataMember()] public ClientConfigurationService clientService { get { return _clientService; } set { _clientService = value; this.NotifyPropertyChanged(); } }

      //private ServerConfigurationService _serverService;

      //[ExpandableObject]
      //[DataMember()] public ServerConfigurationService serverService { get { return _serverService; } set { _serverService = value; this.NotifyPropertyChanged(); } }


      private string nameField;

      private string aliasField;

      [Category("Basic settings")]
      [Description("The name of the service.  This name is the one used to invoke a given service.")]
      [DataMember()]
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

      [Category("Basic settings")]
      [Description("Name of the service on the server machine.If this field is blank, then the " +
         "Name field is used for the service name on the server machine.")]
      [DataMember()]
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
   }

   [DataContract(Name = "WWClientService", Namespace = "http://www.wyde.com")]
   public class WWClientService : ICloneable, INotifyPropertyChanged
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

      public WWClientService(ClientConfigurationService clientService = null)
      {

         if (clientService != null)
         {
            if (clientService.ServicesManagers != null)
            {
               if (clientService.ServicesManagers.Count() > 0)
               {
                  this.Host = clientService.ServicesManagers[0].Host;
                  this.Port = clientService.ServicesManagers[0].Port;
                  this.HttpHost = clientService.ServicesManagers[0].HttpHost;
                  this.HttpPort = clientService.ServicesManagers[0].HttpPort;
                  this.HttpAdditionalHeaders = clientService.ServicesManagers[0].HttpAdditionalHeaders;
                  this.ProxyHost = clientService.ServicesManagers[0].ProxyHost;
                  this.ProxyPort = clientService.ServicesManagers[0].ProxyPort;
                  this.Extension = clientService.ServicesManagers[0].Extension;
                  this.Emergency = clientService.ServicesManagers[0].Emergency;
                  this.Compression = clientService.ServicesManagers[0].Compression;
                  this.ConnectTimeout = clientService.ServicesManagers[0].ConnectTimeout;
                  this.TimeBeforePolling = clientService.ServicesManagers[0].TimeBeforePolling;
                  this.NbMaxConcurrentRequests = clientService.ServicesManagers[0].NbMaxConcurrentRequests;
                  this.TCP = clientService.ServicesManagers[0].TCP;
                  if (this.TCP == null)
                  {
                     this.TCP = new ClientConfigurationTCP();
                  }
               }
            }

            this.Security = clientService.Security;
         }
      }

      public object Clone()
      {
         WWClientService clone = (WWClientService)this.MemberwiseClone();
         return clone;
      }

      public ClientConfigurationService GetClientService(WNetConf.eNetConf outputType)
      {
         ClientConfigurationService result = new ClientConfigurationService();

         result.Security = this.Security;

         result.ServicesManagers = new ObservableCollection<ClientConfigurationServicesManager>();
         ClientConfigurationServicesManager servicesManager = new ClientConfigurationServicesManager();

         switch (outputType)
         {
            case WNetConf.eNetConf.Full:
               servicesManager.Host = this.Host;
               servicesManager.Port = this.Port;
               servicesManager.HttpHost = this.HttpHost;
               servicesManager.HttpPort = this.HttpPort;
               servicesManager.HttpAdditionalHeaders = this.HttpAdditionalHeaders;
               servicesManager.ProxyHost = this.ProxyHost;
               servicesManager.ProxyPort = this.ProxyPort;
               servicesManager.Extension = this.Extension;
               break;

            case WNetConf.eNetConf.SingleService:
            case WNetConf.eNetConf.Client:
               if (this.HttpHost != null && this.HttpHost != "")
               {
                  servicesManager.Host = null;
                  servicesManager.HttpHost = this.HttpHost;
                  servicesManager.HttpPort = this.HttpPort;
                  servicesManager.HttpAdditionalHeaders = this.HttpAdditionalHeaders;
                  servicesManager.ProxyHost = this.ProxyHost;
                  servicesManager.ProxyPort = this.ProxyPort;
                  servicesManager.Extension = this.Extension;
               }
               else
               {
                  servicesManager.HttpHost = null;
                  servicesManager.Extension = null;
                  servicesManager.ProxyHost = null;
                  servicesManager.HttpAdditionalHeaders = null;
                  servicesManager.Host = this.Host;
                  servicesManager.Port = this.Port;
               }
               break;

            case WNetConf.eNetConf.WSMISAPI:
               servicesManager.HttpHost = null;
               servicesManager.Extension = null;
               servicesManager.ProxyHost = null;
               servicesManager.HttpAdditionalHeaders = null;
               servicesManager.Host = this.Host;
               servicesManager.Port = this.Port;
               break;

            case WNetConf.eNetConf.Server:
            default:
               break;
         }

         servicesManager.Compression = this.Compression;
         servicesManager.Emergency = this.Emergency;
         servicesManager.TimeBeforePolling = this.TimeBeforePolling;
         servicesManager.NbMaxConcurrentRequests = this.NbMaxConcurrentRequests;
         servicesManager.ConnectTimeout = this.ConnectTimeout;
         servicesManager.TCP = this.TCP;
         if (servicesManager.TCP == null)
         {
            servicesManager.TCP = new ClientConfigurationTCP();
         }

         servicesManager.Security = this.Security;

         result.ServicesManagers.Add(servicesManager);

         return result;
      }
      

      private string httpHostField;

      private int httpPortField;

      private string proxyHostField;

      private int proxyPortField;

      private string httpAdditionalHeadersField;

      private string extensionField;

      private string hostField;

      private int portField;

      private int emergencyField;

      private eCompression compressionField;

      private int connectTimeoutField;

      private int timeBeforePollingField;

      private int nbMaxConcurrentRequestsField;

      private ConfigurationSecurity securityField;

      private ClientConfigurationTCP tcpField;


      [DefaultValue("")]      [Description("Name of host computer of the HTTP server.")]      [DataMember()]
      public string HttpHost
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

      [DefaultValue(0)]
      [Description("Port on host computer of the HTTP server.")]
      [DataMember()]
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

      [DefaultValue("")]
      [Description("Name of any proxy computer used to attain the HTTP server.")]
      [DataMember()]
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

      [DefaultValue(0)]
      [Description("Port on proxy computer used to attain the HTTP server.")]
      [DataMember()]
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

      [DefaultValue("")]
      [Description("By default set to false (value 0). Set to true (value 1) means wydeweb's " +
         "requests will contain the Content-Type header and the Cookie header (that contains " +
         "the session id).")]
      [DataMember()]
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

      [DefaultValue("")]
      [Description("DLL on HTTP server that the HTTP server calls to communicate with WSM on " +
         "the server.  By default, this DLL is installed on the server in the directory " +
         "'scripts/wyseman/wsmisapi.dll'.")]
      [DataMember()]
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

      [DefaultValue("localhost")]
      [Description("Name of host computer where WSM executes.")]
      [DataMember()]
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

      [DefaultValue(0)]
      [Description("Port on host computer through which communication occurs.")]
      [DataMember()]
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

      [Category("Client settings")]
      [Description("")]
      [DataMember()]
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

      [Category("Client settings")]
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
      [DataMember()]
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

      [Category("Client settings")]
      [Description("Time of inactivity after which the connection will be closed by the server.")]
      [DataMember()]
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

      [Category("Client settings")]
      [Description("")]
      [DataMember()]
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

      [Category("Client settings")]
      [Description("A given client request is assigned to a thread. Multiple simultaneous requests can thus be processed.  Note that the number of threads isn't necessarily equal to the number of current sessions, but rather to the number of requests that at any given time are being executed simultaneously.")]
      [DataMember()]
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

      [Category("Client settings")]
      [Description("This property overrides global security settings.")]
      [ExpandableObject]
      [DataMember()]
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

      [Category("Client settings")]
      [Description("TCP Keep alive settings.")]
      [ExpandableObject]
      [DataMember()]
      public ClientConfigurationTCP TCP
      {
         get
         {
            return this.tcpField;
         }
         set
         {
            this.tcpField = value;
         }
      }


   }

   [DataContract(Name = "WWServerService", Namespace = "http://www.wyde.com")]
   public class WWServerService : ICloneable, INotifyPropertyChanged
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

      public WWServerService(ServerConfigurationService serverService = null)
      {

         if (serverService != null)
         {
            this.Compression = serverService.Compression;
            this.Encryption = serverService.Encryption;

            if (serverService.Process != null)
            {
               //this.Process = serverService.Process;

               this.CommandLine = serverService.Process.CommandLine;
               this.CurrentDirectory = serverService.Process.CurrentDirectory;
               this.EnvironmentVariables = new ObservableCollection<NetConfEnvironmentVariable>();
               this.EnvironmentVariables = serverService.Process.EnvironmentVariables;
               this.LoadBalancing = serverService.Process.LoadBalancing;
               this.NbMaxConcurrentRequests = serverService.Process.NbMaxConcurrentRequests;
               this.NbMaxProcesses = serverService.Process.NbMaxProcesses;
               this.UseResponsiveProcessesOnly = serverService.Process.UseResponsiveProcessesOnly;
               this.UserDomain = serverService.Process.UserDomain;
               this.UserName = serverService.Process.UserName;
               this.UserPassword = serverService.Process.UserPassword;
               this.ProcessManagement = serverService.Process.ProcessManagement;
            }
         }
      }

      public object Clone()
      {
         WWService clone = (WWService)this.MemberwiseClone();

         return clone;
      }

      public ServerConfigurationService GetServerService()
      {
         ServerConfigurationService result = new ServerConfigurationService();
         
         result.Compression = this.Compression;
         result.Encryption = this.Encryption;
         result.EnvironmentVariables = this.EnvironmentVariables;

         result.Process = new ServerConfigurationProcess();
         result.Process.CommandLine = this.CommandLine;
         result.Process.CurrentDirectory = this.CurrentDirectory;
         result.Process.NbMaxConcurrentRequests = this.NbMaxConcurrentRequests;
         result.Process.NbMaxProcesses = this.NbMaxProcesses;
         result.Process.UseResponsiveProcessesOnly = this.UseResponsiveProcessesOnly;
         result.Process.LoadBalancing = this.LoadBalancing;
         result.Process.ProcessManagement = this.ProcessManagement;

         result.Process.UserDomain = this.UserDomain;
         result.Process.UserName = this.UserName;
         result.Process.UserPassword = this.UserPassword;

         return result;
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

      //private ConfigurationSecurity securityField;

      private eCompression compressionField;

      private eYesNo encryptionField;

      [Category("Server settings")]
      [Description("Command line to launch when process is invoked. This command line includes the executable and any parameters, just as if you were launching the service from the DOS command line.")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("It can be advisable to separate execution of client's requests into several processes. This way, not all client execution runs in the same Windows process, and any problem such as a crash will have limited damage on the executing sessions. This number is the maximum number of processes that this service will have running at one time.")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("A given client request is assigned to a thread. Multiple simultaneous requests can thus be processed.  Note that the number of threads isn't necessarily equal to the number of current sessions, but rather to the number of requests that at any given time are being executed simultaneously.")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("The directory the service is to be switched to before launching it.")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("User name of system account authorized to launch the service.")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("Reserved for future use.")]
      [DataMember()]
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

      [Category("Server settings")]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("Environment Variables embeded in this service's process.")]
      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("Handles how and when the process is stopped.")]
      [ExpandableObject]
      [DataMember()]
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

      [Category("Server settings")]
      [Description("Defines how to divide sessions among processes.")]
      [ExpandableObject]
      [DataMember()]
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


      //[Category("Server settings")]
      //[Description("This property overrides global security settings.")]
      //[ExpandableObject]
      //[DataMember()]
      //public ConfigurationSecurity Security
      //{
      //   get
      //   {
      //      return this.securityField;
      //   }
      //   set
      //   {
      //      this.securityField = value;
      //   }
      //}

      [Category("Server settings")]
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
      [DataMember()]
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

      [Category("Server settings")]
      [Description("Data is exchanged between the clients and the WSM servers. This data can " +
         "be encrypted. You specify at the client side whether to activate encryption or not, and " +
         "at the server end whether encryption is required or not. WSM uses the SSPI encryption " +
         "technique, which has minimal impact on performance. For data encryption, the client " +
         "might need to identify himself with the server.")]
      [DataMember()]
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
   }

}


