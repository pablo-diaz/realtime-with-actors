using System;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Domain;

public class Coords: ValueObject
{
    public decimal Latitude { get; }
    public decimal Longitude { get; }

    private Coords(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Result<Coords> For(decimal latitude, decimal longitude)
    {
        if(latitude < -90 || latitude > 90)
            return Result.Failure<Coords>($"Wrong latitude value '{latitude}'. Please specify a valid value");

        if(longitude < -180 || longitude > 180)
            return Result.Failure<Coords>($"Wrong longitude value '{longitude}'. Please specify a valid value");

        return new Coords(latitude: latitude, longitude: longitude);
    }

    public decimal GetDistanceInKm(Coords to)
    {
        // Haversine impl

        decimal theta = this.Longitude - to.Longitude;

        double distance = 60 * 1.1515 * (180/Math.PI) * Math.Acos(
            Math.Sin((double)this.Latitude * (Math.PI/180)) * Math.Sin((double)to.Latitude * (Math.PI/180)) + 
            Math.Cos((double)this.Latitude * (Math.PI/180)) * Math.Cos((double)to.Latitude * (Math.PI/180)) *
            Math.Cos((double)theta * (Math.PI/180))
        );

        return (decimal) Math.Round(distance * 1.609344, 2);
    }

    public static explicit operator Coords((decimal Latitude, decimal Longitude) from) =>
        For(latitude: from.Latitude, longitude: from.Longitude).Value;

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return this.Latitude;
        yield return this.Longitude;
    }
}