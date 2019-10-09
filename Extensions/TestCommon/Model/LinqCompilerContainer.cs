using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm;

namespace TestCommon.Model
{
  [CompilerContainer(typeof (Expression))]
  public static class LinqCompilerContainer
  {
    [Compiler(typeof (Foo), "Name1", TargetKind.PropertyGet)]
    public static Expression Text1(Expression expression)
    {
      Expression<Func<Foo, string>> ex = p => p.Name + "test";
      return ex.BindParameters(expression);
    }
  }
}