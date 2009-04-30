// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;
using Xtensive.Storage.Indexing.Model.Resources;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Sequence.
  /// </summary>
  [Serializable]
  public sealed class SequenceInfo : NodeBase<StorageInfo>
  {
    private TypeInfo type;
    private long startValue;
    private long increment;

    /// <summary>
    /// Gets or sets the start value.
    /// </summary>
    [Property(IgnoreInComparison = true)]
    public long StartValue
    {
      get { return startValue; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("StartValue", value)) {
          startValue = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the increment.
    /// </summary>
    [Property]
    public long Increment
    {
      get { return increment; }
      set
      {
        ArgumentValidator
          .EnsureArgumentIsInRange(value, 1, long.MaxValue, "Increment");
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Increment", value)) {
          increment = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    [Property]
    public TypeInfo Type
    {
      get { return type; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Type", value)) {
          type = value;
          scope.Commit();
        }
      }
    }

    /// <summary>
    /// Gets or sets the current sequence value.
    /// </summary>
    public long? Current { get; set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SequenceInfo, StorageInfo, SequenceInfoCollection>(this, "Sequences");
    }

    /// <inheritdoc/>
    /// <exception cref="ValidationException"><c>ValidationException</c>.</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (Increment<=0)
          ea.Execute(() => {
            throw new ValidationException(
              string.Format(Strings.ExInvalideIncrementValue),
              Path);
          });
        if (Type==null)
          ea.Execute(() => {
            throw new ValidationException(
              string.Format(string.Format(Strings.ExUndefinedTypeOfSequenceX, Name)),
              Path);
          });
      }
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="name">The name.</param>
    public SequenceInfo(StorageInfo parent, string name)
      : base(parent, name)
    {
    }
  }
}