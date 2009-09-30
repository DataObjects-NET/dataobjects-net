// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains information about entity version.
  /// </summary>
  [DebuggerDisplay("{Value}")]
  public struct VersionInfo : IEquatable<VersionInfo>
  {
    private int cachedHashCode;
    private bool isHashCodeCalculated;
    private readonly Tuple value;

    /// <summary>
    /// Gets a value indicating whether this instance is not contains version.
    /// </summary>
    public bool IsEmpty
    {
      [DebuggerStepThrough]
      get { return value==null; }
    }

    internal Tuple Value
    {
      [DebuggerStepThrough]
      get { return value; }
    }

    #region Equals, GetHashCode, ==, != 

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(VersionInfo other)
    {
      if (IsEmpty)
        return other.IsEmpty;
      return Value.Equals(other.Value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (IsEmpty)
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
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator !=(VersionInfo left, VersionInfo right)
    {
      return !Equals(left, right);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      if (!isHashCodeCalculated && !IsEmpty) {
        cachedHashCode = Value.GetHashCode();
        isHashCodeCalculated = true;
      }
      return cachedHashCode;
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
    }
  }
}