using Npgsql;
using System;
using System.Text;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.PostgreSql.Resources;

namespace Xtensive.Sql.PostgreSql
{
  internal static class ConnectionFactory
  {
    private static readonly string[] mTrueStrings = new[] {"t", "true", "1", "on", "yes", "y"};
    private static readonly string[] mFalseStrings = new[] {"f", "false", "0", "off", "no", "n"};
    
    private static readonly string mAllowedBoolStringsText;

    public static NpgsqlConnection CreateConnection(UrlInfo url)
    {
      var connectionString = BuildConnectionString(url);
      return new NpgsqlConnection(connectionString);
    }

    private static string BuildConnectionString(UrlInfo url)
    {
      SqlHelper.ValidateConnectionUrl(url);

      var csBuilder = new StringBuilder();

      if (url.Host!=string.Empty) {
        csBuilder.Append("Server=");
        csBuilder.Append(url.Host);
      }
      else
        throw new ArgumentException("URL doesn't contain Host specification.");

      if (url.Port!=0) {
        csBuilder.Append(";Port=");
        csBuilder.Append(url.Port);
      }

      if (url.User!=string.Empty && url.Password!=string.Empty) {
        csBuilder.Append(";User Id=");
        csBuilder.Append(url.User);
        csBuilder.Append(";Password=");
        csBuilder.Append(url.Password);
      }
      else
        throw new ArgumentException("URL doesn't contain User/Password specification.");

      if (url.Resource!=string.Empty) {
        csBuilder.Append(";Database=");
        csBuilder.Append(url.Resource);
      }
      else
        throw new ArgumentException("URL doesn't contain Database specification.");

      string protocolParameter;
      if (url.Params.TryGetValue("Protocol", out protocolParameter)) {
        if (protocolParameter!="3" && protocolParameter!="2")
          throw IncorrectUrl("Protocol", "2 or 3");
        csBuilder.AppendFormat(";Protocol={0}", protocolParameter);
      }

      string encodingParameter;
      if (url.Params.TryGetValue("Encoding", out encodingParameter)) {
        encodingParameter = encodingParameter.ToUpperInvariant();
        if (encodingParameter!="ASCII" && encodingParameter!="UNICODE")
          throw IncorrectUrl("Encoding", "ASCII or UNICODE.");
        csBuilder.AppendFormat(";Encoding={0}", encodingParameter);
      }

      string poolingParameter;
      if (url.Params.TryGetValue("Pooling", out poolingParameter)) {
        if (!IsBoolString(poolingParameter))
          throw IncorrectUrl("Pooling", mAllowedBoolStringsText);
        csBuilder.AppendFormat(";Pooling={0}", ToNpgsqlBooleanString(poolingParameter));

        // Reading pooling settings

        string minPoolSizeParameter;
        if (url.Params.TryGetValue("MinPoolSize", out minPoolSizeParameter)) {
          int minPoolSize;
          if (!int.TryParse(minPoolSizeParameter, out minPoolSize) || minPoolSize < 1)
            throw IncorrectUrl("MinPoolSize", "a positive integer");
          csBuilder.AppendFormat(";MinPoolSize={0}", minPoolSize);
        }

        string maxPoolSizeParameter;
        if (url.Params.TryGetValue("MaxPoolSize", out maxPoolSizeParameter)) {
          int maxPoolSize;
          if (!int.TryParse(maxPoolSizeParameter, out maxPoolSize) || maxPoolSize < 1)
            throw IncorrectUrl("MaxPoolSize", "a positive integer");
          csBuilder.AppendFormat(";MaxPoolSize={0}", maxPoolSize);
        }
      }
      
      string timeoutParameter;
      if (url.Params.TryGetValue("Timeout", out timeoutParameter)) {
        int timeout;
        if (!int.TryParse(timeoutParameter, out timeout) || timeout < 0)
          throw IncorrectUrl("Timeout", "a non-negative integer.");
        csBuilder.Append(";Timeout=");
        csBuilder.Append(timeout);
      }

      string sslParameter;
      if (url.Params.TryGetValue("SSL", out sslParameter)) {
        if (!IsBoolString(sslParameter))
          throw IncorrectUrl("SSL", mAllowedBoolStringsText);
        csBuilder.AppendFormat(";SSL={0}", ToNpgsqlBooleanString(sslParameter));
      }

      string sslModeParameter;
      if (url.Params.TryGetValue("Sslmode", out sslModeParameter)) {
        if (sslModeParameter!="Prefer" && sslModeParameter!="Require" && sslModeParameter!="Allow" && sslModeParameter!="Disable")
          throw IncorrectUrl("Sslmode", "Prefer, Require, Allow, or Disable.");
        csBuilder.AppendFormat(";Sslmode={0}", sslModeParameter);
      }

      string preloadReaderParameter;
      if (url.Params.TryGetValue("Preload Reader", out preloadReaderParameter)) {
        csBuilder.AppendFormat(";Preload Reader={0}", preloadReaderParameter);
      }

      csBuilder.Append(";");

      return csBuilder.ToString();
    }

    private static bool IsBoolString(string s)
    {
      return IsFalseString(s) || IsTrueString(s);
    }

    private static bool IsTrueString(string s)
    {
      return mTrueStrings.Contains(s.ToLowerInvariant());
    }

    private static bool IsFalseString(string s)
    {
      return mFalseStrings.Contains(s.ToLowerInvariant());
    }

    private static string ToNpgsqlBooleanString(string s)
    {
      if (IsTrueString(s))
        return "True";
      if (IsFalseString(s))
        return "False";
      throw new ArgumentException("Invalid value: " + s, "s");
    }

    private static ArgumentException IncorrectUrl(string parameterName, string expectedValue)
    {
      return new ArgumentException(string.Format(
        Strings.ExUrlContainsInvalidXSpecificationXHasToBeY, parameterName, expectedValue));
    }

    // Type initializer

    static ConnectionFactory()
    {
      mAllowedBoolStringsText =
        string.Format("{0} or {1}", string.Join("/", mTrueStrings), string.Join("/", mFalseStrings));
    }
  }
}