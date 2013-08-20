﻿// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Weaver
{
  [Serializable]
  public sealed class ProcessorConfiguration
  {
    internal static class Formatter
    {
      private const string ProjectId = "projectId";
      private const string InputFile = "input";
      private const string OutputFile = "output";
      private const string UseDebugSymbols = "debugSymbols";
      private const string StrongNameKey = "strongNameKey";
      private const string TargetFramework = "targetFramework";
      private const string ReferencedAssembly = "reference";

      private static readonly char[] KeyValueSeparators = new[] {':', '='};

      private static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

      public static bool Parse(ProcessorConfiguration configuration, string item)
      {
        string key;
        string value;

        if (!ParseKeyValue(item, out key, out value))
          return false;

        if (Comparer.Equals(key, ProjectId))
          configuration.ProjectId = value;
        else if (Comparer.Equals(key, InputFile))
          configuration.InputFile = value;
        else if (Comparer.Equals(key, OutputFile))
          configuration.OutputFile = value;
        else if (Comparer.Equals(key, UseDebugSymbols))
          configuration.UseDebugSymbols = ParseBool(value);
        else if (Comparer.Equals(key, StrongNameKey))
          configuration.StrongNameKey = value;
        else if (Comparer.Equals(key, TargetFramework))
          configuration.TargetFramework = value;
        else if (Comparer.Equals(key, ReferencedAssembly))
          configuration.ReferencedAssemblies.Add(value);
        else
          return false;

        return true;
      }

      public static IEnumerable<string> Dump(ProcessorConfiguration configuration)
      {
        if (!string.IsNullOrEmpty(configuration.ProjectId))
          yield return FormatKeyValue(ProjectId, configuration.ProjectId);

        if (!string.IsNullOrEmpty(configuration.InputFile))
          yield return FormatKeyValue(InputFile, configuration.InputFile);

        if (!string.IsNullOrEmpty(configuration.OutputFile))
          yield return FormatKeyValue(OutputFile, configuration.OutputFile);

        if (configuration.UseDebugSymbols)
          yield return FormatKeyValue(UseDebugSymbols, "true");

        if (!string.IsNullOrEmpty(configuration.StrongNameKey))
          yield return FormatKeyValue(StrongNameKey, configuration.StrongNameKey);

        if (!string.IsNullOrEmpty(configuration.StrongNameKey))
          yield return FormatKeyValue(TargetFramework, configuration.TargetFramework);

        if (configuration.ReferencedAssemblies!=null)
          foreach (var assemblyFile in configuration.ReferencedAssemblies)
            yield return FormatKeyValue(ReferencedAssembly, assemblyFile);
      }

      private static string FormatKeyValue(string key, string value)
      {
        var format = value.Any(char.IsWhiteSpace) ? "{0}=\"{1}\"" : "{0}={1}";
        var quotedValue = value.Replace("\"", "\\\"");
        return string.Format(format, key, quotedValue);
      }

      private static bool ParseKeyValue(string item, out string key, out string value)
      {
        var position = item.IndexOfAny(KeyValueSeparators);
        if (position < 0 || position > item.Length - 2) {
          key = null;
          value = null;
          return false;
        }
        key = item.Substring(0, position);
        var rawValue = item.Substring(position + 1);
        if (rawValue.StartsWith("\"") && rawValue.EndsWith("\""))
          value = rawValue.Substring(1, rawValue.Length - 2).Replace("\\\"", "\"");
        else
          value = rawValue;
        return true;
      }

      private static bool ParseBool(string value)
      {
        bool result;
        return bool.TryParse(value, out result) && result;
      }
    }

    private string projectId;
    private string inputFile;
    private string outputFile;
    private string strongNameKey;
    private string targetFramework;
    private IList<string> referencedAssemblies;
    private bool useDebugSymbols;

    public string ProjectId
    {
      get { return projectId; }
      set { projectId = value; }
    }

    public string InputFile
    {
      get { return inputFile; }
      set { inputFile = value; }
    }

    public string OutputFile
    {
      get { return outputFile; }
      set { outputFile = value; }
    }

    public bool UseDebugSymbols
    {
      get { return useDebugSymbols; }
      set { useDebugSymbols = value; }
    }

    public string StrongNameKey
    {
      get { return strongNameKey; }
      set { strongNameKey = value; }
    }

    public string TargetFramework
    {
      get { return targetFramework; }
      set { targetFramework = value; }
    }

    public IList<string> ReferencedAssemblies
    {
      get { return referencedAssemblies; }
      set { referencedAssemblies = value; }
    }

    public IEnumerable<string> Dump()
    {
      return Formatter.Dump(this);
    }

    public bool Parse(string parameterString)
    {
      if (parameterString==null)
        throw new ArgumentNullException("parameterString");
      return Formatter.Parse(this, parameterString);
    }

    public ProcessorConfiguration()
    {
      ReferencedAssemblies = new List<string>();
    }
  }
}