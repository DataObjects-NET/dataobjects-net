// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using System.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Database alias element within a configuration file.
  /// </summary>
  public class DatabaseConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string RealNameElementName = "realName";
    private const string MinTypeIdElementName = "minTypeId";
    private const string MaxTypeIdElementName = "maxTypeId";

    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="DatabaseConfiguration.Name" />
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DatabaseConfiguration.RealName" />
    /// </summary>
    [ConfigurationProperty(RealNameElementName)]
    public string RealName
    {
      get { return (string) this[RealNameElementName]; }
      set { this[RealNameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DatabaseConfiguration.MinTypeId" />
    /// </summary>
    [ConfigurationProperty(MinTypeIdElementName, DefaultValue = TypeInfo.MinTypeId)]
    public int MinTypeId
    {
      get { return (int) this[MinTypeIdElementName]; }
      set { this[MinTypeIdElementName] = value; }
    }

    /// <summary>
    /// <see cref="DatabaseConfiguration.MaxTypeId" />
    /// </summary>
    [ConfigurationProperty(MaxTypeIdElementName, DefaultValue = int.MaxValue)]
    public int MaxTypeId
    {
      get { return (int) this[MaxTypeIdElementName]; }
      set { this[MaxTypeIdElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public DatabaseConfiguration ToNative()
    {
      return new DatabaseConfiguration(Name) {
        RealName = RealName,
        MinTypeId = MinTypeId,
        MaxTypeId = MaxTypeId,
      };
    }
  }
}