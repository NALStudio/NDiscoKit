using System.Text.Json.Serialization;

namespace NDiscoKit.PhilipsHue.Models.Clip.Generic;
public readonly struct HueLightOn : IEquatable<HueLightOn>
{
    public static readonly HueLightOn On = new(true);
    public static readonly HueLightOn Off = new(false);

    [JsonPropertyName("on")]
    public bool State { get; private init; }

    // JsonConstructor doesn't use the cached instances, but I can't do anything about it
    // as I can't create another constructor with the same arguments anyways...
    [JsonConstructor]
    private HueLightOn(bool state)
    {
        State = state;
    }

    public static explicit operator HueLightOn(bool state) => state ? On : Off;
    public static implicit operator bool(HueLightOn on) => on.State;

    public bool Equals(HueLightOn other) => State == other.State;
    public override bool Equals(object? obj) => obj is HueLightOn on && Equals(on);
    public static bool operator ==(HueLightOn left, HueLightOn right) => left.Equals(right);
    public static bool operator !=(HueLightOn left, HueLightOn right) => !left.Equals(right);
    public static bool operator !(HueLightOn on) => on.State ? Off : On;
    public override int GetHashCode() => State.GetHashCode();
}
