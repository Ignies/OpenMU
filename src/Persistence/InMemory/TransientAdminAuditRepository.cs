// <copyright file="TransientAdminAuditRepository.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.InMemory;

using System.Threading;
using MUnique.OpenMU.Persistence.AdminAuth;

/// <summary>
/// The admin audit of an in-memory (demo) install: kept while the process runs, at most <see cref="Capacity"/> entries,
/// so the history and revert of the admin operations work without a database.
/// </summary>
public sealed class TransientAdminAuditRepository : IAdminAuditRepository
{
    /// <summary>The most entries kept; the oldest go first.</summary>
    public const int Capacity = 5000;

    private readonly List<AdminAuditEntry> _entries = [];

    /// <inheritdoc />
    public ValueTask AddAsync(AdminAuditEntry entry, CancellationToken cancellationToken = default)
    {
        lock (this._entries)
        {
            this._entries.Add(entry);
            if (this._entries.Count > Capacity)
            {
                this._entries.RemoveAt(0);
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask CompleteAsync(Guid id, string outcome, string resultJson, CancellationToken cancellationToken = default)
    {
        lock (this._entries)
        {
            if (this._entries.FirstOrDefault(e => e.Id == id) is { } entry)
            {
                entry.Outcome = outcome;
                entry.ResultJson = resultJson;
                entry.CompletedAt = DateTime.UtcNow;
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<AdminAuditEntry?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        lock (this._entries)
        {
            return ValueTask.FromResult(this._entries.FirstOrDefault(e => e.Id == id));
        }
    }

    /// <inheritdoc />
    public ValueTask<(IReadOnlyList<AdminAuditEntry> Items, int Total)> ReadAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        lock (this._entries)
        {
            return ValueTask.FromResult<(IReadOnlyList<AdminAuditEntry>, int)>((this._entries.OrderByDescending(e => e.StartedAt).Skip(offset).Take(limit).ToArray(), this._entries.Count));
        }
    }
}
