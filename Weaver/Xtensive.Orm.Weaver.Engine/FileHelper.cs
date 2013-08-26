// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System.IO;

namespace Xtensive.Orm.Weaver
{
  internal static class FileHelper
  {
    public static string GetDebugSymbolsFile(string targetFile)
    {
      return Path.ChangeExtension(targetFile, ".pdb");
    }

    public static string GetStatusFile(string targetFile)
    {
      return targetFile + ".weaver-status";
    }

    public static string GetStampFile(string targetFile)
    {
      return targetFile + ".weaver-stamp";
    }
  }
}