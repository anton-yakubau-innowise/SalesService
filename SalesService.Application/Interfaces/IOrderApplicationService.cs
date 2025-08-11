using SalesService.Application.Dtos;

namespace SalesService.Application.Interfaces;

public interface IOrderApplicationService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderWithCancellationReasonDto?> GetOrderWithCancellationReasonByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task BeginAwaitingPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ConfirmOrderPaymentAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task ConfirmOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}