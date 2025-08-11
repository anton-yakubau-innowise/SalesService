using System.Linq.Expressions;
using AutoMapper;
using FluentAssertions;
using Moq;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using SalesService.Application.Services;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Domain.Repositories;
using SalesService.Domain.ValueObjects;

namespace SalesService.UnitTests.Application;

public class OrderApplicationServiceTests
{
    private readonly Mock<IOrderRepository> orderRepositoryMock;
    private readonly Mock<IUnitOfWork> unitOfWorkMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly Mock<IVehicleServiceApiClient> vehicleServiceMock;
    private readonly OrderApplicationService sut;

    public OrderApplicationServiceTests()
    {
        orderRepositoryMock = new Mock<IOrderRepository>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        mapperMock = new Mock<IMapper>();
        vehicleServiceMock = new Mock<IVehicleServiceApiClient>();

        unitOfWorkMock.Setup(uow => uow.Orders).Returns(orderRepositoryMock.Object);

        sut = new OrderApplicationService(
            unitOfWorkMock.Object,
            vehicleServiceMock.Object,
            mapperMock.Object
        );
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));
        var orderDto = new OrderDto(orderId, order.CustomerId, order.VehicleId, order.Status, 100, "USD", order.CreatedAt);

        orderRepositoryMock
            .Setup(r => r.GetByIdAsNoTrackingAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        mapperMock
            .Setup(m => m.Map<OrderDto>(order))
            .Returns(orderDto);

        // Act
        var result = await sut.GetOrderByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(orderDto);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        orderRepositoryMock
            .Setup(r => r.GetByIdAsNoTrackingAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await sut.GetOrderByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        mapperMock.Verify(m => m.Map<OrderDto>(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderWithCancellationReasonByIdAsync_WhenOrderExists_ReturnsOrderWithCancellationReasonDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));
        var orderDto = new OrderWithCancellationReasonDto(orderId, order.CustomerId, order.VehicleId, order.Status, 100, "USD", order.CreatedAt, null);

        orderRepositoryMock
            .Setup(r => r.GetByIdAsNoTrackingAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        mapperMock
            .Setup(m => m.Map<OrderWithCancellationReasonDto>(order))
            .Returns(orderDto);

        // Act
        var result = await sut.GetOrderWithCancellationReasonByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(orderDto);
    }

    [Fact]
    public async Task GetOrderWithCancellationReasonByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        orderRepositoryMock
            .Setup(r => r.GetByIdAsNoTrackingAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await sut.GetOrderWithCancellationReasonByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        mapperMock.Verify(m => m.Map<OrderWithCancellationReasonDto>(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WhenRecordsExist_ReturnsCollectionOrderDtos()
    {
        // Arrange
        var orders = new List<Order>
        {
            Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD")),
            Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(200, "USD"))
        };

        var orderDtos = orders.Select(o => new OrderDto(o.Id, o.CustomerId, o.VehicleId, o.Status, o.TotalPrice.Amount, o.TotalPrice.Currency, o.CreatedAt));

        orderRepositoryMock
            .Setup(r => r.ListAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        mapperMock
            .Setup(m => m.Map<IEnumerable<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await sut.GetAllOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(orderDtos);
    }

    [Fact]
    public async Task GetAllOrdersAsync_WhenRecordsDoNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var orders = new List<Order>();

        orderRepositoryMock
            .Setup(r => r.ListAllAsNoTrackingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await sut.GetAllOrdersAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WhenOrdersExist_ReturnsOrderDtos()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            Order.Create(customerId, Guid.NewGuid(), new Money(100, "USD")),
            Order.Create(customerId, Guid.NewGuid(), new Money(200, "USD"))
        };

        var orderDtos = orders.Select(o => new OrderDto(o.Id, o.CustomerId, o.VehicleId, o.Status, o.TotalPrice.Amount, o.TotalPrice.Currency, o.CreatedAt));

        orderRepositoryMock
            .Setup(r => r.ListAsNoTrackingAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        mapperMock
            .Setup(m => m.Map<IEnumerable<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await sut.GetCustomerOrdersAsync(customerId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(orderDtos);
    }

    [Fact]
    public async Task GetCustomerOrdersAsync_WhenOrdersDoNotExist_ReturnsEmptyCollection()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        orderRepositoryMock
            .Setup(r => r.ListAsNoTrackingAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await sut.GetCustomerOrdersAsync(customerId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ReturnsNewGuid()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), Guid.NewGuid());
        var vehicleDetails = new VehicleDetailsDto(request.VehicleId, "Tesla Model Y", 50000m, "USD");

        vehicleServiceMock
            .Setup(v => v.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);

        //Act
        var result = await sut.CreateOrderAsync(request, CancellationToken.None);

        //Assert
        result.Should().NotBeEmpty();
        orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenVehicleNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), Guid.NewGuid());

        vehicleServiceMock
            .Setup(v => v.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleDetailsDto?)null);

        // Act
        Func<Task> act = () => sut.CreateOrderAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task BeginAwaitingPaymentAsync_WhenOrderExists_ChangesOrderStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await sut.BeginAwaitingPaymentAsync(orderId, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    [Fact]
    public async Task BeginAwaitingPaymentAsync_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = () => sut.BeginAwaitingPaymentAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ConfirmOrderPaymentAsync_WhenIsProcessed_ChangesOrderStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        order.CompleteProcessing();

        // Act
        await sut.ConfirmOrderPaymentAsync(orderId, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task ConfirmOrderPaymentAsync_WhenIsNotProcessed_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Func<Task> act = () => sut.ConfirmOrderPaymentAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ConfirmOrderAsync_WhenIsPaid_ChangesOrderStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        order.CompleteProcessing();
        order.ConfirmPayment();

        // Act
        await sut.ConfirmOrderAsync(orderId, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmOrderAsync_WhenIsNotPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        order.CompleteProcessing();

        // Act
        Func<Task> act = () => sut.ConfirmOrderAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelOrderAsync_WhenIsConfirmed_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));
        var reason = "Test reason";

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        order.CompleteProcessing();
        order.ConfirmPayment();
        order.ConfirmOrder();

        // Act
        Func<Task> act = () => sut.CancelOrderAsync(orderId, reason, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CancelOrderAsync_WhenIsNotConfirmed_ChangesStatusToCancelled()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));
        var reason = "Test reason";

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        order.CompleteProcessing();
        order.ConfirmPayment();

        // Act
        await sut.CancelOrderAsync(orderId, reason, CancellationToken.None);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
        unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderExists_DeletesOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(100, "USD"));

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await sut.DeleteOrderAsync(orderId, CancellationToken.None);

        // Assert
        orderRepositoryMock.Verify(r => r.Delete(order), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act
        Func<Task> act = () => sut.DeleteOrderAsync(orderId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}