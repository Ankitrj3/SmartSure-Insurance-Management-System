using MassTransit;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.PolicyService.Consumers;

/// <summary>
/// Represent or implements UserRegisteredConsumer.
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Performs the Consume operation.
    /// </summary>
    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        _logger.LogInformation(
            "[PolicyService] UserRegistered received: {Email}. Ready to seed customer record.",
            context.Message.Email);
        
        // TODO: Create a Customer record in Policy database if needed for future policy linking.
        
        return Task.CompletedTask;
    }
}
