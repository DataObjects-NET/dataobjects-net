// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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
