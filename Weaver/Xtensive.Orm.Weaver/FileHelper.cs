// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.IO;

namespace Xtensive.Orm.Weaver
{
  internal static class FileHelper
  {
    public static string GetDebugSymbolsFile(string file)
    {
      return Path.ChangeExtension(file, ".pdb");
    }

    public static string GetStatusFile(string file)
    {
      return file + ".weaver-status";
    }

    public static string GetStampFile(string file)
    {
      return file + ".weaver-stamp";
    }

    public static string GetBackupFile(string file)
    {
      var directory = Path.GetDirectoryName(file);
      var name = Path.GetFileName(file);
      return Path.Combine(directory, "BeforeWeaving", name);
    }

    public static string ExpandPath(string path)
    {
      return string.IsNullOrEmpty(path) || Path.IsPathRooted(path)
        ? path
        : Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    public static void CopyWithPdb(ProcessorContext context, string sourceFile, string targetFile)
    {
      var targetDirectory = Path.GetDirectoryName(targetFile);
      if (!Directory.Exists(targetDirectory))
        Directory.CreateDirectory(targetDirectory);
      File.Copy(sourceFile, targetFile, true);
      if (context.Configuration.ProcessDebugSymbols) {
        var sourcePdb = GetDebugSymbolsFile(sourceFile);
        var targetPdb = GetDebugSymbolsFile(targetFile);
        File.Copy(sourcePdb, targetPdb, true);
      }
    }

    public static void Touch(string file)
    {
      if (!File.Exists(file))
        File.OpenWrite(file).Dispose();
      File.SetLastWriteTimeUtc(file, DateTime.UtcNow);
    }
  }
}