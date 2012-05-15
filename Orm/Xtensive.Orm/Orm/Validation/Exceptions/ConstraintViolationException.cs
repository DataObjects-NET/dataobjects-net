// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using System.Reflection;
using System.Runtime.Serialization;


namespace Xtensive.Orm.Validation
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
    ///   Initializes a new instance of this class.
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintViolationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    ///   
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
    protected ConstraintViolationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}