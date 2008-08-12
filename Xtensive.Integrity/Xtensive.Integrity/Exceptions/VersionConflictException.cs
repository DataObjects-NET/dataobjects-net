// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity
{
  /// <summary>
  /// Thrown as the result of version conflict.
  /// </summary>
  [Serializable]
  public class VersionConflictException : ConcurrencyConflictException
  {
    private object target;
    private string propertyName;
    private object expectedValue;
    private object actualValue;

    /// <summary>
    /// An object on which version conflict was detected.
    /// </summary>
    [DebuggerStepThrough]
    public object Target
    {
      get { return target; }
    }

    /// <summary>
    /// Name of the property of the <see cref="Target"/>, which value differs from the expected one.
    /// </summary>
    [DebuggerStepThrough]
    public string PropertyName
    {
      get { return propertyName; }
    }

    /// <summary>
    /// Expected value of the <see cref="PropertyName"/> property of the <see cref="Target"/>.
    /// </summary>
    [DebuggerStepThrough]
    public object ExpectedValue
    {
      get { return expectedValue; }
    }

    /// <summary>
    /// Actual value of the <see cref="PropertyName"/> property of the <see cref="Target"/>.
    /// </summary>
    [DebuggerStepThrough]
    public object ActualValue
    {
      get { return actualValue; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public VersionConflictException ()
      : base(Strings.ExVersionConflict)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="target">Initial <see cref="Target"/> property value.</param>
    /// <param name="propertyName">Initial <see cref="PropertyName"/> property value.</param>
    /// <param name="expectedValue">Initial <see cref="ExpectedValue"/> property value.</param>
    /// <param name="actualValue">Initial <see cref="ActualValue"/> property value.</param>
    public VersionConflictException(object target, string propertyName, object expectedValue, object actualValue) 
      : base(String.Format(Strings.ExVersionConflictEx, target, propertyName, expectedValue, actualValue))
    {
      this.target = target;
      this.propertyName = propertyName;
      this.expectedValue = expectedValue;
      this.actualValue = actualValue;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="text">Text of message.</param>
    public VersionConflictException(string text)
      : base(text)
    {
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected VersionConflictException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      target = info.GetValue("Target", typeof(object));
      propertyName = info.GetString("PropertyName");
      expectedValue = info.GetValue("ExpectedValue", typeof(object));
      actualValue = info.GetValue("ActualValue", typeof(object));
    }
  }
}