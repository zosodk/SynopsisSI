using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations; 

namespace SynopsisSI.Services.OrderService.Application.Features.Orders.Commands.PlaceOrder; 

public class PlaceOrderCommand { [Required] 
    public string BuyerId { get; set; } = string.Empty; [Required, MinLength(1)] 
    public List<OrderItemData> Items { get; set; } = new(); [Required] 
    public ShippingAddressData ShippingAddress { get; set; } = new(); [Required, StringLength(3, MinimumLength = 3)] 
    public string Currency { get; set; } = "USD"; } 

public class OrderItemData { [Required] 
    public string ListingId { get; set; } = string.Empty; [Required] 
    public string ProductTitleSnapshot { get; set; } = string.Empty; [Range(0.01, double.MaxValue)] 
    public decimal PriceAtPurchase { get; set; } [Range(1, 100)] public int Quantity { get; set; } = 1; } 

public class ShippingAddressData { [Required, StringLength(200, MinimumLength = 2)] 
    public string Street { get; set; } = string.Empty; [Required, StringLength(100, MinimumLength = 2)] 
    public string City { get; set; } = string.Empty; [Required, StringLength(20, MinimumLength = 2)] 
    public string PostalCode { get; set; } = string.Empty; [Required, StringLength(100, MinimumLength = 2)] 
    public string Country { get; set; } = string.Empty; }