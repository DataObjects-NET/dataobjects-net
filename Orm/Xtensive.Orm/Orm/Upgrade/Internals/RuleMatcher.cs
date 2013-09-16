using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal abstract class RuleMatcher<T> where T:Node
  {
    private const string maskSymbol = "*";

    class NameMatcher : RuleMatcher<T>
    {
      private readonly string searchingName;
      private readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        return items.Where(el=>comparer.Compare(el.Name, searchingName)==0).ToList();
      }

      public NameMatcher(string name)
      {
        searchingName = name;
      }
    }

    class PatternMatcher : RuleMatcher<T>
    {
      private readonly Regex matcher;

      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        return items.Where(el => IsMatch(el.Name)).ToList();
      }

      private bool IsMatch(string value)
      {
        return matcher.IsMatch(value);
      }

      private string CreateRegexPattern(string pattern)
      {
        var substrings = pattern.Split(maskSymbol.ToCharArray());
        for (int i = 0; i<substrings.Length; i++) {
          substrings[i] = Regex.Escape(substrings[i]);
        }
        return string.Join(".*", substrings);
      }

      public PatternMatcher(string pattern)
      {
        matcher = new Regex(CreateRegexPattern(pattern));
      }
    }

    class AllMatcher : RuleMatcher<T>
    {
      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        return items.ToList();
      }
    }

    public abstract IEnumerable<T> Get(NodeCollection<T> items); 

    private static bool NameIsMasked(string name)
    {
      return (!string.IsNullOrEmpty(name)) && name.Contains(maskSymbol);
    }

    private static bool IsOnlyMaskSymbol(string name)
    {
      return (string.IsNullOrEmpty(name) || name==maskSymbol);
    }

    public static RuleMatcher<T> GetMatcher(string pattern)
    {
      if(IsOnlyMaskSymbol(pattern))
        return new AllMatcher();
      if(NameIsMasked(pattern))
        return new PatternMatcher(pattern);
      return new NameMatcher(pattern);
    }
  }
}
