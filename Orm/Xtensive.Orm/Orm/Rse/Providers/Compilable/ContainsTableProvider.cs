using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
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

    public Func<string> SearchCriteria { get; private set; }

    public IndexInfoRef PrimaryIndex { get; private set; }

    public bool FullFeatured { get; private set; }

    public Func<int> TopN { get; private set; }

    public ReadOnlyList<FullTextColumnInfo> TargetColumns { get; private set; } 

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<string> searchCriteria, string rankColumnName, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, new List<ColumnInfo>(), null, fullFeatured)
    {
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, targetColumns, null, fullFeatured)
    {
      
    }

    public ContainsTableProvider(FullTextIndexInfo index, Func<string> searchCriteria, string rankColumnName, IList<ColumnInfo> targetColumns, Func<int> topNByRank, bool fullFeatured)
      : base(ProviderType.ContainsTable)
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
      TargetColumns = new ReadOnlyList<FullTextColumnInfo>(targetColumns.Select(tc=>index.Columns.First(c=>c.Column==tc)).ToList());
      TopN = topNByRank;
      if (FullFeatured) {
        var primaryIndexRecordsetHeader =
          index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
        var rankColumn = new MappedColumn(rankColumnName, primaryIndexRecordsetHeader.Length, typeof (double));
        indexHeader = primaryIndexRecordsetHeader.Add(rankColumn);
      }
      else {
        if (index.PrimaryIndex.KeyColumns.Count!=1)
          throw new InvalidOperationException(Strings.ExOnlySingleColumnKeySupported);
        var fieldTypes = index
          .PrimaryIndex
          .KeyColumns
          .Select(columnInfo => columnInfo.Key.ValueType)
          .AddOne(typeof (double));
        TupleDescriptor tupleDescriptor = TupleDescriptor.Create(fieldTypes);
        var columns = index
          .PrimaryIndex
          .KeyColumns
          .Select((c, i) => (Column) new MappedColumn("KEY", i, c.Key.ValueType))
          .AddOne(new MappedColumn("RANK", tupleDescriptor.Count, typeof (double)));
        indexHeader = new RecordSetHeader(tupleDescriptor, columns);
      }
      Initialize();
    }
  }
}
