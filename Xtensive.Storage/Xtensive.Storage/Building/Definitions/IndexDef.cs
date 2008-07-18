// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.09.29

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Definitions
{
  [DebuggerDisplay("{Name}; Attributes = {Attributes}.")]
  [Serializable]
  public class IndexDef : MappingNode
  {
    private IndexAttributes attributes;
    private double fillFactor;
    private readonly DirectionCollection<string> keyFields = new DirectionCollection<string>();
    private Collection<string> includedFields = new Collection<string>();

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
    /// Gets the attributes.
    /// </summary>
    public IndexAttributes Attributes
    {
      get { return attributes; }
      protected set { attributes = value; }
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
    /// Performs additional custom processes before setting new name to this instance.
    /// </summary>
    /// <param name="nameToValidate">The new name of this instance.</param>
    protected override void Validate(string nameToValidate)
    {
      base.Validate(nameToValidate);

      Validator.IsNameValid(nameToValidate, ValidationRule.Index);
    }


    // Constructors

    internal IndexDef()
    {
    }
  }
}
