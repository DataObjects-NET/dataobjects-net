using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  public class SqlTableNameProvider
  {
    private readonly Dictionary<SqlTable, string> aliasTable = new Dictionary<SqlTable, string>(16);
    private readonly Set<string> aliasIndex = new Set<string>();
    private int counter;
    private byte prefixIndex;
    private byte suffix;
    private const string UserDefinedPrefix = "u";
    private readonly SqlCompilerContext context;

    // prefix is used for user defined aliases renaming, i.e. "alias" -> "ualias".
    // words[] should not contain the prefix.
    private static readonly string[] Prefixes =
      new[] {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "v",
        "w", "x", "y", "z"
      };

    internal string GetName(SqlTable table)
    {
      if ((context.NamingOptions & SqlCompilerNamingOptions.TableAliasing) == 0)
      return table.Name;

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

      aliasTable[table] = result;
      aliasIndex.Add(result);
      return result;
    }

    internal void Reset()
    {
      aliasIndex.Clear();
      aliasTable.Clear();
      prefixIndex = 0;
      suffix = 0;
    }

    private static string ConvertTableName(string name)
    {
      return (name.StartsWith(UserDefinedPrefix)) ? name : UserDefinedPrefix + name;
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