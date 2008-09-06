// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class AssociationInfo : Node
  {
    private Multiplicity multiplicity;
    private AssociationInfo pairTo;
    private Type entityType;

    /// <summary>
    /// Gets the referencing type.
    /// </summary>
    public TypeInfo ReferencingType
    {
      get { return ReferencingField.DeclaringType; }
    }

    /// <summary>
    /// Gets the referencing field.
    /// </summary>
    public FieldInfo ReferencingField { get; private set; }

    /// <summary>
    /// Gets the referenced type.
    /// </summary>
    public TypeInfo ReferencedType { get; private set; }

    /// <summary>
    /// Gets the type of association entity.
    /// </summary>
    public Type EntityType
    {
      get { return entityType; }
      set
      {
        this.EnsureNotLocked();
        entityType = value;
      }
    }

    /// <summary>
    /// Gets the association multiplicity.
    /// </summary>
    public Multiplicity Multiplicity
    {
      get { return multiplicity; }
      set
      {
        this.EnsureNotLocked();
        multiplicity = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is master association.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is master association; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMaster
    {
      get { return pairTo==null; }
    }

    /// <summary>
    /// Gets the <see cref="ReferentialAction"/> that will be applied on <see cref="ReferencedType"/> object removal.
    /// </summary>
    public ReferentialAction OnDelete { get; private set; }

    /// <summary>
    /// Gets or sets the master <see cref="AssociationInfo"/> for this instance.
    /// </summary>
    public AssociationInfo PairTo
    {
      get { return pairTo; }
      set
      {
        this.EnsureNotLocked();
        pairTo = value;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="referencingField">The referencing field.</param>
    /// <param name="referencedType">The referenced type.</param>
    /// <param name="multiplicity">The association multiplicity.</param>
    /// <param name="onDelete">The <see cref="ReferentialAction"/> that will be applied on <see cref="ReferencedType"/> object removal.</param>
    public AssociationInfo(FieldInfo referencingField, TypeInfo referencedType, Multiplicity multiplicity, ReferentialAction onDelete)
    {
      ReferencingField = referencingField;
      ReferencedType = referencedType;
      Multiplicity = multiplicity;
      OnDelete = onDelete;
      referencingField.Association = this;
    }
  }
}