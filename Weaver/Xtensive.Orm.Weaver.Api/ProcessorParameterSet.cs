// Copyright (C) 2013 Xtensive LLC.
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
  public sealed class ProcessorParameterSet
  {
    internal static class Formatter
    {
      private const string ProjectId = "projectId";
      private const string InputFile = "input";
      private const string OutputFile = "output";
      private const string StrongNameKey = "strongNameKey";
      private const string TargetFramework = "targetFramework";
      private const string ReferencedAssembly = "reference";

      private static readonly char[] KeyValueSeparators = new[] {':', '='};

      private static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;

      public static bool Parse(ProcessorParameterSet parameters, string item)
      {
        string key;
        string value;

        if (!ParseKeyValue(item, out key, out value))
          return false;

        if (Comparer.Equals(key, ProjectId))
          parameters.ProjectId = value;
        else if (Comparer.Equals(key, InputFile))
          parameters.InputFile = value;
        else if (Comparer.Equals(key, OutputFile))
          parameters.OutputFile = value;
        else if (Comparer.Equals(key, StrongNameKey))
          parameters.StrongNameKey = value;
        else if (Comparer.Equals(key, TargetFramework))
          parameters.TargetFramework = value;
        else if (Comparer.Equals(key, ReferencedAssembly))
          parameters.ReferencedAssemblies.Add(value);
        else
          return false;

        return true;
      }

      public static IEnumerable<string> Dump(ProcessorParameterSet parameters)
      {
        if (!string.IsNullOrEmpty(parameters.ProjectId))
          yield return FormatKeyValue(ProjectId, parameters.ProjectId);

        if (!string.IsNullOrEmpty(parameters.InputFile))
          yield return FormatKeyValue(InputFile, parameters.InputFile);

        if (!string.IsNullOrEmpty(parameters.OutputFile))
          yield return FormatKeyValue(OutputFile, parameters.OutputFile);

        if (!string.IsNullOrEmpty(parameters.StrongNameKey))
          yield return FormatKeyValue(StrongNameKey, parameters.StrongNameKey);

        if (!string.IsNullOrEmpty(parameters.StrongNameKey))
          yield return FormatKeyValue(TargetFramework, parameters.TargetFramework);

        if (parameters.ReferencedAssemblies!=null)
          foreach (var assemblyFile in parameters.ReferencedAssemblies)
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
    }

    private string projectId;
    private string inputFile;
    private string outputFile;
    private string strongNameKey;
    private string targetFramework;
    private IList<string> referencedAssemblies;

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

    public ProcessorParameterSet()
    {
      ReferencedAssemblies = new List<string>();
    }
  }
}