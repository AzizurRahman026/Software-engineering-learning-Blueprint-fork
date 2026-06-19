namespace Application.Settings;

/// <summary>
/// Represents configuration settings for connecting to a MongoDB database, including connection string, 
/// database name, connection pooling, TLS, and retry options.
/// </summary>
/// <remarks>Use this class to configure MongoDB client behavior such as connection pooling limits, TLS
/// encryption, and automatic retry policies. Adjust the properties based on your application's performance, 
/// security, and reliability requirements. These settings are typically provided during application 
/// startup or dependency injection configuration.</remarks>
public class MongoSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the minimum number of connections maintained in the connection pool.
    /// </summary>
    /// <remarks>Setting this property ensures that the connection pool always retains at least the specified
    /// number of active connections, which can improve performance for applications with consistent connection usage.
    /// If set to 0, no minimum is enforced.</remarks>
    public int MinConnectionPoolSize { get; set; } = 10;
    /// <summary>
    /// Gets or sets the maximum number of connections allowed in the connection pool.
    /// </summary>
    /// <remarks>Setting this property limits the total number of simultaneous connections that can be pooled
    /// for reuse. If the pool reaches this limit, additional connection requests may be queued or blocked until a
    /// connection becomes available. Adjust this value based on expected workload and resource constraints.</remarks>
    public int MaxConnectionPoolSize { get; set; } = 100;
    /// <summary>
    /// MongoDB client encrypts all network traffic using TLS
    /// Without TLS: Anyone on the network could read or modify data
    /// </summary>
    public bool UseTls { get; set; } = true;
    /// <summary>
    /// maximum number of connections that can be concurrently in the process of being created.
    /// </summary>
    public int MaxConnecting { get; set; } = 3;

    /// <summary>
    /// maximum lifetime, in minutes, for a connection before it's closed.
    /// 0 indicate indifinite life time default 30 min
    /// </summary>
    public int MaxConnectionLifeTime { get; set; } = 30; // min
    /// <summary>
    /// how long second a request will wait for free a connection.
    /// </summary>
    public int WaitQueTimeout { get; set; } = 60;
    /// <summary>
    /// Indicating whether load balancing is enabled/disabled for connections.
    /// </summary>
    public bool LoadBalanced { get; set; } = false;
    /// <summary>
    /// Indicating whether write operations should be retried automatically on 
    /// transient failures.
    /// </summary>
    /// <remarks>When enabled, the system will attempt to retry write operations 
    /// that fail due to temporary issues, such as network interruptions or server 
    /// timeouts. This can improve reliability but may result in duplicate writes 
    /// if the operation is not idempotent. Consider the implications for your 
    /// application's data consistency before enabling.</remarks>
    public bool RetryWrites { get; set; } = true;
    /// <summary>
    /// Indicating whether write operations should be retried automatically
    /// </summary>
    public bool RetryReads { get; set; } = false;
}


/*
 * add the settings reference: todo
 * */
