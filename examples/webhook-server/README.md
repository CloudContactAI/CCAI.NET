# CloudContactAI Webhook Server

A simple ASP.NET Core webhook server that processes CloudContactAI webhook events with enhanced parsing and logging.

## Features

- **CloudContactAI Event Processing**: Automatically parses and displays all 5 CloudContactAI event types
- **Enhanced Logging**: Color-coded console output with emojis for easy identification
- **Event Type Support**:
  - `message.sent` - Message delivery confirmations
  - `message.incoming` - Incoming replies from recipients
  - `message.excluded` - Messages excluded from campaigns
  - `message.error.carrier` - Carrier-level delivery failures
  - `message.error.cloudcontact` - CloudContactAI system errors
- **Backward Compatibility**: Still handles legacy webhook formats
- **CORS Enabled**: Works with ngrok and external services

## Quick Start

1. **Build the server**:
   ```bash
   git clone https://github.com/CloudContactAI/CCAI.NET.git
   cd CCAI.NET/examples/webhook-server
   dotnet add package CloudContactAI.CCAI.NET
   dotnet add package DotNetEnv
   dotnet build 
   ```

2. **Configure the server**:

   Create a `.env` file in your project root:

   ```bash
   CCAI_CLIENT_ID=1231
   CCAI_API_KEY=your-api-key-here
   ```
3. **Start the Server**
   ```bash
   dotnet run
   ```

4. **Server runs on**: `http://localhost:3000`

5. **Test with sample events**:
   ```bash
   ./test_webhook.sh
   ```

## Usage with ngrok

1. **Install ngrok**: https://ngrok.com/download

2. **Start the webhook server**:
   ```bash
   dotnet run
   ```

3. **In another terminal, start ngrok**:
   ```bash
   ngrok http 3000
   ```

4. **Configure CloudContact**:
   - Copy the ngrok HTTPS URL (e.g., `https://abc123.ngrok.io`)
   - In CloudContactAI Settings\Webhooks, set webhook URL to: `https://abc123.ngrok.io/webhook`

5. **Send a message with CloudContactAI**:
   - Send a message with the sms-sender project [here](https://github.com/CloudContactAI/CCAI.NET/tree/main/examples/sms-sender)
   - Send a message through the CloudContactAI UI


## Sample Output

When a webhook is received, you'll see output like:

```
🔔 Received webhook event at /webhook path!
⏰ Time: 2025-09-02 17:52:33 UTC
📋 Headers:
  Content-Type: application/json
  User-Agent: CloudContact-Webhook/1.0
📄 Raw Body:
{"eventType":"message.sent","data":{"SmsSid":12345,"MessageStatus":"DELIVERED"...}}

🎯 Parsed CloudContactAI Event:
   Event Type: message.sent
   Message Status: DELIVERED
   To: +1234567890
   Message: Hello! Your order #12345 has been shipped.
   ✅ Message delivered successfully!
   💰 Cost: $0.0200
   📊 Segments: 2
   📢 Campaign: Order Notifications (ID: 67890)
   📝 Custom Data: order_id:12345,customer_type:premium
   🆔 External ID: customer_abc123
─────────────────────────────────────────────────────
```

## Event Types

### message.sent
- Confirms successful message delivery
- Shows cost, segments, and campaign info
- Includes delivery timestamps

### message.incoming  
- Processes replies from recipients
- Links back to original campaigns
- Useful for engagement tracking

### message.excluded
- Shows why messages were filtered out
- Common reasons: duplicates, invalid numbers, opt-outs
- Helps optimize campaign targeting

### message.error.carrier
- Carrier-level delivery failures
- Error codes like 30008 (invalid number)
- Helps identify bad phone numbers

### message.error.cloudcontact
- System-level errors
- Account issues (CCAI-001: insufficient balance)
- Service configuration problems

## Integration

Use this server as a starting point for your webhook processing. You can:

- Add database logging
- Implement business logic for each event type
- Forward events to other systems
- Set up alerting for errors
- Track campaign performance metrics

## Testing

The included `test_webhook.sh` script sends sample events of all types to test your webhook processing logic.
