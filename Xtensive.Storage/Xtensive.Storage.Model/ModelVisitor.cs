// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.02

using System;

namespace Xtensive.Storage.Model
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
    protected virtual TResult Visit(Node node)
    {
      var domainModel = node as DomainModel;
      if (domainModel != null)
        return VisitDomainModel(domainModel);
      var generator = node as GeneratorInfo;
      if (generator != null)
        return VisitGeneratorInfo(generator);
      var key = node as KeyInfo;
      if (key != null)
        return VisitKeyInfo(key);
      var keyField = node as KeyField;
      if (keyField != null)
        return VisitKeyField(keyField);
      var association = node as AssociationInfo;
      if (association != null)
        return VisitAssociationInfo(association);
      var field = node as FieldInfo;
      if (field != null)
        return VisitFieldInfo(field);
      var type = node as TypeInfo;
      if (type != null)
        return VisitTypeInfo(type);
      var column = node as ColumnInfo;
      if (column != null)
        return VisitColumnInfo(column);
      var hierarchy = node as HierarchyInfo;
      if (hierarchy != null)
        return VisitHierarchyInfo(hierarchy);
      var index = node as IndexInfo;
      if (index != null)
        return VisitIndexInfo(index);

      throw new ArgumentException(Resources.Strings.ExNodeTypeIsUnknown, "node");
    }

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
    /// Visits a key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitKeyInfo(KeyInfo key);

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
    /// Visits a generator.
    /// </summary>
    /// <param name="generator">The generator.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitGeneratorInfo(GeneratorInfo generator);

    /// <summary>
    /// Visits domain model.
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <returns>Visit result.</returns>
    protected abstract TResult VisitDomainModel(DomainModel domainModel);
    
  }
}