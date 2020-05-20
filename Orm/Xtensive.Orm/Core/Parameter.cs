// Copyright (C) 2003-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Diagnostics;


namespace Xtensive.Core
{
  /// <summary>
  /// Parameter - an object identifying its value in active <see cref="ParameterContext"/>.
  /// </summary>
  public abstract class Parameter
  {
    private readonly object expectedValue;

    /// <summary>
    /// Indicates whether the property <see cref="ExpectedValue"/> is set.
    /// </summary>
    public readonly bool IsExpectedValueSet;

    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>    
    public string Name { get; }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="ParameterContext"/> is not activated.</exception>
    /// <exception cref="InvalidOperationException">Value for the parameter is not set.</exception>
    public object Value {
      [DebuggerStepThrough]
      get => throw new NotSupportedException();
    }

    /// <summary>
    /// Gets the expected value of the parameter.
    /// </summary>
    public virtual object ExpectedValue {
      [DebuggerStepThrough]
      get => expectedValue;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    /// <param name="expectedValue">The expected value of this parameter.</param>
    [DebuggerStepThrough]
    protected Parameter(string name, object expectedValue)
    {
      ArgumentValidator.EnsureArgumentNotNull(name, nameof(name));
      Name = name;
      this.expectedValue = expectedValue;
      IsExpectedValueSet = true;
    }
  }
}