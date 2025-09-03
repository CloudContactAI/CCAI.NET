// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CCAI.NET.Webhook;
using System.Threading.Tasks;

namespace CCAI.NET.Examples;

/// <summary>
/// Production-ready webhook endpoint example for CloudContact webhooks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly WebhookService _webhookService;

    public WebhookController(ILogger<WebhookController> logger, WebhookService webhookService)
    {
        _logger = logger;
        _webhookService = webhookService;
    }

    /// <summary>
    /// Handle CloudContact webhook events
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] CloudContactWebhookEvent webhookEvent)
    {
        try
        {
            _logger.LogInformation("Received webhook event: {EventType}", webhookEvent.EventType);

            // Process the webhook event based on type
            switch (webhookEvent.EventType)
            {
                case "message.sent":
                    await HandleMessageSent(webhookEvent.Data);
                    break;
                    
                case "message.incoming":
                    await HandleMessageIncoming(webhookEvent.Data);
                    break;
                    
                case "message.excluded":
                    await HandleMessageExcluded(webhookEvent.Data);
                    break;
                    
                case "message.error.carrier":
                    await HandleCarrierError(webhookEvent.Data);
                    break;
                    
                case "message.error.cloudcontact":
                    await HandleCloudContactError(webhookEvent.Data);
                    break;
                    
                default:
                    _logger.LogWarning("Unknown webhook event type: {EventType}", webhookEvent.EventType);
                    break;
            }

            return Ok(new { status = "success", message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook event: {EventType}", webhookEvent.EventType);
            return StatusCode(500, new { status = "error", message = "Internal server error" });
        }
    }

    private Task HandleMessageSent(WebhookEventData data)
    {
        _logger.LogInformation("Message sent successfully: SMS ID {SmsSid}, Cost: ${Cost}", 
            data.SmsSid, data.TotalPrice);
        
        // Update your database with delivery confirmation
        // await _messageService.MarkAsDelivered(data.SmsSid, data.TotalPrice);
        
        // Track campaign performance
        if (data.CampaignId > 0)
        {
            // await _campaignService.RecordDelivery(data.CampaignId, data.TotalPrice);
        }
        
        return Task.CompletedTask;
    }
    
    private Task HandleMessageIncoming(WebhookEventData data)
    {
        _logger.LogInformation("Received reply from {From}: {Message}", data.From, data.Message);
        
        // Store the incoming message
        // await _messageService.StoreIncomingMessage(data);
        
        // Check for auto-response triggers
        var message = data.Message.ToLower();
        if (message.Contains("stop") || message.Contains("unsubscribe"))
        {
            // Handle opt-out
            // await _contactService.OptOut(data.From);
        }
        else if (message.Contains("help"))
        {
            // Send help information
            // await _smsService.SendHelpMessage(data.From);
        }
        
        // Trigger lead scoring or CRM updates
        // await _crmService.UpdateContactEngagement(data.ClientExternalId, data.Message);
        
        return Task.CompletedTask;
    }
    
    private Task HandleMessageExcluded(WebhookEventData data)
    {
        _logger.LogWarning("Message excluded: {Reason} for {Phone}", data.ExcludedReason, data.To);
        
        // Track exclusion reasons for campaign optimization
        // await _campaignService.RecordExclusion(data.CampaignId, data.ExcludedReason);
        
        // Handle specific exclusion reasons
        if (data.ExcludedReason?.Contains("Duplicate") == true)
        {
            // Clean up duplicate contacts
            // await _contactService.MergeDuplicates(data.To);
        }
        else if (data.ExcludedReason?.Contains("Invalid") == true)
        {
            // Mark phone number as invalid
            // await _contactService.MarkAsInvalid(data.To);
        }
        
        return Task.CompletedTask;
    }
    
    private Task HandleCarrierError(WebhookEventData data)
    {
        _logger.LogError("Carrier error {ErrorCode}: {ErrorMessage} for {Phone}", 
            data.ErrorCode, data.ErrorMessage, data.To);
        
        // Update message status in database
        // await _messageService.MarkAsFailed(data.SmsSid, data.ErrorCode, data.ErrorMessage);
        
        // Handle specific error codes
        switch (data.ErrorCode)
        {
            case "30008": // Unknown destination handset
            case "21614": // Invalid mobile number
                // Mark number as invalid to prevent future sends
                // await _contactService.MarkAsInvalid(data.To);
                break;
                
            case "30007": // Message delivery failed
                // Could be temporary, maybe retry later
                // await _messageService.ScheduleRetry(data.SmsSid);
                break;
        }
        
        return Task.CompletedTask;
    }
    
    private Task HandleCloudContactError(WebhookEventData data)
    {
        _logger.LogError("CloudContact error {ErrorCode}: {ErrorMessage}", 
            data.ErrorCode, data.ErrorMessage);
        
        // Handle system-level errors
        // await _alertService.NotifyAdmins($"CloudContact system error: {data.ErrorMessage}");
        // await _messageService.MarkAsFailed(data.SmsSid, data.ErrorCode, data.ErrorMessage);
        
        // Handle specific CloudContact error codes
        switch (data.ErrorCode)
        {
            case "CCAI-001": // Insufficient balance
                // Send alert to admin
                // await _alertService.SendLowBalanceAlert();
                break;
                
            case "CCAI-002": // Account suspended
                // Pause all campaigns
                // await _campaignService.PauseAllCampaigns();
                break;
                
            case "CCAI-003": // Message quota exceeded
                // Implement rate limiting
                // await _rateLimitService.EnableRateLimit();
                break;
        }
        
        return Task.CompletedTask;
    }
}
