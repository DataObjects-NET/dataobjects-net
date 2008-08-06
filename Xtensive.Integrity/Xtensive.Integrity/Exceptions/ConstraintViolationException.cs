// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.25

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Integrity
{
  /// <summary>
  /// Thrown as the result of violation of constraint.
  /// </summary>
  [Serializable]
  public class ConstraintViolationException : Exception
  {
    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>  
    /// <param name="message">The error message.</param>
    public ConstraintViolationException(string message)
      : base(message)
    {      
    }
  }
}