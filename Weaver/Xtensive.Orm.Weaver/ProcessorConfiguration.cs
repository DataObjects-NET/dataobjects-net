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
  public sealed class ProcessorConfiguration
  {
    private static class Formatter
    {
      private const string WriteStatus = "status";
      private const string WriteStamp = "stamp";
      private const string MakeBackup = "backup";
      private const string ProcessDebugSymbols = "pdb";

      private const string InputFile = "input";
      private const string OutputFile = "output";
      private const string StrongNameKey = "snk";
      private const string ProjectType = "type";
      private const string ReferencedAssembly = "r";

      private static readonly char[] KeyValueSeparators = new[] {':', '='};

      private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

      public static bool Parse(ProcessorConfiguration configuration, string item)
      {
        if (Comparer.Equals(item, MakeBackup)) {
          configuration.MakeBackup = true;
          return true;
        }

        if (Comparer.Equals(item, WriteStatus)) {
          configuration.WriteStatusFile = true;
          return true;
        }

        if (Comparer.Equals(item, WriteStamp)) {
          configuration.WriteStampFile = true;
          return true;
        }

        if (Comparer.Equals(item, ProcessDebugSymbols)) {
          configuration.ProcessDebugSymbols = true;
          return true;
        }

        string key;
        string value;

        if (!ParseKeyValue(item, out key, out value))
          return false;

        if (Comparer.Equals(key, InputFile)) {
          configuration.InputFile = value;
          return true;
        }

        if (Comparer.Equals(key, OutputFile)) {
          configuration.OutputFile = value;
          return true;
        }

        if (Comparer.Equals(key, StrongNameKey)) {
          configuration.StrongNameKey = value;
          return true;
        }

        if (Comparer.Equals(key, ProjectType)) {
          configuration.ProjectType = value;
          return true;
        }

        if (Comparer.Equals(key, ReferencedAssembly)) {
          configuration.ReferencedAssemblies.Add(value);
          return true;
        }

        return false;
      }

      public static IEnumerable<string> Dump(ProcessorConfiguration configuration)
      {
        if (configuration.MakeBackup)
          yield return MakeBackup;

        if (configuration.WriteStatusFile)
          yield return WriteStatus;

        if (configuration.WriteStampFile)
          yield return WriteStamp;

        if (configuration.ProcessDebugSymbols)
          yield return ProcessDebugSymbols;

        if (!string.IsNullOrEmpty(configuration.InputFile))
          yield return FormatKeyValue(InputFile, configuration.InputFile);

        if (!string.IsNullOrEmpty(configuration.OutputFile))
          yield return FormatKeyValue(OutputFile, configuration.OutputFile);

        if (!string.IsNullOrEmpty(configuration.StrongNameKey))
          yield return FormatKeyValue(StrongNameKey, configuration.StrongNameKey);

        if (!string.IsNullOrEmpty(configuration.ProjectType))
          yield return FormatKeyValue(ProjectType, configuration.ProjectType);

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
    }

    private string projectId;
    private string inputFile;
    private string outputFile;
    private string strongNameKey;
    private string projectType;
    private bool makeBackup;
    private bool writeStatusFile;
    private bool writeStampFile;
    private bool processDebugSymbols;
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

    public string ProjectType
    {
      get { return projectType; }
      set { projectType = value; }
    }

    public bool MakeBackup
    {
      get { return makeBackup; }
      set { makeBackup = value; }
    }

    public bool ProcessDebugSymbols
    {
      get { return processDebugSymbols; }
      set { processDebugSymbols = value; }
    }

    public bool WriteStatusFile
    {
      get { return writeStatusFile; }
      set { writeStatusFile = value; }
    }

    public bool WriteStampFile
    {
      get { return writeStampFile; }
      set { writeStampFile = value; }
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