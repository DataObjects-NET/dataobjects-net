// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Compilable
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
    public string ExistenceColumnName { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return new RecordSetHeader(
        TupleDescriptor.Create(new[] { typeof(bool) }),
        new[] { new SystemColumn(ExistenceColumnName, 0, typeof(bool)) });
    }

    /// <inheritdoc/>
    protected override DirectionCollection<int> CreateExpectedColumnsOrdering()
    {
      return EmptyOrder;
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExistenceProvider(CompilableProvider source, string existenceColumnName)
      : base(ProviderType.Existence, source)
    {
      ExistenceColumnName = existenceColumnName;
    }
  }
}