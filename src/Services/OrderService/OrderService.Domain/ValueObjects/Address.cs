using System.Collections.Generic;
using System.Linq;
using System;

namespace SynopsisSI.Services.OrderService.Domain.ValueObjects;
public class Address : ValueObject // Requires ValueObject base class
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }
    private Address() { Street = City = PostalCode = Country = string.Empty; }
    public static Address Create(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentNullException(nameof(street));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentNullException(nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentNullException(nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentNullException(nameof(country));
        return new Address { Street = street, City = city, PostalCode = postalCode, Country = country };
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street; yield return City; yield return PostalCode; yield return Country;
    }
}