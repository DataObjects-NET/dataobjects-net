// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.12.28

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Tuples;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Executes full-text search (free text query) against specified <see cref="PrimaryIndex"/>.
  /// </summary>
  [Serializable]
  public sealed class FreeTextProvider : CompilableProvider
  {
    public Func<ParameterContext, string> SearchCriteria { get; }

    public Func<ParameterContext, int> TopN { get; }

    public IndexInfoRef PrimaryIndex { get; }

    public bool FullFeatured { get; }


    // Constructors

    private static RecordSetHeader BuildHeader(FullTextIndexInfo index, string rankColumnName, bool fullFeatured)
    {
      if (fullFeatured) {
        var primaryIndexRecordsetHeader = index.PrimaryIndex.ReflectedType.Indexes.PrimaryIndex.GetRecordSetHeader();
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

    public FreeTextProvider(FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, bool fullFeatured)
      : this(index, searchCriteria, rankColumnName, null, fullFeatured)
    {
    }

    public FreeTextProvider(
      FullTextIndexInfo index, Func<ParameterContext, string> searchCriteria, string rankColumnName, Func<ParameterContext, int> topN, bool fullFeatured)
      : base(ProviderType.FreeText, BuildHeader(index, rankColumnName, fullFeatured))
    {
      SearchCriteria = searchCriteria;
      FullFeatured = fullFeatured;
      TopN = topN;
      PrimaryIndex = new IndexInfoRef(index.PrimaryIndex);
    }
  }
}
