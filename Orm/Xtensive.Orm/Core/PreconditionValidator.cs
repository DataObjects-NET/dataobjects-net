using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class PreconditionValidator
{
    [return: NotNull]
    public static T NotNull<T>(
        [NotNull] this T obj,
        string message = default,
        [CallerArgumentExpression("obj")]
        string parameterName = default)
        where T : class
    {
        return obj ?? throw new ArgumentNullException(parameterName, message);
    }

}