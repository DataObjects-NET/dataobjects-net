// Copyright (C) 2003-2010 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a query.
  /// </summary>
  public class QueryInfo : LockableBase
  {
    private int maxLength;
    private int maxComparisonOperations;
    private int maxNestedQueriesAmount;
    private string parameterPrefix;
    private QueryFeatures features = QueryFeatures.None;
    private int maxQueryParameterCount;

    /// <summary>
    /// Gets or sets the maximal length of a query text in characters.
    /// </summary>
    public int MaxLength
    {
      get => maxLength;
      set {
        EnsureNotLocked();
        maxLength = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximal number of comparison operations for a single query.
    /// </summary>
    public int MaxComparisonOperations
    {
      get => maxComparisonOperations;
      set {
        EnsureNotLocked();
        maxComparisonOperations = value;
      }
    }

    /// <summary>
    /// Gets or sets the nested subqueries amount.
    /// </summary>
    public int MaxNestedSubqueriesAmount
    {
      get => maxNestedQueriesAmount;
      set {
        EnsureNotLocked();
        maxNestedQueriesAmount = value;
      }
    }

    /// <summary>
    /// Gets or sets the Parameter prefix.
    /// </summary>
    public string ParameterPrefix
    {
      get => parameterPrefix;
      set {
        EnsureNotLocked();
        parameterPrefix = value;
      }
    }

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public QueryFeatures Features
    {
      get => features;
      set {
        EnsureNotLocked();
        features = value;
      }
    }

    /// <summary>
    /// Gets or sets max query parameter count.
    /// </summary>
    public int MaxQueryParameterCount
    {
      get => maxQueryParameterCount;
      set {
        EnsureNotLocked();
        maxQueryParameterCount = value;
      }
    }
  }
}
