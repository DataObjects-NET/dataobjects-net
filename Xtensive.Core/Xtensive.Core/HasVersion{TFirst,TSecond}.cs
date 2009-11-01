// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.06.01

using System;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// A pair of <see cref="Value"/> and its <see cref="Version"/>.
  /// </summary>
  /// <typeparam name="TValue">The <see cref="Type"/> of <see cref="Value"/>.</typeparam>
  /// <typeparam name="TVersion">The <see cref="Type"/> of <see cref="Version"/>.</typeparam>
  [Serializable]
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

    /// <inheritdoc/>
    public bool Equals(HasVersion<TValue, TVersion> other)
    {
      return AdvancedComparerStruct<TValue>.System.Equals(Value, other.Value) && 
        AdvancedComparerStruct<TVersion>.System.Equals(Version, other.Version);
    }

    /// <inheritdoc/>
    public int CompareTo(HasVersion<TValue, TVersion> other)
    {
      int result = AdvancedComparerStruct<TValue>.System.Compare(Value, other.Value);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<TVersion>.System.Compare(Version, other.Version);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is HasVersion<TValue, TVersion>) {
        var other = (HasVersion<TValue, TVersion>)obj;
        return Equals(other);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int firstHash = Value == null ? 0 : Value.GetHashCode();
      int secondHash = Version == null ? 0 : Version.GetHashCode();
      return firstHash ^ 29 * secondHash;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.HasVersionFormat, Value, Version);
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