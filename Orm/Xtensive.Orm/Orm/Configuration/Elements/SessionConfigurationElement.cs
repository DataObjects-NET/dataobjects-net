// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using System.IO;
using System.Transactions;
using Xtensive.Core;


namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// <see cref="Session"/> configuration element within a configuration file.
  /// </summary>
  public class SessionConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string UserNameElementName = "userName";
    private const string PasswordElementName = "password";
    private const string CacheSizeElementName = "cacheSize";
    private const string CacheTypeElementName = "cacheType";
    private const string OptionsElementName = "options";
    private const string IsolationLevelElementName = "isolationLevel";
    private const string CommandTimeoutElementName = "commandTimeout";
    private const string BatchSizeElementName = "batchSize";
    private const string ReaderPreloadingElementName = "readerPreloading";
    private const string ServiceContainerTypeElementName = "serviceContainerType";
    private const string EntityChangeRegistrySizeElementName = "entityChangeRegistrySize";
    private const string ConnectionStringElementName = "connectionString";
    private const string ConnectionUrlElementName = "connectionUrl";

    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="SessionConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true, DefaultValue = "Default")]
    public string Name {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.UserName" copy="true"/>
    /// </summary>
    [ConfigurationProperty(UserNameElementName)]
    public string UserName {
      get { return (string) this[UserNameElementName]; }
      set { this[UserNameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Password" copy="true"/>
    /// </summary>
    [ConfigurationProperty(PasswordElementName)]
    public string Password {
      get { return (string)this[PasswordElementName]; }
      set { this[PasswordElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.CacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName,
      DefaultValue = SessionConfiguration.DefaultCacheSize)]
    public int CacheSize {
      get { return (int) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.CacheType" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CacheTypeElementName, DefaultValue = "Default")]
    public string CacheType {
      get { return (string)this[CacheTypeElementName]; }
      set { this[CacheTypeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Options" copy="true"/>
    /// </summary>
    [ConfigurationProperty(OptionsElementName, DefaultValue = "Default")]
    public string Options {
      get { return (string)this[OptionsElementName]; }
      set { this[OptionsElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.DefaultIsolationLevel" copy="true" />
    /// </summary>
    [ConfigurationProperty(IsolationLevelElementName, DefaultValue = "RepeatableRead")]
    public string DefaultIsolationLevel {
      get { return (string) this[IsolationLevelElementName]; }
      set { this[IsolationLevelElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.DefaultCommandTimeout" copy="true" />
    /// </summary>
    [ConfigurationProperty(CommandTimeoutElementName, DefaultValue = null)]
    public int? DefaultCommandTimeout {
      get { return (int?) this[CommandTimeoutElementName]; }
      set { this[CommandTimeoutElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.BatchSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(BatchSizeElementName,
      DefaultValue = SessionConfiguration.DefaultBatchSize)]
    public int BatchSize {
      get { return (int) this[BatchSizeElementName]; }
      set { this[BatchSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.ReaderPreloading" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ReaderPreloadingElementName, DefaultValue = "Default")]
    public string ReaderPreloading {
      get { return (string) this[ReaderPreloadingElementName]; }
      set { this[ReaderPreloadingElementName] = value; }
    }

    /// <summary>
    /// <see cref="DomainConfiguration.ServiceContainerType" copy="true"/>
    /// </summary>
    [ConfigurationProperty(ServiceContainerTypeElementName, DefaultValue = null)]
    public string ServiceContainerType {
      get { return (string) this[ServiceContainerTypeElementName]; }
      set { this[ServiceContainerTypeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.EntityChangeRegistrySize" copy="true"/>.
    /// </summary>
    [ConfigurationProperty(EntityChangeRegistrySizeElementName,
      DefaultValue = SessionConfiguration.DefaultEntityChangeRegistrySize)]
    public int EntityChangeRegistrySize {
      get { return (int) this[EntityChangeRegistrySizeElementName]; }
      set { this[EntityChangeRegistrySizeElementName] = value; }
    }

    [ConfigurationProperty(ConnectionStringElementName, DefaultValue = null)]
    public string ConnectionString {
      get { return (string) this[ConnectionStringElementName]; }
      set { this[ConnectionStringElementName] = value; }
    }

    [ConfigurationProperty(ConnectionUrlElementName, DefaultValue = null)]
    public string ConnectionUrl {
      get { return (string) this[ConnectionUrlElementName]; }
      set { this[ConnectionUrlElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="SessionConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public SessionConfiguration ToNative()
    {
      // Minor hack:
      // We should not require user to specify provider name.
      // We actually know it when opening new session.
      // However, we do not know it in this method
      // We are going easy way and substituting a fake provider.
      // SQL SessionHandler is aware of this and always uses correct provider.

      var connectionInfo = ConnectionInfoParser.GetConnectionInfo(ConnectionUrl, "_dummy_", ConnectionString);

      var result = new SessionConfiguration(Name) {
        UserName = UserName,
        Password = Password,
        CacheSize = CacheSize,
        BatchSize = BatchSize,
        CacheType = (SessionCacheType) Enum.Parse(typeof (SessionCacheType), CacheType, true),
        Options = (SessionOptions) Enum.Parse(typeof (SessionOptions), Options, true),
        DefaultIsolationLevel = (IsolationLevel) Enum.Parse(typeof (IsolationLevel), DefaultIsolationLevel, true),
        ReaderPreloading = (ReaderPreloadingPolicy) Enum.Parse(typeof (ReaderPreloadingPolicy), ReaderPreloading, true),
        ServiceContainerType = Type.GetType(ServiceContainerType),
        EntityChangeRegistrySize = EntityChangeRegistrySize,
        DefaultCommandTimeout = DefaultCommandTimeout,
        ConnectionInfo = connectionInfo,
      };
      return result;
    }
  }
}