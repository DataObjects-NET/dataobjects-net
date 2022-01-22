// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.03.20

using System;
using Xtensive.Reflection;
using Xtensive.Tuples;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that returns <see cref="bool"/> column. 
  /// Column value is <see langword="true" /> if <see cref="UnaryProvider.Source"/> contains any result; otherwise <see langword="false" />.
  /// </summary>
  [Serializable]
  public sealed class ExistenceProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the name of the existence column.
    /// </summary>
    public string ExistenceColumnName { get; }

    private static readonly TupleDescriptor BoolTupleDescriptor = TupleDescriptor.Create(new[] {WellKnownTypes.Bool});


    // Constructors

    private static RecordSetHeader BuildHeader(string existenceColumnName)
    {
      return new RecordSetHeader(
        BoolTupleDescriptor, new[] { new SystemColumn(existenceColumnName, 0, WellKnownTypes.Bool) });
    }
 
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ExistenceProvider(CompilableProvider source, string existenceColumnName)
      : base(ProviderType.Existence, BuildHeader(existenceColumnName), source)
    {
      ExistenceColumnName = existenceColumnName;
    }
  }
}