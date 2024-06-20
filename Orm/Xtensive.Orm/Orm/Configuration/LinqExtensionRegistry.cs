// Copyright (C) 2011-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      EnsureNotLocked();
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
      EnsureNotLocked();
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

    /// <inheritdoc/>
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public LinqExtensionRegistry Clone() => new LinqExtensionRegistry(this);


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
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