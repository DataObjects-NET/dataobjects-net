// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.01

using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Compilation.DefaultCompilers;

namespace Xtensive.Storage.Providers.Index
{
  public class DomainHandler: Providers.DomainHandler
  {
    private readonly Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>> realIndexes = new Dictionary<IndexInfo, IUniqueOrderedIndex<Tuple, Tuple>>();
    private readonly Dictionary<IndexInfo, MapTransform> indexTransforms = new Dictionary<IndexInfo, MapTransform>();

    public override void Build()
    {
      BuildRealIndexes();
      foreach (IndexInfo indexInfo in ExecutionContext.Model.Types.SelectMany(type => type.Indexes)) {
        MapTransform transform = BuildIndexTransform(indexInfo);
        indexTransforms.Add(indexInfo, transform);
      }
    }

    protected override CompilationContext GetCompilationContext()
    {
      return new CompilationContext(new CompilerResolver[] { new Compilers.CompilerResolver(ExecutionContext), new DefaultCompilerResolver() });
    }

    private MapTransform BuildIndexTransform(IndexInfo indexInfo)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(indexInfo.Columns.Select(columnInfo => columnInfo.ValueType));
      int[] map = indexInfo.Columns.Select(column => column.Field.MappingInfo.Offset).ToArray();
      return new MapTransform(true, descriptor, map);
    }

    private void BuildRealIndexes()
    {
      foreach (IndexInfo indexInfo in ExecutionContext.Model.RealIndexes) {
        var indexConfig = new IndexConfiguration<Tuple, Tuple>();
        DirectionCollection<ColumnInfo> orderingRule;
        if (indexInfo.IsUnique | indexInfo.IsPrimary)
          orderingRule = new DirectionCollection<ColumnInfo>(indexInfo.KeyColumns);
        else
          orderingRule = new DirectionCollection<ColumnInfo>(
            indexInfo.KeyColumns
              .Union(indexInfo.ValueColumns.Select(info => new KeyValuePair<ColumnInfo, Direction>(info, Direction.Positive))));
        // TODO: manage CultureInfo for columns \ fields
        indexConfig.KeyComparer = AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(
          ComparisonRule.Positive,
          orderingRule.Select(pair => (ComparisonRules)new ComparisonRule(pair.Value, CultureInfo.InvariantCulture)).ToArray(),
          ComparisonRules.None));
        indexConfig.KeyExtractor = input => input;
        IUniqueOrderedIndex<Tuple, Tuple> index = IndexFactory.CreateUniqueOrdered<Tuple, Tuple, Index<Tuple, Tuple>>(indexConfig);
        realIndexes[indexInfo] = index;
      }
    }

    internal IUniqueOrderedIndex<Tuple, Tuple> GetRealIndex(IndexInfo indexInfo)
    {
      return realIndexes[indexInfo];
    }

    internal MapTransform GetIndexTransform(IndexInfo indexInfo)
    {
      return indexTransforms[indexInfo];
    }
  }
}