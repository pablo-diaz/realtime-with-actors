using System;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Domain;

public class Temperature: ValueObject
{
    public decimal Value { get; }

    private Temperature(decimal value)
    {
        Value = value;
    }

    public static Result<Temperature> For(decimal value)
    {
        if(value < -100 || value > 100)
            return Result.Failure<Temperature>($"Wrong temperature {value}");

        return new Temperature(value: value);
    }

    internal bool IsSimilar(Temperature to, decimal belowSimilarityThreshold) =>
        Math.Abs(Value - to.Value) <= belowSimilarityThreshold;

    public static bool operator < (Temperature x, Temperature y) => x.Value < y.Value;

    public static bool operator > (Temperature x, Temperature y) => x.Value > y.Value;

    public static explicit operator Temperature(decimal value) => For(value).Value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return this.Value;
    }

    public override string ToString() => $"Temperature: {Value}";

}