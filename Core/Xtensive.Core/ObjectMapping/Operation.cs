// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.07

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.ObjectMapping.Model;

namespace Xtensive.ObjectMapping
{
  /// <summary>
  /// Provides information about an operation that modified state of mapped object.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Object = {Object.GetType().Name}, Type = {Type}, PropertyPath = {propertyPathString}")]
  public sealed class Operation
  {
    // Actually used in [DebuggerDisplay], see above.
    // Note that propertyPath is built only in Debug configuration.
    private readonly string propertyPath;

    /// <summary>
    /// The target (mapped) object.
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
    /// The new property value or the collection item, if any.
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
    public Operation(object obj, OperationType type, TargetPropertyDescription[] propertyPath, object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(obj, "obj");

      Object = obj;
      Type = type;
      PropertyPath = propertyPath!=null ? Array.AsReadOnly(propertyPath) : null;
      Value = value;
      this.propertyPath = null;

#if DEBUG
      if (PropertyPath!=null) {
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < PropertyPath.Count; i++) {
          if (i > 0)
            stringBuilder.Append(".");
          stringBuilder.Append(PropertyPath[i].SystemProperty.Name);
        }
        this.propertyPath = stringBuilder.ToString();
      }
      else
        this.propertyPath = null;
#endif
    }
  }
}