// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.08.13

using System;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for <typeparamref name="TOrigin"/>-typed executable providers.
  /// </summary>
  /// <typeparam name="TOrigin">The type of the <see cref="Origin"/>.</typeparam>
  [Serializable]
  public abstract class ExecutableProvider<TOrigin> : ExecutableProvider
    where TOrigin: CompilableProvider
  {
    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public new TOrigin Origin => (TOrigin) base.Origin;

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(TOrigin origin, params ExecutableProvider[] sources)
      : base(origin, sources)
    {
    }
  }
}