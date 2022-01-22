// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public Func<ParameterContext, string> SearchCriteria { get; }

    public IndexInfoRef PrimaryIndex { get; }

    public bool FullFeatured { get; }

    public Func<ParameterContext, int> TopN { get; }

    public IReadOnlyList<FullTextColumnInfo> TargetColumns { get; } 


    // Constructors

    private static RecordSetHeader BuildHeader(FullTextIndexInfo index, string rankColumnName, bool fullFeatured)
    {
      if (fullFeatured) {
        var primaryIndexRecordsetHeader =
          index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
        var rankColumn = new MappedColumn(rankColumnName, primaryIndexRecordsetHeader.Length, WellKnownTypes.Double);
        return primaryIndexRecordsetHeader.Add(rankColumn);
      }
      else {
        var primaryIndexKeyColumns = index.PrimaryIndex.KeyColumns;
        if (primaryIndexKeyColumns.Count!=1)
          throw new InvalidOperationException(Strings.ExOnlySingleColumnKeySupported);
        var fieldTypes = primaryIndexKeyColumns
          .Select(columnInfo => columnInfo.Key.ValueType)
          .Append(WellKnownTypes.Double)
          .ToArray(primaryIndexKeyColumns.Count + 1);
        var tupleDescriptor = TupleDescriptor.Create(fieldTypes);
        var columns = primaryIndexKeyColumns
          .Select((c, i) => (Column) new MappedColumn("KEY", i, c.Key.ValueType))
          .Append(new MappedColumn("RANK", tupleDescriptor.Count, WellKnownTypes.Double));
        return new RecordSetHeader(tupleDescriptor, columns);
      }
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, targetColumns, null, fullFeatured)
    {      
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, Func<ParameterContext, int> topNByRank, bool fullFeatured)
      : base(ProviderType.ContainsTable, BuildHeader(index, rankColumnName, fullFeatured))
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
      TargetColumns = targetColumns.Select(tc => index.Columns.First(c => c.Column == tc))
        .ToList(targetColumns.Count)
        .AsReadOnly();
      TopN = topNByRank;
    }
  }
}
