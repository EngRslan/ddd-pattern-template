using CertManager.Domain.Services;
using CertManager.Domain.Shared.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CertManager.Infrastructure.Services;

/// <summary>
/// Fake email sender for development - writes to console/logs instead of sending real emails
/// </summary>
public class FakeEmailSender : IEmailSender, ISingletonService
{
    private readonly ILogger<FakeEmailSender> _logger;
    
    public FakeEmailSender(ILogger<FakeEmailSender> logger)
    {
        _logger = logger;
    }
    
    public Task<EmailSendResult> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 FAKE EMAIL SENT");
        _logger.LogInformation("To: {To}", to);
        _logger.LogInformation("Subject: {Subject}", subject);
        
        Console.WriteLine("================== FAKE EMAIL ==================");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {StripHtml(htmlBody)}");
        Console.WriteLine($"Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine("================================================");
        
        return Task.FromResult(EmailSendResult.Success($"fake_email_{Guid.NewGuid()}"));
    }
    
    public async Task<EmailSendResult> SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var recipients = string.Join(", ", to);
        _logger.LogInformation("📧 FAKE EMAIL SENT TO MULTIPLE RECIPIENTS: {Recipients}", recipients);
        
        return await SendEmailAsync(recipients, subject, htmlBody, cancellationToken);
    }
    
    public async Task<EmailSendResult> SendEmailAsync(string to, IEnumerable<string>? cc, IEnumerable<string>? bcc, 
        string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("================== FAKE EMAIL ==================");
        Console.WriteLine($"To: {to}");
        if (cc?.Any() == true) Console.WriteLine($"CC: {string.Join(", ", cc)}");
        if (bcc?.Any() == true) Console.WriteLine($"BCC: {string.Join(", ", bcc)}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {StripHtml(htmlBody)}");
        Console.WriteLine("================================================");
        
        return await SendEmailAsync(to, subject, htmlBody, cancellationToken);
    }
    
    public Task<EmailSendResult> SendEmailWithAttachmentsAsync(string to, string subject, string htmlBody, 
        IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default)
    {
        var attachmentList = attachments.ToList();
        
        Console.WriteLine("================== FAKE EMAIL ==================");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {StripHtml(htmlBody)}");
        Console.WriteLine($"Attachments: {attachmentList.Count} file(s)");
        foreach (var attachment in attachmentList)
        {
            Console.WriteLine($"  - {attachment.FileName} ({attachment.Content.Length} bytes)");
        }
        Console.WriteLine("================================================");
        
        return Task.FromResult(EmailSendResult.Success($"fake_email_{Guid.NewGuid()}"));
    }
    
    public Task<EmailSendResult> SendTemplatedEmailAsync(string to, string templateId, object templateData, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 FAKE TEMPLATED EMAIL");
        _logger.LogInformation("To: {To}", to);
        _logger.LogInformation("Template: {TemplateId}", templateId);
        _logger.LogInformation("Data: {@TemplateData}", templateData);
        
        var subject = $"[Template: {templateId}]";
        var body = $"Template: {templateId}\nData: {System.Text.Json.JsonSerializer.Serialize(templateData)}";
        
        return SendEmailAsync(to, subject, body, cancellationToken);
    }
    
    public async Task<BulkEmailSendResult> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients, 
        CancellationToken cancellationToken = default)
    {
        var result = new BulkEmailSendResult();
        var recipientsList = recipients.ToList();
        result.TotalAttempted = recipientsList.Count;
        
        Console.WriteLine($"================== BULK EMAIL ==================");
        Console.WriteLine($"Sending {recipientsList.Count} emails...");
        
        foreach (var recipient in recipientsList)
        {
            var sendResult = await SendEmailAsync(recipient.Email, recipient.Subject, recipient.HtmlBody, cancellationToken);
            result.Results.Add(new IndividualEmailResult
            {
                Email = recipient.Email,
                Result = sendResult
            });
        }
        
        result.SuccessCount = result.Results.Count;
        result.FailureCount = 0;
        
        Console.WriteLine($"Bulk email completed: {result.SuccessCount}/{result.TotalAttempted} sent");
        Console.WriteLine("================================================");
        
        return result;
    }
    
    private string StripHtml(string html)
    {
        // Simple HTML stripping for console display
        if (string.IsNullOrEmpty(html)) return string.Empty;
        
        // Remove script and style elements
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Extract href URLs and show them in brackets
        html = System.Text.RegularExpressions.Regex.Replace(html, 
            @"<a[^>]+href=[""']([^""']+)[""'][^>]*>([^<]+)</a>", 
            "$2 [Link: $1]",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Remove HTML tags
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", " ");
        
        // Decode HTML entities
        html = System.Net.WebUtility.HtmlDecode(html);
        
        // Clean up whitespace
        html = System.Text.RegularExpressions.Regex.Replace(html, @"\s+", " ").Trim();
        
        // Don't truncate - show full content for important emails like password reset
        // Or use a much higher limit for truncation
        if (html.Length > 2000)
        {
            html = html.Substring(0, 1997) + "...";
        }
        
        return html;
    }
}