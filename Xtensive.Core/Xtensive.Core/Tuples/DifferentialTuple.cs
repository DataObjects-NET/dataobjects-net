// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.22

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Differential tuple. Combines read-only <see cref="Origin"/> tuple
  /// with <see cref="Difference"/> tuple providing all the 
  /// changes made to <see cref="Origin"/>.
  /// </summary>
  public sealed class DifferentialTuple : Tuple
  {
    private readonly Tuple origin;
    private Tuple difference;

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return origin.Descriptor; }
    }

    /// <inheritdoc />
    public override int Count
    {
      get { return origin.Count; }
    }

    /// <summary>
    /// Gets original tuple.
    /// </summary>
    public Tuple Origin
    {
      get { return origin; }
    }

    /// <summary>
    /// Gets or sets difference tuple.
    /// </summary>
    public Tuple Difference
    {
      get { return difference; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(origin, "value");
        if (value.Descriptor!=Descriptor)
          throw new ArgumentException(
            String.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, Descriptor),
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
      return difference.IsAvailable(fieldIndex);
    }

    /// <summary>
    /// Gets the tuple (<see cref="Origin"/> or <see cref="Difference"/>) containing
    /// actual value of the specified field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get the value container for.</param>
    /// <returns>Value container.</returns>
    public Tuple GetContainer(int fieldIndex)
    {
      return difference.IsAvailable(fieldIndex) ? difference : origin;
    }

    #region GetFieldState, GetValueOrDefault, SetValue methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return GetContainer(fieldIndex).GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      return GetContainer(fieldIndex).GetValueOrDefault<T>(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return GetContainer(fieldIndex).GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc />
    public override void SetValue<T>(int fieldIndex, T fieldValue)
    {
      difference.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc />
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      difference.SetValue(fieldIndex, fieldValue);
    }

    #endregion

    #region CreateNew, Clone, Reset methods

    /// <inheritdoc/>
    public override Tuple CreateNew()
    {
      return new DifferentialTuple(Create(Descriptor));
    }

    /// <inheritdoc/>
    public override Tuple Clone()
    {
      return new DifferentialTuple(origin, Create(Descriptor));
    }

    /// <summary>
    /// Resets all the changes in <see cref="Difference"/> by re-creating it.
    /// </summary>
    public void Reset()
    {
      difference = Create(Descriptor);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="origin">Initial <see cref="Origin"/> property value.</param>
    public DifferentialTuple(Tuple origin)
    {
      ArgumentValidator.EnsureArgumentNotNull(origin, "origin");
      this.origin = origin;
      difference = Origin.CreateNew();
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
      if (origin.Descriptor!=difference.Descriptor)
        throw new ArgumentException(
          String.Format(Strings.ExInvalidTupleDescriptorExpectedDescriptorIs, origin.Descriptor),
          "difference");
      this.origin = origin;
      this.difference = difference;
    }
  }
}