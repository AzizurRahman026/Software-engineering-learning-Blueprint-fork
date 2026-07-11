namespace Infrastructure.Configuration;

/// <summary>
/// RabbitMQ connection settings, bound from the "RabbitMQ" configuration section.
/// An empty <see cref="Host"/> means "no broker configured" — messaging then falls back to
/// MassTransit's in-memory transport so the app still boots (mirrors the Redis → in-memory fallback).
/// </summary>
public class RabbitMqOptions
{
    public string Host { get; set; } = string.Empty;
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
