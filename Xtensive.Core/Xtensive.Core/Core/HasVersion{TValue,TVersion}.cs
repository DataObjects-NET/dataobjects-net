// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.06.01

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// A pair of <see cref="Value"/> and its <see cref="Version"/>.
  /// </summary>
  /// <typeparam name="TValue">The <see cref="Type"/> of <see cref="Value"/>.</typeparam>
  /// <typeparam name="TVersion">The <see cref="Type"/> of <see cref="Version"/>.</typeparam>
  [Serializable]
  [DebuggerDisplay("{Value}, Version = {Version}")]
  public struct HasVersion<TValue, TVersion> : 
    IComparable<HasVersion<TValue, TVersion>>,
    IEquatable<HasVersion<TValue, TVersion>>
  {
    /// <summary>
    /// The value.
    /// </summary>
    public TValue Value;

    /// <summary>
    /// The version bound to <see cref="Value"/>.
    /// </summary>
    public TVersion Version;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(HasVersion<TValue, TVersion> other)
    {
      if (!AdvancedComparerStruct<TValue>.System.Equals(Value, other.Value))
        return false;
      return AdvancedComparerStruct<TVersion>.System.Equals(Version, other.Version);
    }

    /// <inheritdoc/>
    public int CompareTo(HasVersion<TValue, TVersion> other)
    {
      int result = AdvancedComparerStruct<TValue>.System.Compare(Value, other.Value);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<TVersion>.System.Compare(Version, other.Version);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (HasVersion<TValue, TVersion>))
        return false;
      return Equals((HasVersion<TValue, TVersion>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return 
          ((Value!=null ? Value.GetHashCode() : 0) * 397) ^ 
          (Version!=null ? Version.GetHashCode() : 0);
      }
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true"/>
    public static bool operator ==(HasVersion<TValue, TVersion> left, HasVersion<TValue, TVersion> right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true"/>
    public static bool operator !=(HasVersion<TValue, TVersion> left, HasVersion<TValue, TVersion> right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.HasVersionFormat, Value, Version);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Initial <see cref="Value"/> value.</param>
    /// <param name="version">Initial <see cref="Version"/> value.</param>
    public HasVersion(TValue value, TVersion version)
    {
      Value = value;
      Version = version;
    }
  }
}