// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using StreamingContext=System.Runtime.Serialization.StreamingContext;
using System.Runtime.Serialization;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains information about entity version.
  /// </summary>
  [DebuggerDisplay("{Value}")]
  [Serializable]
  public struct VersionInfo : IEquatable<VersionInfo>
  {
    private const string EmptyStringValue = "Void";
    [NonSerialized]
    private int cachedHashCode;
    [NonSerialized]
    private bool isHashCodeCalculated;
    [NonSerialized]
    private Tuple value;

    private object serializedValue;

    /// <summary>
    /// Gets a value indicating whether this instance is not contains version.
    /// </summary>
    public bool IsVoid
    {
      [DebuggerStepThrough]
      get { return value==null; }
    }

    internal Tuple Value
    {
      [DebuggerStepThrough]
      get { return value; }
    }

    #region Equals, GetHashCode, ==, !=, ToString

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(VersionInfo other)
    {
      if (IsVoid || other.IsVoid)
        return true;
      return Value.Equals(other.Value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (IsVoid)
        return false;
      if (ReferenceEquals(obj, null))
        return false;
      if (obj.GetType()!= typeof(VersionInfo))
        return false;
      if (obj.GetHashCode() != GetHashCode())
        return false;
      return Equals((VersionInfo) obj);
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator ==(VersionInfo left, VersionInfo right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator !=(VersionInfo left, VersionInfo right)
    {
      return !left.Equals(right);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      if (!isHashCodeCalculated) {
        if (!IsVoid)
          cachedHashCode = Value.GetHashCode();
        isHashCodeCalculated = true;
      }
      return cachedHashCode;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string ToString()
    {
      return value==null ? EmptyStringValue : value.ToString(true);
    }

    # endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="version">The version tuple.</param>
    public VersionInfo(Tuple version)
    {
      value = version;
      isHashCodeCalculated = false;
      cachedHashCode = 0;
      serializedValue = null;
    }


    // Serialization

    [OnSerializing]
    internal void OnSerializing(StreamingContext context)
    {
      serializedValue = new SerializedTuple(value);
    }

    [OnSerialized]
    internal void OnSerialized(StreamingContext context)
    {
      serializedValue = null;
    }

    [OnDeserialized]
    internal void OnDeserialized(StreamingContext context)
    {
      value = ((SerializedTuple) serializedValue).Value;
      serializedValue = null;
    }
  }
}