// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Property compare result.
  /// </summary>
  public class PropertyCompareResult : CompareResult<object>
  {
    private string propertyName;

    /// <summary>
    /// Gets property name.
    /// </summary>
    public string PropertyName
    {
      get { return propertyName; }
      set
      {
        this.EnsureNotLocked(); 
        propertyName = value;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="result">Compare result.</param>
    /// <param name="oldValue">Old property value.</param>
    /// <param name="newValue">New property value.</param>
    /// <param name="propertyName">Property name.</param>
    public PropertyCompareResult(CompareResultType result, object oldValue, object newValue, string propertyName)
    {
      this.propertyName = propertyName;
    }
  }
}