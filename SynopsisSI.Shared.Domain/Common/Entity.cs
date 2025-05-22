using System;
using System.Collections.Generic; // Req EqualityComparer

namespace SynopsisSI.Shared.Domain.Common;
public abstract class Entity<TId> where TId : IEquatable<TId>
{
    public TId Id { get; protected set; }

    // public DateTime CreatedAt { get; protected set; }
    // public string? CreatedBy { get; protected set; } // User ID or system
    // public DateTime? LastModifiedAt { get; protected set; }
    // public string? LastModifiedBy { get; protected set; }

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default(TId)!)) 
        {
            throw new ArgumentException("Entity ID cannot be the default value.", nameof(id));
        }
        Id = id;
    }

    // Parameterless constructor for EF Cos
    protected Entity() {
        // Initialize Id if it's a reference type that could be null, e.g. string
        if (typeof(TId) == typeof(string) && Id == null)
        {
            // might not be needed if Id is always set in constructor or by DB
            // For Guid strings,  better to initialize in derived classes or set explicitly
        }
     }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetRealType() != other.GetRealType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default(TId)!) || EqualityComparer<TId>.Default.Equals(other.Id, default(TId)!))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetRealType().ToString() + Id?.ToString()).GetHashCode(); // Added null conditional for Id
    }

    private Type GetRealType()
    {
        Type type = GetType();
        if (type.ToString().Contains("Castle.Proxies.")) // Handle EF Core proxies
            return type.BaseType ?? type;
        return type;
    }
}
