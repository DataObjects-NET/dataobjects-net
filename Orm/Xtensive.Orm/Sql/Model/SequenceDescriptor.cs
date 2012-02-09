// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a set of information concerning sequence.
  /// </summary>
  [Serializable]
  public class SequenceDescriptor : Node, ICloneable
  {
    private ISequenceable owner;
    private long? startValue;
    private long? increment;
    private long? maxValue;
    private long? minValue;
    private bool? isCyclic;
    private long? lastValue;

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public ISequenceable Owner
    {
      get { return owner; }
      set {
        this.EnsureNotLocked();
        ISequenceable old = owner;
        owner = value;
        if (old!=null && old.SequenceDescriptor==this)
          old.SequenceDescriptor = null;
        if (owner!=null && owner.SequenceDescriptor!=this)
          owner.SequenceDescriptor = this;
      }
    }

    /// <summary>
    /// Gets or sets the start value.
    /// </summary>
    /// <remarks>
    /// The start value must lie between the minimum and maximum value.
    /// </remarks>
    public long? StartValue
    {
      get { return startValue; }
      set
      {
        this.EnsureNotLocked();
        startValue = value;
      }
    }

    /// <summary>
    /// Gets or sets the increment.
    /// </summary>
    /// <remarks>
    /// If increment is not specified, then an increment of 1 
    /// is implicit. Increment must not be 0.
    /// </remarks>
    public long? Increment
    {
      get { return increment; }
      set {
        this.EnsureNotLocked();
        if (value.HasValue && value.Value == 0)
          throw new ArgumentException(Strings.ExIncrementMustNotBeZero);
        increment = value;
      }
    }

    /// <summary>
    /// Gets or sets the max value.
    /// </summary>
    /// <remarks>
    /// The maximum value must be greater than the minimum value.
    /// </remarks>
    public long? MaxValue
    {
      get { return maxValue; }
      set {
        this.EnsureNotLocked();
        maxValue = value;
      }
    }

    /// <summary>
    /// Gets or sets the min value.
    /// </summary>
    /// <remarks>
    /// The maximum value must be greater than the minimum value.
    /// </remarks>
    public long? MinValue
    {
      get { return minValue; }
      set
      {
        this.EnsureNotLocked();
        minValue = value;
      }
    }

    /// <summary>
    /// Gets or sets the last value.
    /// </summary>
    public long? LastValue
    {
      get { return lastValue; }
      set
      {
        this.EnsureNotLocked();
        lastValue = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is isCyclic.
    /// </summary>
    /// <value><see langword="true"/> if isCyclic; otherwise, <see langword="false"/>.</value>
    public bool? IsCyclic
    {
      get { return isCyclic; }
      set
      {
        this.EnsureNotLocked();
        isCyclic = value;
      }
    }

   
    #region IClonable Members

    ///<summary>
    ///Creates a new object that is a copy of the current instance.
    ///</summary>
    ///<returns>
    ///A new object that is a copy of this instance.
    ///</returns>
    public object Clone()
    {
      return new SequenceDescriptor(owner, startValue, increment, maxValue, minValue, isCyclic);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    public SequenceDescriptor(ISequenceable owner)
    {
      this.owner = owner;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="startValue">The start value.</param>
    public SequenceDescriptor(ISequenceable owner, long? startValue)
      : this(owner, startValue, null, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="startValue">The start value.</param>
    /// <param name="increment">The increment.</param>
    public SequenceDescriptor(ISequenceable owner, long? startValue, long? increment)
      : this(owner, startValue, increment, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="startValue">The start value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="maxValue">The max value.</param>
    /// <param name="minValue">The min value.</param>
    public SequenceDescriptor(ISequenceable owner, long? startValue, long? increment, long? maxValue, long? minValue)
      : this(owner, startValue, increment, maxValue, minValue, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="startValue">The start value.</param>
    /// <param name="increment">The increment.</param>
    /// <param name="maxValue">The max value.</param>
    /// <param name="minValue">The min value.</param>
    /// <param name="isCyclic">The is cyclic.</param>
    public SequenceDescriptor(ISequenceable owner, long? startValue, long? increment, long? maxValue, long? minValue, bool? isCyclic)
    {
      StartValue = startValue;
      Increment = increment;
      this.maxValue = maxValue;
      this.minValue = minValue;
      this.isCyclic = isCyclic;
    }

    #endregion
  }
}