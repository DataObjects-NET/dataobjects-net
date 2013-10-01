// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.20

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal abstract class RuleMatcher<T>
    where T : Node
  {
    private class NameMatcher : RuleMatcher<T>
    {
      private readonly string itemName;

      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        var result = items[itemName];
        if (result!=null)
          yield return result;
      }

      public NameMatcher(string itemName)
      {
        this.itemName = itemName;
      }
    }

    private class PatternMatcher : RuleMatcher<T>
    {
      private readonly Regex matcher;

      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        return items.Where(item => matcher.IsMatch(item.Name)).ToList();
      }

      private string CreateRegexPattern(string pattern)
      {
        var items = pattern.Split(MatchingHelper.WildcardSymbol.ToCharArray());
        for (int i = 0; i < items.Length; i++)
          items[i] = Regex.Escape(items[i]);
        return string.Format("^{0}$", string.Join(".*", items));
      }

      public PatternMatcher(string pattern)
      {
        matcher = new Regex(CreateRegexPattern(pattern), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
      }
    }

    private class AllMatcher : RuleMatcher<T>
    {
      public override IEnumerable<T> Get(NodeCollection<T> items)
      {
        return items.ToList();
      }
    }

    public abstract IEnumerable<T> Get(NodeCollection<T> items);

    public static RuleMatcher<T> Create(string pattern)
    {
      if (MatchingHelper.IsMatchAll(pattern))
        return new AllMatcher();
      if (MatchingHelper.ContainsWildcardSymbols(pattern))
        return new PatternMatcher(pattern);
      return new NameMatcher(pattern);
    }
  }
}
