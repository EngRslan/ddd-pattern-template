using CertManager.Domain.Services;
using CertManager.Domain.Shared.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CertManager.Infrastructure.Services;

/// <summary>
/// Fake SMS sender for development - writes to console/logs instead of sending real SMS
/// </summary>
public class FakeSmsSender : ISmsSender, ISingletonService
{
    private readonly ILogger<FakeSmsSender> _logger;
    
    public FakeSmsSender(ILogger<FakeSmsSender> logger)
    {
        _logger = logger;
    }
    
    public Task<SmsSendResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📱 FAKE SMS SENT");
        _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
        _logger.LogInformation("Message: {Message}", message);
        _logger.LogInformation("Timestamp: {Timestamp}", DateTime.UtcNow);
        
        Console.WriteLine("================== FAKE SMS ==================");
        Console.WriteLine($"To: {phoneNumber}");
        Console.WriteLine($"Message: {message}");
        Console.WriteLine($"Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine("==============================================");
        
        return Task.FromResult(SmsSendResult.Success($"fake_sms_{Guid.NewGuid()}", 1));
    }
    
    public async Task<BulkSmsSendResult> SendSmsAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default)
    {
        var result = new BulkSmsSendResult();
        var numbers = phoneNumbers.ToList();
        result.TotalAttempted = numbers.Count;
        
        foreach (var number in numbers)
        {
            var sendResult = await SendSmsAsync(number, message, cancellationToken);
            result.Results.Add(new IndividualSmsResult
            {
                PhoneNumber = number,
                Result = sendResult
            });
        }
        
        result.SuccessCount = result.Results.Count;
        result.FailureCount = 0;
        
        return result;
    }
    
    public Task<SmsSendResult> SendTemplatedSmsAsync(string phoneNumber, string templateId, object templateData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📱 FAKE TEMPLATED SMS");
        _logger.LogInformation("To: {PhoneNumber}", phoneNumber);
        _logger.LogInformation("Template: {TemplateId}", templateId);
        _logger.LogInformation("Data: {@TemplateData}", templateData);
        
        return Task.FromResult(SmsSendResult.Success($"fake_template_{Guid.NewGuid()}", 1));
    }
    
    public Task<SmsSendResult> SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes = 5, CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {otpCode} (expires in {expiryMinutes} minutes)";
        
        Console.WriteLine("================== OTP SMS ==================");
        Console.WriteLine($"To: {phoneNumber}");
        Console.WriteLine($"OTP Code: {otpCode}");
        Console.WriteLine($"Expires in: {expiryMinutes} minutes");
        Console.WriteLine("==============================================");
        
        return SendSmsAsync(phoneNumber, message, cancellationToken);
    }
    
    public async Task<BulkSmsSendResult> SendBulkSmsAsync(IEnumerable<BulkSmsRecipient> recipients, CancellationToken cancellationToken = default)
    {
        var result = new BulkSmsSendResult();
        var recipientsList = recipients.ToList();
        result.TotalAttempted = recipientsList.Count;
        
        foreach (var recipient in recipientsList)
        {
            var sendResult = await SendSmsAsync(recipient.PhoneNumber, recipient.Message, cancellationToken);
            result.Results.Add(new IndividualSmsResult
            {
                PhoneNumber = recipient.PhoneNumber,
                Result = sendResult
            });
        }
        
        result.SuccessCount = result.Results.Count;
        result.FailureCount = 0;
        
        return result;
    }
    
    public Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SmsDeliveryStatus
        {
            MessageId = messageId,
            Status = SmsStatus.Delivered,
            StatusDescription = "Fake delivery - always successful",
            UpdatedAt = DateTime.UtcNow,
            DeliveredAt = DateTime.UtcNow
        });
    }
    
    public PhoneNumberValidationResult ValidatePhoneNumber(string phoneNumber)
    {
        // Simple validation - just check if it has digits
        var hasDigits = phoneNumber.Any(char.IsDigit);
        
        return new PhoneNumberValidationResult
        {
            IsValid = hasDigits && phoneNumber.Length >= 10,
            FormattedNumber = phoneNumber,
            CountryCode = "+1",
            NumberType = PhoneNumberType.Mobile,
            ErrorMessage = hasDigits ? null : "Invalid phone number format"
        };
    }
}