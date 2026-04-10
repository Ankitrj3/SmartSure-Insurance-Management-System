using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartSure.PolicyService.DTOs;
using SmartSure.PolicyService.Models;
using SmartSure.PolicyService.Repositories;
using SmartSure.PolicyService.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SmartSure.PolicyService.Tests
{
    /// <summary>
    /// Unit tests for PaymentService - covering Razorpay order creation,
    /// signature verification, payment recording, policy activation, and failure flows.
    /// </summary>
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository>           _paymentRepoMock;
        private readonly Mock<IPolicyMgmtService>           _policyServiceMock;
        private readonly Mock<IRazorpayService>             _razorpayServiceMock;
        private readonly Mock<IConfiguration>               _configMock;
        private readonly Mock<ILogger<PaymentService>>      _loggerMock;
        private readonly PaymentService                     _service;

        public PaymentServiceTests()
        {
            _paymentRepoMock    = new Mock<IPaymentRepository>();
            _policyServiceMock  = new Mock<IPolicyMgmtService>();
            _razorpayServiceMock= new Mock<IRazorpayService>();
            _configMock         = new Mock<IConfiguration>();
            _loggerMock         = new Mock<ILogger<PaymentService>>();

            _configMock.Setup(c => c["Razorpay:KeyId"]).Returns("rzp_test_abc123");

            _service = new PaymentService(
                _paymentRepoMock.Object,
                _policyServiceMock.Object,
                _razorpayServiceMock.Object,
                _configMock.Object,
                _loggerMock.Object
            );
        }

        // ── CreateRazorpayOrder Tests ──────────────────────────────────────────

        [Fact]
        public async Task CreateRazorpayOrder_ReturnsOrderIdAndKeyId()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var dto = new CreateRazorpayOrderDTO { PolicyId = policyId, Amount = 15000m, Currency = "INR" };

            var razorpayOrder = new Razorpay.Api.Order();
            // Simulate dictionary-like access via Razorpay entity
            // We use a simple dynamic mock approach: just verify that CreateOrderAsync is called
            _razorpayServiceMock.Setup(r => r.CreateOrderAsync(
                    dto.Amount, dto.Currency, It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(new Dictionary<string, object> { { "id", "order_testABC123" } });

            // Act
            var result = await _service.CreateRazorpayOrderAsync(dto);

            // Assert
            Assert.Equal("order_testABC123", result.OrderId);
            Assert.Equal("rzp_test_abc123",  result.KeyId);
            Assert.Equal(policyId,            result.PolicyId);
            Assert.Equal(15000m,             result.Amount);
        }

        // ── VerifyAndRecordRazorpayPayment Tests ──────────────────────────────

        [Fact]
        public async Task VerifyPayment_InvalidSignature_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _razorpayServiceMock.Setup(r => r.VerifyPaymentSignature(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var dto = new VerifyRazorpayPaymentDTO
            {
                PolicyId           = Guid.NewGuid(),
                RazorpayOrderId    = "order_ABC",
                RazorpayPaymentId  = "pay_XYZ",
                RazorpaySignature  = "bad_signature"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.VerifyAndRecordRazorpayPaymentAsync(dto));
        }

        [Fact]
        public async Task VerifyPayment_ValidSignature_Captured_RecordsSuccessAndActivatesPolicy()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var dto = new VerifyRazorpayPaymentDTO
            {
                PolicyId          = policyId,
                RazorpayOrderId   = "order_OK",
                RazorpayPaymentId = "pay_OK",
                RazorpaySignature = "valid_sig",
                PaymentMethod     = "UPI"
            };

            _razorpayServiceMock.Setup(r => r.VerifyPaymentSignature("order_OK", "pay_OK", "valid_sig"))
                .Returns(true);

            _razorpayServiceMock.Setup(r => r.GetPaymentDetailsAsync("pay_OK"))
                .ReturnsAsync(new Dictionary<string, object>
                {
                    { "amount", 1500000 }, // 1500000 paise = ₹15000
                    { "status", "captured" }
                });

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _policyServiceMock.Setup(p => p.ActivatePolicyAsync(policyId)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.VerifyAndRecordRazorpayPaymentAsync(dto);

            // Assert
            Assert.Equal("Success",   result.Status);
            Assert.Equal(15000m,      result.Amount);
            Assert.Equal(policyId,    result.PolicyId);
            Assert.Equal("UPI",       result.PaymentMethod);
            _policyServiceMock.Verify(p => p.ActivatePolicyAsync(policyId), Times.Once);
        }

        [Fact]
        public async Task VerifyPayment_ValidSignature_NotCaptured_RecordsFailedPayment_DoesNotActivate()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var dto = new VerifyRazorpayPaymentDTO
            {
                PolicyId          = policyId,
                RazorpayOrderId   = "order_F",
                RazorpayPaymentId = "pay_F",
                RazorpaySignature = "sig_f",
                PaymentMethod     = "Card"
            };

            _razorpayServiceMock.Setup(r => r.VerifyPaymentSignature("order_F", "pay_F", "sig_f")).Returns(true);
            _razorpayServiceMock.Setup(r => r.GetPaymentDetailsAsync("pay_F"))
                .ReturnsAsync(new Dictionary<string, object>
                {
                    { "amount", 1000000 },
                    { "status", "failed" } // Not "captured"
                });

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.VerifyAndRecordRazorpayPaymentAsync(dto);

            // Assert
            Assert.Equal("Failed", result.Status);
            // Policy should NOT be activated on failed payment
            _policyServiceMock.Verify(p => p.ActivatePolicyAsync(It.IsAny<Guid>()), Times.Never);
        }

        // ── RecordFailedPayment Tests ─────────────────────────────────────────

        [Fact]
        public async Task RecordFailedPayment_CreatesFailedPaymentRecord()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            Payment? savedPayment = null;

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
                .Callback<Payment>(p => savedPayment = p)
                .Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RecordFailedPaymentAsync(policyId, 8000m, "Insufficient funds");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Failed",   result.Status);
            Assert.Equal(8000m,      result.Amount);
            Assert.Equal(policyId,   result.PolicyId);
            Assert.Equal("Razorpay", result.PaymentMethod);
            Assert.StartsWith("FAILED_", result.TransactionReference);

            Assert.NotNull(savedPayment);
            Assert.Equal("Failed", savedPayment!.Status);
        }

        // ── RecordPayment Tests (Legacy) ──────────────────────────────────────

        [Fact]
        public async Task RecordPayment_RecordsSuccessAndActivatesPolicy()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var dto = new RecordPaymentDTO
            {
                PolicyId       = policyId,
                Amount         = 12000m,
                PaymentMethod  = "Netbanking",
                TransactionReference = "TXN_123"
            };

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _policyServiceMock.Setup(p => p.ActivatePolicyAsync(policyId)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RecordPaymentAsync(dto);

            // Assert
            Assert.Equal("Success",   result.Status);
            Assert.Equal(12000m,      result.Amount);
            Assert.Equal("TXN_123",   result.TransactionReference);
            _policyServiceMock.Verify(p => p.ActivatePolicyAsync(policyId), Times.Once);
        }

        [Fact]
        public async Task RecordPayment_GeneratesTransactionReferenceIfNoneProvided()
        {
            // Arrange
            var dto = new RecordPaymentDTO
            {
                PolicyId      = Guid.NewGuid(),
                Amount        = 5000m,
                PaymentMethod = "Cash",
                TransactionReference = null
            };

            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _policyServiceMock.Setup(p => p.ActivatePolicyAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.RecordPaymentAsync(dto);

            // Assert
            Assert.NotNull(result.TransactionReference);
            Assert.NotEmpty(result.TransactionReference);
        }

        // ── GetByPolicyId/GetUserPayments Tests ───────────────────────────────

        [Fact]
        public async Task GetByPolicyId_ReturnsMappedPaymentDTOs()
        {
            // Arrange
            var policyId = Guid.NewGuid();
            var payments = new List<Payment>
            {
                new Payment { PaymentId = Guid.NewGuid(), PolicyId = policyId, Amount = 5000m, Status = "Success", PaymentMethod = "UPI", TransactionReference = "T1" },
                new Payment { PaymentId = Guid.NewGuid(), PolicyId = policyId, Amount = 5000m, Status = "Failed",  PaymentMethod = "Card", TransactionReference = "T2" }
            };
            _paymentRepoMock.Setup(r => r.GetByPolicyIdAsync(policyId)).ReturnsAsync(payments);

            // Act
            var result = await _service.GetByPolicyIdAsync(policyId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Status == "Success");
            Assert.Contains(result, p => p.Status == "Failed");
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _paymentRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Payment)null!);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }
    }
}
