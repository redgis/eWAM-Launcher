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


namespace eWamLauncher
{

   [DataContract(Name = "WNetConf", Namespace = "http://www.wyde.com")]
   public class WNetConf : INotifyPropertyChanged
   {
      public WNetConf(WydeNetWorkConfiguration wNetConf = null)
      {
         this.services = new ObservableCollection<WWService>();

         if (wNetConf == null)
            return;

         this.security = wNetConf.ServicesManager.Security;
         this.traceConfig = wNetConf.ServicesManager.TraceConfig;

         foreach (ServerConfigurationService serverService in wNetConf.ServicesManager.Services)
         {
            foreach (ClientConfigurationService clientService in wNetConf.ClientConfiguration.Services)
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


      private ObservableCollection<WWService> _services;
      [DataMember()] public ObservableCollection<WWService> services { get { return _services; } set { _services = value; this.NotifyPropertyChanged(); } }

      private ConfigurationSecurity _security;
      [DataMember()] public ConfigurationSecurity security { get { return _security; } set { _security = value; this.NotifyPropertyChanged(); } }

      private ServerConfigurationTraceConfig _traceConfig;
      [DataMember()] public ServerConfigurationTraceConfig traceConfig { get { return _traceConfig; } set { _traceConfig = value; this.NotifyPropertyChanged(); } }



      public WydeNetWorkConfiguration GetWydeNetConf()
      {
         WydeNetWorkConfiguration result = new WydeNetWorkConfiguration();

         result.ClientConfiguration = this.GetClientConfiguration();
         
         result.ServicesManager = this.GetServerConfiguration();

         return result;
      }

      public ClientConfiguration GetClientConfiguration()
      {
         ClientConfiguration result = new ClientConfiguration();

         result.Security = this.security;

         List<ClientConfigurationService> clientServices = new List<ClientConfigurationService>();
         foreach (WWService simplifiedService in this.services)
         {
            clientServices.Add(simplifiedService.GetClientService());
         }

         result.Services = clientServices.ToArray();

         return result;
      }

      public ServerConfiguration GetServerConfiguration()
      {
         ServerConfiguration result = new ServerConfiguration();

         result.Security = this.security;
         result.TraceConfig = this.traceConfig;

         List<ServerConfigurationService> serverServices = new List<ServerConfigurationService>();
         foreach (WWService simplifiedService in this.services)
         {
            serverServices.Add(simplifiedService.GetServerService());
         }

         result.Services = serverServices.ToArray();

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


   [DataContract(Name = "WWService", Namespace = "http://www.wyde.com")]
   public class WWService : INotifyPropertyChanged
   {
      //private ClientConfigurationService _clientService;

      //[ExpandableObject]
      //[DataMember()] public ClientConfigurationService clientService { get { return _clientService; } set { _clientService = value; this.NotifyPropertyChanged(); } }

      //private ServerConfigurationService _serverService;

      //[ExpandableObject]
      //[DataMember()] public ServerConfigurationService serverService { get { return _serverService; } set { _serverService = value; this.NotifyPropertyChanged(); } }


      private string nameField;

      private string aliasField;

      private int emergencyField;
      
      private int compressionField;

      private int encryptionField;

      private ConfigurationSecurity securityField;

      [Category("Basic settings")]
      [DataMember()] public string Name
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
      [DataMember()] public string Alias
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

      [Category("Basic settings")]
      [DataMember()] public int Emergency
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

      [Category("Basic settings")]
      public int Compression
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

      [Category("Basic settings")]
      [DataMember()] public int Encryption
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

      [Category("Basic settings")]
      [Description("This property overrides global security settings.")]
      [ExpandableObject]
      [DataMember()] public ConfigurationSecurity Security
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



      private string httpHostField;

      private int httpPortField;

      private string extensionField;

      private string proxyHostField;

      private int proxyPortField;

      private string hostField;

      private int portField;

      private int connectTimeoutField;

      private int timeBeforePollingField;

      [Category("Client settings")]
      [Description("This property is the server host name of the HTTP tunnel (\"rebond\").")]
      [DataMember()] public string HttpHost
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

      [Category("Client settings")]
      [Description("This property is the server port of the HTTP tunnel (\"rebond\").")]
      [DataMember()] public int HttpPort
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

      [Category("Client settings")]
      [Description("This property is the server path of WSMISAPI.dll for HTTP tunnel (\"rebond\").")]
      [DataMember()] public string Extension
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

      [Category("Client settings")]
      [Description("This property is the host name of the proxy server.")]
      [DataMember()] public string ProxyHost
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

      [Category("Client settings")]
      [Description("This property is the port of the proxy server.")]
      [DataMember()] public int ProxyPort
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

      [Category("Client settings")]
      [Description("This property is the host name of the applicative server (Wyseman).")]
      [DataMember()] public string Host
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

      [Category("Client settings")]
      [Description("This property is the port of the applicative server (Wyseman).")]
      [DataMember()] public int Port
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
      [DataMember()] public int ConnectTimeout
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
      [DataMember()] public int TimeBeforePolling
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




      private string commandLineField;

      private int nbMaxProcessesField;

      private int nbMaxConcurrentRequestsField;

      private bool useResponsiveProcessesOnlyField;

      private string currentDirectoryField;

      private string userNameField;

      private string userDomainField;

      private string userPasswordField;

      private NetConfEnvironmentVariable[] environmentVariablesField;

      private ServerConfigurationProcessManagement processManagementField;

      private ServerConfigurationLoadBalancing loadBalancingField;

      [Category("Server settings")]
      [DataMember()] public string CommandLine
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
      [DataMember()] public int NbMaxProcesses
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
      [DataMember()] public int NbMaxConcurrentRequests
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
      [DataMember()] public bool UseResponsiveProcessesOnly
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
      [DataMember()] public string CurrentDirectory
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
      [DataMember()] public string UserName
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
      [DataMember()] public string UserDomain
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
      [DataMember()] public string UserPassword
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
      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      [DataMember()] public NetConfEnvironmentVariable[] EnvironmentVariables
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
      [ExpandableObject]
      [DataMember()] public ServerConfigurationProcessManagement ProcessManagement
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
      [ExpandableObject]
      [DataMember()] public ServerConfigurationLoadBalancing LoadBalancing
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


      public ClientConfigurationService GetClientService()
      {
         ClientConfigurationService result = new ClientConfigurationService();

         result.Name = this.Name;
         result.Aliases = this.Alias;
         result.Security = this.Security;
         
         result.ServicesManagers = new ClientConfigurationServicesManager[1];
         result.ServicesManagers[0] = new ClientConfigurationServicesManager();

         result.ServicesManagers[0].Compression = this.Compression;
         result.ServicesManagers[0].Emergency = this.Emergency;
         result.ServicesManagers[0].TimeBeforePolling = this.TimeBeforePolling;
         result.ServicesManagers[0].NbMaxConcurrentRequests = this.NbMaxConcurrentRequests;
         result.ServicesManagers[0].ConnectTimeout = this.ConnectTimeout;

         if (this.HttpHost != null && this.HttpHost != "")
         {
            result.ServicesManagers[0].Host = null;
            result.ServicesManagers[0].HttpHost = this.HttpHost;
            result.ServicesManagers[0].HttpPort = this.HttpPort;
            result.ServicesManagers[0].Extension = this.Extension;
         }
         else
         {
            result.ServicesManagers[0].HttpHost = null;
            result.ServicesManagers[0].Extension = null;
            result.ServicesManagers[0].Host = this.Host;
            result.ServicesManagers[0].Port = this.Port;
         }
         
         result.ServicesManagers[0].Security = this.Security;
         result.ServicesManagers[0].Alias = this.Alias;
         result.ServicesManagers[0].ProxyHost = this.ProxyHost;
         result.ServicesManagers[0].ProxyPort = this.ProxyPort;

         return result;
      }
      
      public ServerConfigurationService GetServerService()
      {
         ServerConfigurationService result = new ServerConfigurationService();

         result.Name = this.Name;
         result.Aliases = this.Alias;
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
         {
            if (clientService.ServicesManagers != null)
            {
               if (clientService.ServicesManagers.Count() > 0)
               {
                  this.Host = clientService.ServicesManagers[0].Host;
                  this.Port = clientService.ServicesManagers[0].Port;
                  this.HttpHost = clientService.ServicesManagers[0].HttpHost;
                  this.HttpPort = clientService.ServicesManagers[0].HttpPort;
                  this.Extension = clientService.ServicesManagers[0].Extension;
                  this.ProxyHost = clientService.ServicesManagers[0].ProxyHost;
                  this.ProxyPort = clientService.ServicesManagers[0].ProxyPort;
                  this.Emergency = clientService.ServicesManagers[0].Emergency;
                  this.Alias = clientService.ServicesManagers[0].Alias;
                  this.ConnectTimeout = clientService.ServicesManagers[0].ConnectTimeout;
                  this.TimeBeforePolling = clientService.ServicesManagers[0].TimeBeforePolling;
               }
            }

            this.Security = clientService.Security;
         }

         if (serverService != null)
         {
            this.Name = serverService.Name;
            this.Compression = serverService.Compression;
            this.Encryption = serverService.Encryption;

            if (serverService.Process != null)
            {
               //this.Process = serverService.Process;

               CommandLine = serverService.Process.CommandLine;
               CurrentDirectory = serverService.Process.CurrentDirectory;
               EnvironmentVariables = serverService.Process.EnvironmentVariables;
               LoadBalancing = serverService.Process.LoadBalancing;
               NbMaxConcurrentRequests = serverService.Process.NbMaxConcurrentRequests;
               NbMaxProcesses = serverService.Process.NbMaxProcesses;
               UseResponsiveProcessesOnly = serverService.Process.UseResponsiveProcessesOnly;
               UserDomain = serverService.Process.UserDomain;
               UserName = serverService.Process.UserName;
               UserPassword = serverService.Process.UserPassword;
               ProcessManagement = serverService.Process.ProcessManagement;
            }
         }
      }

      //public object Clone()
      //{
      //   WWService clone = (WWService)this.MemberwiseClone();

      //   clone.EnvironmentVariables
      //   clone.environmentVariables = new ObservableCollection<EnvironmentVariable>();
      //   clone.launchers = new ObservableCollection<Launcher>();

      //   foreach (var variable in this.EnvironmentVariables)
      //   {
      //      clone.EnvironmentVariables.Add(variable.Clone());
      //   }

      //   foreach (Launcher launcher in this.launchers)
      //   {
      //      clone.launchers.Add((Launcher)launcher.Clone());
      //   }

      //   return clone;
      //}

   }
}
