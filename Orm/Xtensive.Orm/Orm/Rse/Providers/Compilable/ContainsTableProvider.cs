// Copyright (C) 2016-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Tuples;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Executes full-text search (contains table query) against specified <see cref="PrimaryIndex"/>.
  /// </summary>
  [Serializable]
  public sealed class ContainsTableProvider : CompilableProvider
  {
    private readonly RecordSetHeader indexHeader;

    public Func<ParameterContext, string> SearchCriteria { get; private set; }

    public IndexInfoRef PrimaryIndex { get; private set; }

    public bool FullFeatured { get; private set; }

    public Func<ParameterContext, int> TopN { get; private set; }

    public IReadOnlyList<FullTextColumnInfo> TargetColumns { get; private set; } 

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, new List<ColumnInfo>(), null, fullFeatured)
    {
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, targetColumns, null, fullFeatured)
    {
      
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, Func<ParameterContext, int> topNByRank, bool fullFeatured)
      : base(ProviderType.ContainsTable)
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
      TargetColumns = targetColumns.SelectToList(tc => index.Columns.First(c => c.Column == tc))
        .AsReadOnly();
      TopN = topNByRank;
      if (FullFeatured) {
        var primaryIndexRecordsetHeader =
          index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
        var rankColumn = new MappedColumn(rankColumnName, primaryIndexRecordsetHeader.Length, WellKnownTypes.Double);
        indexHeader = primaryIndexRecordsetHeader.Add(rankColumn);
      }
      else {
        var primaryIndexKeyColumns = index.PrimaryIndex.KeyColumns;
        if (primaryIndexKeyColumns.Count!=1)
          throw new InvalidOperationException(Strings.ExOnlySingleColumnKeySupported);
        var fieldTypes = primaryIndexKeyColumns
          .Select(static columnInfo => columnInfo.Key.ValueType)
          .Append(WellKnownTypes.Double)
          .ToArray(primaryIndexKeyColumns.Count + 1);
        var tupleDescriptor = TupleDescriptor.Create(fieldTypes);
        var columns = primaryIndexKeyColumns
          .Select(static (c, i) => (Column) new MappedColumn("KEY", i, c.Key.ValueType))
          .Append(new MappedColumn("RANK", tupleDescriptor.Count, WellKnownTypes.Double))
          .ToArray(primaryIndexKeyColumns.Count + 1);;
        indexHeader = new RecordSetHeader(tupleDescriptor, columns);
      }
      Initialize();
    }
  }
}
