using MassTransit;
using SmartSure.AdminService.Services;
using SmartSure.Shared.Contracts.Events;

namespace SmartSure.AdminService.Consumers
{
    /// <summary>
    /// Represent or implements UserRegisteredConsumer.
    /// </summary>
    public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(IAuditService auditService, ILogger<UserRegisteredConsumer> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the Consume operation.
        /// </summary>
        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("[AdminService] UserRegistered: {Email}, GoogleAuth={IsGoogle}", msg.Email, msg.IsGoogleAuth);

            await _auditService.LogAsync("UserRegistered", "User", msg.UserId, null,
                $"New user registered: {msg.FullName} ({msg.Email}). Google: {msg.IsGoogleAuth}");
        }
    }
}
