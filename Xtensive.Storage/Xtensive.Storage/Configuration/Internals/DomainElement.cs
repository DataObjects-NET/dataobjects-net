// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.11

using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// &lt;domain&gt; configuration element within a configuration file.
  /// </summary>
  internal class DomainElement : ConfigurationCollectionElementBase
  {
    /// <summary>
    /// Default session pool size.
    /// </summary>
    public const int DefaultSessionPoolSize = 64;

    private const string NameElementName = "name";
    private const string SessionPoolSizeElementName = "sessionPoolSize";
    private const string NamingConventionElementName = "namingConvention";
    private const string ConnectionInfoElementName = "connectionInfo";
    private const string BuildersElementName = "builders";
    private const string TypesElementName = "types";

    /// <summary>
    /// Gets unique section name.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true, IsRequired = false, DefaultValue = "")]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

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
        this[SessionPoolSizeElementName] = value;
      }
    }

    /// <summary>
    /// Types to register in domain.
    /// </summary>
    [ConfigurationProperty(TypesElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<TypeElement>), AddItemName = "add")]
    public ConfigurationCollection<TypeElement> Types
    {
      get
      {
        return (ConfigurationCollection<TypeElement>)base[TypesElementName];
      }
    }

    /// <summary>
    /// Builders to register in domain.
    /// </summary>
    [ConfigurationProperty(BuildersElementName, IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(ConfigurationCollection<BuilderElement>), AddItemName = "builder")]
    public ConfigurationCollection<BuilderElement> Builders
    {
      get
      {
        return (ConfigurationCollection<BuilderElement>)base[BuildersElementName];
      }
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
        this[ConnectionInfoElementName] = value;
      }
    }

    /// <summary>
    /// Gets or sets the naming convention.
    /// </summary>
    [ConfigurationProperty(NamingConventionElementName, IsRequired = false)]
    public NamingConventionElement NamingConvention
    {
      get { return (NamingConventionElement)this[NamingConventionElementName]; }
      set
      {
        this[NamingConventionElementName] = value;
      }
    }

    /// <inheritdoc/>
    public override object GetKey()
    {
      return Name;
    }
  }
}