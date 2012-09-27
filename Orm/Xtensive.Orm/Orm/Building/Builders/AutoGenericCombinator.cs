using System;
using System.Collections.Generic;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class AutoGenericCombinator
  {
    private readonly Type typeDefinition;
    private readonly Type[][] candidateTypes;
    private readonly Type[] current;
    private readonly HashSet<Type> result;

    public static HashSet<Type> Generate(Type typeDefinition, Type[][] candidateTypes)
    {
      var combinator = new AutoGenericCombinator(typeDefinition, candidateTypes);
      combinator.Generate(0);
      return combinator.result;
    }

    private void Generate(int argumentPosition)
    {
      if (argumentPosition < current.Length - 1)
        foreach (var candidate in candidateTypes[argumentPosition]) {
          current[argumentPosition] = candidate;
          Generate(argumentPosition + 1);
        }
      else
        foreach (var candidate in candidateTypes[argumentPosition]) {
          current[argumentPosition] = candidate;
          result.Add(typeDefinition.MakeGenericType(current));
        }
    }

    private AutoGenericCombinator(Type typeDefinition, Type[][] candidateTypes)
    {
      current = new Type[candidateTypes.Length];
      result = new HashSet<Type>();
      this.typeDefinition = typeDefinition;
      this.candidateTypes = candidateTypes;
    }
  }
}