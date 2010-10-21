// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Validation
{
  /// <summary>
  /// Thrown as the result of violation of constraint.
  /// </summary>
  [Serializable]
  public class ConstraintViolationException : Exception
  {
    /// <summary>
    /// Gets the target type.
    /// </summary>
    public Type TargetType { get; private set;}

    /// <summary>
    /// Gets the target property.
    /// </summary>
    public PropertyInfo TargetProperty { get; private set;}

    /// <summary>
    /// Gets the string representation of property value.
    /// </summary>
    /// <value>The property value.</value>
    public string PropertyValue { get; set;}


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>  
    /// <param name="message">The error message.</param>
    public ConstraintViolationException(string message, Type type, PropertyInfo property, object value)
      : base(message)
    {
      TargetType = type;
      TargetProperty = property;
      PropertyValue = value == null ? "null" : value.ToString();
    }

    // Serialization

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected ConstraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}