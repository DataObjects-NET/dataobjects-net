// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using System.Collections;
using System.Diagnostics;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Runtime.Serialization;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contains information about entity version.
  /// </summary>
  [DebuggerDisplay("{Value}")]
  [Serializable]
  public struct VersionInfo : IEquatable<VersionInfo>
  {
    private static VersionInfo @void = default;

    [NonSerialized]
    private int cachedHashCode;
    private Tuple value;

    /// <summary>
    /// Gets the void <see cref="VersionInfo"/> object.
    /// </summary>
    public static VersionInfo Void {
      get { return @void; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is not contains version.
    /// </summary>
    public bool IsVoid {
      [DebuggerStepThrough]
      get { return Value==null; }
    }

    internal Tuple Value {
      [DebuggerStepThrough]
      get {
        return value;
      }
    }

    /// <summary>
    /// Combines this version with the specified key value tuple and specified version tuple.
    /// </summary>
    /// <param name="key">The key to combine.</param>
    /// <param name="versionInfo">The version info to combine.</param>
    /// <returns>Combined version info.</returns>
    internal VersionInfo Combine(Key key, VersionInfo versionInfo)
    {
      ArgumentNullException.ThrowIfNull(key);

      Tuple resultVersion = Value;
      if (resultVersion==null)
        resultVersion = key.Value;
      else
        resultVersion = resultVersion.Combine(key.Value);
      if (!versionInfo.IsVoid)
        resultVersion = resultVersion.Combine(versionInfo.Value);

      return new VersionInfo(resultVersion.ToRegular());
    }

    /// <summary>
    /// Checks whether this <see cref="VersionInfo"/>
    /// conflicts with <paramref name="other"/> <see cref="VersionInfo"/>.
    /// There is no conflict, if all mutually available fields in
    /// <see cref="Value"/>s are equal, and count of fields is the same.
    /// </summary>
    /// <param name="other">The other <see cref="VersionInfo"/>.</param>
    /// <returns>Check result.</returns>
    public bool ConflictsWith(VersionInfo other)
    {
      if (Equals(other))
        return false;

      var tuple = Value;
      var otherTuple = other.Value;

      if (tuple==null) {
        if (otherTuple==null)
          return false;
        else
          return true;
      }
      else if (otherTuple==null)
        return true;
      
      if (tuple.Count!=otherTuple.Count)
        return true;

      int count = tuple.Count;
      var availableFlags      = tuple.GetFieldStateMap(TupleFieldState.Available);
      var otherAvailableFlags = otherTuple.GetFieldStateMap(TupleFieldState.Available);
      
      for (int i = 0; i<count; i++) {
        if (availableFlags[i] && otherAvailableFlags[i])
          if (!Equals(tuple.GetValue(i), otherTuple.GetValue(i)))
            return true;
      }
      return false;
    }

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(VersionInfo other)
    {
      return Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj) =>
      obj switch {
        null => false,
        VersionInfo otherVersionInfo => GetHashCode()==obj.GetHashCode() && Equals(otherVersionInfo),
        _ => false
      };

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(VersionInfo left, VersionInfo right)
    {
      return !left.ConflictsWith(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(VersionInfo left, VersionInfo right)
    {
      return left.ConflictsWith(right);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      if (cachedHashCode==0) {
        if (!IsVoid)
          cachedHashCode = Value.GetHashCode();
        if (cachedHashCode==0)
          cachedHashCode = -1;
      }
      return cachedHashCode;
    }

    #endregion

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override string ToString()
    {
      return Value==null ? string.Empty : Value.ToString();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="version">The version tuple.</param>
    public VersionInfo(Tuple version)
    {
      value = version;
      cachedHashCode = 0;
    }
  }
}