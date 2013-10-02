// Copyright (C) 2010 Xtensive LLC.

// All rights reserved.

// For conditions of distribution and use, see license.

// Created by: Alexis Kochetov

// Created:    2010.01.14


using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// Full-text index.
  /// </summary>
  [Serializable]
  public sealed class FullTextIndexInfo : NodeBase<TableInfo>
  {
    /// <summary>
    /// Gets columns.
    /// </summary>
    [Property(Priority = -1000)]
    public FullTextColumnRefCollection Columns { get; private set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<FullTextIndexInfo, TableInfo, FullTextIndexInfo>(this, "FullTextIndex");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      if (Columns == null)
        Columns = new FullTextColumnRefCollection(this);
    }

    /// <exception cref="ValidationException"></exception>
    /// <inheritdoc/>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        base.ValidateState();

        var tableColumns = Parent.Columns;
        var columns = Columns.Select(keyRef => keyRef.Value).ToList();
        
        if (columns.Count == 0) {
          ea.Execute(() => {
            throw new ValidationException("IDDQ", Path);
          });
        }

        ea.Complete();
      }
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent table.</param>
    /// <param name="name">The index.</param>
    public FullTextIndexInfo(TableInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}