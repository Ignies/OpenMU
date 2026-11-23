// <copyright file="StarSmithRoundTrip.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.StarSmithing;

using System.Security.Cryptography;

/// <summary>
/// A smith's round trip, measured by the server (Readme.md "PR 6 contract", "Beat ping"): a ping with a token goes out
/// (one at a time, at most one per 2 seconds), the client echoes the token, and the time between on the server's clock
/// is a sample. An answer with another token or after 5 seconds is ignored. The round trip is the lowest of the last 3
/// samples (0 with none), so one slow answer never widens the beat window; the forge caps it again.
/// </summary>
public sealed class StarSmithRoundTrip
{
    /// <summary>The least time between two pings, in milliseconds.</summary>
    public const int PingIntervalMs = 2000;

    /// <summary>How long an answer counts, in milliseconds.</summary>
    public const int AnswerTimeoutMs = 5000;

    /// <summary>The samples kept.</summary>
    public const int Samples = 3;

    private readonly object _lock = new();
    private readonly Queue<int> _samples = new();
    private uint _nextToken = BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(sizeof(uint)));
    private uint? _token;
    private long _sentMs = long.MinValue / 2;

    /// <summary>Gets the round trip a forge start uses, in milliseconds; 0 with no sample.</summary>
    public int RoundTripMs
    {
        get
        {
            lock (this._lock)
            {
                return this._samples.Count == 0 ? 0 : this._samples.Min();
            }
        }
    }

    /// <summary>
    /// Starts a ping when one may go out: none is out (or it expired) and the last went out 2 seconds ago or more.
    /// </summary>
    /// <param name="nowMs">The server time.</param>
    /// <returns>The token to send, or null.</returns>
    public uint? Send(long nowMs)
    {
        lock (this._lock)
        {
            var expired = nowMs - this._sentMs > AnswerTimeoutMs;
            if ((this._token is not null && !expired) || nowMs - this._sentMs < PingIntervalMs)
            {
                return null;
            }

            this._token = unchecked(this._nextToken++);
            this._sentMs = nowMs;
            return this._token;
        }
    }

    /// <summary>
    /// Takes an answer: a sample when it carries the token out and comes within 5 seconds.
    /// </summary>
    /// <param name="token">The token it carries.</param>
    /// <param name="nowMs">The server time it arrived.</param>
    public void Answer(uint token, long nowMs)
    {
        lock (this._lock)
        {
            if (this._token != token)
            {
                return;
            }

            this._token = null;
            var elapsed = nowMs - this._sentMs;
            if (elapsed is < 0 or > AnswerTimeoutMs)
            {
                return;
            }

            this._samples.Enqueue((int)elapsed);
            while (this._samples.Count > Samples)
            {
                this._samples.Dequeue();
            }
        }
    }
}
