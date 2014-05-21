// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Weaver
{
  public sealed class ProcessorConfiguration
  {
    private static class Parser
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

      private static bool ParseKeyValue(string item, out string key, out string value)
      {
        var position = item.IndexOfAny(KeyValueSeparators);
        if (position < 0 || position > item.Length - 2) {
          key = null;
          value = null;
          return false;
        }
        key = item.Substring(0, position);
        value = item.Substring(position + 1);
        return true;
      }
    }

    private string inputFile;
    private string outputFile;
    private string strongNameKey;
    private string projectType;
    private bool makeBackup;
    private bool writeStatusFile;
    private bool writeStampFile;
    private bool processDebugSymbols;
    private IList<string> referencedAssemblies;

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

    public bool Parse(string parameterString)
    {
      if (parameterString==null)
        throw new ArgumentNullException("parameterString");
      return Parser.Parse(this, parameterString);
    }

    public ProcessorConfiguration()
    {
      ReferencedAssemblies = new List<string>();
    }
  }
}