// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Aspects
{
  /// <summary>
  /// Indicates whether transactional aspect should be applied to the method, property or constructor.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, 
    AllowMultiple = false, Inherited = true)]
  public class TransactionalAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the transaction opening mode.
    /// </summary>
    public TransactionOpenMode Mode { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether member marked by this attribute requires transaction or not. 
    /// </summary>
    public bool IsTransactional { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TransactionalAttribute() : 
      this(true)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mode">The transaction opening mode.</param>
    public TransactionalAttribute(TransactionOpenMode mode) : 
      this(true, mode)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="isTransactional">Whether or not member is transactional.</param>
    public TransactionalAttribute(bool isTransactional) :
      this (isTransactional, TransactionOpenMode.Auto)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="isTransactional">Whether or not member is transactional.</param>
    /// <param name="mode">The transaction opening mode.</param>
    public TransactionalAttribute(bool isTransactional, TransactionOpenMode mode)
    {
      IsTransactional = isTransactional;
      Mode = mode;
    }
  }
}