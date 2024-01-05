// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.22

using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Type = System.Type;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataMapping
  {
    private readonly NameBuilder nameBuilder;

    public readonly TypeMapping StringMapping;
    public readonly TypeMapping IntMapping;

    public readonly string Assembly;
    public readonly string AssemblyName;
    public readonly string AssemblyVersion;

    public readonly string Type;
    public readonly string TypeId;
    public readonly string TypeName;

    public readonly string Extension;
    public readonly string ExtensionName;
    public readonly string ExtensionText;

    private string TableOf(Type type)
    {
      var name = type
        .GetCustomAttributes(typeof (TableMappingAttribute), false)
        .Cast<TableMappingAttribute>()
        .Single().Name;
      return nameBuilder.ApplyNamingRules(name);
    }

    private string ColumnOf<TItem, TProperty>(Expression<Func<TItem, TProperty>> expression)
    {
      var memberExpression = (MemberExpression) expression.Body.StripCasts();
      var name = memberExpression.Member.Name;
      return nameBuilder.ApplyNamingRules(name);
    }

    // Constructors

    public MetadataMapping(StorageDriver driver, NameBuilder nameBuilder)
    {
      this.nameBuilder = nameBuilder;

      Assembly = TableOf(typeof (Assembly));
      AssemblyName = ColumnOf((Assembly x) => x.Name);
      AssemblyVersion = ColumnOf((Assembly x) => x.Version);

      Type = TableOf(typeof (Metadata.Type));
      TypeId = ColumnOf((Metadata.Type x) => x.Id);
      TypeName = ColumnOf((Metadata.Type x) => x.Name);

      Extension = TableOf(typeof (Extension));
      ExtensionName = ColumnOf((Extension x) => x.Name);
      ExtensionText = ColumnOf((Extension x) => x.Text);

      StringMapping = driver.GetTypeMapping(WellKnownTypes.String);
      IntMapping = driver.GetTypeMapping(WellKnownTypes.Int32);
    }
  }
}