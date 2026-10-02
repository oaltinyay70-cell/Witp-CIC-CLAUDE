using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WitpCic.Core.Analytics.EntityResolution;

public enum EntityType
{
    SurfaceShip,
    Submarine,
    TaskForce,
    AirUnit,
    GroundUnit,
    Base
}

public sealed record ResolvedEntity(
    Guid Id,
    string CanonicalName,
    EntityType Type,
    IReadOnlySet<string> KnownAliases
);

/// <summary>
/// Unifies fragmented and noisy entity references across operational log lines into canonical IDs.
/// </summary>
public sealed partial class EntityResolutionService
{
    private readonly ConcurrentDictionary<string, Guid> _aliasToId = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, (string CanonicalName, EntityType Type, HashSet<string> Aliases)> _entities = new();

    [GeneratedRegex(@"\b(xAKL|xAK|xAP|APD|AP|AK|DD|CA|BB|SS|CMc|AVD|TF|TK|Sqn|Regiment|Battalion|Corps|Group Army)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex PrefixNoiseRegex();

    [GeneratedRegex(@"\(TF\s*\d+\)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ParenthesizedTfRegex();

    /// <summary>
    /// Normalizes raw entity text by stripping extraneous prefixes and noise.
    /// </summary>
    public static string NormalizeName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName)) return string.Empty;

        // Remove (TF xxx)
        string cleaned = ParenthesizedTfRegex().Replace(rawName, " ");
        
        // Trim and collapse whitespace
        cleaned = Regex.Replace(cleaned.Trim(), @"\s+", " ");
        return cleaned;
    }

    /// <summary>
    /// Resolves or registers an entity from a raw log mention.
    /// </summary>
    public Guid ResolveOrCreate(string rawName, EntityType defaultType = EntityType.SurfaceShip)
    {
        string normalized = NormalizeName(rawName);
        if (string.IsNullOrEmpty(normalized)) normalized = rawName.Trim();

        // 1. Direct alias check
        if (_aliasToId.TryGetValue(normalized, out var existingId))
        {
            return existingId;
        }

        // 2. Check un-prefixed core name (e.g. "Legazpi" for "xAKL Legazpi")
        string coreName = PrefixNoiseRegex().Replace(normalized, "").Trim();
        if (!string.IsNullOrEmpty(coreName) && _aliasToId.TryGetValue(coreName, out var coreId))
        {
            RegisterAlias(coreId, normalized);
            RegisterAlias(coreId, rawName);
            return coreId;
        }

        // 3. Register new canonical entity
        var newId = Guid.NewGuid();
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rawName, normalized };
        if (!string.IsNullOrEmpty(coreName)) aliases.Add(coreName);

        _entities[newId] = (normalized, defaultType, aliases);
        _aliasToId[normalized] = newId;
        _aliasToId[rawName] = newId;
        if (!string.IsNullOrEmpty(coreName)) _aliasToId[coreName] = newId;

        return newId;
    }

    public void RegisterAlias(Guid entityId, string alias)
    {
        if (string.IsNullOrWhiteSpace(alias)) return;
        if (_entities.TryGetValue(entityId, out var entityData))
        {
            lock (entityData.Aliases)
            {
                entityData.Aliases.Add(alias);
                _aliasToId[alias] = entityId;
            }
        }
    }

    public ResolvedEntity? GetEntity(Guid entityId)
    {
        if (_entities.TryGetValue(entityId, out var data))
        {
            return new ResolvedEntity(entityId, data.CanonicalName, data.Type, data.Aliases);
        }
        return null;
    }

    public bool TryGetId(string alias, out Guid id)
    {
        string normalized = NormalizeName(alias);
        return _aliasToId.TryGetValue(normalized, out id) || _aliasToId.TryGetValue(alias, out id);
    }
}
