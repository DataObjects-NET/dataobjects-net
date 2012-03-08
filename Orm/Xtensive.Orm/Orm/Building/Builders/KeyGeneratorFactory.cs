// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals.KeyGenerators;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;

namespace Xtensive.Orm.Building.Builders
{
  internal static class KeyGeneratorFactory
  {
    public static bool IsSupportedByStandardGenerators(Type valueType)
    {
      return IsSequenceBacked(valueType) || valueType==typeof (Guid) || valueType==typeof (string);
    }

    public static bool IsSequenceBacked(Type valueType)
    {
      return WellKnown.SupportedNumericTypes.Contains(valueType);
    }

    public static Type GetGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType))
        return typeof (StorageSequentalGenerator<>).MakeGenericType(valueType);

      if (valueType==typeof (Guid))
        return typeof (GuidGenerator);

      if (valueType==typeof (string))
        return typeof (StringGenerator);

      throw TypeNotSupported(valueType);
    }

    public static Type GetTemporaryGeneratorType(Type valueType)
    {
      if (IsSequenceBacked(valueType))
        return typeof (TemporarySequentalGenerator<>).MakeGenericType(valueType);

      if (valueType==typeof (Guid))
        return typeof (GuidGenerator);

      if (valueType==typeof (string))
        return typeof (StringGenerator);

      throw TypeNotSupported(valueType);
    }

    public static IEnumerable<ServiceRegistration> CreateStandardRegistrations()
    {
      var types = WellKnown.SupportedNumericTypes.Concat(new[] {typeof (Guid), typeof (string)});

      foreach (var type in types) {
        var name = type.GetShortName();
        yield return new ServiceRegistration(
          typeof (IKeyGenerator), name, GetGeneratorType(type), true);
        yield return new ServiceRegistration(
          typeof (ITemporaryKeyGenerator), name, GetTemporaryGeneratorType(type), true);
      }
    }

    public static IEnumerable<ServiceRegistration> CreateRegistrations(DomainConfiguration configuration)
    {
      var userRegistrations = configuration.Types.KeyGenerators
        .SelectMany(ServiceRegistration.CreateAll)
        .ToList();

      var standardRegistrations =
        CreateStandardRegistrations()
          .Where(reg => !userRegistrations.Any(r => r.Type==reg.Type && r.Name==reg.Name))
          .ToList();

      var allRegistrations = userRegistrations.Concat(standardRegistrations);

      if (!configuration.IsMultidatabase)
        return allRegistrations;

      // If we are in multidatabase mode key generators will have database specific suffixes
      // We need to handle it by building a cross product between all key generators and all databases.
      // TODO: handle user's per-database registrations
      // They should have more priority than user's database-agnostic key generators
      // and standard key generators (which are always database-agnostic).

      var databases = configuration.MappingRules
        .Select(rule => rule.Database)
        .Where(db => !String.IsNullOrEmpty(db))
        .Concat(Enumerable.Repeat(configuration.DefaultDatabase, 1))
        .ToHashSet();

      return allRegistrations.SelectMany(_ => databases, AddLocation);
    }

    private static ServiceRegistration AddLocation(ServiceRegistration originalRegistration, string database)
    {
      return new ServiceRegistration(
        originalRegistration.Type,
        NameBuilder.BuildKeyGeneratorName(originalRegistration.Name, database),
        originalRegistration.MappedType,
        originalRegistration.Singleton);
    }

    private static NotSupportedException TypeNotSupported(Type valueType)
    {
      return new NotSupportedException(String.Format(
        "Type '{0}' is not supported by standard key generators", valueType));
    }
  }
}