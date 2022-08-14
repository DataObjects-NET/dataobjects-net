// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents an object that generates unique numbers, mostly used for primary key values.
  /// </summary>
  [Serializable]
  public class Sequence : SchemaNode, ISequenceable
  {
    private SequenceDescriptor sequenceDescriptor;

    /// <summary>
    /// Gets or sets the sequence descriptor.
    /// </summary>
    /// <value>The sequence descriptor.</value>
    public SequenceDescriptor SequenceDescriptor
    {
      get { return sequenceDescriptor; }
      set {
        EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        sequenceDescriptor = value;
        SequenceDescriptor old = sequenceDescriptor;
        sequenceDescriptor = value;
        if (old!=null && old.Owner==this)
          old.Owner = null;
        if (sequenceDescriptor!=null && sequenceDescriptor.Owner!=this)
          sequenceDescriptor.Owner = this;
      }
    }

    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Sequences.Remove(this);
      if (value!=null)
        value.Sequences.Add(this);
    }

    #endregion

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (sequenceDescriptor!=null)
        sequenceDescriptor.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal Sequence(Schema schema, string name) : base(schema, name)
    {
      sequenceDescriptor = new SequenceDescriptor(this);
    }

    #endregion
  }
}
