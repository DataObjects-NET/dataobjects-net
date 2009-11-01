// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.15

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Integrity.Relations
{
  /// <summary>
  /// Relation synchronization context.
  /// </summary>
  /// <typeparam name="TContext">Actual scope type.</typeparam>
  public class RelationSyncScope<TContext>: Scope<TContext>
    where TContext: class
  {
    /// <summary>
    /// Gets current <see cref="TContext"/> object in this type of scope.
    /// </summary>
    public new static TContext CurrentContext {
      get {
        var scope = CurrentScope as RelationSyncScope<TContext>;
        return scope!=null ? scope.Context : null;
      }
    }

    /// <summary>
    /// Gets <see cref="TContext"/> object associated with this scope.
    /// </summary>
    public new TContext Context
    {
      get { return base.Context; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="context">The context.</param>
    public RelationSyncScope(TContext context)
      : base(context)
    {
    }
  }
}