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
  /// Indicates whether a session should be activated on the method's or property's boundaries.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, 
    AllowMultiple = false, Inherited = true)]
  public class ActivateSessionAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets a value indicating whether a session should be activated on the method boundaries
    /// </summary>
    public bool Activate { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ActivateSessionAttribute() :
      this(true)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="activate">Whether or not a session should be activated on the method boundaries.</param>
    public ActivateSessionAttribute(bool activate)
    {
      Activate = activate;
    }
  }
}