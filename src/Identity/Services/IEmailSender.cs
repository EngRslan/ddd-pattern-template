namespace CertManager.Identity.Services;

/// <summary>
/// Interface for sending emails
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
    /// <returns>Task representing the async operation</returns>
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email to multiple recipients
    /// </summary>
    /// <param name="to">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email with attachments
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <param name="attachments">List of attachments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendEmailWithAttachmentsAsync(string to, string subject, string htmlBody, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a templated email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="templateId">Template identifier</param>
    /// <param name="templateData">Template data/variables</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendTemplatedEmailAsync(string to, string templateId, object templateData, CancellationToken cancellationToken = default);
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
}