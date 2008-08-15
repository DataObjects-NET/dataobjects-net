// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Result of properties compare.
  /// </summary>
  public class PropertyCompareResult : CompareResult
  {
    private readonly string propertyName;
    private readonly object newValue;
    private readonly object oldValue;
    private readonly CompareResultType result;

    /// <summary>
    /// Gets property name.
    /// </summary>
    public string PropertyName
    {
      get { return propertyName; }
    }

    /// <summary>
    /// Gets old property value.
    /// </summary>
    public object OldValue
    {
      get { return oldValue; }
    }

    /// <summary>
    /// Gets new property value.
    /// </summary>
    public object NewValue
    {
      get { return newValue; }
    }

    /// <inheritdoc/>
    public override bool HasChanges
    {
      get { return true; }
    }

    /// <inheritdoc/>
    public override CompareResultType Result
    {
      get { return result; }
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
      this.result = result;
      this.oldValue = oldValue;
      this.propertyName = propertyName;
      this.newValue = newValue;
    }
  }
}