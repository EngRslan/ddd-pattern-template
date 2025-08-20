namespace CertManager.Domain.Services;

/// <summary>
/// Interface for sending emails across the application
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Send an email to a single recipient
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<EmailSendResult> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email to multiple recipients
    /// </summary>
    /// <param name="to">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<EmailSendResult> SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email with CC and BCC recipients
    /// </summary>
    /// <param name="to">Primary recipient email address</param>
    /// <param name="cc">CC recipients</param>
    /// <param name="bcc">BCC recipients</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<EmailSendResult> SendEmailAsync(string to, IEnumerable<string>? cc, IEnumerable<string>? bcc, string subject, string htmlBody, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email with attachments
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="attachments">List of attachments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<EmailSendResult> SendEmailWithAttachmentsAsync(string to, string subject, string htmlBody, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a templated email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="templateId">Template identifier</param>
    /// <param name="templateData">Template data/variables</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with send result</returns>
    Task<EmailSendResult> SendTemplatedEmailAsync(string to, string templateId, object templateData, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a bulk email to multiple recipients (each gets individual email)
    /// </summary>
    /// <param name="recipients">List of recipient configurations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation with bulk send results</returns>
    Task<BulkEmailSendResult> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an email attachment
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// File name including extension
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME type of the attachment
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";
    
    /// <summary>
    /// File content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Optional content ID for inline attachments
    /// </summary>
    public string? ContentId { get; set; }
    
    /// <summary>
    /// Indicates if this is an inline attachment
    /// </summary>
    public bool IsInline { get; set; }
}

/// <summary>
/// Email send result
/// </summary>
public class EmailSendResult
{
    /// <summary>
    /// Indicates if the email was sent successfully
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Message ID from the email provider
    /// </summary>
    public string? MessageId { get; set; }
    
    /// <summary>
    /// Timestamp when the email was sent
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Provider-specific status code
    /// </summary>
    public string? StatusCode { get; set; }
    
    /// <summary>
    /// Create a success result
    /// </summary>
    public static EmailSendResult Success(string messageId) => new()
    {
        IsSuccess = true,
        MessageId = messageId,
        SentAt = DateTime.UtcNow
    };
    
    /// <summary>
    /// Create a failure result
    /// </summary>
    public static EmailSendResult Failure(string errorMessage, string? statusCode = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        StatusCode = statusCode
    };
}

/// <summary>
/// Bulk email recipient configuration
/// </summary>
public class BulkEmailRecipient
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional recipient name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Email subject (can be personalized)
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// HTML body content (can be personalized)
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional template data for templated emails
    /// </summary>
    public object? TemplateData { get; set; }
    
    /// <summary>
    /// Optional metadata for tracking
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Bulk email send result
/// </summary>
public class BulkEmailSendResult
{
    /// <summary>
    /// Total number of emails attempted
    /// </summary>
    public int TotalAttempted { get; set; }
    
    /// <summary>
    /// Number of successfully sent emails
    /// </summary>
    public int SuccessCount { get; set; }
    
    /// <summary>
    /// Number of failed emails
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Individual results for each recipient
    /// </summary>
    public List<IndividualEmailResult> Results { get; set; } = new();
    
    /// <summary>
    /// Overall success rate percentage
    /// </summary>
    public double SuccessRate => TotalAttempted > 0 ? (double)SuccessCount / TotalAttempted * 100 : 0;
}

/// <summary>
/// Individual email result in bulk send
/// </summary>
public class IndividualEmailResult
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Send result for this recipient
    /// </summary>
    public EmailSendResult Result { get; set; } = new();
}