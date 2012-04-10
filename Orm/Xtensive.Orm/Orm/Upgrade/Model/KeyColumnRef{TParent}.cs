// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Helpers;

using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public abstract class KeyColumnRef<TParent> : ColumnInfoRef<TParent>
    where TParent: StorageIndexInfo
  {
    private Direction direction;

    /// <summary>
    /// Gets or sets the column direction.
    /// </summary>
    [Property(Priority = -1000)]
    public Direction Direction {
      get { return direction; }
      set {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Direction", value)) {
          direction = value;
          scope.Commit();
        }
      }
    }

    
    /// <exception cref="ValidationException">Invalid <see cref="Direction"/> value 
    /// (<see cref="Core.Direction.None"/>).</exception>
    protected override void ValidateState()
    {
      using (var ea = new ExceptionAggregator()) {
        ea.Execute(base.ValidateState);
        if (direction==Direction.None) {
          ea.Execute(() => {
            throw new ValidationException(Strings.ExInvalidDirectionValue, Path);
          });
        }
        ea.Complete();
      }
    }


    // Constructors

    
    public KeyColumnRef(TParent parent)
      : base(parent)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public KeyColumnRef(TParent parent, StorageColumnInfo column)
      : base(parent, column)
    {
      Direction = Direction.Positive;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="direction">The direction.</param>
    public KeyColumnRef(TParent parent, StorageColumnInfo column, Direction direction)
      : base(parent, column)
    {
      Direction = direction;
    }
  }
}