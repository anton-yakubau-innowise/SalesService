using AutoMapper;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using SalesService.Domain.Common;
using SalesService.Domain.Entities;
using MassTransit;
using Contracts;

namespace SalesService.Application.Services;

public class OrderApplicationService(
    IUnitOfWork unitOfWork,
    IVehicleServiceApiClient vehicleService,
    IUserServiceApiClient userService,
    IMapper mapper,
    IPublishEndpoint publishEndpoint) : IOrderApplicationService
{

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetNoTrackingOrderNotEnsureExistsAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return mapper.Map<OrderDto>(order);
    }

    public async Task<OrderWithCancellationReasonDto?> GetOrderWithCancellationReasonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetNoTrackingOrderNotEnsureExistsAsync(id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return mapper.Map<OrderWithCancellationReasonDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.ListAllAsNoTrackingAsync(cancellationToken);

        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var orders = await unitOfWork.Orders.ListAsNoTrackingAsync(o => o.CustomerId == customerId, cancellationToken);

        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var vehicleDetails = await vehicleService.GetVehicleDetailsAsync(request.VehicleId, cancellationToken)
            ?? throw new ArgumentException($"Vehicle with ID {request.VehicleId} not found.");

        var userDetails = await userService.GetUserContactInfoAsync(request.CustomerId, cancellationToken)
            ?? throw new ArgumentException($"User with ID {request.CustomerId} not found.");

        var totalPrice = new Domain.ValueObjects.Money(vehicleDetails.Price, vehicleDetails.Currency);

        var order = Order.Create(
            request.CustomerId,
            request.VehicleId,
            totalPrice
        );

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await PublishOrderCreated(userDetails, order, cancellationToken);

        await vehicleService.ReserveVehicleAsync(request.VehicleId, cancellationToken);

        return order.Id;
    }

    public async Task BeginAwaitingPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAndEnsureExistsAsync(orderId, cancellationToken);

        order.CompleteProcessing();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmOrderPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAndEnsureExistsAsync(orderId, cancellationToken);

        order.ConfirmPayment();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAndEnsureExistsAsync(orderId, cancellationToken);

        order.ConfirmOrder();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        var order = await GetOrderAndEnsureExistsAsync(orderId, cancellationToken);

        order.Cancel(reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetOrderAndEnsureExistsAsync(id, cancellationToken);

        unitOfWork.Orders.Delete(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order> GetOrderAndEnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found.");
        }

        return order;
    }

    private async Task<Order?> GetNoTrackingOrderNotEnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        Guard.AgainstEmptyGuid(id, nameof(id));
        var order = await unitOfWork.Orders.GetByIdAsNoTrackingAsync(id, cancellationToken);

        return order;
    }

    private async Task PublishOrderCreated(UserContactInfoDto userDetails, Order order, CancellationToken cancellationToken)
    {
        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.CustomerId,
            order.VehicleId,
            userDetails.Email,
            userDetails.PhoneNumber,
            new MoneyDto(
                Amount: order.TotalPrice.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                CurrencyCode: order.TotalPrice.Currency
            )
        );

        await publishEndpoint.Publish(orderCreatedEvent, cancellationToken);
    }
}
