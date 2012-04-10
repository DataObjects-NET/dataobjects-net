// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.27

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;


namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Registration entry for LINQ extension.
  /// </summary>
  public sealed class LinqExtensionRegistration
  {
    /// <summary>
    /// Gets member this extension is intended for.
    /// </summary>
    public MemberInfo Member { get; private set; }

    /// <summary>
    /// Gets substitution that is performed when LINQ translator encouters <see cref="Member"/> access.
    /// </summary>
    public LambdaExpression Substitution { get; private set; }

    /// <summary>
    /// Gets action that is performed when LINQ translator encouters <see cref="Member"/> access.
    /// </summary>
    public Func<MemberInfo, Expression, Expression[], Expression> Compiler { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="member">Value for <see cref="Member"/>.</param>
    /// <param name="substitution">Value for <see cref="Substitution"/>.</param>
    public LinqExtensionRegistration(MemberInfo member, LambdaExpression substitution)
    {
      ArgumentValidator.EnsureArgumentNotNull(member, "member");
      ArgumentValidator.EnsureArgumentNotNull(substitution, "substitution");

      Member = member;
      Substitution = substitution;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="member">Value for <see cref="Member"/>.</param>
    /// <param name="compiler">Value for <see cref="Compiler"/>.</param>
    public LinqExtensionRegistration(MemberInfo member, Func<MemberInfo, Expression, Expression[], Expression> compiler)
    {
      ArgumentValidator.EnsureArgumentNotNull(member, "member");
      ArgumentValidator.EnsureArgumentNotNull(compiler, "compiler");

      Member = member;
      Compiler = compiler;
    }
  }
}