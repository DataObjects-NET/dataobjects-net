// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
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
    private string quoteToken;
    private QueryFeatures features = QueryFeatures.None;

    /// <summary>
    /// Gets or sets the maximal length of a query text in characters.
    /// </summary>
    public int MaxLength
    {
      get { return maxLength; }
      set
      {
        this.EnsureNotLocked();
        maxLength = value;
      }
    }

    /// <summary>
    /// Gets or sets the maximal number of comparison operations for a single query.
    /// </summary>
    public int MaxComparisonOperations
    {
      get { return maxComparisonOperations; }
      set
      {
        this.EnsureNotLocked();
        maxComparisonOperations = value;
      }
    }

    /// <summary>
    /// Gets or sets the nested subqueries amount.
    /// </summary>
    public int MaxNestedSubqueriesAmount
    {
      get { return maxNestedQueriesAmount; }
      set
      {
        this.EnsureNotLocked();
        maxNestedQueriesAmount = value;
      }
    }

    /// <summary>
    /// Gets or sets the Parameter prefix.
    /// </summary>
    public string ParameterPrefix
    {
      get { return parameterPrefix; }
      set
      {
        this.EnsureNotLocked();
        parameterPrefix = value;
      }
    }

    /// <summary>
    /// Gets or sets the identifier quote token.
    /// </summary>
   public string QuoteToken
    {
      get { return quoteToken; }
      set
      {
        this.EnsureNotLocked();
        quoteToken = value;
      }
    }

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public QueryFeatures Features
    {
      get { return features; }
      set
      {
        this.EnsureNotLocked();
        features = value;
      }
    }
  }
}
