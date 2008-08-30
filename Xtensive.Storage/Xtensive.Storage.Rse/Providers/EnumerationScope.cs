// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.21

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// <see cref="EnumerationContext"/> activation scope.
  /// </summary>
  public class EnumerationScope : Scope<EnumerationContext>
  {
    /// <summary>
    /// Gets the current context.
    /// </summary>
    public new static EnumerationContext CurrentContext
    {
      get { return Scope<EnumerationContext>.CurrentContext; }
    }

    /// <summary>
    /// Gets the context of this scope.
    /// </summary>
    public new EnumerationContext Context
    {
      get { return base.Context; }
    }

    /// <summary>
    /// Create the new <see cref="EnumerationScope"/> using 
    /// <see cref="CompilationContext.CreateEnumerationContext"/> method of the
    /// <see cref="CompilationContext.Current"/> compilation context, 
    /// if <see cref="CurrentContext"/> is <see langword="null" />.
    /// Otherwise, returns <see langword="null" />.
    /// </summary>
    /// <returns>Either new <see cref="EnumerationScope"/> or <see langword="null" />.</returns>
    public static EnumerationScope Open()
    {
      if (CurrentContext==null)
        return CompilationContext.Current.CreateEnumerationContext().Activate();
      else
        return null;
    }

    /// <summary>
    /// Create the new <see cref="EnumerationScope"/> having
    /// <see cref="EnumerationContext"/> property set to <see langword="null" />, 
    /// if <see cref="CurrentContext"/> is not <see langword="null" />.
    /// Otherwise, returns <see langword="null" />.
    /// In fact, temporarily blocks current <see cref="EnumerationContext"/>
    /// and ensures next call to <see cref="Open"/> will return 
    /// a new <see cref="EnumerationScope"/>.
    /// </summary>
    /// <returns>Either new <see cref="EnumerationScope"/> or <see langword="null" />.</returns>
    public static EnumerationScope Block()
    {
      if (CurrentContext==null)
        return null;
      else
        return new EnumerationScope(null);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="context">The context.</param>
    public EnumerationScope(EnumerationContext context)
      : base(context)
    {}
  }
}