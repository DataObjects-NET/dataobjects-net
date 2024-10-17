// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class TypeRegistrationOptions : IIdentifyableOptions,
    IValidatableOptions,
    IToNativeConvertable<TypeRegistration>
  {
    public object Identifier
    {
      get {
        if (!Type.IsNullOrEmpty())
          return Type;
        else
          return (Assembly, Namespace.IsNullOrEmpty() ? null : Namespace);
      }
    }

    /// <summary>
    /// Gets or sets the name of the type to register.
    /// </summary>
    public string Type { get; set; } = null;

    /// <summary>
    /// Gets or sets the assembly where types to register are located.
    /// </summary>
    public string Assembly { get; set; } = null;

    /// <summary>
    /// Gets or sets the namespace withing the <see cref="Assembly"/>, 
    /// where types to register are located.
    /// If <see langword="null" /> or <see cref="string.Empty"/>, 
    /// all the persistent types from the <see cref="Assembly"/> will be registered.
    /// </summary>
    public string Namespace { get; set; } = null;

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Combination of properties is not valid.</exception>
    public void Validate()
    {
      if (!Type.IsNullOrEmpty() && (!Assembly.IsNullOrEmpty() || !Namespace.IsNullOrEmpty()))
        throw new ArgumentException("Either type or assembly can be declared, not both at the same time.");
      if (!Assembly.IsNullOrEmpty() && !Type.IsNullOrEmpty())
        throw new ArgumentException("Either type or assembly can be declared, not both at the same time.");
      if (!Namespace.IsNullOrEmpty() && Assembly.IsNullOrEmpty())
        throw new ArgumentException("Namespace can only be declared with Assembly.");
    }

    /// <inheritdoc />
    public TypeRegistration ToNative()
    {
      Validate();

      if (!Type.IsNullOrEmpty())
        return new TypeRegistration(System.Type.GetType(Type, true));
      else if (!Assembly.IsNullOrEmpty()) {
        var assembly = System.Reflection.Assembly.Load(Assembly);
        if (Namespace.IsNullOrEmpty())
          return new TypeRegistration(assembly);
        else
          return new TypeRegistration(assembly, Namespace);
      }
      throw new InvalidOperationException();
    }
  }
}
