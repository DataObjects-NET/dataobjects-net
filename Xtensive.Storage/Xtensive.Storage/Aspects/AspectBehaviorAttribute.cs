// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.07.30

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Describes aspect behavior related to a particular method.
  /// </summary>
  [Serializable]
  [Obsolete("Use TransactionalAttribute and ActivateSessionAttribute instead.")]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = true)]
  public sealed class AspectBehaviorAttribute : StorageAttribute
  {
    private bool openSession = true;
    private bool openTransaction = true;

    /// <summary>
    /// Indicates whether a <see cref="Session"/> must 
    /// be activated for the method this attribute is applied to.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool OpenSession
    {
      get { return openSession; }
      set { openSession = value; }
    }

    /// <summary>
    /// Indicates whether a <see cref="Transaction"/> must 
    /// be opened for the method this attribute is applied to.
    /// Default value is <see langword="true" />.
    /// </summary>
    public bool OpenTransaction
    {
      get { return openTransaction; }
      set { openTransaction = value; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public AspectBehaviorAttribute()
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="openSessionAndTransaction">Mutually sets values of <see cref="OpenSession"/>
    /// and <see cref="OpenTransaction"/>.</param>
    public AspectBehaviorAttribute(bool openSessionAndTransaction)
    {
      OpenSession = openSessionAndTransaction;
      OpenTransaction = openSessionAndTransaction;
    }
  }
}