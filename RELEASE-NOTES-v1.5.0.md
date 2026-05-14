# Release Notes - Version 1.5.0

## New Features
- Added `ContactValidator` service for validating email addresses and phone numbers
- `ValidateEmailAsync` / `ValidateEmail` — validate a single email, returns `valid`, `invalid`, or `risky` status with `safe_to_send` and `ai_verdict` metadata
- `ValidateEmailsAsync` / `ValidateEmails` — bulk email validation (up to 50 addresses) with summary counts
- `ValidatePhoneAsync` / `ValidatePhone` — validate a single phone number, returns `valid`, `invalid`, or `landline` status with carrier metadata
- `ValidatePhonesAsync` / `ValidatePhones` — bulk phone validation (up to 50 numbers) with summary counts including landline count
- New models: `EmailValidationResult`, `PhoneValidationResult`, `ValidationSummary`, `BulkEmailValidationResult`, `BulkPhoneValidationResult`, `PhoneInput`
- `IContactValidatorService` interface for dependency injection and testing
