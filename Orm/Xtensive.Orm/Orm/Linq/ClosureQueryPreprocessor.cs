// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2019.03.15

using System.Linq.Expressions;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Linq.Rewriters;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// Replases access to queries in closure with actual expression form closured query.
  /// <remarks>
  /// <para>Register the processor to <see cref="DomainConfiguration.Types"/> and make your implementation of <see cref="IQueryPreprocessor2"/>, <see cref="IQueryPreprocessor"/> or <see cref="QueryPreprocessor"/>
  /// from this preprocessor to have expression with processed closure access.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code>
  /// [Service(typeof (IQueryPreprocessor))]
  /// public class CustomPreprocessor : QueryPreprocessor
  /// {
  ///   public override Expression Apply(Session session, Expression query)
  ///   {
  ///     //some query processing
  ///   }

  ///   public override bool IsDependentOn(IQueryPreprocessor other)
  ///   {
  ///     if (other is ClosureQueryPreprocessor)
  ///       return true;
  ///     // some other checks
  ///   }
  /// }
  /// </code>
  /// </example>
  /// </summary>
  [Service(typeof(IQueryPreprocessor))]
  public sealed class ClosureQueryPreprocessor : QueryPreprocessor
  {
    public override Expression Apply(Session session, Expression query)
    {
      return ClosureAccessRewriter.Rewrite(query);
    }

    public override bool IsDependentOn(IQueryPreprocessor other)
    {
      return false;
    }
  }
}
