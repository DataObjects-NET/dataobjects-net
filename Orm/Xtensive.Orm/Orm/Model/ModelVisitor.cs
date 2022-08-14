// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// An abstract base class for all <see cref="DomainModel"/> visitors.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class ModelVisitor<TResult>
  {
    /// <summary>
    /// Visits the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>Visit result.</returns>
    /// <exception cref="ArgumentException">Node type is unknown.</exception>
    protected virtual TResult Visit(Node node) =>
      node switch {
        DomainModel domainModel => VisitDomainModel(domainModel),
        KeyInfo keyProviderInfo => VisitKeyInfo(keyProviderInfo),
        SequenceInfo sequenceInfo => VisitSequenceInfo(sequenceInfo),
        KeyField keyField => VisitKeyField(keyField),
        AssociationInfo association => VisitAssociationInfo(association),
        FieldInfo field => VisitFieldInfo(field),
        TypeInfo type => VisitTypeInfo(type),
        ColumnInfo column => VisitColumnInfo(column),
        HierarchyInfo hierarchy => VisitHierarchyInfo(hierarchy),
        IndexInfo index => VisitIndexInfo(index),
        FullTextIndexInfo fullTextIndex => VisitFullTextIndexInfo(fullTextIndex),
        _ => throw new ArgumentException(Strings.ExNodeTypeIsUnknown, "node")
      };

    /// <summary>
    /// Visits key field.
    /// </summary>
    /// <param name="keyField">The key field.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitKeyField(KeyField keyField);

    /// <summary>
    /// Visits a column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitColumnInfo(ColumnInfo column);

    /// <summary>
    /// Visits a field.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitFieldInfo(FieldInfo field);

    /// <summary>
    /// Visits a <see cref="KeyInfo"/> node.
    /// </summary>
    /// <param name="keyInfo">The key provider.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitKeyInfo(KeyInfo keyInfo);

    /// <summary>
    /// Visits a <see cref="SequenceInfo"/> node.
    /// </summary>
    /// <param name="sequenceInfo">The sequence info.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitSequenceInfo(SequenceInfo sequenceInfo);

    /// <summary>
    /// Visits an index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitIndexInfo(IndexInfo index);

    /// <summary>
    /// Visits a hierarchy.
    /// </summary>
    /// <param name="hierarchy">The hierarchy.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitHierarchyInfo(HierarchyInfo hierarchy);

    /// <summary>
    /// Visits a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitTypeInfo(TypeInfo type);

    /// <summary>
    /// Visits an association.
    /// </summary>
    /// <param name="association">The association.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitAssociationInfo(AssociationInfo association);

    /// <summary>
    /// Visits domain model.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDomainModel(DomainModel domainModel);

    /// <summary>
    /// Visits the full text index info.
    /// </summary>
    /// <param name="fullTextIndex">Full index of the text.</param>
    protected abstract TResult VisitFullTextIndexInfo(FullTextIndexInfo fullTextIndex);
  }
}