// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System.IO;

namespace Xtensive.Orm.Weaver
{
  internal static class FileUtility
  {
    public static string GetDebugSymbolsFile(string file)
    {
      return Path.ChangeExtension(file, ".pdb");
    }
  }
}