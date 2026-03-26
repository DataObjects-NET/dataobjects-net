// Copyright (C) 2018-2025 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2018.08.31

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xtensive.Orm.Tests
{
  public static class AssemblyExtensions
  {
    private const string MainTestAsseblyNsPrefix = "Xtensive.Orm.Tests.";
    private const string IssuesNsPrefix = "Xtensive.Orm.Tests.Issues.";
    private const string UpgradeNsPrefix = "Xtensive.Orm.Tests.Upgrade.";

    private static readonly byte[] ThisAssemblyPkt = typeof(AssemblyExtensions).Assembly.GetName().GetPublicKeyToken();
    
    private static readonly ConcurrentDictionary<Assembly, Type[]> TypesPerAssembly = new();
    private static readonly ConcurrentDictionary<char, int> MainTestsAssemblyNsAlphabeticIndex = new();
    private static readonly ConcurrentDictionary<char, int> MainTestsAssemblyIssuesNsAlphabeticIndex = new();
    private static readonly ConcurrentDictionary<char, int> MainTestsAssemblyUpgradeNsAlphabeticIndex = new();

    public static System.Configuration.Configuration GetAssemblyConfiguration(this Assembly assembly)
    {
      return ConfigurationManager.OpenExeConfiguration(assembly.Location);
    }

    public static IReadOnlyList<Type> GetTypesFromNamespaceCaching(this Assembly assembly, string @namespace)
    {
      if (string.IsNullOrWhiteSpace(@namespace))
        throw new ArgumentException("Namespace cannot be null, empty or contain only white spaces");

      // These two dummy mentions are here to not forget to sync filtration algorithm here and in the classes,
      // in particular BaseType property, if the property changed result type then this algorighm should be updated as well
      _ = nameof(Xtensive.IoC.ServiceTypeRegistrationProcessor.BaseType);
      _ = nameof(Xtensive.Orm.Configuration.DomainTypeRegistrationHandler.BaseType);

      var assemblyNameInfo = assembly.GetName();
      var isMainTestAssembly = assemblyNameInfo.Name == "Xtensive.Orm.Tests"
        && !ThisAssemblyPkt.Except(assemblyNameInfo.GetPublicKeyToken()).Any();

      var assemblyTypes = TypesPerAssembly.GetOrAdd(assembly, static (a, isMain) => {
        var allTypes = a.GetTypes();

        var objectType = typeof(object);
        var list = new List<Type>(allTypes.Length);
        var currentIndex = 0;
        foreach (var t in allTypes) {
          if (t.IsSubclassOf(objectType) && t.GetCustomAttribute<CompilerGeneratedAttribute>() == null) {
            list.Add(t);
            if (isMain) {
              var nSpace = t.Namespace;
              if (nSpace != null && nSpace.StartsWith(MainTestAsseblyNsPrefix, StringComparison.Ordinal)) {
                var firstLetter = nSpace[MainTestAsseblyNsPrefix.Length];

                _ = MainTestsAssemblyNsAlphabeticIndex.TryAdd(firstLetter, currentIndex);
                if (firstLetter == 'I'/*ssue*/ && nSpace.StartsWith(IssuesNsPrefix, StringComparison.Ordinal)) {
                  var firstIssuesLetter = nSpace[IssuesNsPrefix.Length];
                  _ = MainTestsAssemblyIssuesNsAlphabeticIndex.TryAdd(firstIssuesLetter, currentIndex);
                }
                if (firstLetter == 'U'/*pdate*/ && nSpace.StartsWith(UpgradeNsPrefix, StringComparison.Ordinal)) {
                  var firstIssuesLetter = nSpace[UpgradeNsPrefix.Length];
                  _ = MainTestsAssemblyUpgradeNsAlphabeticIndex.TryAdd(firstIssuesLetter, currentIndex);
                }
              }
            }
            currentIndex++;
          }
        }
        return list.ToArray();
      }, isMainTestAssembly);

      return FindSegment(assemblyTypes, @namespace, isMainTestAssembly);
    }

    private static IReadOnlyList<Type> FindSegment(Type[] types, string ns, bool isMainAssembly)
    {
      // We rely on the fact that types are sorted by full name.
      // That opens an opportunity to optimize search of types from given
      // namespace and subnamespaces.

      const int windowSize = 6;

      var searchFrom = GetSearchStartPosition(ns, isMainAssembly);

      var nsWithDot = ns + ".";
      var startSearchBoundary = FindFirstEntry(types, searchFrom, nsWithDot);

      var endSearchBoundary = startSearchBoundary;
      var lastItemIndex = types.Length - 1;

      bool wrongNsFound;
      do {
        endSearchBoundary += windowSize;
        if (endSearchBoundary > lastItemIndex)
          endSearchBoundary = lastItemIndex;

        var tail = types[endSearchBoundary];
        wrongNsFound = tail.FullName.IndexOf(nsWithDot, StringComparison.InvariantCulture) < 0;
      }
      while (!wrongNsFound && endSearchBoundary < lastItemIndex);

      for (var tailIndex = endSearchBoundary; tailIndex >= startSearchBoundary; tailIndex--) {
        var tail = types[tailIndex];
        endSearchBoundary = tailIndex;
        if (tail.FullName.IndexOf(nsWithDot, StringComparison.InvariantCulture) >= 0) {
          break;
        }
      }

      return new ArraySegment<Type>(types, startSearchBoundary, endSearchBoundary - startSearchBoundary + 1);
    }

    private static int GetSearchStartPosition(string ns, bool isMainAssembly)
    {
      if (!isMainAssembly)
        return 0;
      if (ns.StartsWith(IssuesNsPrefix, StringComparison.Ordinal))
        return MainTestsAssemblyIssuesNsAlphabeticIndex[ns[IssuesNsPrefix.Length]];
      else if (ns.StartsWith(UpgradeNsPrefix, StringComparison.Ordinal))
        return MainTestsAssemblyUpgradeNsAlphabeticIndex[ns[UpgradeNsPrefix.Length]];
      else if (ns.StartsWith(MainTestAsseblyNsPrefix, StringComparison.Ordinal))
        return MainTestsAssemblyNsAlphabeticIndex [ns[MainTestAsseblyNsPrefix.Length]];
      return 0;
    }

    private static int FindFirstEntry(Type[] types, in int serachFrom, in string nsAndDot)
    {
      var firstEntryIndex = -1;

      for (int headIndex = serachFrom, count = types.Length; headIndex < count; headIndex++) {
        var head = types[headIndex];
        if (head.FullName.IndexOf(nsAndDot, StringComparison.InvariantCulture) >= 0) {
          firstEntryIndex = headIndex;
          break;
        }
      }

      if (firstEntryIndex == -1)
        throw new Exception($"There is no any entry for given namespace.");
      return firstEntryIndex;
    }
  }
}