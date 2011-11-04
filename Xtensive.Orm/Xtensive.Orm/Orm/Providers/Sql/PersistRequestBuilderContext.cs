// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="PersistRequestBuilder"/> context.
  /// </summary>
  public sealed class PersistRequestBuilderContext
  {
    public SqlBatch Batch { get; private set; }

    public PersistRequestBuilderTask Task { get; private set; }

    public TypeInfo Type { get; private set; }

    public ReadOnlyList<IndexInfo> AffectedIndexes { get; private set;}

    public IndexInfo PrimaryIndex { get; private set; }

    public Dictionary<ColumnInfo, PersistParameterBinding> ParameterBindings { get; private set; }


    // Constructors

    public PersistRequestBuilderContext(PersistRequestBuilderTask task)
    {
      Task = task;
      Type = task.Type;
      var affectedIndexes = Type.AffectedIndexes.Where(index => index.IsPrimary).ToList();
      affectedIndexes.Sort((left, right)=>{
          if (left.ReflectedType.GetAncestors().Contains(right.ReflectedType))
            return 1;
          if (right.ReflectedType.GetAncestors().Contains(left.ReflectedType))
            return -1;
          return 0;});
      AffectedIndexes = new ReadOnlyList<IndexInfo>(affectedIndexes);
      PrimaryIndex = Task.Type.Indexes.PrimaryIndex;
      ParameterBindings = new Dictionary<ColumnInfo, PersistParameterBinding>();
    }
  }
}