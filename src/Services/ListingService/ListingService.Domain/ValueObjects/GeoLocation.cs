using System;
    using System.Collections.Generic; // For IEnumerable

    namespace SynopsisSI.Services.ListingService.Domain.ValueObjects;

    // Value Object: Immutable, identified by its attributes, not an ID.
    public class GeoLocation : ValueObject
    {
        public string Type { get; private set; } = "Point"; // GeoJSON type
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }

        // For EF Core, ensure there's a way to construct it, possibly private with public factory
        private GeoLocation() { }

        public static GeoLocation FromCoordinates(double longitude, double latitude)
        {
            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");

            return new GeoLocation { Longitude = longitude, Latitude = latitude };
        }

        // For MongoDB storage as [longitude, latitude] array for 2dsphere index
        public double[] ToCoordinatesArray() => new[] { Longitude, Latitude };

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
            yield return Longitude;
            yield return Latitude;
        }
    }

