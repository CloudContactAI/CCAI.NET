// .NET SDK integration tests — 52 tests
// Covers: SMS (1-6), MMS (7-17), Email (18-22), Webhook (23-29), Contact (30-31),
// Brands (32-36), Campaigns (37-42), ContactValidator (43-46), Negative cases (47-52)
//
// Test results use three states:
//   PASS — the test ran and all assertions held
//   FAIL — the test ran and an assertion (or the API call) failed
//   SKIP — a prerequisite test failed, so this test could not run
//
// Resources created during the run (webhooks, brands, campaigns) are tracked and
// deleted in a final cleanup block even if tests fail midway.
// Exits with code 1 if any test fails, 2 if required env vars are missing.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CCAI.NET;
using CCAI.NET.Brands;
using CCAI.NET.Campaigns;
using CCAI.NET.Email;
using CCAI.NET.SMS;
using CCAI.NET.Webhook;
using CCAI.NET.ContactValidator;

int passed = 0;
int failed = 0;
int skipped = 0;

// ── Helpers ───────────────────────────────────────────────────────────────────

async Task Run(string name, Func<Task> fn)
{
    try
    {
        await fn();
        Console.WriteLine($"  PASS [{name}]");
        passed++;
    }
    catch (SkipTestException ex)
    {
        Console.WriteLine($"  SKIP [{name}]: {ex.Message}");
        skipped++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  FAIL [{name}]: {ex.Message}");
        failed++;
    }
}

// Runs fn and asserts that it throws — used by the negative test cases.
async Task ExpectErrorAsync(string what, Func<Task> fn)
{
    bool didFail;
    try
    {
        await fn();
        didFail = false;
    }
    catch (SkipTestException)
    {
        throw;
    }
    catch (Exception)
    {
        didFail = true;
    }
    if (!didFail) throw new Exception($"expected {what} to fail, but it succeeded");
}

// Asserts that a send-style response carries a campaign/message identifier.
void AssertSmsResponse(SMSResponse? resp)
{
    if (resp is null) throw new Exception("empty response");
    if (string.IsNullOrEmpty(resp.Id) && string.IsNullOrEmpty(resp.CampaignId))
        throw new Exception("response has no Id/CampaignId");
}

void AssertEmailResponse(EmailResponse? resp)
{
    if (resp is null) throw new Exception("empty response");
    if (string.IsNullOrEmpty(resp.Id) && string.IsNullOrEmpty(resp.CampaignId) && string.IsNullOrEmpty(resp.ResponseId))
        throw new Exception("response has no Id/CampaignId/ResponseId");
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

// Validate ALL required env vars up front and report every missing one,
// instead of failing later with a cryptic API error.
string[] requiredEnv =
[
    "CCAI_CLIENT_ID", "CCAI_API_KEY",
    "CCAI_TEST_PHONE", "CCAI_TEST_PHONE_2", "CCAI_TEST_PHONE_3",
    "CCAI_TEST_EMAIL", "CCAI_TEST_EMAIL_2", "CCAI_TEST_EMAIL_3",
    "CCAI_TEST_FIRST_NAME", "CCAI_TEST_LAST_NAME",
    "CCAI_TEST_FIRST_NAME_2", "CCAI_TEST_LAST_NAME_2",
    "CCAI_TEST_FIRST_NAME_3", "CCAI_TEST_LAST_NAME_3",
    "WEBHOOK_URL",
];
var missing = requiredEnv.Where(k => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(k))).ToList();
if (missing.Count > 0)
{
    Console.Error.WriteLine($"ERROR: required env vars are not set: {string.Join(", ", missing)}");
    Environment.Exit(2);
}

var clientId = Environment.GetEnvironmentVariable("CCAI_CLIENT_ID")!;
var apiKey   = Environment.GetEnvironmentVariable("CCAI_API_KEY")!;
var phone1   = Environment.GetEnvironmentVariable("CCAI_TEST_PHONE")!;
var phone2   = Environment.GetEnvironmentVariable("CCAI_TEST_PHONE_2")!;
var phone3   = Environment.GetEnvironmentVariable("CCAI_TEST_PHONE_3")!;
var email1   = Environment.GetEnvironmentVariable("CCAI_TEST_EMAIL")!;
var email2   = Environment.GetEnvironmentVariable("CCAI_TEST_EMAIL_2")!;
var email3   = Environment.GetEnvironmentVariable("CCAI_TEST_EMAIL_3")!;
var fn1      = Environment.GetEnvironmentVariable("CCAI_TEST_FIRST_NAME")!;
var ln1      = Environment.GetEnvironmentVariable("CCAI_TEST_LAST_NAME")!;
var fn2      = Environment.GetEnvironmentVariable("CCAI_TEST_FIRST_NAME_2")!;
var ln2      = Environment.GetEnvironmentVariable("CCAI_TEST_LAST_NAME_2")!;
var fn3      = Environment.GetEnvironmentVariable("CCAI_TEST_FIRST_NAME_3")!;
var ln3      = Environment.GetEnvironmentVariable("CCAI_TEST_LAST_NAME_3")!;

// Unique per-run suffix so parallel SDK runs don't collide on the same webhook URL
var runId = $"dotnet-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
var webhookBase = Environment.GetEnvironmentVariable("WEBHOOK_URL")!;
var webhookUrl = webhookBase + (webhookBase.Contains('?') ? "&" : "?") + $"run={runId}";

var senderEmail = Environment.GetEnvironmentVariable("CCAI_TEST_SENDER_EMAIL") is { Length: > 0 } se ? se : "noreply@cloudcontactai.com";
var replyEmail = senderEmail;
const string SenderName = "CCAI Test";
var webhookSecret = Environment.GetEnvironmentVariable("CCAI_WEBHOOK_SECRET") is { Length: > 0 } ws ? ws : "test-webhook-secret-dotnet";

// If EXISTING_WEBHOOK_ID is set, reuse that webhook instead of creating one,
// and keep it at the end (do not delete).
var existingWebhookId = int.TryParse(Environment.GetEnvironmentVariable("EXISTING_WEBHOOK_ID"), out var ewid) ? ewid : 0;

// Use CCAI_BASE_URL if set (local dev), otherwise fall back to test environment
var client = new CCAIClient(new CCAIConfig
{
    ClientId = clientId,
    ApiKey = apiKey,
    UseTestEnvironment = Environment.GetEnvironmentVariable("CCAI_BASE_URL") is null
});

var pngPath = WriteTempPng();

// IDs of resources created by the tests; anything still listed here at the end
// of the run is deleted by the finally block (tests remove entries they already
// deleted themselves).
var cleanupWebhookIds = new List<int>();
var cleanupBrandIds = new List<long>();
var cleanupCampaignIds = new List<long>();

Console.WriteLine("==============================================");
Console.WriteLine("  CCAI .NET SDK Integration Tests");
Console.WriteLine("==============================================");

try
{
    // ── SMS Tests (1-6) ───────────────────────────────────────────────────────
    Console.WriteLine("\n--- SMS ---");

    // 01 — SMS.SendSingleAsync
    await Run("01 SMS.SendSingleAsync", async () =>
    {
        var resp = await client.SMS.SendSingleAsync(fn1, ln1, phone1, "Hello from .NET SDK!", ".NET Test");
        AssertSmsResponse(resp);
    });

    // 02 — SMS.SendAsync (1 recipient)
    await Run("02 SMS.SendAsync (1 recipient)", async () =>
    {
        var resp = await client.SMS.SendAsync(
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
            "Hello 1 recipient!", ".NET Test");
        AssertSmsResponse(resp);
    });

    // 03 — SMS.SendAsync (2 recipients)
    await Run("03 SMS.SendAsync (2 recipients)", async () =>
    {
        var resp = await client.SMS.SendAsync(
            [
                new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
                new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
            ],
            "Hello 2 recipients!", ".NET Test");
        AssertSmsResponse(resp);
    });

    // 04 — SMS.SendAsync (3 recipients)
    await Run("04 SMS.SendAsync (3 recipients)", async () =>
    {
        var resp = await client.SMS.SendAsync(
            [
                new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
                new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
                new Account { FirstName = fn3, LastName = ln3, Phone = phone3 },
            ],
            "Hello 3 recipients!", ".NET Test");
        AssertSmsResponse(resp);
    });

    // 05 — SMS.SendAsync with Data
    await Run("05 SMS.SendAsync with Data", async () =>
    {
        var resp = await client.SMS.SendAsync(
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, Data = new() { ["city"] = "Miami", ["offer"] = "20% off" } }],
            "Hello from ${city}! Claim your ${offer}.", ".NET Test Data");
        AssertSmsResponse(resp);
    });

    // 06 — SMS.SendAsync with CustomData
    await Run("06 SMS.SendAsync with CustomData", async () =>
    {
        var resp = await client.SMS.SendAsync(
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, CustomData = "{\"trackingId\":\"abc123\"}" }],
            "Hello with messageData!", ".NET Test MsgData");
        AssertSmsResponse(resp);
    });

    // ── MMS Tests (7-17) ──────────────────────────────────────────────────────
    Console.WriteLine("\n--- MMS ---");

    SignedUrlResponse? signedUrlResp = null;
    bool uploadOk = false;

    // 07 — MMS.GetSignedUploadUrlAsync
    await Run("07 MMS.GetSignedUploadUrlAsync", async () =>
    {
        var resp = await client.MMS.GetSignedUploadUrlAsync("test_image.png", "image/png");
        if (string.IsNullOrEmpty(resp.SignedS3Url)) throw new Exception("SignedS3Url is empty");
        if (string.IsNullOrEmpty(resp.FileKey)) throw new Exception("FileKey is empty");
        signedUrlResp = resp;
    });

    // 08 — MMS.UploadImageToSignedUrlAsync
    await Run("08 MMS.UploadImageToSignedUrlAsync", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var ok = await client.MMS.UploadImageToSignedUrlAsync(signedUrlResp.SignedS3Url, pngPath, "image/png");
        if (!ok) throw new Exception("upload returned false");
        uploadOk = true;
    });

    // 09 — MMS.SendSingleAsync
    await Run("09 MMS.SendSingleAsync", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendSingleAsync(signedUrlResp.FileKey, fn1, ln1, phone1, "MMS single!", ".NET MMS Test");
        AssertSmsResponse(resp);
    });

    // 10 — MMS.SendAsync (1 recipient)
    await Run("10 MMS.SendAsync (1 recipient)", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendAsync(signedUrlResp.FileKey,
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
            "MMS 1 recipient!", ".NET MMS Test");
        AssertSmsResponse(resp);
    });

    // 11 — MMS.SendAsync (2 recipients)
    await Run("11 MMS.SendAsync (2 recipients)", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendAsync(signedUrlResp.FileKey,
            [
                new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
                new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
            ],
            "MMS 2 recipients!", ".NET MMS Test");
        AssertSmsResponse(resp);
    });

    // 12 — MMS.SendAsync (3 recipients)
    await Run("12 MMS.SendAsync (3 recipients)", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendAsync(signedUrlResp.FileKey,
            [
                new Account { FirstName = fn1, LastName = ln1, Phone = phone1 },
                new Account { FirstName = fn2, LastName = ln2, Phone = phone2 },
                new Account { FirstName = fn3, LastName = ln3, Phone = phone3 },
            ],
            "MMS 3 recipients!", ".NET MMS Test");
        AssertSmsResponse(resp);
    });

    // 13 — MMS.SendAsync with Data
    await Run("13 MMS.SendAsync with Data", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendAsync(signedUrlResp.FileKey,
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, Data = new() { ["product"] = "Widget" } }],
            "Check out ${product}!", ".NET MMS Data");
        AssertSmsResponse(resp);
    });

    // 14 — MMS.SendAsync with CustomData
    await Run("14 MMS.SendAsync with CustomData", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        var resp = await client.MMS.SendAsync(signedUrlResp.FileKey,
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1, CustomData = "{\"campaignId\":\"mms-net-001\"}" }],
            "MMS with messageData!", ".NET MMS MsgData");
        AssertSmsResponse(resp);
    });

    // 15 — MMS.CheckFileUploadedAsync — the file uploaded in test 08 must actually exist
    await Run("15 MMS.CheckFileUploadedAsync", async () =>
    {
        if (signedUrlResp == null) throw new SkipTestException("dependency test 07 failed");
        if (!uploadOk) throw new SkipTestException("dependency test 08 failed");
        var resp = await client.MMS.CheckFileUploadedAsync(signedUrlResp.FileKey);
        if (resp is null || string.IsNullOrEmpty(resp.StoredUrl))
            throw new Exception($"expected non-empty StoredUrl for uploaded file {signedUrlResp.FileKey}");
    });

    // 16 — MMS.SendWithImageAsync (fresh upload)
    await Run("16 MMS.SendWithImageAsync (fresh upload)", async () =>
    {
        var resp = await client.MMS.SendWithImageAsync(pngPath, "image/png",
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
            "MMS with image!", ".NET MMS Image",
            forceNewCampaign: true);
        AssertSmsResponse(resp);
    });

    // 17 — MMS.SendWithImageAsync (cached)
    await Run("17 MMS.SendWithImageAsync (cached)", async () =>
    {
        var resp = await client.MMS.SendWithImageAsync(pngPath, "image/png",
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
            "MMS cached image!", ".NET MMS Cache",
            forceNewCampaign: true);
        AssertSmsResponse(resp);
    });

    // ── Email Tests (18-22) ───────────────────────────────────────────────────
    Console.WriteLine("\n--- Email ---");

    // 18 — Email.SendSingleAsync
    await Run("18 Email.SendSingleAsync", async () =>
    {
        var resp = await client.Email.SendSingleAsync(
            fn1, ln1, email1,
            ".NET SDK Test Email", "<p>Hello from .NET SDK!</p>",
            senderEmail: senderEmail, replyEmail: replyEmail, senderName: SenderName,
            title: ".NET Email Test");
        AssertEmailResponse(resp);
    });

    // 19 — Email.SendCampaignAsync (1 recipient)
    await Run("19 Email.SendCampaignAsync (1 recipient)", async () =>
    {
        var resp = await client.Email.SendCampaignAsync(new EmailCampaign
        {
            Subject = ".NET SDK Email 1",
            Title = ".NET Email Test",
            Message = "<p>Hello 1!</p>",
            SenderEmail = senderEmail, ReplyEmail = replyEmail, SenderName = SenderName,
            Accounts = [new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 }]
        });
        AssertEmailResponse(resp);
    });

    // 20 — Email.SendCampaignAsync (2 recipients)
    await Run("20 Email.SendCampaignAsync (2 recipients)", async () =>
    {
        var resp = await client.Email.SendCampaignAsync(new EmailCampaign
        {
            Subject = ".NET SDK Email 2",
            Title = ".NET Email Test",
            Message = "<p>Hello 2!</p>",
            SenderEmail = senderEmail, ReplyEmail = replyEmail, SenderName = SenderName,
            Accounts =
            [
                new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
                new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
            ]
        });
        AssertEmailResponse(resp);
    });

    // 21 — Email.SendCampaignAsync (3 recipients)
    await Run("21 Email.SendCampaignAsync (3 recipients)", async () =>
    {
        var resp = await client.Email.SendCampaignAsync(new EmailCampaign
        {
            Subject = ".NET SDK Email 3",
            Title = ".NET Email Test",
            Message = "<p>Hello 3!</p>",
            SenderEmail = senderEmail, ReplyEmail = replyEmail, SenderName = SenderName,
            Accounts =
            [
                new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
                new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
                new EmailAccount { FirstName = fn3, LastName = ln3, Email = email3 },
            ]
        });
        AssertEmailResponse(resp);
    });

    // 22 — Email.SendCampaignAsync (full campaign object)
    await Run("22 Email.SendCampaignAsync (full campaign)", async () =>
    {
        var resp = await client.Email.SendCampaignAsync(new EmailCampaign
        {
            Subject = ".NET SDK Campaign Test",
            Title = ".NET Email Campaign",
            Message = "<p>Campaign email from .NET SDK!</p>",
            SenderEmail = senderEmail, ReplyEmail = replyEmail, SenderName = SenderName,
            Accounts =
            [
                new EmailAccount { FirstName = fn1, LastName = ln1, Email = email1 },
                new EmailAccount { FirstName = fn2, LastName = ln2, Email = email2 },
            ]
        });
        AssertEmailResponse(resp);
    });

    // ── Webhook Tests (23-29) ─────────────────────────────────────────────────
    Console.WriteLine("\n--- Webhook ---");

    int registeredWebhookId = 0;

    // 23 — Webhook.RegisterAsync (or reuse EXISTING_WEBHOOK_ID)
    await Run("23 Webhook.RegisterAsync", async () =>
    {
        if (existingWebhookId > 0)
        {
            Console.WriteLine($"    (reusing EXISTING_WEBHOOK_ID={existingWebhookId}, not registering a new one)");
            registeredWebhookId = existingWebhookId;
            return;
        }
        var resp = await client.Webhook.RegisterAsync(new WebhookConfig
        {
            Url = webhookUrl,
            Secret = webhookSecret
        });

        if (resp.Id == 0) throw new Exception("webhook ID is 0 after register");
        registeredWebhookId = resp.Id;
        cleanupWebhookIds.Add(resp.Id);
    });

    // 24 — Webhook.ListAsync — must contain the webhook from test 23
    await Run("24 Webhook.ListAsync", async () =>
    {
        var hooks = await client.Webhook.ListAsync();
        if (hooks == null || hooks.Count == 0) throw new Exception("expected at least one webhook, got 0");
        if (registeredWebhookId != 0 && hooks.All(h => h.Id != registeredWebhookId))
            throw new Exception($"webhook {registeredWebhookId} from test 23 not present in ListAsync()");
    });

    // 25 — Webhook.UpdateAsync — then verify via ListAsync that the URL actually changed
    await Run("25 Webhook.UpdateAsync", async () =>
    {
        if (registeredWebhookId == 0) throw new SkipTestException("dependency test 23 failed");
        await client.Webhook.UpdateAsync(registeredWebhookId, new WebhookConfig
        {
            Url = webhookUrl + "&updated=1",
            Secret = "updated-secret-dotnet"
        });
        var hooks = await client.Webhook.ListAsync();
        var hook = hooks?.FirstOrDefault(h => h.Id == registeredWebhookId);
        if (hook is null) throw new Exception($"webhook {registeredWebhookId} not found in ListAsync() after update");
        if (!hook.Url.Contains("updated=1"))
            throw new Exception($"webhook URL was not updated: expected to contain \"updated=1\", got \"{hook.Url}\"");
    });

    // 26 — Webhook.VerifySignature (valid)
    await Run("26 Webhook.VerifySignature (valid)", async () =>
    {
        await Task.CompletedTask;
        const string eventHash = "abc123eventHash";
        var sig = HmacSha256Base64(webhookSecret, $"{clientId}:{eventHash}");
        var ok = client.Webhook.VerifySignature(sig, clientId, eventHash, webhookSecret);
        if (!ok) throw new Exception("expected valid signature to return true");
    });

    // 27 — Webhook.VerifySignature (invalid)
    await Run("27 Webhook.VerifySignature (invalid)", async () =>
    {
        await Task.CompletedTask;
        var ok = client.Webhook.VerifySignature("invalidsig==", clientId, "somehash", webhookSecret);
        if (ok) throw new Exception("expected invalid signature to return false");
    });

    // 28 — Webhook.ParseCloudContactEvent
    await Run("28 Webhook.ParseCloudContactEvent", async () =>
    {
        await Task.CompletedTask;
        var payload = "{\"eventType\":\"message.sent\",\"data\":{\"To\":\"+15005550001\",\"Message\":\"test\",\"MessageStatus\":\"DELIVERED\",\"SmsSid\":1,\"CampaignId\":0,\"CampaignTitle\":\"\",\"CustomData\":\"\",\"ClientExternalId\":\"\"}}";
        var evt = client.Webhook.ParseCloudContactEvent(payload);
        if (evt.EventType != "message.sent")
            throw new Exception($"expected EventType \"message.sent\", got \"{evt.EventType}\"");
    });

    // 29 — Webhook.DeleteAsync — then verify via ListAsync that it is gone
    await Run("29 Webhook.DeleteAsync", async () =>
    {
        if (registeredWebhookId == 0) throw new SkipTestException("dependency test 23 failed");
        if (existingWebhookId > 0)
            throw new SkipTestException("EXISTING_WEBHOOK_ID set — keeping the shared webhook");
        await client.Webhook.DeleteAsync(registeredWebhookId);
        cleanupWebhookIds.Remove(registeredWebhookId);
        var hooks = await client.Webhook.ListAsync();
        if (hooks != null && hooks.Any(h => h.Id == registeredWebhookId))
            throw new Exception($"webhook {registeredWebhookId} still present in ListAsync() after delete");
    });

    // ── Contact Tests (30-31) ─────────────────────────────────────────────────
    Console.WriteLine("\n--- Contact ---");

    // 30 — Contact.SetDoNotTextAsync(true)
    await Run("30 Contact.SetDoNotTextAsync(true)", async () =>
    {
        var resp = await client.Contact.SetDoNotTextAsync(true, phone: phone1);
        if (resp is null) throw new Exception("empty response");
    });

    // 31 — Contact.SetDoNotTextAsync(false)
    await Run("31 Contact.SetDoNotTextAsync(false)", async () =>
    {
        var resp = await client.Contact.SetDoNotTextAsync(false, phone: phone1);
        if (resp is null) throw new Exception("empty response");
    });

    // ── Brand Tests (32-36) ───────────────────────────────────────────────────
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
        cleanupBrandIds.Add(resp.Id);
    });

    // 33 — Brand.GetAsync
    await Run("33 Brand.GetAsync", async () =>
    {
        if (brandId == 0) throw new SkipTestException("dependency test 32 failed");
        var resp = await client.Brands.GetAsync(brandId);
        if (resp.Id != brandId) throw new Exception("Brand id mismatch");
        if (resp.LegalCompanyName != "Test Company LLC")
            throw new Exception($"expected LegalCompanyName \"Test Company LLC\", got \"{resp.LegalCompanyName}\"");
    });

    // 34 — Brand.ListAsync — must contain the brand created in test 32
    await Run("34 Brand.ListAsync", async () =>
    {
        var list = await client.Brands.ListAsync();
        if (list == null) throw new Exception("Null response");
        if (brandId != 0 && list.All(b => b.Id != brandId))
            throw new Exception($"brand {brandId} created in test 32 not present in ListAsync()");
    });

    // 35 — Brand.UpdateAsync — then verify via GetAsync that the field actually changed
    await Run("35 Brand.UpdateAsync", async () =>
    {
        if (brandId == 0) throw new SkipTestException("dependency test 32 failed");
        var resp = await client.Brands.UpdateAsync(brandId, new BrandRequest { City = "Orlando" });
        if (resp.Id != brandId) throw new Exception("Brand id mismatch after update");
        var fetched = await client.Brands.GetAsync(brandId);
        if (fetched.City != "Orlando")
            throw new Exception($"expected City \"Orlando\" after update, got \"{fetched.City}\"");
    });

    // 36 — Brand.DeleteAsync — then verify via GetAsync that it is gone
    await Run("36 Brand.DeleteAsync", async () =>
    {
        if (brandId == 0) throw new SkipTestException("dependency test 32 failed");
        await client.Brands.DeleteAsync(brandId);
        cleanupBrandIds.Remove(brandId);
        await ExpectErrorAsync($"get of deleted brand {brandId}", () => client.Brands.GetAsync(brandId));
    });

    // ── Campaign Tests (37-42) ────────────────────────────────────────────────
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
        cleanupBrandIds.Add(resp.Id);
    });

    // 38 — Campaign.CreateAsync
    await Run("38 Campaign.CreateAsync", async () =>
    {
        if (campaignBrandId == 0) throw new SkipTestException("dependency test 37 failed");
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
        cleanupCampaignIds.Add(resp.Id);
    });

    // 39 — Campaign.GetAsync
    await Run("39 Campaign.GetAsync", async () =>
    {
        if (campaignId == 0) throw new SkipTestException("dependency test 38 failed");
        var resp = await client.Campaigns.GetAsync(campaignId);
        if (resp.Id != campaignId) throw new Exception("Campaign id mismatch");
        if (resp.BrandId != campaignBrandId)
            throw new Exception($"expected BrandId {campaignBrandId}, got {resp.BrandId}");
    });

    // 40 — Campaign.ListAsync — must contain the campaign created in test 38
    await Run("40 Campaign.ListAsync", async () =>
    {
        var list = await client.Campaigns.ListAsync();
        if (list == null) throw new Exception("Null response");
        if (campaignId != 0 && list.All(c => c.Id != campaignId))
            throw new Exception($"campaign {campaignId} created in test 38 not present in ListAsync()");
    });

    // 41 — Campaign.UpdateAsync — then verify via GetAsync that the field actually changed
    await Run("41 Campaign.UpdateAsync", async () =>
    {
        if (campaignId == 0) throw new SkipTestException("dependency test 38 failed");
        const string newDescription = "Updated integration test campaign description";
        var resp = await client.Campaigns.UpdateAsync(campaignId, new CampaignRequest
        {
            Description = newDescription
        });
        if (resp.Id != campaignId) throw new Exception("Campaign id mismatch after update");
        var fetched = await client.Campaigns.GetAsync(campaignId);
        if (fetched.Description != newDescription)
            throw new Exception($"expected updated description after update, got \"{fetched.Description}\"");
    });

    // 42 — Campaign.DeleteAsync + cleanup brand — then verify via GetAsync that it is gone
    await Run("42 Campaign.DeleteAsync", async () =>
    {
        if (campaignId == 0) throw new SkipTestException("dependency test 38 failed");
        await client.Campaigns.DeleteAsync(campaignId);
        cleanupCampaignIds.Remove(campaignId);
        await ExpectErrorAsync($"get of deleted campaign {campaignId}", () => client.Campaigns.GetAsync(campaignId));
        if (campaignBrandId != 0)
        {
            await client.Brands.DeleteAsync(campaignBrandId);
            cleanupBrandIds.Remove(campaignBrandId);
        }
    });

    // ── Contact Validator (43-46) ─────────────────────────────────────────────
    Console.WriteLine("\n--- ContactValidator ---");

    // 43 — ContactValidator.ValidateEmailAsync
    await Run("43 ContactValidator.ValidateEmailAsync", async () =>
    {
        var resp = await client.ContactValidator.ValidateEmailAsync(email1);
        if (string.IsNullOrEmpty(resp.Status)) throw new Exception("status is empty");
    });

    // 44 — ContactValidator.ValidateEmailsAsync
    await Run("44 ContactValidator.ValidateEmailsAsync", async () =>
    {
        var resp = await client.ContactValidator.ValidateEmailsAsync(new[] { email1, email2 });
        if (resp.Summary.Total != 2) throw new Exception($"expected summary.total=2, got {resp.Summary.Total}");
        if (resp.Results.Count != 2) throw new Exception($"expected 2 results, got {resp.Results.Count}");
    });

    // 45 — ContactValidator.ValidatePhoneAsync
    await Run("45 ContactValidator.ValidatePhoneAsync", async () =>
    {
        var resp = await client.ContactValidator.ValidatePhoneAsync(phone1);
        if (string.IsNullOrEmpty(resp.Status)) throw new Exception("status is empty");
    });

    // 46 — ContactValidator.ValidatePhonesAsync
    await Run("46 ContactValidator.ValidatePhonesAsync", async () =>
    {
        var resp = await client.ContactValidator.ValidatePhonesAsync(new[]
        {
            new PhoneInput { Phone = phone1 },
            new PhoneInput { Phone = phone2 }
        });
        if (resp.Summary.Total != 2) throw new Exception($"expected summary.total=2, got {resp.Summary.Total}");
        if (resp.Results.Count != 2) throw new Exception($"expected 2 results, got {resp.Results.Count}");
    });

    // ── Negative & Permissive Tests (47-52) ───────────────────────────────────
    // 47/49/50 PASS when the operation fails as expected. 48/51/52 document
    // permissive behavior observed in the test API: those
    // operations succeed even with invalid input, so the tests assert success.
    Console.WriteLine("\n--- Negative cases ---");

    // 47 — invalid API key must be rejected
    await Run("47 NEGATIVE: SMS.SendSingleAsync with invalid API key", async () =>
    {
        using var badClient = new CCAIClient(new CCAIConfig
        {
            ClientId = clientId,
            ApiKey = "invalid-api-key-for-negative-test",
            UseTestEnvironment = Environment.GetEnvironmentVariable("CCAI_BASE_URL") is null
        });
        await ExpectErrorAsync("send with invalid API key",
            () => badClient.SMS.SendSingleAsync(fn1, ln1, phone1, "should fail", ".NET Negative 47"));
    });

    // 48 — the test API accepts malformed phone numbers: the
    // send succeeds instead of failing. If the API starts validating phone format,
    // change this back to ExpectErrorAsync.
    await Run("48 PERMISSIVE: SMS.SendSingleAsync with malformed phone (API accepts)", async () =>
    {
        var resp = await client.SMS.SendSingleAsync(fn1, ln1, "abc", "malformed phone accepted", ".NET Permissive 48");
        AssertSmsResponse(resp);
    });

    // 49 — getting a nonexistent brand must fail
    await Run("49 NEGATIVE: Brand.GetAsync(nonexistent)", async () =>
    {
        await ExpectErrorAsync("get of nonexistent brand", () => client.Brands.GetAsync(99999999));
    });

    // 50 — deleting a nonexistent webhook must fail
    await Run("50 NEGATIVE: Webhook.DeleteAsync(nonexistent)", async () =>
    {
        await ExpectErrorAsync("delete of nonexistent webhook", () => client.Webhook.DeleteAsync(99999999));
    });

    // 51 — the test environment's validator reports "valid" even for syntactically
    // invalid emails — upstream validation is not enforced
    // there, so only assert that a status is returned.
    await Run("51 PERMISSIVE: ContactValidator.ValidateEmailAsync(invalid input)", async () =>
    {
        var resp = await client.ContactValidator.ValidateEmailAsync("not-an-email");
        if (string.IsNullOrEmpty(resp.Status)) throw new Exception("status is empty");
    });

    // 52 — the test API accepts MMS sends with a nonexistent fileKey: it does not
    // verify the file exists at send time. If the API
    // starts validating the fileKey, change this back to ExpectErrorAsync.
    await Run("52 PERMISSIVE: MMS.SendAsync with nonexistent fileKey (API accepts)", async () =>
    {
        var fakeKey = $"{clientId}/campaign/nonexistent_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.png";
        var resp = await client.MMS.SendAsync(fakeKey,
            [new Account { FirstName = fn1, LastName = ln1, Phone = phone1 }],
            "nonexistent fileKey accepted", ".NET Permissive 52");
        AssertSmsResponse(resp);
    });
}
finally
{
    // ── Cleanup — always runs, even if the test body threw ────────────────────
    foreach (var id in cleanupCampaignIds)
    {
        try
        {
            await client.Campaigns.DeleteAsync(id);
            Console.WriteLine($"  CLEANUP: deleted leftover campaign {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  CLEANUP: could not delete campaign {id}: {ex.Message}");
        }
    }
    foreach (var id in cleanupBrandIds)
    {
        try
        {
            await client.Brands.DeleteAsync(id);
            Console.WriteLine($"  CLEANUP: deleted leftover brand {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  CLEANUP: could not delete brand {id}: {ex.Message}");
        }
    }
    foreach (var id in cleanupWebhookIds)
    {
        try
        {
            await client.Webhook.DeleteAsync(id);
            Console.WriteLine($"  CLEANUP: deleted leftover webhook {id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  CLEANUP: could not delete webhook {id}: {ex.Message}");
        }
    }
    client.Dispose();
    if (File.Exists(pngPath)) File.Delete(pngPath);
}

// ── Results ───────────────────────────────────────────────────────────────────

Console.WriteLine("\n==============================================");
Console.WriteLine($"  RESULTS: {passed} passed, {failed} failed, {skipped} skipped");
Console.WriteLine("==============================================");

var summary = JsonSerializer.Serialize(new { sdk = "dotnet", passed, failed, skipped, total = passed + failed + skipped });
Console.WriteLine($"\nSUMMARY_JSON: {summary}");

Environment.Exit(failed > 0 ? 1 : 0);

/// <summary>Thrown when a test cannot run because a prerequisite test failed.</summary>
file class SkipTestException : Exception
{
    public SkipTestException(string message) : base(message) { }
}
