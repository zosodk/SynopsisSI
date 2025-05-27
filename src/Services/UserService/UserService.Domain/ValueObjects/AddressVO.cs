using System.Collections.Generic;
using System.Linq; // For SequenceEqual
using System; // For ArgumentNullException

namespace SynopsisSI.Services.UserService.Domain.ValueObjects;

public class AddressVO : ValueObject // Requires ValueObject base class
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private AddressVO() { Street = City = PostalCode = Country = string.Empty; } // For EF Core

    public static AddressVO Create(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentNullException(nameof(street));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentNullException(nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentNullException(nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentNullException(nameof(country));
        return new AddressVO { Street = street, City = city, PostalCode = postalCode, Country = country };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}