// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Internals.KeyGenerators;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal static class KeyGeneratorFactory
  {
    private static readonly Type[] SupportedNumericTypes =
    {
      WellKnownTypes.SByte,
      WellKnownTypes.Byte,
      WellKnownTypes.Int16,
      WellKnownTypes.UInt16,
      WellKnownTypes.Int32,
      WellKnownTypes.UInt32,
      WellKnownTypes.Int64,
      WellKnownTypes.UInt64
    };

    private static readonly Type[] SupportedTypes = SupportedNumericTypes
      .Concat(new[] { WellKnownTypes.Guid, WellKnownTypes.String })
      .ToArray();

    private static readonly Type
      StorageSequentalGeneratorType = typeof(StorageSequentalGenerator<>),
      TemporarySequentalGeneratorType = typeof(TemporarySequentalGenerator<>),
      GuidGeneratorType = typeof(GuidGenerator),
      StringGeneratorType = typeof(StringGenerator);

    public static bool IsSupported(Type valueType) => SupportedTypes.Contains(valueType);

    public static bool IsSequenceBacked(Type valueType) => SupportedNumericTypes.Contains(valueType);

    private static Type GetGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType)) {
        return StorageSequentalGeneratorType.CachedMakeGenericType(valueType);
      }

      if (valueType == WellKnownTypes.Guid) {
        return GuidGeneratorType;
      }

      if (valueType == WellKnownTypes.String) {
        return StringGeneratorType;
      }

      throw TypeNotSupported(valueType);
    }

    private static Type GetTemporaryGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType)) {
        return TemporarySequentalGeneratorType.CachedMakeGenericType(valueType);
      }

      if (valueType == WellKnownTypes.Guid) {
        return GuidGeneratorType;
      }

      if (valueType == WellKnownTypes.String) {
        return StringGeneratorType;
      }

      throw TypeNotSupported(valueType);
    }

    private static IEnumerable<ServiceRegistration> GetStandardRegistrations(string name, Type valueType)
    {
      yield return new ServiceRegistration(
        typeof (KeyGenerator), name, GetGeneratorType(valueType), true);
      yield return new ServiceRegistration(
        typeof (TemporaryKeyGenerator), name, GetTemporaryGeneratorType(valueType), true);
    }

    public static IEnumerable<ServiceRegistration> GetRegistrations(BuildingContext context)
    {
      var standardRegistrations = context.Model.Hierarchies.Select(h => h.Key)
        .Where(key => key.GeneratorKind == KeyGeneratorKind.Default && key.IsFirstAmongSimilarKeys)
        .SelectMany(key => GetStandardRegistrations(key.GeneratorName, key.SingleColumnType));

      var userRegistrations = context.Configuration.Types.KeyGenerators
        .SelectMany(ServiceRegistration.CreateAll)
        .ToList();

      return userRegistrations.Concat(standardRegistrations);
    }

    private static NotSupportedException TypeNotSupported(Type valueType) =>
      new NotSupportedException($"Type '{valueType}' is not supported by standard key generators");
  }
}