using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisApp.Controllers;

/// <summary>
/// Basic Redis Operations Controller
///
/// ⚠️  WARNING: THIS CODE IS FOR REFERENCE AND LEARNING PURPOSES ONLY
/// ⚠️  NOT INTENDED FOR PRODUCTION USE WITHOUT PROPER SECURITY REVIEW
/// ⚠️  MISSING: Authentication, Authorization, Input Validation
/// ⚠️  MISSING: Rate Limiting, Data Sanitization, Security Headers
/// ⚠️  MISSING: Comprehensive Error Handling, Audit Logging
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisController> _logger;

    public RedisController(IDatabase database, ILogger<RedisController> logger)
    {
        _database = database;
        _logger = logger;
    }

    [HttpPost("set/{key}")]
    public async Task<IActionResult> SetValue(string key, [FromBody] string value)
    {
        try
        {
            await _database.StringSetAsync(key, value);
            _logger.LogInformation("Set key '{Key}' with value '{Value}'", key, value);
            return Ok(new { message = $"Key '{key}' set successfully", key, value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting key '{Key}'", key);
            return StatusCode(500, new { error = "Failed to set value in Redis" });
        }
    }

    [HttpGet("get/{key}")]
    public async Task<IActionResult> GetValue(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return NotFound(new { message = $"Key '{key}' not found" });
            }

            _logger.LogInformation("Retrieved key '{Key}' with value '{Value}'", key, value);
            return Ok(new { key, value = value.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting key '{Key}'", key);
            return StatusCode(500, new { error = "Failed to get value from Redis" });
        }
    }

    [HttpDelete("delete/{key}")]
    public async Task<IActionResult> DeleteValue(string key)
    {
        try
        {
            var deleted = await _database.KeyDeleteAsync(key);
            if (!deleted)
            {
                return NotFound(new { message = $"Key '{key}' not found" });
            }

            _logger.LogInformation("Deleted key '{Key}'", key);
            return Ok(new { message = $"Key '{key}' deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key '{Key}'", key);
            return StatusCode(500, new { error = "Failed to delete key from Redis" });
        }
    }

    [HttpGet("keys")]
    public IActionResult GetAllKeys()
    {
        try
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "*").Select(key => key.ToString()).ToList();

            _logger.LogInformation("Retrieved {Count} keys from Redis", keys.Count);
            return Ok(new { keys, count = keys.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all keys");
            return StatusCode(500, new { error = "Failed to get keys from Redis" });
        }
    }

    [HttpPost("hash/set/{hashKey}/{field}")]
    public async Task<IActionResult> SetHashValue(string hashKey, string field, [FromBody] string value)
    {
        try
        {
            await _database.HashSetAsync(hashKey, field, value);
            _logger.LogInformation("Set hash '{HashKey}' field '{Field}' with value '{Value}'", hashKey, field, value);
            return Ok(new { message = $"Hash field set successfully", hashKey, field, value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting hash '{HashKey}' field '{Field}'", hashKey, field);
            return StatusCode(500, new { error = "Failed to set hash value in Redis" });
        }
    }

    [HttpGet("hash/get/{hashKey}/{field}")]
    public async Task<IActionResult> GetHashValue(string hashKey, string field)
    {
        try
        {
            var value = await _database.HashGetAsync(hashKey, field);
            if (!value.HasValue)
            {
                return NotFound(new { message = $"Hash '{hashKey}' field '{field}' not found" });
            }

            _logger.LogInformation("Retrieved hash '{HashKey}' field '{Field}' with value '{Value}'", hashKey, field, value);
            return Ok(new { hashKey, field, value = value.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hash '{HashKey}' field '{Field}'", hashKey, field);
            return StatusCode(500, new { error = "Failed to get hash value from Redis" });
        }
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            await _database.PingAsync();
            return Ok(new { status = "healthy", message = "Redis connection is working" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return StatusCode(500, new { status = "unhealthy", error = "Redis connection failed" });
        }
    }
}
