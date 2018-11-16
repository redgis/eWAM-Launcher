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

namespace eWamLauncher
{
   // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
   public partial class WydeNetWorkConfiguration : INotifyPropertyChanged
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

      private ClientConfiguration clientConfigurationField;

      private ServerConfiguration servicesManagerField;

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
   public partial class ClientConfiguration : INotifyPropertyChanged
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

      private ClientConfigurationService[] servicesField;

      private ConfigurationSecurity securityField;

      [System.Xml.Serialization.XmlArrayItemAttribute("Service", IsNullable = false)]
      public ClientConfigurationService[] Services
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
   public partial class ClientConfigurationService : INotifyPropertyChanged
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

      private string nameField;

      private ClientConfigurationServicesManager[] servicesManagersField;

      private ConfigurationSecurity securityField;

      private string aliasesField;

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

      [System.Xml.Serialization.XmlArrayItemAttribute("ServicesManager", IsNullable = false)]
      public ClientConfigurationServicesManager[] ServicesManagers
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
   public partial class ClientConfigurationServicesManager : INotifyPropertyChanged
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

      private string httpHostField;

      private int httpPortField;

      private string extensionField;

      private string proxyHostField;

      private int proxyPortField;

      private string hostField;

      private int portField;

      private int nbMaxConcurrentRequestsField;

      private string aliasField;

      private int timeBeforePollingField;

      private int emergencyField;

      private int compressionField;

      private ConfigurationSecurity securityField;

      private int connectTimeoutField;
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

   //   private int encryptionField;

   //   private int authenticateField;

   //   private string userNameField;

   //   private string userPasswordField;

   //   private string userDomainField;

   //   
   //   public int Encryption
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
   //   public int Authenticate
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
   public partial class ServerConfiguration : INotifyPropertyChanged
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

      private ServerConfigurationService[] servicesField;

      private ConfigurationSecurity securityField;

      private ServerConfigurationTraceConfig traceConfigField;

      [System.Xml.Serialization.XmlArrayItemAttribute("Service", IsNullable = false)]
      public ServerConfigurationService[] Services
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
   public partial class ServerConfigurationService : INotifyPropertyChanged
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

      private string nameField;

      private int compressionField;

      private int encryptionField;

      private string aliasesField;

      private NetConfEnvironmentVariable[] environmentVariablesField;

      private ServerConfigurationProcess processField;

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

      public int Encryption
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

      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      public NetConfEnvironmentVariable[] EnvironmentVariables
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
   public partial class ServerConfigurationProcess : INotifyPropertyChanged
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

      public bool UseResponsiveProcessesOnly
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

      [System.Xml.Serialization.XmlArrayItemAttribute("Var", IsNullable = true)]
      public NetConfEnvironmentVariable[] EnvironmentVariables
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
   public partial class ServerConfigurationLoadBalancing : INotifyPropertyChanged
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

      private bool forceRandomField;

      private int loadBalancingByNbSessionsField;

      private int loadBalancingByCPUField;

      private int loadBalancingByMemoryField;

      public bool ForceRandom
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
   public partial class ServerConfigurationProcessManagement : INotifyPropertyChanged
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

      private int nbMaxSimultaneousSessionsField;

      private int requestOnMaxSimultaneousSessionsQueuedField;

      private int maxMemoryUsageField;

      private int requestOnMaxMemoryUsageQueuedField;

      private byte stopAfterLastSessionField;

      private int stopAfterRunningTimeField;

      private int stopAfterInactivityTimeoutField;

      private int stopAfterMemoryLimitField;

      private int stopAfterNbSessionsField;

      private PeriodicalStop periodicalStopField;

      private AutomaticStart automaticStartField;

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

      public byte StopAfterLastSession
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
   public partial class PeriodicalStop : INotifyPropertyChanged
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

      private int stopAtTimeField;

      private int stopEveryField;

      private int cannotBeLaunchedDuringTimeField;

      private int stoppingModeField;

      private int timeToEndSessionsField;

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

      public int StopEvery
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

      public int StoppingMode
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
   public partial class AutomaticStart : INotifyPropertyChanged
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

      private int nbProcessesField;

      private int fromDayField;

      private int toDayField;

      private int fromTimeField;

      private int toTimeField;

      private byte preloadField;

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

      public int FromDay
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

      public int ToDay
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

      public byte Preload
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
   public partial class ConfigurationSecurity : INotifyPropertyChanged
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

      private int encryptionField;

      private int authenticateField;

      private string userNameField;

      private string userPasswordField;

      private string userDomainField;

      private string loginsField;

      public int Encryption
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

      public int Authenticate
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

      private byte startRunningField;

      private int timestepField;

      private byte servicesField;

      private byte processesField;

      public byte StartRunning
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

      public byte Services
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

      public byte Processes
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

      private string nameField;

      private string valueField;

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
