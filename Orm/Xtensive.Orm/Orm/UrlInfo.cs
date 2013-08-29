// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Holds an URL and provides easy access to its different parts.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The common URL format that would be converted 
  /// to the <see cref="UrlInfo"/> can be represented 
  /// in the BNF form as following:
  /// <code lang="BNF" outline="true">
  /// url ::= protocol://[user[:password]@]host[:port]/resource[?parameters]
  /// protocol ::= alphanumx[protocol]
  /// user ::= alphanumx[user]
  /// password ::= alphanumx[password]
  /// host ::= hostname | hostnum
  /// port ::= digits
  /// resource ::= name
  /// parameters ::= parameter[&amp;parameter]
  /// 
  /// hostname ::= name[.hostname]
  /// hostnum ::= digits.digits.digits.digits
  /// 
  /// parameter ::= name=[name]
  /// 
  /// name ::= alpanumx[name]
  /// 
  /// digits ::= digit[digits]
  /// alphanumx ::= alphanum | escape | $ | - | _ | . | + | ! | * | " | ' | ( | ) | , | ; | # | space
  /// alphanum ::= alpha | digit
  /// escape ::= % hex hex
  /// hex ::= digit | a | b | c | d | e | f | A | B | C | D | E | F
  /// digit ::= 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
  /// alpha ::= /* represents any unicode alpa character */
  /// </code>
  /// </para>
  /// <note>
  /// This not fully precise notation because it slightly simplified to be shorter.
  /// But it almost completely reflects <see cref="UrlInfo"/> URL parser
  /// capabilities.
  /// </note>
  /// <para>
  /// Here you can see several valid URL samples:
  /// <pre>
  /// tcp://localhost/
  /// tcp://server:40000/myResource
  /// tcp://admin:admin@localhost:40000/myResource?askTimeout=60
  /// </pre>
  /// </para>
  /// </remarks>
  [Serializable]
  [DebuggerDisplay("{url}")]
  [TypeConverter(typeof(UrlInfoConverter))]
  public class UrlInfo : 
    IEquatable<UrlInfo>,
    IComparable<UrlInfo>,
    ISerializable
  {
    private static readonly Regex Pattern = new Regex(
          @"^(?'proto'[^:]*)://" +
          @"((?'username'[^:@]*)" +
          @"(:(?'password'[^@]*))?@)?" +
          @"(?'host'[^:/]*)" +
          @"(:(?'port'\d+))?" +
          @"/(?'resource'[^?]*)?" +
          @"(\?(?'params'.*))?", 
          RegexOptions.Compiled|RegexOptions.Singleline);

    private string url = string.Empty;
    private string protocol = string.Empty;
    private string host = string.Empty;
    private int    port;
    private string resource = string.Empty;
    private string user = string.Empty;
    private string password = string.Empty;
    private ReadOnlyDictionary<string, string> parameters;

    #region Properties: Url, Protocol, Host, etc...

    /// <summary>
    /// Gets an URL this instance describes.
    /// </summary>
    public string Url
    {
      [DebuggerStepThrough]
      get { return url; }
    }

    /// <summary>
    /// Gets the protocol part of the current <see cref="Url"/>
    /// (e.g. <b>"tcp"</b> is the protocol part of the "<b>tcp</b>://admin:password@localhost/resource" URL).
    /// </summary>
    public string Protocol
    {
      [DebuggerStepThrough]
      get { return protocol; }
    }

    /// <summary>
    /// Gets the host part of the current <see cref="Url"/>
    /// (e.g. <b>"localhost"</b> is the host part of the "tcp://admin:password@<b>localhost</b>/resource" URL).
    /// </summary>
    public string Host
    {
      [DebuggerStepThrough]
      get { return host; }
    }

    /// <summary>
    /// Gets the port part of the current <see cref="Url"/>
    /// (e.g. <b>40000</b> is the port part of the "tcp://admin:password@localhost:<b>40000</b>/resource" URL).
    /// </summary>
    public int Port
    {
      [DebuggerStepThrough]
      get { return port; }
    }

    /// <summary>
    /// Gets the resource name part of the current <see cref="Url"/>
    /// (e.g. <b>"resource"</b> is the resource name part of the "tcp://admin:password@localhost/<b>resource</b>" URL).
    /// </summary>
    public string Resource
    {
      [DebuggerStepThrough]
      get { return resource; }
    }

    /// <summary>
    /// Gets the user name part of the current <see cref="Url"/>
    /// (e.g. <b>"admin"</b> is the user name part of the "tcp://<b>admin</b>:password@localhost/resource" URL).
    /// </summary>
    public string User
    {
      [DebuggerStepThrough]
      get { return user; }
    }

    /// <summary>
    /// Gets the password part of the current <see cref="Url"/>
    /// (e.g. <b>"password"</b> is the password part of the "tcp://admin:<b>password</b>@localhost/resource" URL).
    /// </summary>
    public string Password
    {
      [DebuggerStepThrough]
      get { return password; }
    }

    /// <summary>
    /// Gets additional parameters of the current <see cref="Url"/>
    /// (e.g. <b>"param1=value1&amp;param2=value2"</b> is the additional parameters part
    /// of the "tcp://admin:password@localhost/resource?<b>param1=value1&amp;param2=value2</b>" URL).
    /// </summary>
    /// <remarks>
    /// <para>The mentioned part of the <see cref="Url"/> is parsed
    /// and represented in a <see cref="Dictionary{String,String}"/> form.</para>
    /// </remarks>
    public ReadOnlyDictionary<string, string> Params
    {
      [DebuggerStepThrough]
      get { return parameters; }
    }

    #endregion

    /// <summary>
    /// Splits URL into parts (protocol, host, port, resource, user, password) and set all
    /// derived values to the corresponding properties of the instance.
    /// </summary>
    /// <param name="url">URL to split</param>
    /// <remarks>
    /// The expected URL format is as the following:
    /// proto://[[user[:password]@]host[:port]]/resource.
    /// Note that the empty URL will cause an exception.
    /// </remarks>
    /// <exception cref="ArgumentException">Specified <paramref name="url"/> is invalid (cannot be parsed).</exception>
    public static UrlInfo Parse(string url)
    {
      var result = new UrlInfo();
      Parse(url, result);
      return result;
    }

    private static void Parse(string url, UrlInfo info)
    {
      try {
        string tUrl = url;
        if (tUrl.Length==0)
          tUrl = ":///";

        var result = Pattern.Match(tUrl);
        if (!result.Success)
          throw Exceptions.InvalidUrl(url, "url");

        int @port = 0;

        if (result.Result("${port}").Length!=0)
          @port = int.Parse(result.Result("${port}"));
        if (@port<0 || @port>65535)
          throw Exceptions.InvalidUrl(url, "port");

        string tParams = result.Result("${params}");
        string[] aParams = tParams.Split('&');
        var @params = new Dictionary<string, string>();
        if (tParams!=string.Empty) {
          foreach (string sPair in aParams) {
            string[] aNameValue = sPair.Split(new char[] {'='}, 2);
            if (aNameValue.Length!=2)
              throw Exceptions.InvalidUrl(url, "parameters");
            @params.Add(UrlDecode(aNameValue[0]), UrlDecode(aNameValue[1]));
          }
        }

        info.url = url;
        info.user = UrlDecode(result.Result("${username}"));
        info.password = UrlDecode(result.Result("${password}"));
        info.resource = UrlDecode(result.Result("${resource}"));
        info.host = UrlDecode(result.Result("${host}"));
        info.protocol = UrlDecode(result.Result("${proto}"));
        info.port = @port;
        info.parameters = new ReadOnlyDictionary<string, string>(@params);
      }
      catch (Exception e) {
        if (e is ArgumentException || e is InvalidOperationException)
          throw;
        else
          throw Exceptions.InvalidUrl(url, "url");
      }
    }

    #region Nested type: UrlDecoder

    private class UrlDecoder
    {
      // Fields
      private int m_bufferSize;
      private byte[] m_byteBuffer;
      private char[] m_charBuffer;
      private Encoding m_encoding;
      private int m_numBytes;
      private int m_numChars;

      // Methods
      internal UrlDecoder(int bufferSize, Encoding encoding)
      {
        m_bufferSize = bufferSize;
        m_encoding = encoding;
        m_charBuffer = new char[bufferSize];
      }

      internal void AddByte(byte b)
      {
        if (m_byteBuffer==null)
          m_byteBuffer = new byte[m_bufferSize];
        m_byteBuffer[m_numBytes++] = b;
      }

      internal void AddChar(char ch)
      {
        if (m_numBytes>0)
          FlushBytes();
        m_charBuffer[m_numChars++] = ch;
      }

      private void FlushBytes()
      {
        if (m_numBytes>0) {
          m_numChars += m_encoding.GetChars(m_byteBuffer, 0, m_numBytes, m_charBuffer, m_numChars);
          m_numBytes = 0;
        }
      }

      internal string GetString()
      {
        if (m_numBytes>0)
          FlushBytes();
        if (m_numChars>0)
          return new string(m_charBuffer, 0, m_numChars);
        return string.Empty;
      }
    }

    #endregion

    #region Private \ internal methods

    private static string UrlDecode(string str)
    {
      return UrlDecode(str, Encoding.UTF8);
    }

    private static string UrlDecode(string s, Encoding e)
    {
      int len = s.Length;
      UrlDecoder decoder = new UrlDecoder(len, e);
      for (int i = 0; i<len; i++) {
        char c = s[i];
        if (c=='+') {
          c = ' ';
        }
        else if (c=='%' && i<(len-2)) {
          if (s[i+1]=='u' && i<(len-5)) {
            int num3 = HexToInt(s[i+2]);
            int num4 = HexToInt(s[i+3]);
            int num5 = HexToInt(s[i+4]);
            int num6 = HexToInt(s[i+5]);
            if ((num3<0 || num4<0) || (num5<0 || num6<0))
              goto loc_1;
            c = (char)((ushort)((((num3 << 12)|(num4 << 8))|(num5 << 4))|num6));
            i += 5;
            decoder.AddChar(c);
            continue;
          }
          int num7 = HexToInt(s[i+1]);
          int num8 = HexToInt(s[i+2]);
          if (num7>=0 && num8>=0) {
            byte num9 = (byte)((num7 << 4)|num8);
            i += 2;
            decoder.AddByte(num9);
            continue;
          }
        }
        loc_1:
        if ((c&0xff80)=='\0')
          decoder.AddByte((byte)c);
        else
          decoder.AddChar(c);
      }
      return decoder.GetString().Trim();
    }

    private static int HexToInt(char h)
    {
      if (h>='0' && h<='9')
        return h-'0';
      if (h<'a' || h>'f') {
        if (h>='A' && h<='F')
          return h-'A'+'\n';
        return -1;
      }
      return h-'a'+'\n';
    }

    #endregion

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(UrlInfo other)
    {
      return AdvancedComparerStruct<string>.System.Equals(url, other.url);
    }

    /// <inheritdoc/>
    public int CompareTo(UrlInfo other)
    {
      return AdvancedComparerStruct<string>.System.Compare(url, other.url);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (UrlInfo))
        return false;
      return Equals((UrlInfo) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (url!=null ? url.GetHashCode() : 0);
    }

    /// <see cref="ClassDocTemplate.OperatorEq"/>
    public static bool operator ==(UrlInfo left, UrlInfo right)
    {
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(UrlInfo left, UrlInfo right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return url;
    }


    // Constructors

    private UrlInfo()
    {
    }

    #region ISerializable members, deserializing constructor

    ///<summary>
    /// Deserilizing constructor.
    ///</summary>
    /// <param name="context">The source (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this deserialization. </param>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate the data from. </param>
    protected UrlInfo(SerializationInfo info, StreamingContext context)
    {
      Parse(info.GetString("Url"), this);
    }

    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization. </param>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data. </param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Url", url);
    }

    #endregion
  }
}