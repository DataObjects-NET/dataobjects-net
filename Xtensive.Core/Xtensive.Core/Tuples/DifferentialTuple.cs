// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.22

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Differential tuple. Combines read-only <see cref="Origin"/> tuple
  /// with <see cref="Difference"/> tuple providing all the 
  /// changes made to <see cref="Origin"/>.
  /// </summary>
  [Serializable]
  public sealed class DifferentialTuple : Tuple
  {
//    private static ThreadSafeList<Delegate[]> getGetValueDelegates = ThreadSafeList<Delegate[]>.Create(new object());
    private Tuple origin;
    private Tuple difference;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor {
      [DebuggerStepThrough]
      get { return origin.Descriptor; }
    }

    /// <inheritdoc />
    public override int Count {
      [DebuggerStepThrough]
      get { return origin.Count; }
    }

    /// <summary>
    /// Gets original tuple.
    /// </summary>
    public Tuple Origin {
      [DebuggerStepThrough]
      get { return origin; }
      [DebuggerStepThrough]
      set { origin = value; }
    }

    /// <summary>
    /// Gets or sets difference tuple.
    /// Can be <see langword="null" /> (acts as if no values are available in this tuple).
    /// </summary>
    public Tuple Difference {
      [DebuggerStepThrough]
      get { return difference; }
      [DebuggerStepThrough]
      set {
        if (value!=null && value.Descriptor!=Descriptor)
          throw new ArgumentException(
            string.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, Descriptor),
            "value");
        difference = value;
      }
    }

    /// <summary>
    /// Indicates whether field with specified <paramref name="fieldIndex"/> is changed.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to check.</param>
    /// <returns><see langword="True"/> if specified field is changed;
    /// otherwise, <see langword="false"/>.</returns>
    public bool IsChanged(int fieldIndex)
    {
      if (difference==null)
        return false;
      return difference.GetFieldState(fieldIndex).IsAvailable();
    }

    /// <summary>
    /// Merges the <see cref="Origin"/> with the <see cref="Difference"/>.
    /// </summary>
    public void Merge()
    {
      if (difference==null)
        return;
      origin = origin.ToRegular();
      origin.MergeWith(difference, 0, origin.Descriptor.Count, MergeBehavior.PreferDifference);
      difference = null;
    }

    /// <inheritdoc/>
    protected override Tuple GetContainer(int fieldIndex)
    {
      if (difference==null)
        return origin;
      return difference.GetFieldState(fieldIndex).IsAvailable() 
        ? difference 
        : origin;
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return GetContainer(fieldIndex).GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      return GetContainer(fieldIndex).GetValue(fieldIndex, out fieldState);
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (difference==null)
        difference = origin.CreateNew();
      difference.SetValue(fieldIndex, fieldValue);
    }

    #endregion

    #region Get Delegate methods

    protected internal override Delegate GetGetValueDelegate(int fieldIndex)
    {
      var container = GetContainer(fieldIndex);
      return container.GetGetValueDelegate(fieldIndex);
    }

    protected internal override Delegate GetGetNullableValueDelegate(int fieldIndex)
    {
      var container = GetContainer(fieldIndex);
      return container.GetGetNullableValueDelegate(fieldIndex);
    }

    protected internal override Delegate GetSetValueDelegate(int fieldIndex)
    {
      if (difference == null)
        difference = origin.CreateNew();
      return difference.GetSetValueDelegate(fieldIndex);
    }

    protected internal override Delegate GetSetNullableValueDelegate(int fieldIndex)
    {
      if (difference == null)
        difference = origin.CreateNew();
      return difference.GetSetNullableValueDelegate(fieldIndex);
    }

    #endregion


    #region CreateNew, Clone, Reset methods

    /// <inheritdoc/>
    public override Tuple CreateNew()
    {
      return new DifferentialTuple(Create(origin.Descriptor));
    }

    /// <inheritdoc/>
    public override Tuple Clone()
    {
      return new DifferentialTuple(origin.Clone(), difference==null ? null : difference.Clone());
    }

    /// <summary>
    /// Resets all the changes in <see cref="Difference"/> by re-creating it.
    /// </summary>
    public void Reset()
    {
      difference = null;
    }

    #endregion

//    #region Private static methods
//
//    private static GetValueDelegate<TFieldType> GetGetValueDelegate<TFieldType>(int fieldIndex)
//    {
//      return (Tuple t, out TupleFieldState fs) => {
//        var dTuple = (DifferentialTuple) t;
//        var originDelegate = dTuple.origin.GetGetValueDelegate(fieldIndex) as GetValueDelegate<TFieldType>;
//        if (originDelegate == null)
//          return null;
//
//               fs = TupleFieldState.Available;
//               return default(TFieldType);
//             };
//    }
//
//    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="origin">Initial <see cref="Origin"/> property value.</param>
    public DifferentialTuple(Tuple origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");
      this.origin = origin;
      difference = null;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="origin">Initial <see cref="Origin"/> property value.</param>
    /// <param name="difference">Initial <see cref="Difference"/> property value.</param>
    /// <exception cref="ArgumentException">Tuple descriptors mismatch.</exception>
    public DifferentialTuple(Tuple origin, Tuple difference)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");
      if (difference!=null && origin.Descriptor!=difference.Descriptor)
        throw new ArgumentException(
          string.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, origin.Descriptor),
          "difference");
      this.origin = origin;
      this.difference = difference;
    }
  }
}