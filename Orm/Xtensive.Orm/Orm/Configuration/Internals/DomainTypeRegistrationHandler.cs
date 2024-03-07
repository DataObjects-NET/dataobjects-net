// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// <see cref="ITypeRegistrationProcessor"/> for processing <see cref="SessionBound"/>
  /// and <see cref="IEntity"/> descendants registration in
  /// <see cref="DomainConfiguration.Types"/> registry.
  /// </summary>
  /// <remarks>This implementation provides topologically sorted list
  /// of <see cref="Type"/>s.</remarks>
  internal sealed class DomainTypeRegistrationHandler : TypeRegistrationProcessorBase
  {
    private readonly static Type objectType = WellKnownTypes.Object;
    private const string providersNamespace = "Xtensive.Orm.Providers.";
    private const string preprocessorName = "Xtensive.Orm.Linq.ClosureQueryPreprocessor";

    /// <inheritdoc/>
    public override Type BaseType
    {
      get { return objectType; }
    }

    protected override bool IsAcceptable(TypeRegistration registration, Type type)
    {
      // Disallow implicit (via assembly scan) registration of types in Orm.Providers namespace
      return base.IsAcceptable(registration, type)
        && (registration.Type != null
           || (!type.FullName.StartsWith(providersNamespace, StringComparison.Ordinal)
               && !type.FullName.Equals(preprocessorName, StringComparison.Ordinal)));
    }

    /// <inheritdoc/>
    protected override void Process(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      if (type==null || registry.Contains(type))
        return;
      if (!DomainTypeRegistry.IsInterestingType(type))
        return;
      // The type is interesting;
      // If it is a persistent type, let's register all its bases
      if (DomainTypeRegistry.IsPersistentType(type))
        Process(registry, registration, type.BaseType);
      Type[] interfaces = type.FindInterfaces(
        (_type, filterCriteria) => DomainTypeRegistry.IsPersistentType(_type), type);
      for (int index = 0; index < interfaces.Length; index++)
        Process(registry, registration, interfaces[index]);
      // Final registration
      base.Process(registry, registration, type);
    }
  }
}
