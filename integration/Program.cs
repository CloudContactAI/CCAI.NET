// .NET SDK integration tests — 42 tests
// Covers: SMS (1-6), MMS (7-17), Email (18-22), Webhook (23-29), Contact (30-31), Brands (32-36), Campaigns (37-42)

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CCAI.NET;
using CCAI.NET.Brands;
using CCAI.NET.Campaigns;
using CCAI.NET.Email;
using CCAI.NET.SMS;
using CCAI.NET.Webhook;

int passed = 0;
int failed = 0;

// ── Helpers ───────────────────────────────────────────────────────────────────

async Task Run(string name, Func<Task> fn)
{
    try
    {
        await fn();
        Console.WriteLine($"  PASS [{name}]");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAIL [{name}]: {ex.Message}");
        failed++;
    }
}

string MustEnv(string key)
{
    var val = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrEmpty(val))
    {
        Console.Error.WriteLine($"ERROR: required env var {key} is not set");
        Environment.Exit(2);
    }
    return val!;
}

string HmacSha256Base64(string secret, string message)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
    return Convert.ToBase64String(hash);
}

string WriteTempPng()
{
    var pngB64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwADhQGAWjR9awAAAABJRU5ErkJggg==";
    var buf = Convert.FromBase64String(pngB64);
    var path = Path.Combine(Path.GetTempPath(), $"ccai_test_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.png");
    File.WriteAllBytes(path, buf);
    return path;
}

// ── Setup ─────────────────────────────────────────────────────────────────────

var clientId  = MustEnv("CCAI_CLIENT_ID");
var apiKey    = MustEnv("CCAI_API_KEY");
var phone1    = MustEnv("CCAI_TEST_PHONE");
var phone2    = MustEnv("CCAI_TEST_PHONE_2");
var phone3    = MustEnv("CCAI_TEST_PHONE_3");
var email1    = MustEnv("CCAI_TEST_EMAIL");
var email2    = MustEnv("CCAI_TEST_EMAIL_2");
var email3    = MustEnv("CCAI_TEST_EMAIL_3");
var fn1       = MustEnv("CCAI_TEST_FIRST_NAME");
var ln1       = MustEnv("CCAI_TEST_LAST_NAME");
var fn2       = MustEnv("CCAI_TEST_FIRST_NAME_2");
var ln2       = MustEnv("CCAI_TEST_LAST_NAME_2");
var fn3       = MustEnv("CCAI_TEST_FIRST_NAME_3");
var ln3       = MustEnv("CCAI_TEST_LAST_NAME_3");
var webhookUrl = MustEnv("WEBHOOK_URL");

var client = new CCAIClient(new CCAIConfig
{
    ClientId = clientId,
    ApiKey = apiKey,
    UseTestEnvironment = true
});

var pngPath = WriteTempPng();

Console.WriteLine("==============================================");
Console.WriteLine("  CCAI .NET SDK Integration Tests");
Console.WriteLine("==============================================");

// ── SMS Tests (1-6) ───────────────────────────────────────────────────────────
Console.WriteLine("\n--- SMS ---");

// 01 — SMS.SendSingleAsync
await Run("01 SMS.SendSingleAsync", async () =>
{
    await client.SMS.SendSingleAsync(fn1, ln1, phone1, "Hello from .NET SDK!", ".NET Test");
});

// 02 — SMS.SendAsync (1 recipient)
await Run("02 SMS.SendAsync (1 recipient)", async () =>
{
    await client.SMS.SendAsync(
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
        "Hello 1 recipient!", ".NET Test");
});

// 03 — SMS.SendAsync (2 recipients)
await Run("03 SMS.SendAsync (2 recipients)", async () =>
{
    await client.SMS.SendAsync(
        [
            new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
            new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
        ],
        "Hello 2 recipients!", ".NET Test");
});

// 04 — SMS.SendAsync (3 recipients)
await Run("04 SMS.SendAsync (3 recipients)", async () =>
{
    await client.SMS.SendAsync(
        [
            new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
            new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
            new Account { FirstName = fn3, LastName = ln3, Phone = phone3 },
        ],
        "Hello 3 recipients!", ".NET Test");
});

// 05 — SMS.SendAsync with Data
await Run("05 SMS.SendAsync with Data", async () =>
{
    await client.SMS.SendAsync(
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, Data = new() { ["city"] = "Miami", ["offer"] = "20% off" } }],
        "Hello from ${city}! Claim your ${offer}.", ".NET Test Data");
});

// 06 — SMS.SendAsync with CustomData
await Run("06 SMS.SendAsync with CustomData", async () =>
{
    await client.SMS.SendAsync(
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, CustomData = "{\"trackingId\":\"abc123\"}" }],
        "Hello with messageData!", ".NET Test MsgData");
});

// ── MMS Tests (7-17) ──────────────────────────────────────────────────────────
Console.WriteLine("\n--- MMS ---");

SignedUrlResponse? signedUrlResp = null;
bool mmsDep = false;

// 07 — MMS.GetSignedUploadUrlAsync
await Run("07 MMS.GetSignedUploadUrlAsync", async () =>
{
    var resp = await client.MMS.GetSignedUploadUrlAsync("test_image.png", "image/png");
    if (string.IsNullOrEmpty(resp.SignedS3Url))
    {
        mmsDep = true;
        throw new Exception("SignedS3Url is empty");
    }
    signedUrlResp = resp;
});

// 08 — MMS.UploadImageToSignedUrlAsync
await Run("08 MMS.UploadImageToSignedUrlAsync", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    var ok = await client.MMS.UploadImageToSignedUrlAsync(signedUrlResp.SignedS3Url, pngPath, "image/png");
    if (!ok) throw new Exception("upload returned false");
});

// 09 — MMS.SendSingleAsync
await Run("09 MMS.SendSingleAsync", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendSingleAsync(signedUrlResp.FileKey, fn1, ln1, phone1, "MMS single!", ".NET MMS Test");
});

// 10 — MMS.SendAsync (1 recipient)
await Run("10 MMS.SendAsync (1 recipient)", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendAsync(signedUrlResp.FileKey,
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
        "MMS 1 recipient!", ".NET MMS Test");
});

// 11 — MMS.SendAsync (2 recipients)
await Run("11 MMS.SendAsync (2 recipients)", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendAsync(signedUrlResp.FileKey,
        [
            new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
            new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
        ],
        "MMS 2 recipients!", ".NET MMS Test");
});

// 12 — MMS.SendAsync (3 recipients)
await Run("12 MMS.SendAsync (3 recipients)", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendAsync(signedUrlResp.FileKey,
        [
            new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
            new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
            new Account { FirstName = fn3, LastName = ln3, Phone = phone3 },
        ],
        "MMS 3 recipients!", ".NET MMS Test");
});

// 13 — MMS.SendAsync with Data
await Run("13 MMS.SendAsync with Data", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendAsync(signedUrlResp.FileKey,
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, Data = new() { ["product"] = "Widget" } }],
        "Check out ${product}!", ".NET MMS Data");
});

// 14 — MMS.SendAsync with CustomData
await Run("14 MMS.SendAsync with CustomData", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.SendAsync(signedUrlResp.FileKey,
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, CustomData = "{\"campaignId\":\"mms-net-001\"}" }],
        "MMS with messageData!", ".NET MMS MsgData");
});

// 15 — MMS.CheckFileUploadedAsync
await Run("15 MMS.CheckFileUploadedAsync", async () =>
{
    if (mmsDep || signedUrlResp == null) throw new Exception("dependency test 07 failed");
    await client.MMS.CheckFileUploadedAsync(signedUrlResp.FileKey);
});

// 16 — MMS.SendWithImageAsync (fresh upload)
await Run("16 MMS.SendWithImageAsync (fresh upload)", async () =>
{
    if (mmsDep) throw new Exception("dependency test 07 failed");
    await client.MMS.SendWithImageAsync(pngPath, "image/png",
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
        "MMS with image!", ".NET MMS Image",
        forceNewCampaign: true);
});

// 17 — MMS.SendWithImageAsync (cached)
await Run("17 MMS.SendWithImageAsync (cached)", async () =>
{
    if (mmsDep) throw new Exception("dependency test 07 failed");
    await client.MMS.SendWithImageAsync(pngPath, "image/png",
        [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
        "MMS cached image!", ".NET MMS Cache",
        forceNewCampaign: true);
});

// ── Email Tests (18-22) ───────────────────────────────────────────────────────
Console.WriteLine("\n--- Email ---");

const string SenderEmail = "noreply@cloudcontactai.com";
const string SenderName  = "CCAI Test";
const string ReplyEmail  = "noreply@cloudcontactai.com";

// 18 — Email.SendSingleAsync
await Run("18 Email.SendSingleAsync", async () =>
{
    await client.Email.SendSingleAsync(
        fn1, ln1, email1,
        ".NET SDK Test Email", "<p>Hello from .NET SDK!</p>",
        senderEmail: SenderEmail, replyEmail: ReplyEmail, senderName: SenderName,
        title: ".NET Email Test");
});

// 19 — Email.SendCampaignAsync (1 recipient)
await Run("19 Email.SendCampaignAsync (1 recipient)", async () =>
{
    await client.Email.SendCampaignAsync(new EmailCampaign
    {
        Subject = ".NET SDK Email 1",
        Title = ".NET Email Test",
        Message = "<p>Hello 1!</p>",
        SenderEmail = SenderEmail, ReplyEmail = ReplyEmail, SenderName = SenderName,
        Accounts = [new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 }]
    });
});

// 20 — Email.SendCampaignAsync (2 recipients)
await Run("20 Email.SendCampaignAsync (2 recipients)", async () =>
{
    await client.Email.SendCampaignAsync(new EmailCampaign
    {
        Subject = ".NET SDK Email 2",
        Title = ".NET Email Test",
        Message = "<p>Hello 2!</p>",
        SenderEmail = SenderEmail, ReplyEmail = ReplyEmail, SenderName = SenderName,
        Accounts =
        [
            new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
            new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
        ]
    });
});

// 21 — Email.SendCampaignAsync (3 recipients)
await Run("21 Email.SendCampaignAsync (3 recipients)", async () =>
{
    await client.Email.SendCampaignAsync(new EmailCampaign
    {
        Subject = ".NET SDK Email 3",
        Title = ".NET Email Test",
        Message = "<p>Hello 3!</p>",
        SenderEmail = SenderEmail, ReplyEmail = ReplyEmail, SenderName = SenderName,
        Accounts =
        [
            new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
            new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
            new EmailAccount { FirstName = fn3, LastName = ln3, Email = email3 },
        ]
    });
});

// 22 — Email.SendCampaignAsync (full campaign object)
await Run("22 Email.SendCampaignAsync (full campaign)", async () =>
{
    await client.Email.SendCampaignAsync(new EmailCampaign
    {
        Subject = ".NET SDK Campaign Test",
        Title = ".NET Email Campaign",
        Message = "<p>Campaign email from .NET SDK!</p>",
        SenderEmail = SenderEmail, ReplyEmail = ReplyEmail, SenderName = SenderName,
        Accounts =
        [
            new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
            new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
        ]
    });
});

// ── Webhook Tests (23-29) ─────────────────────────────────────────────────────
Console.WriteLine("\n--- Webhook ---");

const string WebhookSecret = "test-webhook-secret-dotnet";
int registeredWebhookId = 0;

// 23 — Webhook.RegisterAsync
await Run("23 Webhook.RegisterAsync", async () =>
{
    var resp = await client.Webhook.RegisterAsync(new WebhookConfig
    {
        Url = webhookUrl,
        Secret = WebhookSecret
    });

    if (resp.Id == 0) throw new Exception("webhook ID is 0 after register");
    registeredWebhookId = resp.Id;
});

// 24 — Webhook.ListAsync
await Run("24 Webhook.ListAsync", async () =>
{
    var hooks = await client.Webhook.ListAsync();
    if (hooks == null || hooks.Count == 0) throw new Exception("expected at least one webhook, got 0");
});

// 25 — Webhook.UpdateAsync
await Run("25 Webhook.UpdateAsync", async () =>
{
    if (registeredWebhookId == 0) throw new Exception("no webhook ID from test 23");
    await client.Webhook.UpdateAsync(registeredWebhookId, new WebhookConfig
    {
        Url = webhookUrl + "?updated=1",
        Secret = "updated-secret-dotnet"
    });
});

// 26 — Webhook.VerifySignature (valid)
await Run("26 Webhook.VerifySignature (valid)", async () =>
{
    await Task.CompletedTask;
    const string eventHash = "abc123eventHash";
    var sig = HmacSha256Base64(WebhookSecret, $"{clientId}:{eventHash}");
    var ok = client.Webhook.VerifySignature(sig, clientId, eventHash, WebhookSecret);
    if (!ok) throw new Exception("expected valid signature to return true");
});

// 27 — Webhook.VerifySignature (invalid)
await Run("27 Webhook.VerifySignature (invalid)", async () =>
{
    await Task.CompletedTask;
    var ok = client.Webhook.VerifySignature("invalidsig==", clientId, "somehash", WebhookSecret);
    if (ok) throw new Exception("expected invalid signature to return false");
});

// 28 — Webhook.ParseCloudContactEvent
await Run("28 Webhook.ParseCloudContactEvent", async () =>
{
    await Task.CompletedTask;
    var payload = "{\"eventType\":\"message.sent\",\"data\":{\"To\":\"+15005550001\",\"Message\":\"test\",\"MessageStatus\":\"DELIVERED\",\"SmsSid\":1,\"CampaignId\":0,\"CampaignTitle\":\"\",\"CustomData\":\"\",\"ClientExternalId\":\"\"}}";
    var evt = client.Webhook.ParseCloudContactEvent(payload);
    if (string.IsNullOrEmpty(evt.EventType)) throw new Exception("EventType is empty after ParseCloudContactEvent");
});

// 29 — Webhook.DeleteAsync
await Run("29 Webhook.DeleteAsync", async () =>
{
    if (registeredWebhookId == 0) throw new Exception("no webhook ID from test 23");
    await client.Webhook.DeleteAsync(registeredWebhookId);
});

// ── Contact Tests (30-31) ─────────────────────────────────────────────────────
Console.WriteLine("\n--- Contact ---");

// 30 — Contact.SetDoNotTextAsync(true)
await Run("30 Contact.SetDoNotTextAsync(true)", async () =>
{
    await client.Contact.SetDoNotTextAsync(true, phone: phone1);
});

// 31 — Contact.SetDoNotTextAsync(false)
await Run("31 Contact.SetDoNotTextAsync(false)", async () =>
{
    await client.Contact.SetDoNotTextAsync(false, phone: phone1);
});

// ── Brand Tests (32-36) ───────────────────────────────────────────────────────
Console.WriteLine("\n--- Brands ---");

long brandId = 0;

// 32 — Brand.CreateAsync
await Run("32 Brand.CreateAsync", async () =>
{
    var resp = await client.Brands.CreateAsync(new BrandRequest
    {
        LegalCompanyName = "Test Company LLC",
        EntityType       = "PRIVATE_PROFIT",
        TaxId            = "123456789",
        TaxIdCountry     = "US",
        Country          = "US",
        VerticalType     = "TECHNOLOGY",
        WebsiteUrl       = "https://example.com",
        Street           = "123 Main St",
        City             = "Miami",
        State            = "FL",
        PostalCode       = "33101",
        ContactFirstName = fn1,
        ContactLastName  = ln1,
        ContactEmail     = email1,
        ContactPhone     = phone1
    });
    if (resp.Id == 0) throw new Exception("Invalid brand id");
    brandId = resp.Id;
});

// 33 — Brand.GetAsync
await Run("33 Brand.GetAsync", async () =>
{
    if (brandId == 0) throw new Exception("dependency test 32 failed");
    var resp = await client.Brands.GetAsync(brandId);
    if (resp.Id != brandId) throw new Exception("Brand id mismatch");
});

// 34 — Brand.ListAsync
await Run("34 Brand.ListAsync", async () =>
{
    var list = await client.Brands.ListAsync();
    if (list == null) throw new Exception("Null response");
});

// 35 — Brand.UpdateAsync
await Run("35 Brand.UpdateAsync", async () =>
{
    if (brandId == 0) throw new Exception("dependency test 32 failed");
    var resp = await client.Brands.UpdateAsync(brandId, new BrandRequest { City = "Orlando" });
    if (resp.Id != brandId) throw new Exception("Brand id mismatch after update");
});

// 36 — Brand.DeleteAsync
await Run("36 Brand.DeleteAsync", async () =>
{
    if (brandId == 0) throw new Exception("dependency test 32 failed");
    await client.Brands.DeleteAsync(brandId);
});

// ── Campaign Tests (37-42) ────────────────────────────────────────────────────
Console.WriteLine("\n--- Campaigns ---");

long campaignBrandId = 0;
long campaignId = 0;

// 37 — Campaign setup: create brand
await Run("37 Campaign setup — Brand.CreateAsync", async () =>
{
    var resp = await client.Brands.CreateAsync(new BrandRequest
    {
        LegalCompanyName = "Campaign Test LLC",
        EntityType       = "PRIVATE_PROFIT",
        TaxId            = "987654321",
        TaxIdCountry     = "US",
        Country          = "US",
        VerticalType     = "TECHNOLOGY",
        WebsiteUrl       = "https://example.com",
        Street           = "456 Test Ave",
        City             = "Miami",
        State            = "FL",
        PostalCode       = "33101",
        ContactFirstName = fn1,
        ContactLastName  = ln1,
        ContactEmail     = email1,
        ContactPhone     = phone1
    });
    if (resp.Id == 0) throw new Exception("Invalid brand id");
    campaignBrandId = resp.Id;
});

// 38 — Campaign.CreateAsync
await Run("38 Campaign.CreateAsync", async () =>
{
    if (campaignBrandId == 0) throw new Exception("dependency test 37 failed");
    var resp = await client.Campaigns.CreateAsync(new CampaignRequest
    {
        BrandId          = campaignBrandId,
        UseCase          = "MARKETING",
        Description      = "Integration test campaign for automated testing",
        MessageFlow      = "Customers opt-in via website form at https://example.com/sms-signup",
        HasEmbeddedLinks = false,
        HasEmbeddedPhone = false,
        IsAgeGated       = false,
        IsDirectLending  = false,
        OptInKeywords    = ["START", "YES"],
        OptInMessage     = "You have opted in to receive messages. Reply STOP to unsubscribe.",
        OptInProofUrl    = "https://example.com/opt-in-proof",
        HelpKeywords     = ["HELP", "INFO"],
        HelpMessage      = "For help reply HELP or call 1-800-555-0000.",
        OptOutKeywords   = ["STOP", "END"],
        OptOutMessage    = "You have been unsubscribed. Reply START to opt back in. STOP",
        SampleMessages   =
        [
            "Hello ${firstName}, this is a test message. Reply STOP to unsubscribe.",
            "Reminder: your appointment is tomorrow. Reply HELP for assistance."
        ]
    });
    if (resp.Id == 0) throw new Exception("Invalid campaign id");
    campaignId = resp.Id;
});

// 39 — Campaign.GetAsync
await Run("39 Campaign.GetAsync", async () =>
{
    if (campaignId == 0) throw new Exception("dependency test 38 failed");
    var resp = await client.Campaigns.GetAsync(campaignId);
    if (resp.Id != campaignId) throw new Exception("Campaign id mismatch");
});

// 40 — Campaign.ListAsync
await Run("40 Campaign.ListAsync", async () =>
{
    var list = await client.Campaigns.ListAsync();
    if (list == null) throw new Exception("Null response");
});

// 41 — Campaign.UpdateAsync
await Run("41 Campaign.UpdateAsync", async () =>
{
    if (campaignId == 0) throw new Exception("dependency test 38 failed");
    var resp = await client.Campaigns.UpdateAsync(campaignId, new CampaignRequest
    {
        Description = "Updated integration test campaign description"
    });
    if (resp.Id != campaignId) throw new Exception("Campaign id mismatch after update");
});

// 42 — Campaign.DeleteAsync + cleanup brand
await Run("42 Campaign.DeleteAsync", async () =>
{
    if (campaignId == 0) throw new Exception("dependency test 38 failed");
    await client.Campaigns.DeleteAsync(campaignId);
    if (campaignBrandId != 0) await client.Brands.DeleteAsync(campaignBrandId);
});

// ── Cleanup & Results ──────────────────────────────────────────────────────────
client.Dispose();
if (File.Exists(pngPath)) File.Delete(pngPath);

Console.WriteLine("\n==============================================");
Console.WriteLine($"  RESULTS: {passed} passed, {failed} failed");
Console.WriteLine("==============================================");

var summary = JsonSerializer.Serialize(new { sdk = "dotnet", passed, failed, total = passed + failed });
Console.WriteLine($"\nSUMMARY_JSON: {summary}");

Environment.Exit(failed > 0 ? 1 : 0);
