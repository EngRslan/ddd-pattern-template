using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Engrslan.Services;

/// <summary>
/// Default implementation of IEmailSender using SMTP
/// Can be replaced with SendGrid, AWS SES, or other email providers
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailSettings _settings;
    
    public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _settings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
    }
    
    public async Task<EmailSendResult> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = CreateMailMessage(to, subject, htmlBody);
            using var client = CreateSmtpClient();
            
            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            return EmailSendResult.Success(Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return EmailSendResult.Failure(ex.Message);
        }
    }
    
    public async Task<EmailSendResult> SendEmailAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = to.ToList();
            using var message = CreateMailMessage(recipients.First(), subject, htmlBody);
            
            // Add additional recipients
            foreach (var recipient in recipients.Skip(1))
            {
                message.To.Add(recipient);
            }
            
            using var client = CreateSmtpClient();
            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Count} recipients", recipients.Count);
            return EmailSendResult.Success(Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to multiple recipients");
            return EmailSendResult.Failure(ex.Message);
        }
    }
    
    public async Task<EmailSendResult> SendEmailAsync(string to, IEnumerable<string>? cc, IEnumerable<string>? bcc, 
        string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = CreateMailMessage(to, subject, htmlBody);
            
            // Add CC recipients
            if (cc != null)
            {
                foreach (var ccRecipient in cc)
                {
                    message.CC.Add(ccRecipient);
                }
            }
            
            // Add BCC recipients
            if (bcc != null)
            {
                foreach (var bccRecipient in bcc)
                {
                    message.Bcc.Add(bccRecipient);
                }
            }
            
            using var client = CreateSmtpClient();
            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {To} with CC and BCC", to);
            return EmailSendResult.Success(Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with CC/BCC");
            return EmailSendResult.Failure(ex.Message);
        }
    }
    
    public async Task<EmailSendResult> SendEmailWithAttachmentsAsync(string to, string subject, string htmlBody, 
        IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = CreateMailMessage(to, subject, htmlBody);
            
            // Add attachments
            foreach (var attachment in attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                
                if (!string.IsNullOrEmpty(attachment.ContentId))
                {
                    mailAttachment.ContentId = attachment.ContentId;
                }
                
                if (attachment.IsInline)
                {
                    mailAttachment.ContentDisposition!.Inline = true;
                }
                
                message.Attachments.Add(mailAttachment);
            }
            
            using var client = CreateSmtpClient();
            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email with attachments sent successfully to {To}", to);
            return EmailSendResult.Success(Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to {To}", to);
            return EmailSendResult.Failure(ex.Message);
        }
    }
    
    public async Task<EmailSendResult> SendTemplatedEmailAsync(string to, string templateId, object templateData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Load template (this is a simple implementation - in production, use a template engine)
            var template = await LoadEmailTemplate(templateId, cancellationToken);
            var htmlBody = ProcessTemplate(template, templateData);
            var subject = ExtractSubjectFromTemplate(template, templateData);
            
            return await SendEmailAsync(to, subject, htmlBody, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email to {To} with template {TemplateId}", to, templateId);
            return EmailSendResult.Failure(ex.Message);
        }
    }
    
    public async Task<BulkEmailSendResult> SendBulkEmailAsync(IEnumerable<BulkEmailRecipient> recipients, 
        CancellationToken cancellationToken = default)
    {
        var result = new BulkEmailSendResult();
        var recipientsList = recipients.ToList();
        result.TotalAttempted = recipientsList.Count;
        
        // Send emails in parallel with throttling
        var semaphore = new SemaphoreSlim(_settings.MaxConcurrentSends);
        var tasks = recipientsList.Select(async recipient =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var sendResult = await SendEmailAsync(recipient.Email, recipient.Subject, recipient.HtmlBody, cancellationToken);
                return new IndividualEmailResult
                {
                    Email = recipient.Email,
                    Result = sendResult
                };
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var results = await Task.WhenAll(tasks);
        
        result.Results.AddRange(results);
        result.SuccessCount = results.Count(r => r.Result.IsSuccess);
        result.FailureCount = results.Count(r => !r.Result.IsSuccess);
        
        _logger.LogInformation("Bulk email completed: {Success}/{Total} successful", 
            result.SuccessCount, result.TotalAttempted);
        
        return result;
    }
    
    private MailMessage CreateMailMessage(string to, string subject, string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        
        message.To.Add(to);
        
        // Add reply-to if configured
        if (!string.IsNullOrEmpty(_settings.ReplyToEmail))
        {
            message.ReplyToList.Add(new MailAddress(_settings.ReplyToEmail));
        }
        
        return message;
    }
    
    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient
        {
            Host = _settings.SmtpHost,
            Port = _settings.SmtpPort,
            EnableSsl = _settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
        };
        
        return client;
    }
    
    private async Task<string> LoadEmailTemplate(string templateId, CancellationToken cancellationToken)
    {
        // In production, load from database or file system
        // This is a simple example
        var templatePath = Path.Combine(_settings.TemplatesPath, $"{templateId}.html");
        
        if (File.Exists(templatePath))
        {
            return await File.ReadAllTextAsync(templatePath, cancellationToken);
        }
        
        // Return default templates
        return templateId switch
        {
            "welcome" => "<h1>Welcome {{Name}}!</h1><p>Thank you for joining Engrslan.</p>",
            "reset-password" => "<h1>Reset Your Password</h1><p>Click <a href='{{ResetLink}}'>here</a> to reset your password.</p>",
            "email-confirmation" => "<h1>Confirm Your Email</h1><p>Click <a href='{{ConfirmationLink}}'>here</a> to confirm your email.</p>",
            _ => "<p>{{Content}}</p>"
        };
    }
    
    private string ProcessTemplate(string template, object data)
    {
        // Simple template processing - in production, use a proper template engine like Handlebars or Razor
        var result = template;
        var properties = data.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
            var value = prop.GetValue(data)?.ToString() ?? string.Empty;
            result = result.Replace($"{{{{{prop.Name}}}}}", value);
        }
        
        return result;
    }
    
    private string ExtractSubjectFromTemplate(string template, object data)
    {
        // Extract subject from template if it contains a subject tag
        // Otherwise use a default subject
        if (template.Contains("<!--SUBJECT:") && template.Contains("-->"))
        {
            var startIndex = template.IndexOf("<!--SUBJECT:") + 12;
            var endIndex = template.IndexOf("-->", startIndex);
            var subject = template.Substring(startIndex, endIndex - startIndex);
            return ProcessTemplate(subject, data);
        }
        
        return "Message from Engrslan";
    }
}

/// <summary>
/// Email configuration settings
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@certmanager.com";
    public string FromName { get; set; } = "Engrslan";
    public string? ReplyToEmail { get; set; }
    public string TemplatesPath { get; set; } = "EmailTemplates";
    public int MaxConcurrentSends { get; set; } = 5;
}