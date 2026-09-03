using System;

namespace RoadRage.Shared.Definitions
{
    /// <summary>
    /// Identifiant stable pour les definitions statiques chargees au bootstrap.
    /// </summary>
    [Serializable]
    public readonly struct DefinitionId : IEquatable<DefinitionId>
    {
        public DefinitionId(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(Value); }
        }

        public bool Equals(DefinitionId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DefinitionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(DefinitionId left, DefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DefinitionId left, DefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}
