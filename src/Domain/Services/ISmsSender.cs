namespace Engrslan.Services;

/// <summary>
/// Interface for sending SMS messages across the application
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// Send an SMS to a single recipient
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (with country code)</param>
    /// <param name="message">SMS message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<SmsSendResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send SMS to multiple recipients
    /// </summary>
    /// <param name="phoneNumbers">List of recipient phone numbers</param>
    /// <param name="message">SMS message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with bulk send result</returns>
    Task<BulkSmsSendResult> SendSmsAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a templated SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="templateId">Template identifier</param>
    /// <param name="templateData">Template variables</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<SmsSendResult> SendTemplatedSmsAsync(string phoneNumber, string templateId, object templateData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send OTP (One-Time Password) SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="otpCode">OTP code to send</param>
    /// <param name="expiryMinutes">OTP expiry time in minutes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<SmsSendResult> SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send bulk SMS with personalized messages
    /// </summary>
    /// <param name="recipients">List of recipients with personalized messages</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with bulk send result</returns>
    Task<BulkSmsSendResult> SendBulkSmsAsync(IEnumerable<BulkSmsRecipient> recipients, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check SMS delivery status
    /// </summary>
    /// <param name="messageId">Message ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with delivery status</returns>
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate phone number format
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <returns>Validation result</returns>
    PhoneNumberValidationResult ValidatePhoneNumber(string phoneNumber);
}

/// <summary>
/// SMS send result
/// </summary>
public class SmsSendResult
{
    /// <summary>
    /// Indicates if the SMS was sent successfully
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Message ID from the SMS provider
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// Timestamp when the SMS was sent
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Provider-specific status code
    /// </summary>
    public string? StatusCode { get; set; }
    
    /// <summary>
    /// Number of SMS segments (for long messages)
    /// </summary>
    public int? SegmentCount { get; set; }
    
    /// <summary>
    /// Cost of sending the SMS (if available)
    /// </summary>
    public decimal? Cost { get; set; }
    
    /// <summary>
    /// Currency of the cost
    /// </summary>
    public string? Currency { get; set; }
    
    /// <summary>
    /// Create a success result
    /// </summary>
    public static SmsSendResult Success(string messageId, int? segmentCount = null) => new()
    {
        IsSuccess = true,
        MessageId = messageId,
        SentAt = DateTime.UtcNow,
        SegmentCount = segmentCount
    };
    
    /// <summary>
    /// Create a failure result
    /// </summary>
    public static SmsSendResult Failure(string errorMessage, string? statusCode = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        StatusCode = statusCode
    };
}

/// <summary>
/// Bulk SMS recipient configuration
/// </summary>
public class BulkSmsRecipient
{
    /// <summary>
    /// Recipient phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// SMS message content (can be personalized)
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional recipient name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Optional metadata for tracking
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Optional scheduled send time
    /// </summary>
    public DateTime? ScheduledTime { get; set; }
}

/// <summary>
/// Bulk SMS send result
/// </summary>
public class BulkSmsSendResult
{
    /// <summary>
    /// Total number of SMS attempted
    /// </summary>
    public int TotalAttempted { get; set; }
    
    /// <summary>
    /// Number of successfully sent SMS
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed SMS
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Individual results for each recipient
    /// </summary>
    public List<IndividualSmsResult> Results { get; set; } = new();
    
    /// <summary>
    /// Total cost of bulk send
    /// </summary>
    public decimal? TotalCost { get; set; }
    
    /// <summary>
    /// Currency of the total cost
    /// </summary>
    public string? Currency { get; set; }
    
    /// <summary>
    /// Overall success rate percentage
    /// </summary>
    public double SuccessRate => TotalAttempted > 0 ? (double)SuccessCount / TotalAttempted * 100 : 0;
}

/// <summary>
/// Individual SMS result in bulk send
/// </summary>
public class IndividualSmsResult
{
    /// <summary>
    /// Recipient phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Send result for this recipient
    /// </summary>
    public SmsSendResult Result { get; set; } = new();
}

/// <summary>
/// SMS delivery status
/// </summary>
public class SmsDeliveryStatus
{
    /// <summary>
    /// Message ID
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current delivery status
    /// </summary>
    public SmsStatus Status { get; set; }
    
    /// <summary>
    /// Status description
    /// </summary>
    public string? StatusDescription { get; set; }
    
    /// <summary>
    /// Timestamp of the last status update
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Delivery timestamp (if delivered)
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// Failure reason (if failed)
    /// </summary>
    public string? FailureReason { get; set; }
}

/// <summary>
/// SMS delivery status enum
/// </summary>
public enum SmsStatus
{
    /// <summary>
    /// Message is pending
    /// </summary>
    Pending,
    
    /// <summary>
    /// Message has been sent
    /// </summary>
    Sent,
    
    /// <summary>
    /// Message has been delivered
    /// </summary>
    Delivered,
    
    /// <summary>
    /// Message delivery failed
    /// </summary>
    Failed,
    
    /// <summary>
    /// Message has expired
    /// </summary>
    Expired,
    
    /// <summary>
    /// Message was rejected
    /// </summary>
    Rejected,
    
    /// <summary>
    /// Status unknown
    /// </summary>
    Unknown
}

/// <summary>
/// Phone number validation result
/// </summary>
public class PhoneNumberValidationResult
{
    /// <summary>
    /// Indicates if the phone number is valid
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Formatted phone number (E.164 format)
    /// </summary>
    public string? FormattedNumber { get; set; }
    
    /// <summary>
    /// Country code
    /// </summary>
    public string? CountryCode { get; set; }
    
    /// <summary>
    /// Country ISO code
    /// </summary>
    public string? CountryIso { get; set; }
    
    /// <summary>
    /// Phone number type (mobile, landline, etc.)
    /// </summary>
    public PhoneNumberType? NumberType { get; set; }
    
    /// <summary>
    /// Validation error message
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Phone number type
/// </summary>
public enum PhoneNumberType
{
    /// <summary>
    /// Mobile phone
    /// </summary>
    Mobile,
    
    /// <summary>
    /// Landline phone
    /// </summary>
    Landline,
    
    /// <summary>
    /// VoIP phone
    /// </summary>
    VoIP,
    
    /// <summary>
    /// Toll-free number
    /// </summary>
    TollFree,
    
    /// <summary>
    /// Premium rate number
    /// </summary>
    Premium,
    
    /// <summary>
    /// Unknown type
    /// </summary>
    Unknown
}