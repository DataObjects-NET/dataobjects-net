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
using Xtensive.Storage.Indexing.Model.Resources;

namespace Xtensive.Storage.Indexing.Model
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

    [Property(Priority = -2000)]
    public string FullTextCatalog { get; set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<FullTextIndexInfo, TableInfo, FullTextIndexInfoCollection>(this, "FullTextIndexes");
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
            throw new ValidationException(Strings.ExEmptyColumnsCollection, Path);
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