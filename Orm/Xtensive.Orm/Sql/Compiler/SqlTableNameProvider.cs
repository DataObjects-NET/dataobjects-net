// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Table name provider.
  /// </summary>
  public class SqlTableNameProvider
  {
    private readonly Dictionary<SqlTable, string> aliasMap = new Dictionary<SqlTable, string>(16);
    private readonly HashSet<string> aliasIndex = new HashSet<string>();
    private byte prefixIndex;
    private byte suffix;
    private readonly SqlCompilerContext context;

    private static readonly string[] Prefixes =
      new[] {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v",
        "w", "x", "y", "z"
      };

    public string GetName(SqlTable table)
    {
      if ((context.NamingOptions & SqlCompilerNamingOptions.TableAliasing) == 0) {
        aliasIndex.Add(table.Name);
        aliasMap[table] = table.Name;
        return table.Name;
      }

      string result;
      if (aliasMap.TryGetValue(table, out result))
        return result;

      var tableRef = table as SqlTableRef;
      // Table reference
      if (tableRef!=null) {
        // Alias
        if (tableRef.Name!=tableRef.DataTable.Name) {
          result = tableRef.Name;
          if (aliasIndex.Contains(result))
            result = GenerateAlias();
        }
        else
          result = GenerateAlias();
      }
      // Table
      else {
        if (!string.IsNullOrEmpty(table.Name)) {
          result = table.Name;
          if (aliasIndex.Contains(result))
            result = GenerateAlias();
        }
        else
          result = GenerateAlias();
      }

      aliasMap[table] = result;
      aliasIndex.Add(result);
      return result;
    }

    internal void Reset()
    {
      aliasIndex.Clear();
      aliasMap.Clear();
      prefixIndex = 0;
      suffix = 0;
    }

    private string GenerateAlias()
    {
      UpdateWordIndex();
      while (true) {
        string result = Prefixes[prefixIndex++] + ((suffix > 0) ? suffix.ToString() : string.Empty);
        if (!aliasIndex.Contains(result))
          return result;
      }
    }

    private void UpdateWordIndex()
    {
      if (prefixIndex < Prefixes.Length)
        return;

      prefixIndex = 0;
      suffix++;
    }


    // Constructor

    public SqlTableNameProvider(SqlCompilerContext context)
    {
      this.context = context;
    }
  }
}
