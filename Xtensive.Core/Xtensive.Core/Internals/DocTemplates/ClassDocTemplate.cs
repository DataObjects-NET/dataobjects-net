// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.14

using System;

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// Class documentation template.
  /// </summary>
  public class ClassDocTemplate: IDisposable
  {
    /// <summary>
    /// Implements the equality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the comparison for equality.</returns>
    public static bool OperatorEq(ClassDocTemplate left, ClassDocTemplate right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the inequality operator.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>The result of the comparison for inequality.</returns>
    public static bool OperatorNeq(ClassDocTemplate left, ClassDocTemplate right)
    {
      return !Equals(left, right);
    }


    // Initializers

    /// <summary>
    /// Configures a new instance of the <see cref="ClassDocTemplate"/> class.
    /// </summary>
    public void Configure()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDocTemplate"/> class.
    /// </summary>
    public void Initialize()
    {
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassDocTemplate"/> class.
    /// </summary>
    public static void Ctor()
    {
    }

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="ClassDocTemplate"/> is reclaimed by garbage collection.
    /// </summary>
    public void Dtor()
    {
    }

    /// <see cref="Dtor" copy="true" />
    ~ClassDocTemplate()
    {
    }

    /// <summary>
    /// Performs the tasks associated with freeing, releasing, or resetting unmanaged resources
    /// or associated <see cref="IDisposable"/> objects.
    /// </summary>
    public void Dispose()
    {
    }


    // Type initializer
    
    /// <summary>
    /// Initializes the <see cref="ClassDocTemplate"/> type.
    /// </summary>
    public static void TypeInitializer()
    {
    }
  }
}