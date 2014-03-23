// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2007.09.29

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines a single index.
  /// </summary>
  [DebuggerDisplay("{Name}; Attributes = {Attributes}.")]
  [Serializable]
  public sealed class IndexDef : MappedNode
  {
    /// <summary>
    /// Default fill factor.
    /// </summary>
    public const double DefaultFillFactor = 0.8;

    private IndexAttributes attributes;
    private LambdaExpression filterExpression;
    private double fillFactor = DefaultFillFactor;
    private readonly DirectionCollection<string> keyFields = new DirectionCollection<string>();
    private Collection<string> includedFields = new Collection<string>();
    private readonly Validator validator;

    /// <summary>
    /// Gets <see cref="Definitions.TypeDef"/> that this index is bound to.
    /// </summary>
    public TypeDef Type { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is primary index.
    /// </summary>
    public bool IsPrimary
    {
      get { return (attributes & IndexAttributes.Primary) > 0; }
      internal set
      {
        attributes = value
                       ? attributes | IndexAttributes.Primary | IndexAttributes.Unique
                       : attributes & ~IndexAttributes.Primary;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is primary index.
    /// </summary>
    public bool IsUnique
    {
      get { return (attributes & IndexAttributes.Unique) > 0; }
      set
      {
        attributes = value
                       ? attributes | IndexAttributes.Unique
                       : attributes & ~IndexAttributes.Primary & ~IndexAttributes.Unique;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is secondary index.
    /// </summary>
    public bool IsSecondary
    {
      get { return (attributes & IndexAttributes.Secondary) > 0; }
      set
      {
        attributes = value
                       ? attributes | IndexAttributes.Secondary
                       : attributes & ~IndexAttributes.Secondary;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is clustered index.
    /// </summary>
    public bool IsClustered
    {
      get { return (attributes & IndexAttributes.Clustered) > 0; }
      set
      {
        attributes = value
                       ? attributes | IndexAttributes.Clustered
                       : attributes & ~IndexAttributes.Clustered;
      }
    }

    /// <summary>
    /// Gets the attributes.
    /// </summary>
    public IndexAttributes Attributes
    {
      get { return attributes; }
    }

    /// <summary>
    /// Gets or sets the fill factor for index, must be a real number between <see langword="0"/> and <see langword="1"/>.
    /// </summary>
    public double FillFactor
    {
      get { return fillFactor; }
      set
      {
        if (fillFactor < 0 || fillFactor > 1)
          throw new DomainBuilderException(
            string.Format(Strings.ExInvalidFillFactorXValueMustBeBetween0And1, fillFactor));

        fillFactor = value;
      }
    }

    /// <summary>
    /// Gets the key fields that are included in this instance.
    /// </summary>
    public DirectionCollection<string> KeyFields
    {
      get { return keyFields; }
    }

    /// <summary>
    /// Gets the non key fields that are included in this instance.
    /// </summary>
    public Collection<string> IncludedFields
    {
      get { return includedFields; }
    }

    /// <summary>
    /// Gets or sets expression that defines range for partial index.
    /// </summary>
    public LambdaExpression FilterExpression
    {
      get { return filterExpression; }
      set {
        if (!IsSecondary)
          throw new DomainBuilderException(Strings.ExOnlySecondaryIndexesCanBeDeclaredPartial);
        filterExpression = value;
        if (filterExpression != null)
          attributes |= IndexAttributes.Partial;
        else
          attributes &= ~IndexAttributes.Partial;
      }
    }

    /// <summary>
    /// Performs additional custom processes before setting new name to this instance.
    /// </summary>
    /// <param name="newName">The new name of this instance.</param>
    protected override void ValidateName(string newName)
    {
      base.ValidateName(newName);
      validator.ValidateName(newName, ValidationRule.Index);
    }


    // Constructors

    internal IndexDef(TypeDef type, Validator validator)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      Type = type;
      this.validator = validator;
    }
  }
}
