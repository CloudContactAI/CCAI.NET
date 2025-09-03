#!/bin/bash

echo "Testing CloudContact Webhook Server"
echo "=================================="

# Test message.sent event
echo "1. Testing message.sent event..."
curl -X POST http://localhost:3000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "message.sent",
    "data": {
      "SmsSid": 12345,
      "MessageStatus": "DELIVERED",
      "To": "+1234567890",
      "Message": "Hello! Your order #12345 has been shipped.",
      "CustomData": "order_id:12345,customer_type:premium",
      "ClientExternalId": "customer_abc123",
      "CampaignId": 67890,
      "CampaignTitle": "Order Notifications",
      "Segments": 2,
      "TotalPrice": 0.02
    }
  }'

echo -e "\n\n2. Testing message.incoming event..."
curl -X POST http://localhost:3000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "message.incoming",
    "data": {
      "SmsSid": 0,
      "MessageStatus": "RECEIVED",
      "To": "+0987654321",
      "Message": "Yes, I am interested in learning more!",
      "CustomData": "",
      "ClientExternalId": "customer_abc123",
      "CampaignId": 67890,
      "CampaignTitle": "Lead Generation Campaign",
      "From": "+1234567890"
    }
  }'

echo -e "\n\n3. Testing message.excluded event..."
curl -X POST http://localhost:3000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "message.excluded",
    "data": {
      "SmsSid": 0,
      "MessageStatus": "EXCLUDED",
      "To": "+1234567890",
      "Message": "Hi {{name}}, check out our new products!",
      "CustomData": "lead_source:website,segment:new_users",
      "ClientExternalId": "customer_xyz789",
      "CampaignId": 67890,
      "CampaignTitle": "Product Launch Campaign",
      "ExcludedReason": "Duplicate phone number in campaign"
    }
  }'

echo -e "\n\n4. Testing message.error.carrier event..."
curl -X POST http://localhost:3000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "message.error.carrier",
    "data": {
      "SmsSid": 12345,
      "MessageStatus": "FAILED",
      "To": "+1234567890",
      "Message": "Your verification code is: 123456",
      "CustomData": "verification_attempt:1",
      "ClientExternalId": "user_def456",
      "CampaignId": 0,
      "CampaignTitle": "",
      "ErrorCode": "30008",
      "ErrorMessage": "Unknown destination handset",
      "ErrorType": "carrier"
    }
  }'

echo -e "\n\n5. Testing message.error.cloudcontact event..."
curl -X POST http://localhost:3000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "eventType": "message.error.cloudcontact",
    "data": {
      "SmsSid": 12345,
      "MessageStatus": "FAILED",
      "To": "+1234567890",
      "Message": "Welcome to our service!",
      "CustomData": "signup_source:landing_page",
      "ClientExternalId": "new_user_ghi789",
      "CampaignId": 67890,
      "CampaignTitle": "Welcome Series",
      "ErrorCode": "CCAI-001",
      "ErrorMessage": "Insufficient account balance",
      "ErrorType": "cloudcontact"
    }
  }'

echo -e "\n\nAll webhook tests completed!"
