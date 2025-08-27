using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.Persistence;

namespace SalesService.IntegrationTests.API;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly Mock<IVehicleServiceApiClient> vehicleServiceMock;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly CustomWebApplicationFactory<Program> factory;

    public OrdersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        this.factory = factory;
        vehicleServiceMock = factory.VehicleServiceMock;
        client = factory.CreateClient();

        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        ResetDatabaseState();
    }

    [Fact]
    public async Task GetAllOrders_WhenOrdersExist_ReturnsOkAndOrderList()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 10000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);

        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
        orders.Should().NotBeNull();
        orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllOrders_WhenNoOrdersExist_ReturnsOkAndEmptyList()
    {
        // Arrange

        // Act
        var response = await client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
        orders.Should().NotBeNull();
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerOrders_WhenOrdersExistForCustomer_ReturnsOkAndCustomerOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 10000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);

        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, vehicleDetails.Id));
        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, vehicleDetails.Id));
        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(otherCustomerId, vehicleDetails.Id));

        // Act
        var response = await client.GetAsync($"/api/orders/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
        orders.Should().NotBeNull();
        orders.Should().HaveCount(2);
        orders.Should().OnlyContain(o => o.CustomerId == customerId);

        response.Headers.Should().ContainKey("Set-Cookie");
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.StartsWith("LastCustomerId=" + customerId));
    }

    [Fact]
    public async Task GetCustomerOrders_WhenNoOrdersExistForCustomer_ReturnsOkAndEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/orders/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
        orders.Should().NotBeNull();
        orders.Should().BeEmpty();

        response.Headers.Should().ContainKey("Set-Cookie");
        response.Headers.GetValues("Set-Cookie").Should().Contain(c => c.StartsWith("LastCustomerId=" + customerId));
    }

    [Fact]
    public async Task GetDefaultCustomerOrders_WhenCookieExists_ReturnsOkAndOrders()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);

        await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(customerId, vehicleDetails.Id));

        var responseWithCookie = await client.GetAsync($"/api/orders/customer/{customerId}");
        var cookieHeader = responseWithCookie.Headers.GetValues("Set-Cookie").First();
        var cookieValue = cookieHeader.Split(';').First();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/orders/default-customer");
        request.Headers.Add("Cookie", cookieValue);
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>(jsonOptions);
        orders.Should().NotBeNull();
        orders.Should().HaveCount(1);
        orders!.First().CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task GetDefaultCustomerOrders_WhenCookieDoesNotExist_ReturnsBadRequest()
    {
        // Arrange

        // Act
        var response = await client.GetAsync("/api/orders/default-customer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderExists_ReturnsOkAndOrder()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.GetAsync($"/api/orders/{createdOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>(jsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrderId);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetOrderByIdWithCancellationReason_WhenOrderExists_ReturnsOkAndOrder()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.GetAsync($"/api/orders/{createdOrderId}/with-cancellation-reason");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderWithCancellationReasonDto>(jsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrderId);
        order.CancellationReason.Should().BeNull();
    }

    [Fact]
    public async Task GetOrderByIdWithCancellationReason_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/orders/{nonExistentId}/with-cancellation-reason");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsCreatedAndOrderId()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var request = new CreateOrderRequest(customerId, vehicleId);

        var vehicleDetails = new VehicleDetailsDto(vehicleId, "Test Model", 60000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdOrderId = await response.Content.ReadFromJsonAsync<Guid>();
        createdOrderId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdOrderDto = await getResponse.Content.ReadFromJsonAsync<OrderDto>(jsonOptions);
        createdOrderDto.Should().NotBeNull();
        createdOrderDto!.CustomerId.Should().Be(customerId);
        createdOrderDto.VehicleId.Should().Be(vehicleId);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.Empty, Guid.NewGuid());

        // Act
        var response = await client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteOrder_WhenOrderExists_ReturnsNoContent()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.DeleteAsync($"/api/orders/{createdOrderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/orders/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BeginAwaitingPayment_WhenOrderIsPending_ReturnsNoContentAndUpdatesStatus()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>(jsonOptions);
        updatedOrder!.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    [Fact]
    public async Task BeginAwaitingPayment_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/api/orders/{nonExistentId}/await-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BeginAwaitingPayment_WhenOrderIsNotPending_ReturnsBadRequest()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmOrderPayment_WhenOrderIsAwaitingPayment_ReturnsNoContentAndUpdatesStatus()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/confirm-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>(jsonOptions);
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task ConfirmOrderPayment_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/api/orders/{nonExistentId}/confirm-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmOrderPayment_WhenOrderIsNotAwaitingPayment_ReturnsBadRequest()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/confirm-payment", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConfirmOrder_WhenOrderIsPaid_ReturnsNoContentAndUpdatesStatus()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        
        await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);
        await client.PostAsync($"/api/orders/{createdOrderId}/confirm-payment", null);

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>(jsonOptions);
        updatedOrder!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.PostAsync($"/api/orders/{nonExistentId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmOrder_WhenOrderIsNotPaid_ReturnsBadRequest()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();

        await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);

        // Act
        var response = await client.PostAsync($"/api/orders/{createdOrderId}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsCancellable_ReturnsNoContentAndUpdatesStatus()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        var cancelRequest = new CancelOrderRequest("Customer changed their mind");

        // Act
        var response = await client.PostAsJsonAsync($"/api/orders/{createdOrderId}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/orders/{createdOrderId}/with-cancellation-reason");
        var updatedOrder = await getResponse.Content.ReadFromJsonAsync<OrderWithCancellationReasonDto>(jsonOptions);
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
        updatedOrder.CancellationReason.Should().Be(cancelRequest.CancellationReason);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var cancelRequest = new CancelOrderRequest("Test reason");

        // Act
        var response = await client.PostAsJsonAsync($"/api/orders/{nonExistentId}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelOrder_WhenOrderIsConfirmed_ReturnsBadRequest()
    {
        // Arrange
        var vehicleDetails = new VehicleDetailsDto(Guid.NewGuid(), "Test Car", 1000m, "USD");
        vehicleServiceMock
            .Setup(s => s.GetVehicleDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehicleDetails);
        var createResponse = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(Guid.NewGuid(), vehicleDetails.Id));
        var createdOrderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        var cancelRequest = new CancelOrderRequest("Test reason");

        await client.PostAsync($"/api/orders/{createdOrderId}/await-payment", null);
        await client.PostAsync($"/api/orders/{createdOrderId}/confirm-payment", null);
        await client.PostAsync($"/api/orders/{createdOrderId}/confirm", null);

        // Act
        var response = await client.PostAsJsonAsync($"/api/orders/{createdOrderId}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }







    private void ResetDatabaseState()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        dbContext.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Orders\" RESTART IDENTITY CASCADE");
    }
}