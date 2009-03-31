// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using Xtensive.Indexing.Storage;
using Xtensive.Indexing.Storage.Model;
using Xtensive.Modelling.Actions;

namespace Xtensive.Indexing.Storage
{
  /// <summary>
  /// Model manager API (DDL API).
  /// Manages the indexes stored in the <see cref="IStorage"/>.
  /// </summary>
  public interface IModelManager
  {
    /// <summary>
    /// Gets the current storage model.
    /// </summary>
    StorageInfo Model { get; }

    /// <summary>
    /// Updates the <see cref="Model"/> by applying specified 
    /// action sequence to it.
    /// </summary>
    /// <param name="sequence">The sequence to apply.</param>
    void Update(ActionSequence sequence);
  }
}