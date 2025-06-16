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
    private const string MainTestAsseblyNsPrefix = "Xtensive.Orm.Tests."; // keep the dot at the end
    private const string IssuesNsPrefix = "Xtensive.Orm.Tests.Issues.";// keep the dot at the end
    private const string UpgradeNsPrefix = "Xtensive.Orm.Tests.Upgrade.";// keep the dot at the end

    
    private static readonly byte[] ThisAssemblyPkt = typeof(AssemblyExtensions).Assembly.GetName().GetPublicKeyToken();
    
    private static readonly ConcurrentDictionary<Assembly, Type[]> TypesPerAssembly = new();
    private static readonly ConcurrentDictionary<char, int> XtensiveOrmTestsNsAlphabeticIndex = new();
    private static readonly ConcurrentDictionary<char, int> MainTestsForUpgrade = new();
    private static readonly ConcurrentDictionary<char, int> MainTestsForIssues = new();

    public static System.Configuration.Configuration GetAssemblyConfiguration(this Assembly assembly)
    {
      return ConfigurationManager.OpenExeConfiguration(assembly.Location);
    }

    public static IReadOnlyList<Type> GetTypesFromNamespaceCaching(this Assembly assembly, string @namespace)
    {
      if (string.IsNullOrWhiteSpace(@namespace))
        throw new ArgumentException("Namespace cannot be null, empty or contains only white spaces");

      // these two dummy mentionsa are to not forget to sync filtration algorithm here and in the classes,
      // in particular BaseType property, if the property changed then this algorighm should be changed as well
      _ = nameof(Xtensive.IoC.ServiceTypeRegistrationProcessor.BaseType);
      _ = nameof(Xtensive.Orm.Configuration.DomainTypeRegistrationHandler.BaseType);

      var assemblyNameInfo = assembly.GetName();
      var isMainTestAssembly = assemblyNameInfo.Name == "Xtensive.Orm.Tests" && !ThisAssemblyPkt.Except(assemblyNameInfo.GetPublicKeyToken()).Any();

      var assemblyTypes = TypesPerAssembly.GetOrAdd(assembly, static (a, isMain) => {
        var allTypes = a.GetTypes();

        var objectType = typeof(object);
        var list = new List<Type>(allTypes.Length);
        var currentIndex = 0;
        foreach (var t in allTypes) {
          // we ignore compiler generated types because usuallty they are
          // at the end of sorted types
          if (t.IsSubclassOf(objectType) && t.GetCustomAttribute<CompilerGeneratedAttribute>() == null) {
            list.Add(t);
            if (isMain) {
              var nSpace = t.Namespace;
              if (nSpace != null && nSpace.StartsWith(MainTestAsseblyNsPrefix, StringComparison.Ordinal)) {
                var firstLetter = nSpace[MainTestAsseblyNsPrefix.Length];
                // main test library has 5000+ types, to not enumerate them every type from the beginning
                // we try to have parts by first letter
                _ = XtensiveOrmTestsNsAlphabeticIndex.TryAdd(firstLetter, currentIndex);
                if (firstLetter == 'I'/*ssue*/ && nSpace.StartsWith(IssuesNsPrefix)) {
                  var firstIssuesLetter = nSpace[IssuesNsPrefix.Length];
                  _ = MainTestsForIssues.TryAdd(firstIssuesLetter, currentIndex);
                }
                if (firstLetter== 'U'/*pdate*/ && nSpace.StartsWith(UpgradeNsPrefix)) {
                  var firstIssuesLetter = nSpace[UpgradeNsPrefix.Length];
                  _ = MainTestsForUpgrade.TryAdd(firstIssuesLetter, currentIndex);
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

    private static int GetSearchStartPosition(string ns, bool isMainAssembly)
    {
      var searchStart = 0;
      if (isMainAssembly) {
        if (ns.StartsWith(IssuesNsPrefix)) {
          searchStart = MainTestsForIssues[ns[IssuesNsPrefix.Length]];
        }
        else if (ns.StartsWith(UpgradeNsPrefix)) {
          searchStart = MainTestsForUpgrade[ns[UpgradeNsPrefix.Length]];
        }
        else if (ns.StartsWith(MainTestAsseblyNsPrefix)) {
          searchStart = XtensiveOrmTestsNsAlphabeticIndex[ns[MainTestAsseblyNsPrefix.Length]];
        }
      }

      return searchStart;
    }

    private static int FindFirstEntry(Type[] types, in int serachFrom, in string nsAndDot)
    {
      var firstHit = -1;

      for (int headIndex = serachFrom, count = types.Length; headIndex < count; headIndex++) {
        var head = types[headIndex];
        if (head.FullName.IndexOf(nsAndDot, StringComparison.InvariantCulture) >= 0) {
          firstHit = headIndex;
          break;
        }
      }

      if (firstHit == -1)
        throw new Exception($"There is no any entry for fiven namespace.");

      return firstHit;
    }

    private static IReadOnlyList<Type> FindSegment(Type[] types, string ns, bool isMainAssembly)
    {
      // We rely on the fact that types are sorted by full name.
      // That opens an opportunity to optimize search of types from given
      // namespace and subnamespaces.

      const int windowSize = 6;

      var searchFrom = GetSearchStartPosition(ns, isMainAssembly);

      var nsAndDot = ns + ".";
      var startSearchBoundary = FindFirstEntry(types, searchFrom, nsAndDot);

      var wrongNsFound = false;
      var endSearchBoundary = startSearchBoundary;
      var lastTypeIndex = types.Length - 1;
      do {
        endSearchBoundary += windowSize;
        if (endSearchBoundary > lastTypeIndex)
          endSearchBoundary = lastTypeIndex;

        var tail = types[endSearchBoundary];
        if (tail.FullName.IndexOf(nsAndDot, StringComparison.InvariantCulture) < 0)
          wrongNsFound = true;
      }
      while (!wrongNsFound || endSearchBoundary < lastTypeIndex);

      for (var tailIndex = endSearchBoundary; tailIndex >= startSearchBoundary; tailIndex--) {
        var tail = types[tailIndex];
        endSearchBoundary = tailIndex;
        if (tail.FullName.IndexOf(nsAndDot, StringComparison.InvariantCulture) >= 0) {
          break;
        }
      }

      return new ArraySegment<Type>(types, startSearchBoundary, endSearchBoundary - startSearchBoundary + 1);
    }
  }
}