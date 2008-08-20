// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Property compare result.
  /// </summary>
  [Serializable]
  public class PropertyComparisonResult<T> : ComparisonResult<T>
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
  }
}