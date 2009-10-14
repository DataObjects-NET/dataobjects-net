// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System;
using System.Configuration;
using System.Transactions;
using Xtensive.Core.Configuration;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// <see cref="Session"/> configuration element within a configuration file.
  /// </summary>
  public class SessionElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string UserNameElementName = "userName";
    private const string PasswordElementName = "password";
    private const string CacheSizeElementName = "cacheSize";
    private const string CacheTypeElementName = "cacheType";
    private const string OptionsElementName = "options";
    private const string IsolationLevelElementName = "isolationLevel";
    private const string BatchSizeElementName = "batchSize";

    /// <inheritdoc/>
    public override object Identifier
    {
      get { return Name; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false, IsKey = true, DefaultValue = "Default")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.UserName" copy="true"/>
    /// </summary>
    [ConfigurationProperty(UserNameElementName, IsRequired = false, IsKey = false)]
    public string UserName
    {
      get { return (string) this[UserNameElementName]; }
      set { this[UserNameElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Password" copy="true"/>
    /// </summary>
    [ConfigurationProperty(PasswordElementName, IsRequired = false, IsKey = false)]
    public string Password
    {
      get { return (string)this[PasswordElementName]; }
      set { this[PasswordElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.CacheSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CacheSizeElementName, IsRequired = false, DefaultValue = SessionConfiguration.DefaultCacheSize, IsKey = false)]
    public int CacheSize
    {
      get { return (int) this[CacheSizeElementName]; }
      set { this[CacheSizeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.CacheType" copy="true"/>
    /// </summary>
    [ConfigurationProperty(CacheTypeElementName, IsRequired = false, DefaultValue = "Default", IsKey = false)]
    public string CacheType
    {
      get { return (string)this[CacheTypeElementName]; }
      set { this[CacheTypeElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.Options" copy="true"/>
    /// </summary>
    [ConfigurationProperty(OptionsElementName, IsRequired = false, DefaultValue = "Default", IsKey = false)]
    public string Options
    {
      get { return (string) this[OptionsElementName]; }
      set { this[OptionsElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.DefaultIsolationLevel" copy="true"/>
    /// </summary>
    [ConfigurationProperty(IsolationLevelElementName, IsRequired = false, DefaultValue = "ReadCommitted", IsKey = false)]
    public string DefaultIsolationLevel
    {
      get { return (string)this[IsolationLevelElementName]; }
      set { this[IsolationLevelElementName] = value; }
    }

    /// <summary>
    /// <see cref="SessionConfiguration.BatchSize" copy="true"/>
    /// </summary>
    [ConfigurationProperty(BatchSizeElementName, IsRequired = false, DefaultValue = SessionConfiguration.DefaultBatchSize, IsKey = false)]
    public int BatchSize
    {
      get { return (int) this[BatchSizeElementName]; }
      set { this[BatchSizeElementName] = value; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="SessionConfiguration"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public SessionConfiguration ToNative()
    {
      var result = new SessionConfiguration(Name) {
        UserName = UserName,
        Password = Password,
        CacheSize = CacheSize,
        CacheType =  (SessionCacheType) Enum.Parse(typeof (SessionCacheType), CacheType, true),
        Options = (SessionOptions) Enum.Parse(typeof (SessionOptions), Options, true),
        DefaultIsolationLevel = (IsolationLevel) Enum.Parse(typeof (IsolationLevel), DefaultIsolationLevel, true),
        BatchSize = BatchSize
      };
      return result;
    }
  }
}