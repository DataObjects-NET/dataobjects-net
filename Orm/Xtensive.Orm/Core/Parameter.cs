// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2008.08.14

using System.Diagnostics;


namespace Xtensive.Core
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  public abstract class Parameter
  {
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override string ToString() => Name;

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    [DebuggerStepThrough]
    protected Parameter(string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, nameof(name));
      Name = name;
    }
  }
}