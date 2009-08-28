// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  public class AliasProvider
  {
    private readonly Dictionary<SqlTable, string> aliasTable = new Dictionary<SqlTable, string>(16);
    private readonly Set<string> aliasIndex = new Set<string>();
    private int counter;
    private byte prefixIndex;
    private byte suffix;

    // prefix is used for user defined aliases renaming, i.e. "alias" -> "ualias".
    // words[] should not contain the prefix.
    private static string userDefinedPrefix = "u";

    private static string[] prefixes =
      new[] {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "v",
        "w", "x", "y", "z"
      };

    internal void Reset()
    {
      aliasIndex.Clear();
      aliasTable.Clear();
      prefixIndex = 0;
      suffix = 0;
    }

    internal void Register(SqlTable table, string alias)
    {
      aliasTable[table] = alias;
      aliasIndex.Add(alias);
    }

    internal string GetAlias(SqlTable table)
    {
      string result;
      if (aliasTable.TryGetValue(table, out result))
        return result;

      var tableRef = table as SqlTableRef;
      if (tableRef!=null) {
        if (tableRef.Name!=tableRef.DataTable.Name) {
          result = ConvertTableName(tableRef.Name);
          if (aliasIndex.Contains(result))
            result = GenerateAlias();
        }
        else
          result = GenerateAlias();
      }
      else {
        if (!string.IsNullOrEmpty(table.Name))
          result = ConvertTableName(table.Name);
        else
          result = GenerateAlias();
      }

      Register(table, result);
      return result;
    }

    private static string ConvertTableName(string name)
    {
      return (name.StartsWith(userDefinedPrefix)) ? name : userDefinedPrefix + name;
    }

    private string GenerateAlias()
    {
      VerifyWordIndex();
      while (true) {
        string result = prefixes[prefixIndex++] + ((suffix > 0) ? suffix.ToString() : string.Empty);
        if (!aliasIndex.Contains(result))
          return result;
      }
    }

    private void VerifyWordIndex()
    {
      if (prefixIndex >= prefixes.Length) {
        prefixIndex = 0;
        suffix++;
      }
    }
  }
}