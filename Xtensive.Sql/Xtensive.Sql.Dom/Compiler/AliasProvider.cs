// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Compiler
{
  public class AliasProvider
  {
    private Dictionary<SqlTable, string> aliasTable = new Dictionary<SqlTable, string>(16);
    private Dictionary<SqlTable, string> nameTable = new Dictionary<SqlTable, string>(16);
    private Set<string> aliasIndex = new Set<string>();
    private bool enabled;
    private byte prefixIndex;
    private byte suffix;

    // prefix is used for user defined aliases renaming, i.e. "alias" -> "ualias".
    // words[] should not contain the prefix.
    private static string userDefinedPrefix = "u";

    private static string[] prefixes =
      new string[]
        {
          "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "v",
          "w", "x", "y", "z"
        };

    public bool Enabled
    {
      get { return enabled; }
      set { enabled = value; }
    }

    internal void Reset()
    {
      aliasIndex.Clear();
      aliasTable.Clear();
      nameTable.Clear();
      prefixIndex = 0;
      suffix = 0;
    }

    internal void Substitute(SqlTable table)
    {
      if (aliasTable.ContainsKey(table))
        return;

      string alias;

      SqlTableRef tableRef = table as SqlTableRef;
      if (tableRef != null) {
        if (enabled) {
          if (tableRef.Name != tableRef.DataTable.Name) {
            alias = ConvertTableName(tableRef.Name);
            if (aliasIndex.Contains(alias))
              alias = GenerateAlias();
          }
          else
            alias = GenerateAlias();
        }
        else
          alias = tableRef.DataTable.Name;
      }
      else {
        if (enabled) {
          if(!string.IsNullOrEmpty(table.Name))
            alias = ConvertTableName(table.Name);
          else
            alias = GenerateAlias();
        }
        else
          alias = string.Empty;
      }

//      if (enabled) {
//        if ((tableRef != null && tableRef.Name != tableRef.DataTable.Name && !aliasIndex.Contains(tableRef.Name)) || (tableRef == null && !string.IsNullOrEmpty(table.Name)))
//          alias = ConvertTableName(table.Name);
//        else
//          alias = GenerateAlias();
//      }
//      else
//        alias = (tableRef != null) ? tableRef.DataTable.Name : string.Empty;

      aliasTable[table] = alias;
      nameTable[table] = table.Name;
      aliasIndex.Add(alias);
      table.Name = alias;
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

    internal void Restore()
    {
      foreach (SqlTable t in nameTable.Keys)
        t.Name = nameTable[t];

      Reset();
    }
  }
}