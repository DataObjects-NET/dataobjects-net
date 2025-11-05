// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.12.28

using System;
using System.Diagnostics;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Executes full-text search (free text query) against specified <see cref="PrimaryIndex"/>.
  /// </summary>
  [Serializable]
  public sealed class FreeTextProvider : CompilableProvider
  {
    private readonly RecordSetHeader indexHeader;

    public Func<ParameterContext, string> SearchCriteria { get; private set; }

    public Func<ParameterContext, int> TopN { get; private set; }

    public IndexInfoRef PrimaryIndex { get; private set; }

    public bool FullFeatured { get; private set; }

    protected override RecordSetHeader BuildHeader()
    {
      return indexHeader;
    }

    public FreeTextProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, null, fullFeatured)
    {
    }

    public FreeTextProvider(
      FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, Func<ParameterContext, int> topN, bool fullFeatured)
      : base(ProviderType.FreeText)
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      TopN = topN;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
      if (FullFeatured) {
        var primaryIndexRecordsetHeader = index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
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
          .ToArray(primaryIndexKeyColumns.Count + 1);
        indexHeader = new RecordSetHeader(tupleDescriptor, columns);
      }
      Initialize();
    }
  }
}
