// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class MappingRuleOptions : IIdentifyableOptions,
    IValidatableOptions,
    IToNativeConvertable<MappingRule>
  {
    public object Identifier => (Assembly ?? string.Empty, Namespace ?? string.Empty);

    /// <summary>
    /// Assembly condition.
    /// See <see cref="MappingRule.Assembly"/> for details.
    /// </summary>
    public string Assembly { get; set; }

    /// <summary>
    /// Namespace condition.
    /// See <see cref="MappingRule.Namespace"/> for details.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Database that is assigned to mapped type when this rule is applied.
    /// See <see cref="MappingRule.Database"/> for details.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Schema that is assigned to mapped type when this rule is applied.
    /// See <see cref="MappingRule.Schema"/> for details.
    /// </summary>
    public string Schema { get; set; }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Configuration of values in properties is not valid.</exception>
    public void Validate()
    {
      if (Assembly.IsNullOrEmpty() && Namespace.IsNullOrEmpty())
        throw new ArgumentException("Mapping rule should declare at least either Assembly or Namespace");
      if (Database.IsNullOrEmpty() && Schema.IsNullOrEmpty())
        throw new ArgumentException("Mapping rule should map assembly and(or) namespace to database, schema or both");
    }

    /// <inheritdoc />
    public MappingRule ToNative()
    {
      Validate();

      var assembly = !string.IsNullOrEmpty(Assembly) ? System.Reflection.Assembly.Load(Assembly) : null;
      return new MappingRule(assembly, Namespace, Database, Schema);
    }
  }
}
