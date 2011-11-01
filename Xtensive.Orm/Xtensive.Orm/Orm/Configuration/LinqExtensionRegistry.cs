// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Registry of custom compilers used by LINQ translator.
  /// </summary>
  public sealed class LinqExtensionRegistry : LockableBase, ICloneable, IEnumerable<LinqExtensionRegistration>
  {
    private readonly Dictionary<MemberInfo, LinqExtensionRegistration> registrations;

    /// <summary>
    /// Gets all registered substitutions.
    /// </summary>
    public IEnumerable<LinqExtensionRegistration> Substitutions
    {
      get { return registrations.Values.Where(item => item.Substitution!=null); }
    }

    /// <summary>
    /// Gets all registered compilers.
    /// </summary>
    public IEnumerable<LinqExtensionRegistration> Compilers
    {
      get { return registrations.Values.Where(item => item.Compiler!=null); }
    }

    /// <summary>
    /// Registers <paramref name="substitution"/> as a substitution for <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to register substitution for.</param>
    /// <param name="substitution">Substitution</param>
    public void Register(MemberInfo member, LambdaExpression substitution)
    {
      this.EnsureNotLocked();
      var registration = new LinqExtensionRegistration(member, substitution);
      registrations.Add(member, registration);
    }

    /// <summary>
    /// Registers <paramref name="compiler"/> as a compiler for <paramref name="member"/>.
    /// </summary>
    /// <param name="member">Member to register compiler for.</param>
    /// <param name="compiler">Compiler.</param>
    public void Register(MemberInfo member, Func<MemberInfo, Expression, Expression[], Expression> compiler)
    {
      this.EnsureNotLocked();
      var registration = new LinqExtensionRegistration(member, compiler);
      registrations.Add(member, registration);
    }

    /// <inheritdoc />
    public IEnumerator<LinqExtensionRegistration> GetEnumerator()
    {
      return registrations.Values.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public object Clone()
    {
      return new LinqExtensionRegistry(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public LinqExtensionRegistry()
    {
      registrations = new Dictionary<MemberInfo, LinqExtensionRegistration>();
    }

    private LinqExtensionRegistry(LinqExtensionRegistry source)
    {
      registrations = new Dictionary<MemberInfo, LinqExtensionRegistration>(source.registrations);
    }
  }
}