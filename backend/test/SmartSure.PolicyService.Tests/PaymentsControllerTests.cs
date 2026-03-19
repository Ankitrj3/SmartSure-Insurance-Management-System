using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartSure.PolicyService.Controllers;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Services;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _mockService;
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
        {
            _mockService = new Mock<IPaymentService>();
            _controller = new PaymentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetPayments_ReturnsOk()
        {
            var policyId = Guid.NewGuid();
            var payments = new List<PaymentDTO> { new PaymentDTO { Amount = 100 } };
            _mockService.Setup(s => s.GetByPolicyIdAsync(policyId)).ReturnsAsync(payments);

            var result = await _controller.GetPayments(policyId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(payments, okResult.Value);
        }

        [Fact]
        public async Task RecordPayment_ReturnsCreatedAt()
        {
            var dto = new RecordPaymentDTO { Amount = 100, PaymentMethod = "Card" };
            var response = new PaymentDTO { PaymentId = Guid.NewGuid(), Amount = 100 };
            _mockService.Setup(s => s.RecordPaymentAsync(dto)).ReturnsAsync(response);

            var result = await _controller.RecordPayment(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task GetPayment_ReturnsOk_WhenFound()
        {
            var paymentId = Guid.NewGuid();
            var payment = new PaymentDTO { PaymentId = paymentId, Amount = 100 };
            _mockService.Setup(s => s.GetByIdAsync(paymentId)).ReturnsAsync(payment);

            var result = await _controller.GetPayment(paymentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(payment, okResult.Value);
        }
    }
}
