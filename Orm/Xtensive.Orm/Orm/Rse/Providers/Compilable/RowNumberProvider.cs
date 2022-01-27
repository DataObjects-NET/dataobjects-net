// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.03.05

using System;
using Xtensive.Reflection;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that adds row number to <see cref="UnaryProvider.Source"/>.
  /// </summary>
  [Serializable]
  public sealed class RowNumberProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the row number column.
    /// </summary>
    public SystemColumn SystemColumn { get; }

    // Constructors

    private static RecordSetHeader CreateHeaderAndColumn(CompilableProvider source, string columnName, out SystemColumn systemColumn)
    {
      var sourceHeader = source.Header;
      systemColumn = new SystemColumn(columnName, sourceHeader.Length, WellKnownTypes.Int64);
      return sourceHeader.Add(systemColumn);
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="columnName">The name of <see cref="SystemColumn"/>.</param>
    public RowNumberProvider(CompilableProvider source, string columnName)
      : base(ProviderType.RowNumber, CreateHeaderAndColumn(source, columnName, out var systemColumn), source)
    {
      SystemColumn = systemColumn;
    }
  }
}