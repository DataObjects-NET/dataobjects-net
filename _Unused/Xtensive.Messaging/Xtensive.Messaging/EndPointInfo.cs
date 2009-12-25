// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Holds end-point URL and provides easy access to its different parts.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Please see <see cref="UrlInfo"/> class for complete description of URL format.
  /// </para>
  /// <para>
  /// Here you can see several valid URL samples:
  /// <pre>
  /// tcp://localhost/
  /// tcp://server:40000\myReceiver
  /// tcp://admin:admin@localhost:40000/myReceiver?askTimeout=60
  /// </pre>
  /// </para>
  /// </remarks>
  [Serializable]
  public sealed class EndPointInfo: UrlInfo
  {
    /// <summary>
    /// Gets the receiver (<see cref="UrlInfo.Resource"/>) name part of the current <see cref="UrlInfo.Url"/>
    /// (e.g. <b>"receiver"</b> is the receiver name part of the "tcp://admin:password@localhost/<b>receiver</b>" URL).
    /// </summary>
    public string Receiver
    {
      get { return Resource; }
    }

    //    /// <summary>
    //    /// Splits URL into parts (protocol, host, port, receiver, user, password) and set all
    //    /// derived values to the corresponding properties of the instance.
    //    /// </summary>
    //    /// <param name="url">URL to split</param>
    //    /// <remarks>
    //    /// The expected URL format is as the following:
    //    /// proto://[[user[:password]@]host[:port]]/receiver.
    //    /// Note that the empty URL would cause an exception.
    //    /// </remarks>
    //    protected override void ParseUrl(string url)
    //    {
    //      // Does nothing new for now; but in future it should probably
    //      // add properties for some paramebers, e.g. timeouts.
    //      base.ParseUrl(url);
    //    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="url">Initial <see cref="UrlInfo.Url"/> property value.</param>
    public EndPointInfo(string url)
      : base(url)
    {
    }

    ///<summary>
    /// Deserializing constructor.
    ///</summary>
    /// <param name="context">The source (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this deserialization. </param>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate the data from. </param>
    private EndPointInfo(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}