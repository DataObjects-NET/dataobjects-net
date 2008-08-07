// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration.TypeRegistry;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// The configuration of the <see cref="Domain"/>.
  /// </summary>
  [Serializable]
  public class DomainConfiguration : ConfigurationSectionBase
  {
    /// <summary>
    /// Default session pool size.
    /// </summary>
    public const int DefaultSessionPoolSize = 64;

    private const string SessionPoolSizeElementName = "SessionPoolSize";
    private const string ConnectionInfoElementName = "ConnectionInfo";
    private const string BuildersElementName = "Builders";
    private const string TypesElementName = "Types";
    private const string NamingConventionElementName = "NamingConvention";

    //    private ServiceRegistry services = new ServiceRegistry();
    private CollectionBase<Type> builders = new CollectionBase<Type>();
    private NamingConvention namingConvention;
    private Registry types = new Registry(new TypeProcessor());

    /// <summary>
    /// Gets or sets the size of the session pool. The default value is <see cref="DefaultSessionPoolSize"/>.
    /// </summary>
    [ConfigurationProperty(SessionPoolSizeElementName, DefaultValue = DefaultSessionPoolSize, IsRequired = false)]
    [IntegerValidator(MinValue = 1, MaxValue = int.MaxValue)]
    public int SessionPoolSize
    {
      get { return (int)this[SessionPoolSizeElementName]; }
      set
      {
        this.EnsureNotLocked();
        this[SessionPoolSizeElementName] = value;
      }
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
    /// Gets the <see cref="ICollection{T}"/> of persistent <see cref="Type"/>s that are about to be 
    /// registered in <see cref="Domain"/>.
    /// </summary>
    public Registry Types
    {
      get { return types; }
    }

    /// <summary>
    /// Gets or sets the connection info.
    /// </summary>
    [ConfigurationProperty(ConnectionInfoElementName, DefaultValue = null, IsRequired = true)]
    public UrlInfo ConnectionInfo
    {
      get { return (UrlInfo)this[ConnectionInfoElementName]; }
      set
      {
        this.EnsureNotLocked();
        this[ConnectionInfoElementName] = value;
      }
    }

    /// <summary>
    /// Gets the collection of custom <see cref="DomainModelDef"/> builders.
    /// </summary>
    public IList<Type> Builders
    {
      get { return builders; }
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      types.Lock(true);
      builders.Lock(true);
      //      services.Lock(true);
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public override void Validate()
    {
    }

    protected override ConfigurationSectionBase CreateClone()
    {
      return new DomainConfiguration();
    }

    protected override void Clone(ConfigurationSectionBase source)
    {
      base.Clone(source);
      var configuration = (DomainConfiguration) source;
      builders = new CollectionBase<Type>(configuration.Builders);
      ConnectionInfo = new UrlInfo(configuration.ConnectionInfo.ToString());
      namingConvention = (NamingConvention) configuration.NamingConvention.Clone();
      types = (Registry) configuration.Types.Clone();
      SessionPoolSize = configuration.SessionPoolSize;
    }

    protected override void PostDeserialize()
    {
      base.PostDeserialize();
      foreach (BuilderConfigElement builder in (ConfigurationCollection<BuilderConfigElement>)this[BuildersElementName]) {
        Type type = Type.GetType(builder.Type, true);
        Builders.Add(type);
      }
      foreach (TypesConfigElement registry in (ConfigurationCollection<TypesConfigElement>)this[TypesElementName]) {
        Assembly assembly = Assembly.Load(registry.Assembly);
        if (registry.Namespace.IsNullOrEmpty())
          Types.Register(assembly);
        else
          Types.Register(assembly, registry.Namespace);
      }
      NamingConventionElement namingConventionElement = this[NamingConventionElementName] as NamingConventionElement;
      namingConvention = namingConventionElement;
    }

    public override bool IsReadOnly()
    {
      return true;
    }

    private void AddConfigurationElements()
    {
      Properties.Add(new ConfigurationProperty(BuildersElementName, typeof(ConfigurationCollection<BuilderConfigElement>)));
      Properties.Add(new ConfigurationProperty(TypesElementName, typeof(ConfigurationCollection<TypesConfigElement>)));
      Properties.Add(new ConfigurationProperty(NamingConventionElementName, typeof(NamingConventionElement)));
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
      ConnectionInfo = new UrlInfo(connectionUrl);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class.
    /// </summary>
    public DomainConfiguration()
    {
      AddConfigurationElements();
      namingConvention = new NamingConvention();
    }
  }
}