using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow requests from ngrok
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use CORS
app.UseCors("AllowAll");

// Configure the webhook endpoint for both root path and /webhook path
app.MapPost("/", async (HttpContext context) =>
{
    Console.WriteLine("Received webhook event at root path!");
    
    // Read the request body
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    // Log headers
    Console.WriteLine("Headers:");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"  {header.Key}: {header.Value}");
    }
    
    // Log body
    Console.WriteLine("Body:");
    Console.WriteLine(body);
    
    // Return a success response
    return Results.Ok("Webhook received");
});

// Also handle /webhook path for compatibility
app.MapPost("/webhook", async (HttpContext context) =>
{
    Console.WriteLine("Received webhook event at /webhook path!");
    
    // Read the request body
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    // Log headers
    Console.WriteLine("Headers:");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"  {header.Key}: {header.Value}");
    }
    
    // Log body
    Console.WriteLine("Body:");
    Console.WriteLine(body);
    
    // Return a success response
    return Results.Ok("Webhook received");
});

// Configure GET endpoints for testing
app.MapGet("/", () => "Webhook server is running. Send POST requests to / or /webhook");
app.MapGet("/webhook", () => "Webhook endpoint is working");

// Set the server to listen on port 3000
app.Urls.Add("http://localhost:3000");

Console.WriteLine("Starting webhook server on http://localhost:3000");
Console.WriteLine("Press Ctrl+C to stop the server");

await app.RunAsync();