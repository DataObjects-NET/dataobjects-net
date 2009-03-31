// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Practices.Unity;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration.Elements;
using Xtensive.Storage.Configuration.TypeRegistry;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// The configuration of the <see cref="Domain"/>.
  /// </summary>
  [Serializable]
  public class DomainConfiguration : ConfigurationBase,
    IEquatable<DomainConfiguration>
  {
    #region Defaults (constants)

    /// <summary>
    /// Default <see cref="BuildMode"/> value:
    /// "<see cref="DomainBuildMode.Default" />".
    /// </summary>
    public const DomainBuildMode DefaultBuildMode = DomainBuildMode.Default;

    /// <summary>
    /// Default <see cref="ForeignKeyMode"/> value:
    /// "<see cref="ForeignKeyMode.Default" />".
    /// </summary>
    public const ForeignKeyMode DefaultForeignKeyMode = ForeignKeyMode.Default;

    /// <summary>
    /// Default <see cref="SectionName"/> value:
    /// "<see langword="Xtensive.Storage" />".
    /// </summary>
    public static string DefaultSectionName = "Xtensive.Storage";

    /// <summary>
    /// Default <see cref="DomainConfiguration.KeyCacheSize"/> value: 
    /// <see langword="16*1024" />.
    /// </summary>
    public const int DefaultKeyCacheSize = 16*1024;

    /// <summary>
    /// Default <see cref="DomainConfiguration.KeyGeneratorCacheSize"/> value: 
    /// <see langword="128" />.
    /// </summary>
    public const int DefaultKeyGeneratorCacheSize = 128;

    /// <summary>
    /// Default <see cref="DomainConfiguration.RecordSetMappingCacheSize"/> value: 
    /// <see langword="1024" />.
    /// </summary>
    public const int DefaultRecordSetMappingCacheSize = 1024;

    /// <summary>
    /// Default <see cref="DomainConfiguration.SessionPoolSize"/> value: 
    /// <see langword="64" />.
    /// </summary>
    public const int DefaultSessionPoolSize = 64;

    /// <summary>
    /// Default <see cref="DomainConfiguration.InconsistentTransactions"/> value: 
    /// <see langword="false" />.
    /// </summary>
    public const bool DefaultInconsistentTransactions = false;

    /// <summary>
    /// Default <see cref="DomainConfiguration.AutoValidation"/> value: 
    /// <see langword="true" />.
    /// </summary>
    public const bool DefaultAutoValidation = true;

    #endregion
    
    private static string sectionName = DefaultSectionName;
    private static bool sectionNameIsDefined;

    private string name = string.Empty;
    private UrlInfo connectionInfo;
    private Registry types = new Registry(new TypeProcessor());
    private CollectionBaseSlim<Type> builders = new CollectionBaseSlim<Type>();
    private NamingConvention namingConvention;
    private int keyCacheSize = DefaultKeyCacheSize;
    private int keyGeneratorCacheSize = DefaultKeyGeneratorCacheSize;
    private int sessionPoolSize = DefaultSessionPoolSize;
    private int recordSetMappingCacheSize = DefaultRecordSetMappingCacheSize;
    private bool autoValidation = true;
    private bool inconsistentTransactions;
    private UnityContainer serviceContainer;
    private SetSlim<Type> mappings;
    private SessionConfigurationCollection sessions;
    private DomainBuildMode buildMode = DefaultBuildMode;
    private ForeignKeyMode foreignKeyMode = DefaultForeignKeyMode;

    /// <summary>
    /// Gets or sets the name of the section where storage configuration is configuration.
    /// </summary>
    /// <exception cref="NotSupportedException">The property is already defined once.</exception>
    public static string SectionName
    {
      get { return sectionName; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(value, "value");
        if (sectionNameIsDefined)
          throw Exceptions.AlreadyInitialized("SectionName");
        sectionName = value;
        sectionNameIsDefined = true;
      }
    }

    /// <summary>
    /// Gets or sets the domain configuration name.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "Name");
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the connection info (URL).
    /// </summary>
    public UrlInfo ConnectionInfo
    {
      get { return connectionInfo; }
      set
      {
        this.EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets the collection of persistent <see cref="Type"/>s that are about to be 
    /// registered in the <see cref="Domain"/>.
    /// </summary>
    public Registry Types
    {
      get { return types; }
    }

    /// <summary>
    /// Gets the collection of custom <see cref="DomainModelDef"/> builders.
    /// </summary>
    public IList<Type> Builders
    {
      get { return builders; }
    }

    /// <summary>
    /// Gets or sets the naming convention.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get { return namingConvention; }
      set
      {
        this.EnsureNotLocked();
        namingConvention = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key cache.
    /// Default value is <see cref="DefaultKeyCacheSize"/>.
    /// </summary>
    public int KeyCacheSize
    {
      get { return keyCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsInRange(value, 1, int.MaxValue, "value");
        keyCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the key generator cache.
    /// Default value is <see cref="DefaultKeyGeneratorCacheSize"/>.
    /// </summary>
    public int KeyGeneratorCacheSize
    {
      get { return keyGeneratorCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsInRange(value, 1, int.MaxValue, "value");
        keyGeneratorCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the session pool.
    /// Default value is <see cref="DefaultSessionPoolSize"/>.
    /// </summary>
    public int SessionPoolSize
    {
      get { return sessionPoolSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsInRange(value, 1, int.MaxValue, "value");
        sessionPoolSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the record set mapping cache.
    /// Default value is <see cref="DefaultRecordSetMappingCacheSize"/>.
    /// </summary>
    public int RecordSetMappingCacheSize
    {
      get { return recordSetMappingCacheSize; }
      set
      {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentIsInRange(value, 1, int.MaxValue, "value");
        recordSetMappingCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating whether changed entities should be validated automatically.
    /// Default value is <see cref="DomainConfigurationElement.AutoValidation"/>.
    /// </summary>
    public bool AutoValidation
    {
      get { return autoValidation; }
      set
      {
        this.EnsureNotLocked();
        autoValidation = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether inconsistent region should be automatically open within the transaction.
    /// I.e. all the entities changed within the transaction should be validated on transaction commit only.
    /// It is recommended to keep this option switched off and define inconsistent regions manually.    
    /// Default value is <see cref="DefaultInconsistentTransactions"/>.
    /// </summary>
    public bool InconsistentTransactions
    {
      get { return inconsistentTransactions; }
      set 
      {
        this.EnsureNotLocked();
        inconsistentTransactions = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating domain upgrade behavior. 
    /// Default value is <see cref="DefaultBuildMode"/>.
    /// </summary>
    public DomainBuildMode BuildMode
    {
      get { return buildMode; }
      set
      {
        this.EnsureNotLocked();
        buildMode = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating foreign key mode. 
    /// Default value is <see cref="DefaultForeignKeyMode"/>.
    /// </summary>
    public ForeignKeyMode ForeignKeyMode
    {
      get { return foreignKeyMode; }
      set
      {
        this.EnsureNotLocked();
        foreignKeyMode = value;
      }
    }

    /// <summary>
    /// Gets available session configurations.
    /// </summary>
    public SessionConfigurationCollection Sessions
    {
      get { return sessions; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        sessions = value;
      }
    }


    /// <summary>
    /// Gets user defined function mappings.
    /// </summary>
    public SetSlim<Type> Mappings
    {
      get { return mappings; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        this.EnsureNotLocked();
        mappings = value;
      }
    }

    /// <summary>
    /// Gets the service container.
    /// </summary>
    public UnityContainer ServiceContainer
    {
      get { return serviceContainer; }
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      types.Lock(true);
      builders.Lock(true);
      sessions.Lock(true);
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public override void Validate()
    {
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new DomainConfiguration();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      var configuration = (DomainConfiguration) source;
      name = configuration.Name;
      connectionInfo = new UrlInfo(configuration.ConnectionInfo.Url);
      types = (Registry) configuration.Types.Clone();
      builders = new CollectionBase<Type>(configuration.Builders);
      namingConvention = (NamingConvention) configuration.NamingConvention.Clone();
      keyCacheSize = configuration.KeyCacheSize;
      keyGeneratorCacheSize = configuration.KeyGeneratorCacheSize;
      sessionPoolSize = configuration.SessionPoolSize;
      recordSetMappingCacheSize = configuration.RecordSetMappingCacheSize;
      sessions = configuration.Sessions;
      mappings = configuration.Mappings;
      serviceContainer = configuration.serviceContainer;
      buildMode = configuration.buildMode;
      foreignKeyMode = configuration.foreignKeyMode;
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(DomainConfiguration other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return builders.EqualsTo(other.builders)
        && Equals(other.connectionInfo, connectionInfo) 
          && Equals(other.namingConvention, namingConvention)
            && types.EqualsTo(other.types)
              && other.sessionPoolSize == sessionPoolSize 
                && Equals(other.name, name) 
                  && Equals(other.sessions, sessions)
                    && other.mappings.IsEqualTo(mappings)
                      && Equals(other.buildMode, buildMode)
                        && Equals(other.foreignKeyMode, foreignKeyMode);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != typeof(DomainConfiguration))
        return false;
      return Equals((DomainConfiguration)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = builders.GetHashCodeRecursive();
        result = (result * 397) ^ (name != null ? name.GetHashCode() : 0);
        result = (result * 397) ^ (connectionInfo != null ? connectionInfo.GetHashCode() : 0);
        result = (result * 397) ^ (namingConvention != null ? namingConvention.GetHashCode() : 0);
        result = (result * 397) ^ (mappings != null ? mappings.GetHashCode() : 0);
        result = (result * 397) ^ types.GetHashCodeRecursive();
        result = (result * 397) ^ keyCacheSize;
        result = (result * 397) ^ recordSetMappingCacheSize;
        result = (result * 397) ^ sessionPoolSize;
        result = (result * 397) ^ (sessions.Default != null ? sessions.Default.GetHashCode() : 0);
        result = (result * 397) ^ (int)buildMode;
        result = (result * 397) ^ (int)foreignKeyMode;
        return result;
      }
    }

    #endregion

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <see cref="SectionName"/>).
    /// </summary>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <see cref="SectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static DomainConfiguration Load(string name)
    {
      return Load(SectionName, name);
    }

    /// <summary>
    /// Loads the <see cref="DomainConfiguration"/> for <see cref="Domain"/>
    /// with the specified <paramref name="name"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <param name="name">Name of the <see cref="Domain"/>.</param>
    /// <returns>
    /// The <see cref="DomainConfiguration"/> for the specified domain.
    /// </returns>
    /// <exception cref="InvalidOperationException">Section <paramref name="sectionName"/>
    /// is not found in application configuration file, or there is no configuration for
    /// the <see cref="Domain"/> with specified <paramref name="name"/>.</exception>
    public static DomainConfiguration Load(string sectionName, string name)
    {
      var section = (Elements.ConfigurationSection)ConfigurationManager.GetSection(sectionName);
      if (section==null) 
        throw new InvalidOperationException(string.Format(
          Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      foreach (DomainConfigurationElement domainElement in section.Domains)
        if (domainElement.Name==name)
          return domainElement.ToNative();
      throw new InvalidOperationException(string.Format(
        Strings.ExConfigurationForDomainIsNotFoundInApplicationConfigurationFile, name, sectionName));
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class.
    /// </summary>
    /// <param name="connectionUrl">The string containing connection URL for <see cref="Domain"/>.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="connectionUrl"/> is null or empty string.</exception>
    public DomainConfiguration(string connectionUrl)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionUrl, "connectionUrl");
      connectionInfo = new UrlInfo(connectionUrl);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class.
    /// </summary>
    public DomainConfiguration()
    {
      namingConvention = new NamingConvention();
      sessions = new SessionConfigurationCollection();
      mappings = new SetSlim<Type>();
      serviceContainer = new UnityContainer();
      serviceContainer.AddExtension(new SingletonExtension());
    }
  }
}