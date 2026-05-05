// Copyright (c) 2025 CloudContactAI LLC
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CCAI.NET.Campaigns;

/// <summary>
/// Campaign service for managing 10DLC campaign registrations
/// </summary>
public class CampaignService
{
    private readonly CCAIClient _client;

    private static readonly HashSet<string> UseCases = new()
    {
        "TWO_FACTOR_AUTHENTICATION", "ACCOUNT_NOTIFICATION", "CUSTOMER_CARE", "DELIVERY_NOTIFICATION",
        "FRAUD_ALERT", "HIGHER_EDUCATION", "LOW_VOLUME_MIXED", "MARKETING", "MIXED",
        "POLLING_VOTING", "PUBLIC_SERVICE_ANNOUNCEMENT", "SECURITY_ALERT"
    };

    private static readonly HashSet<string> SubUseCases = new()
    {
        "TWO_FACTOR_AUTHENTICATION", "ACCOUNT_NOTIFICATION", "CUSTOMER_CARE", "DELIVERY_NOTIFICATION",
        "FRAUD_ALERT", "MARKETING", "POLLING_VOTING"
    };

    private static readonly HashSet<string> MixedUseCases = new() { "MIXED", "LOW_VOLUME_MIXED" };

    public CampaignService(CCAIClient client)
    {
        _client = client;
    }

    public async Task<CampaignResponse> CreateAsync(CampaignRequest data, CancellationToken cancellationToken = default)
    {
        Validate(data, isCreate: true);
        return await _client.CustomRequestAsync<CampaignResponse>(HttpMethod.Post, "/v1/campaigns", data, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public CampaignResponse Create(CampaignRequest data) => CreateAsync(data).GetAwaiter().GetResult();

    public async Task<CampaignResponse> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _client.CustomRequestAsync<CampaignResponse>(HttpMethod.Get, $"/v1/campaigns/{id}", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public CampaignResponse Get(long id) => GetAsync(id).GetAwaiter().GetResult();

    public async Task<CampaignResponse[]> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _client.CustomRequestAsync<CampaignResponse[]>(HttpMethod.Get, "/v1/campaigns", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public CampaignResponse[] List() => ListAsync().GetAwaiter().GetResult();

    public async Task<CampaignResponse> UpdateAsync(long id, CampaignRequest data, CancellationToken cancellationToken = default)
    {
        Validate(data, isCreate: false);
        return await _client.CustomRequestAsync<CampaignResponse>(HttpMethod.Patch, $"/v1/campaigns/{id}", data, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public CampaignResponse Update(long id, CampaignRequest data) => UpdateAsync(id, data).GetAwaiter().GetResult();

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _client.CustomRequestWithoutResponseAsync(HttpMethod.Delete, $"/v1/campaigns/{id}", null, _client.GetComplianceBaseUrl(), cancellationToken);
    }

    public void Delete(long id) => DeleteAsync(id).GetAwaiter().GetResult();

    private static void Validate(CampaignRequest data, bool isCreate)
    {
        var errors = new List<string>();

        if (isCreate)
        {
            if (data.BrandId == null) errors.Add("brandId is required");
            if (string.IsNullOrWhiteSpace(data.UseCase)) errors.Add("useCase is required");
            if (string.IsNullOrWhiteSpace(data.Description)) errors.Add("description is required");
            if (string.IsNullOrWhiteSpace(data.MessageFlow)) errors.Add("messageFlow is required");
            if (data.HasEmbeddedLinks == null) errors.Add("hasEmbeddedLinks is required");
            if (data.HasEmbeddedPhone == null) errors.Add("hasEmbeddedPhone is required");
            if (data.IsAgeGated == null) errors.Add("isAgeGated is required");
            if (data.IsDirectLending == null) errors.Add("isDirectLending is required");
            if (data.OptInKeywords == null || data.OptInKeywords.Count == 0) errors.Add("optInKeywords is required");
            if (string.IsNullOrWhiteSpace(data.OptInMessage)) errors.Add("optInMessage is required");
            if (string.IsNullOrWhiteSpace(data.OptInProofUrl)) errors.Add("optInProofUrl is required");
            if (data.HelpKeywords == null || data.HelpKeywords.Count == 0) errors.Add("helpKeywords is required");
            if (string.IsNullOrWhiteSpace(data.HelpMessage)) errors.Add("helpMessage is required");
            if (data.OptOutKeywords == null || data.OptOutKeywords.Count == 0) errors.Add("optOutKeywords is required");
            if (string.IsNullOrWhiteSpace(data.OptOutMessage)) errors.Add("optOutMessage is required");
            if (data.SampleMessages == null || data.SampleMessages.Count == 0) errors.Add("sampleMessages is required");
        }

        if (data.UseCase != null && !UseCases.Contains(data.UseCase))
            errors.Add("Invalid use case");

        // MIXED/LOW_VOLUME_MIXED sub-use case validation
        if (data.UseCase != null && MixedUseCases.Contains(data.UseCase))
        {
            if (data.SubUseCases == null || data.SubUseCases.Count < 2 || data.SubUseCases.Count > 3)
                errors.Add("MIXED/LOW_VOLUME_MIXED requires 2-3 sub use cases");
            else
                foreach (var suc in data.SubUseCases)
                    if (!SubUseCases.Contains(suc)) errors.Add($"Invalid sub use case: {suc}");
        }
        else if (data.UseCase != null && data.SubUseCases is { Count: > 0 })
        {
            errors.Add("subUseCases should be empty for non-MIXED use cases");
        }

        // sampleMessages count and content validation
        if (data.SampleMessages != null)
        {
            if (data.SampleMessages.Count < 2 || data.SampleMessages.Count > 5)
            {
                errors.Add("sampleMessages must contain 2-5 items");
            }
            else
            {
                var optOutKws = data.OptOutKeywords ?? new List<string>();
                var helpKws = data.HelpKeywords ?? new List<string>();

                var hasOptOut = data.SampleMessages.Any(msg =>
                    msg.Contains("Reply STOP") || optOutKws.Any(kw => msg.Contains($"Reply {kw}")));
                if (!hasOptOut)
                    errors.Add("At least one sample must contain 'Reply STOP' or 'Reply {optOutKeyword}'");

                var hasHelp = data.SampleMessages.Any(msg =>
                    msg.Contains("Reply HELP") || helpKws.Any(kw => msg.Contains($"Reply {kw}")));
                if (!hasHelp)
                    errors.Add("At least one sample must contain 'Reply HELP' or 'Reply {helpKeyword}'");
            }
        }

        // optOutMessage must contain STOP or an opt-out keyword
        if (data.OptOutMessage != null)
        {
            var optOutKws = data.OptOutKeywords ?? new List<string>();
            if (!data.OptOutMessage.Contains("STOP") && !optOutKws.Any(kw => data.OptOutMessage.Contains(kw)))
                errors.Add("optOutMessage must contain 'STOP' or at least one optOutKeyword");
        }

        // helpMessage must contain HELP or a help keyword
        if (data.HelpMessage != null)
        {
            var helpKws = data.HelpKeywords ?? new List<string>();
            if (!data.HelpMessage.Contains("HELP") && !helpKws.Any(kw => data.HelpMessage.Contains(kw)))
                errors.Add("helpMessage must contain 'HELP' or at least one helpKeyword");
        }

        if (data.OptInProofUrl != null && !data.OptInProofUrl.StartsWith("http://") && !data.OptInProofUrl.StartsWith("https://"))
            errors.Add("Opt-in proof URL must start with http:// or https://");

        if (data.TermsLink != null && !data.TermsLink.StartsWith("http://") && !data.TermsLink.StartsWith("https://"))
            errors.Add("Terms link must start with http:// or https://");

        if (data.PrivacyLink != null && !data.PrivacyLink.StartsWith("http://") && !data.PrivacyLink.StartsWith("https://"))
            errors.Add("Privacy link must start with http:// or https://");

        if (errors.Count > 0)
            throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
    }
}
