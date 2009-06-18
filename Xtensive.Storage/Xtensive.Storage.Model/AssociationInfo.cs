// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public class AssociationInfo : Node
  {
    private Multiplicity multiplicity;
    private AssociationInfo reversed;
    private TypeInfo underlyingType;
    private bool isMaster = true;
    private SegmentTransform foreignKeyExtractorTransform;

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
    /// Gets the persistent type that represents this association.
    /// </summary>
    public TypeInfo UnderlyingType
    {
      get { return underlyingType; }
      set
      {
        this.EnsureNotLocked();
        underlyingType = value;
      }
    }

    /// <summary>
    /// Gets the underlying index for this instance.
    /// </summary>
    /// <value>The underlying index.</value>
    public IndexInfo UnderlyingIndex { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is master association.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is master association; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsMaster
    {
      get { return isMaster; }
      set
      {
        this.EnsureNotLocked();
        isMaster = value;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is paired.
    /// </summary>
    public bool IsPaired
    {
      get { return reversed!=null; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance represents loop.
    /// </summary>
    public bool IsLoop
    {
      get { return IsPaired && Reversed == this; }
    }

    /// <summary>
    /// Gets master association.
    /// </summary>
    /// <remarks>
    /// If association is master, returns it. Otherwise returns paired association.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Unable to find master association.</exception>
    public AssociationInfo Master
    {
      get
      {
        if (isMaster) 
          return this;
        if (reversed==null || !reversed.isMaster) 
          throw new InvalidOperationException(String.Format(Strings.ExUnableToFindMasterAssociation, Name));
        return reversed;
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
    /// Gets or sets the reversed paired <see cref="AssociationInfo"/> for this instance.
    /// </summary>
    public AssociationInfo Reversed
    {
      get { return reversed; }
      set
      {
        this.EnsureNotLocked();
        reversed = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="OnRemoveAction"/> that will be applied on <see cref="ReferencedType"/> object removal.
    /// </summary>
    public OnRemoveAction OnRemove { get; private set; }

    /// <summary>
    /// Extracts the foreign key from the specified <see cref="Tuple"/>.
    /// </summary>
    /// <param name="tuple">The tuple.</param>
    /// <returns><see cref="Tuple"/> instance with the extracted foreign key.</returns>
    public Tuple ExtractForeignKey(Tuple tuple)
    {
      return foreignKeyExtractorTransform.Apply(TupleTransformType.TransformedTuple, tuple);
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!ReferencingType.IsEntity)
        return;
      switch (Multiplicity) {
      case Multiplicity.ZeroToOne:
      case Multiplicity.OneToOne:
      case Multiplicity.ManyToOne:
        UnderlyingIndex = ReferencingType.Indexes.PrimaryIndex;
        foreignKeyExtractorTransform = ReferencingField.valueExtractorTransform;
        break;
      case Multiplicity.OneToMany:
        UnderlyingIndex = Reversed.ReferencingType.Indexes.GetIndex(Reversed.ReferencingField.Name);
        break;
      case Multiplicity.ZeroToMany:
      case Multiplicity.ManyToMany:
        if (IsMaster)
          UnderlyingIndex = underlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).Skip(1).First();
        else
          UnderlyingIndex = Reversed.UnderlyingType.Indexes.Where(indexInfo => indexInfo.IsSecondary).First();
        if (foreignKeyExtractorTransform == null) {
          var foreignKeySegment = new Segment<int>(ReferencingType.Hierarchy.KeyInfo.Columns.Count, ReferencedType.Hierarchy.KeyInfo.Columns.Count);
          foreignKeyExtractorTransform = new SegmentTransform(true, UnderlyingIndex.TupleDescriptor, foreignKeySegment);
        }
        break;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="referencingField">The referencing field.</param>
    /// <param name="referencedType">The referenced type.</param>
    /// <param name="multiplicity">The association multiplicity.</param>
    /// <param name="onRemove">The <see cref="OnRemoveAction"/> that will be applied on <see cref="ReferencedType"/> object removal.</param>
    public AssociationInfo(FieldInfo referencingField, TypeInfo referencedType, Multiplicity multiplicity, OnRemoveAction onRemove)
    {
      ReferencingField = referencingField;
      ReferencedType = referencedType;
      Multiplicity = multiplicity;
      OnRemove = onRemove;
      referencingField.Association = this;
    }
  }
}