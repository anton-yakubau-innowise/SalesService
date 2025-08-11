using AutoMapper;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;

namespace SalesService.Application.Services;

public class OrderApplicationService(IUnitOfWork unitOfWork, IMapper mapper) : IOrderApplicationService
{

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetNoTrackingOrderAndEnsureExistsAsync(id, cancellationToken);

        return mapper.Map<OrderDto>(order);
    }

    public async Task<OrderWithCancellationReasonDto?> GetOrderWithCancellationReasonByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetNoTrackingOrderAndEnsureExistsAsync(id, cancellationToken);

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
        var order = Order.Create(
            request.CustomerId,
            request.VehicleId,
            new Domain.ValueObjects.Money(0, "USD") //Later connection to VehicleService to get the price
        );

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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

    private async Task<Order> GetNoTrackingOrderAndEnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdAsNoTrackingAsync(id, cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found.");
        }

        return order;
    }
}
