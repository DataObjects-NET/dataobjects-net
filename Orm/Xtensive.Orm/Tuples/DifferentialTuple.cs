// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;



namespace Xtensive.Tuples
{
  /// <summary>
  /// Differential tuple. Combines read-only <see cref="Origin"/> tuple
  /// with <see cref="Difference"/> tuple providing all the 
  /// changes made to <see cref="Origin"/>.
  /// </summary>
  [Serializable]
  public sealed class DifferentialTuple : Tuple
  {
    private Tuple origin;
    private Tuple difference;
    private DifferentialTuple backupedDifference;

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
      BackupDifference();
      origin = origin.ToRegular();
      origin.MergeWith(difference, 0, origin.Descriptor.Count, MergeBehavior.PreferDifference);
      difference = null;
    }

    internal void RestoreDifference()
    {
      if (backupedDifference != null) {
        origin.MergeWith(backupedDifference.Origin, MergeBehavior.PreferDifference);
        if (difference==null)
          difference = backupedDifference.Difference.Clone();
        else
          difference.MergeWith(backupedDifference, 0, MergeBehavior.PreferDifference);
        backupedDifference = null;
      }
    }

    internal void BackupDifference()
    {
      if (difference != null)
        backupedDifference = (DifferentialTuple) this.Clone();
    }

    internal void DropBackedUpDifference()
    {
      backupedDifference = null;
    }

    /// <inheritdoc/>
    protected internal override Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      Tuple tuple;
      if (isWriting) {
        if (difference == null)
          difference = origin.CreateNew();
        tuple = difference;
      }
      else
        tuple = difference != null && difference.GetFieldState(fieldIndex).IsAvailable() 
          ? difference 
          : origin;
      return tuple.GetMappedContainer(fieldIndex, isWriting);
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      var tuple = difference != null && difference.GetFieldState(fieldIndex).IsAvailable()
        ? difference
        : origin;
      return tuple.GetFieldState(fieldIndex);
    }

    protected internal override void SetFieldState(int fieldIndex, TupleFieldState fieldState)
    {
      var tuple = difference != null && difference.GetFieldState(fieldIndex).IsAvailable()
        ? difference
        : origin;
      tuple.SetFieldState(fieldIndex, fieldState);
    }

    /// <inheritdoc/>
    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      var tuple = difference != null && difference.GetFieldState(fieldIndex).IsAvailable()
        ? difference
        : origin;
      return tuple.GetValue(fieldIndex, out fieldState);
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (difference == null)
        difference = origin.CreateNew();
      difference.SetValue(fieldIndex, fieldValue);
    }

    #endregion

    #region CreateNew, Clone, Reset methods

    /// <inheritdoc/>
    public override Tuple CreateNew()
    {
      return new DifferentialTuple(origin.CreateNew());
    }

    /// <inheritdoc/>
    public override Tuple Clone()
    {
      return new DifferentialTuple(
        origin.Clone(), 
        difference==null ? null : difference.Clone(), 
        backupedDifference==null ? null : (DifferentialTuple) backupedDifference.Clone());
    }

    /// <summary>
    /// Resets all the changes in <see cref="Difference"/> by re-creating it.
    /// </summary>
    public void Reset()
    {
      difference = null;
    }

    #endregion

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="origin">Initial <see cref="Origin"/> property value.</param>
    public DifferentialTuple(Tuple origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");
      this.origin = origin;
      difference = null;
      backupedDifference = null;
    }

    /// <summary>
    /// Initializes a new instance of this type.
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
      backupedDifference = null;
    }

    private DifferentialTuple(Tuple origin, Tuple difference, DifferentialTuple backupedDifference)
      : this(origin, difference)
    {
      this.backupedDifference = backupedDifference;
    }
  }
}