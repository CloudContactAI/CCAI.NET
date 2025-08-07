using CCAI.NET.Examples;

if (args.Length > 0 && args[0] == "webhook")
{
    await RegisterWebhook.RunAsync();
}
else
{
    await SmsSend.RunAsync();
}
