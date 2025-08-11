using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Dtos;
using SalesService.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderApplicationService orderService) : ControllerBase
{
    const string LastCustomerIdCookieName = "LastCustomerId";

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(Guid customerId, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetCustomerOrdersAsync(customerId, cancellationToken);

        AppendCustomerIdCookie(customerId);

        return Ok(orders);
    }

    [HttpGet("default-customer")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDefaultCustomerOrders(CancellationToken cancellationToken)
    {
        var lastCustomerId = Request.Cookies[LastCustomerIdCookieName];

        if (string.IsNullOrEmpty(lastCustomerId) || !Guid.TryParse(lastCustomerId, out var lastCustomerGuid))
        {
            return BadRequest("No valid last customer ID found in cookies.");
        }

        var orders = await orderService.GetCustomerOrdersAsync(lastCustomerGuid, cancellationToken);

        AppendCustomerIdCookie(lastCustomerGuid);

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    [HttpGet("{id:guid}/with-cancellation-reason")]
    [ProducesResponseType(typeof(OrderWithCancellationReasonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByIdWithCancellationReason(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderWithCancellationReasonByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var orderId = await orderService.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        await orderService.DeleteOrderAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/await-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BeginAwaitingPayment(Guid orderId, CancellationToken cancellationToken)
    {
        await orderService.BeginAwaitingPaymentAsync(orderId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/confirm-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmOrderPayment(Guid orderId, CancellationToken cancellationToken)
    {
        await orderService.ConfirmOrderPaymentAsync(orderId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        await orderService.CancelOrderAsync(orderId, request.CancellationReason, cancellationToken);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmOrder(Guid orderId, CancellationToken cancellationToken)
    {
        await orderService.ConfirmOrderAsync(orderId, cancellationToken);
        return NoContent();
    }


    private void AppendCustomerIdCookie(Guid lastCustomerGuid)
    {
        Response.Cookies.Append(
            LastCustomerIdCookieName,
            lastCustomerGuid.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            }
        );
    }
}

