// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Comparison
{
  /// <summary>
  /// A struct providing faster access for key <see cref="AdvancedComparer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IAdvancedComparer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct AdvancedComparerStruct<T>: ISerializable
  {
    /// <summary>
    /// Gets <see cref="AdvancedComparerStruct{T}"/> for <see cref="AdvancedComparer{T}.Default"/> comparer.
    /// </summary>
    public static readonly AdvancedComparerStruct<T> Default = new AdvancedComparerStruct<T>(AdvancedComparer<T>.Default);

    /// <summary>
    /// Gets <see cref="AdvancedComparerStruct{T}"/> for <see cref="AdvancedComparer{T}.System"/> comparer.
    /// </summary>
    public readonly static AdvancedComparerStruct<T> System = new AdvancedComparerStruct<T>(AdvancedComparer<T>.System);

    /// <summary>
    /// Gets the underlying comparer for this cache.
    /// </summary>
    public readonly AdvancedComparer<T> Comparer;

    /// <summary>
    /// Gets <see cref="IComparer{T}.Compare"/> method delegate.
    /// </summary>
    public readonly Func<T, T, int> Compare;

    /// <summary>
    /// Gets <see cref="IEqualityComparer{T}.Equals(T,T)"/> method delegate.
    /// </summary>
    public readonly new Predicate<T, T> Equals;

    /// <summary>
    /// Gets <see cref="IEqualityComparer{T}.GetHashCode(T)"/> method delegate.
    /// </summary>
    public readonly new Func<T, int> GetHashCode;

    /// <summary>
    /// Gets <see cref="INearestValueProvider{T}.GetNearestValue"/> method delegate.
    /// </summary>
    public readonly Func<T, Direction, T> GetNearestValue;

    /// <summary>
    /// Gets <see cref="IHasRangeInfo{T}.ValueRangeInfo"/> value used by the underlying comparer.
    /// </summary>
    public readonly ValueRangeInfo<T> ValueRangeInfo;

    /// <summary>
    /// Wraps this instance with the <see cref="CastingComparer{T,TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type to provide the comparer for (by wrapping this comparer).</typeparam>
    public AdvancedComparerStruct<TTarget> Cast<TTarget>() 
    {
      if (typeof(TTarget)==typeof(T))
        return (AdvancedComparerStruct<TTarget>)(object)this;
      else
        return new AdvancedComparerStruct<TTarget>(Comparer.Cast<TTarget>());
    }

    /// <summary>
    /// Implicit conversion of <see cref="AdvancedComparer{T}"/> to <see cref="AdvancedComparerStruct{T}"/>.
    /// </summary>
    /// <param name="comparer">Comparer to provide the struct for.</param>
    public static implicit operator AdvancedComparerStruct<T>(AdvancedComparer<T> comparer)
    {
      return new AdvancedComparerStruct<T>(comparer);
    }

    
    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="comparer">Comparer to provide the delegates for.</param>
    private AdvancedComparerStruct(AdvancedComparer<T> comparer)
    {
      Comparer = comparer;
      Compare = Comparer==null ? null : Comparer.Compare;
      Equals = Comparer==null ? null : Comparer.Equals;
      GetHashCode = Comparer==null ? null : Comparer.GetHashCode;
      GetNearestValue = Comparer==null ? null : Comparer.GetNearestValue;
      ValueRangeInfo = Comparer==null ? null : Comparer.ValueRangeInfo;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private AdvancedComparerStruct(SerializationInfo info, StreamingContext context)
    {
      Comparer = (AdvancedComparer<T>)info.GetValue("Comparer", typeof(AdvancedComparer<T>));
      Compare = Comparer==null ? null : Comparer.Compare;
      Equals = Comparer==null ? null : Comparer.Equals;
      GetHashCode = Comparer==null ? null : Comparer.GetHashCode;
      GetNearestValue = Comparer==null ? null : Comparer.GetNearestValue;
      ValueRangeInfo = Comparer==null ? null : Comparer.ValueRangeInfo;
    }

    /// <inheritdoc/>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Comparer", Comparer);
    }
  }
}