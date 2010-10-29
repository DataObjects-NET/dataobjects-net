// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// Base class for <see cref="Index{TKey,TItem}"/> page.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Node type.</typeparam>
  [Serializable]
  public abstract class Page<TKey, TItem> :
    IIdentified<IPageRef>,
    IHasVersion<int>,
    IPageRef,
    IDeserializationCallback
  {
    [NonSerialized] private IIndexPageProvider<TKey, TItem> provider;
    [NonSerialized] private IPageRef identifier;
    [NonSerialized] private bool isPersisted;
    private int version;
    private readonly DescriptorPage<TKey, TItem> descriptorPage;

    /// <summary>
    /// Gets <see cref="IIndexPageProvider{TKey,TItem}"/>, associated with 
    /// <see cref="Page{TKey,TValue}"/>
    /// </summary>
    public IIndexPageProvider<TKey, TItem> Provider
    {
      [DebuggerStepThrough]
      get { return provider; }
      [DebuggerStepThrough]
      internal set { provider = value; }
    }

    /// <summary>
    /// Gets or sets page <see cref="IPageRef">identifier</see> 
    /// </summary>
    public IPageRef Identifier
    {
      [DebuggerStepThrough]
      get { return identifier; }
      [DebuggerStepThrough]
      set { identifier = value; }
    }

    /// <summary>
    /// Gets page identifier 
    /// </summary>
    object IIdentified.Identifier
    {
      [DebuggerStepThrough]
      get { return Identifier; }
    }

    /// <summary>
    /// Gets or sets page version.
    /// </summary>
    public int Version
    {
      [DebuggerStepThrough]
      get { return version; }
      [DebuggerStepThrough]
      set { version = value; }
    }

    /// <summary>
    /// Gets page version.
    /// </summary>
    object IHasVersion.Version
    {
      [DebuggerStepThrough]
      get { return Version; }
    }

    /// <summary>
    /// Gets or sets page persisted status.
    /// </summary>
    public bool IsPersisted
    {
      [DebuggerStepThrough]
      get { return isPersisted; }
      [DebuggerStepThrough]
      set { isPersisted = value; }
    }

    #region Shortcuts: AsXxxPage

    /// <summary>
    /// Casts page to the <see cref="DescriptorPage"/> type.
    /// </summary>
    public DescriptorPage<TKey, TItem> AsDescriptorPage
    {
      [DebuggerStepThrough]
      get { return this as DescriptorPage<TKey, TItem>; }
    }

    /// <summary>
    /// Casts page to the <see cref="DataPage{TKey, TValue}"/> type.
    /// </summary>
    public DataPage<TKey, TItem> AsDataPage
    {
      [DebuggerStepThrough]
      get { return this as DataPage<TKey, TItem>; }
    }


    /// <summary>
    /// Casts page to the <see cref="InnerPage{TKey, TValue}"/> type.
    /// </summary>
    public InnerPage<TKey, TItem> AsInnerPage
    {
      [DebuggerStepThrough]
      get { return this as InnerPage<TKey, TItem>; }
    }


    /// <summary>
    /// Casts page to the <see cref="LeafPage{TKey, TValue}"/> type.
    /// </summary>
    public LeafPage<TKey, TItem> AsLeafPage
    {
      [DebuggerStepThrough]
      get { return this as LeafPage<TKey, TItem>; }
    }

    /// <summary>
    /// Gets <see cref="DescriptorPage"/> of the index.
    /// </summary>
    public DescriptorPage<TKey, TItem> DescriptorPage
    {
      [DebuggerStepThrough]
      get { return descriptorPage; }
    }

    #endregion

    /// <summary>
    /// Updates page version.
    /// </summary>
    public void UpdateVersion()
    {
      version++;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor"/>
    /// </summary>
    /// <param name="provider">Index page provider this page is bound to.</param>
    protected Page(IIndexPageProvider<TKey, TItem> provider)
    {
      this.provider = provider;
      descriptorPage = provider.Index.DescriptorPage;
      provider.AssignIdentifier(this);
    }

    #region IDeserializationCallback Members

    /// <inheritdoc/>
    public virtual void OnDeserialization(object sender)
    {
      isPersisted = true;
    }

    #endregion
  }
}