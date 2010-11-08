using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ExpressionVisitor = Xtensive.Core.Linq.ExpressionVisitor;

namespace Xtensive.Storage.Linq.Rewriters
{
  /// <summary>
  /// Rewrites Visula Basic methods ('Microsoft.VisualBasic....') :
  /// 1. IIf(a,b,c) to a?b:c
  /// </summary>
  internal class VbRewriter : ExpressionVisitor
  {
    #if NET40
      private const string VbConversions = "Microsoft.VisualBasic.CompilerServices.Conversions, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string VbInteraction = "Microsoft.VisualBasic.Interaction, Microsoft.VisualBasic, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #else
      private const string VbConversions = "Microsoft.VisualBasic.CompilerServices.Conversions, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
      private const string VbInteraction = "Microsoft.VisualBasic.Interaction, Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
    #endif
    private static readonly Type conversionsType = Type.GetType(VbConversions, false);
    private static readonly Type interactionType = Type.GetType(VbInteraction, false);

    public static Expression Rewrite(Expression e)
    {
      return conversionsType!=null && interactionType!=null 
        ? new VbRewriter().Visit(e) 
        : e;
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType==interactionType)
        switch (mc.Method.Name) {
        case "IIf":
          return Expression.Condition(Visit(mc.Arguments[0]), Visit(mc.Arguments[1]), Visit(mc.Arguments[2]), mc.Type);
        }
      if (mc.Method.DeclaringType==conversionsType) {
        switch (mc.Method.Name) {
          case "ToBoolean":
          return Expression.Convert(Visit(mc.Arguments[0]), typeof(bool));
        }
      }
      return base.VisitMethodCall(mc);
    }

    private VbRewriter()
    {
      
    }
  }
}
