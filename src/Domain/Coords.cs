using System;
using System.Collections.Generic;

using CSharpFunctionalExtensions;

namespace Domain
{
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
            if(latitude <= 0)
                return Result.Failure<Coords>("Wrong latitude. Please specify a valid value");

            if(longitude <= 0)
                return Result.Failure<Coords>("Wrong longitude. Please specify a valid value");

            return new Coords(latitude: latitude, longitude: longitude);
        }

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return this.Latitude;
            yield return this.Longitude;
        }
    }
}