// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.ObjectMapping.Model;

namespace Xtensive.Core.ObjectMapping
{
  /// <summary>
  /// Information about an operation that modified a state of an transformed object.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Object = {Object.GetType().Name}, Type = {Type}, PropertyPath = {propertyPathString}")]
  public struct OperationInfo
  {
    private readonly string propertyPathString;

    /// <summary>
    /// The original transformed object.
    /// </summary>
    public readonly object Object;

    /// <summary>
    /// The path to the property that has been modified.
    /// </summary>
    public readonly ReadOnlyCollection<TargetPropertyDescription> PropertyPath;

    /// <summary>
    /// The operation type.
    /// </summary>
    public readonly OperationType Type;

    /// <summary>
    /// The new property value or the collection's item.
    /// </summary>
    public readonly object Value;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="obj">The value of <see cref="Object"/>.</param>
    /// <param name="type">The value of <see cref="Type"/>.</param>
    /// <param name="propertyPath">The value of <see cref="PropertyPath"/>.</param>
    /// <param name="value">The value of <see cref="Value"/>.</param>
    public OperationInfo(object obj, OperationType type, TargetPropertyDescription[] propertyPath, object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(obj, "obj");

      Object = obj;
      Type = type;
      PropertyPath = propertyPath!=null ? Array.AsReadOnly(propertyPath) : null;
      Value = value;
      propertyPathString = null;

#if DEBUG
      if (PropertyPath!=null) {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < PropertyPath.Count; i++) {
          if (i > 0)
            stringBuilder.Append(".");
          stringBuilder.Append(PropertyPath[i].SystemProperty.Name);
        }
        propertyPathString = stringBuilder.ToString();
      }
      else
        propertyPathString = null;
#endif
    }
  }
}