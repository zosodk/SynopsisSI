namespace SynopsisSI.Services.OrderService.Domain.Entities;
public enum OrderStatus { Pending, ProcessingPayment, PaymentFailed, Paid, AwaitingShipment, Shipped, Delivered, Completed, Cancelled }