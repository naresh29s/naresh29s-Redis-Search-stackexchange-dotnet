using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace RedisApp.Controllers;

/// <summary>
/// WhiteboardTable Controller - Creates Redis search index and searches WBD_WHITEBOARD data
/// Data types matter: TAG for exact matches, TEXT for text search, NUMERIC for range queries
///
/// ⚠️  WARNING: THIS CODE IS FOR REFERENCE AND LEARNING PURPOSES ONLY
/// ⚠️  NOT INTENDED FOR PRODUCTION USE WITHOUT PROPER SECURITY REVIEW
/// ⚠️  MISSING: Authentication, Authorization, Input Validation, Rate Limiting
/// ⚠️  MISSING: Comprehensive Error Handling, Security Headers, Data Sanitization
/// ⚠️  REVIEW: Connection Management, Resource Disposal, Performance Optimization
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WhiteboardTableController : ControllerBase
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<WhiteboardTableController> _logger;

    public WhiteboardTableController(IConnectionMultiplexer connectionMultiplexer, ILogger<WhiteboardTableController> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    /// <summary>
    /// Creates Redis search index for WBD_WHITEBOARD keys
    /// Data types: Whiteboard_ID=TAG, Deal_Start/End=TEXT, Quantity/Price=NUMERIC
    ///
    /// ⚠️  REFERENCE CODE ONLY - NOT PRODUCTION READY
    /// ⚠️  Missing: Input validation, authentication, authorization
    /// ⚠️  Missing: Proper error handling, logging, monitoring
    /// ⚠️  Missing: Index existence validation, conflict resolution
    /// </summary>
    [HttpPost("create-index")]
    public async Task<IActionResult> CreateWhiteboardIndex()
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            const string indexName = "idx:wbd_whiteboard";
            const string keyPrefix = "WBD_WHITEBOARD:";

            // Check if index already exists
            try
            {
                await db.ExecuteAsync("FT.INFO", indexName);
                return Ok(new { message = "Index already exists", indexName, status = "exists" });
            }
            catch (RedisServerException ex) when (ex.Message.Contains("Unknown index name"))
            {
                // Index doesn't exist, create it
            }
            // FT.CREATE whiteboard_index ON HASH PREFIX 1 "WBD_WHITEBOARD:" SCHEMA \
            //     Whiteboard_ID TEXT SORTABLE \
            //     Deal_Start NUMERIC SORTABLE \
            //     Deal_End NUMERIC SORTABLE \
            //     Quantity NUMERIC SORTABLE \
            //     Price NUMERIC SORTABLE
            // FT.CREATE idx:wbd_deshboard ON HASH PREFIX 1 "WBD_WHITEBOARD:" SCHEMA Board_Type TAG SORTABLE Active TEXT SORTABLE Deal_Start TEXT SORTABLE Deal_End TEXT SORTABLE Supply_Sale NUMERIC SORTABLE
            // Create index with correct data types
            await db.ExecuteAsync("FT.CREATE",
                indexName,
                "ON", "HASH",
                "PREFIX", "1", keyPrefix,
                "SCHEMA",
                "Whiteboard_ID", "TAG", "SORTABLE",      // TAG for exact matches
                "Deal_Start", "TEXT", "SORTABLE",        // TEXT for date strings
                "Deal_End", "TEXT", "SORTABLE",          // TEXT for date strings
                "Quantity", "NUMERIC", "SORTABLE",       // NUMERIC for range queries
                "Price", "NUMERIC", "SORTABLE",          // NUMERIC for price range queries
                "startdate_ing", "NUMERIC", "SORTABLE"   // NUMERIC for timestamp range queries
            );

            _logger.LogInformation("Created index '{IndexName}' for prefix '{KeyPrefix}'", indexName, keyPrefix);

            return Ok(new {
                message = "Index created successfully",
                indexName,
                keyPrefix,
                status = "created"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index");
            return StatusCode(500, new { error = "Failed to create index", details = ex.Message });
        }
    }



    // FT.SEARCH idx:wbd_whiteboard "@Whiteboard_ID:{6777127.0}"
    // FT.SEARCH idx:wbd_whiteboard "@Quantity:[1000 +inf]"
    // FT.SEARCH idx:wbd_whiteboard "@Price:[0.5 3]"
    // FT.SEARCH idx:wbd_whiteboard "@startdate_ing:[1750783324 1756052212]"
    // FT.SEARCH idx:wbd_whiteboard "@Whiteboard_ID:{6777127.0} @Quantity:[1000 +inf] @Price:[0.5 5]"
    // FT.SEARCH idx:wbd_whiteboard "@Quantity:[1000 +inf]" RETURN 3 Whiteboard_ID Quantity Price LIMIT 0 2

    /// <summary>
    /// Search whiteboard data with flexible parameters and advanced Redis search features
    ///
    /// Examples:
    /// - Basic: ?query=*
    /// - Field search: ?query=@Whiteboard_ID:{6777127.0}
    /// - Range: ?query=@Quantity:[1000 +inf]
    /// - Advanced: ?query=@Quantity:[1000 +inf]&returnFields=Whiteboard_ID,Quantity,Price&limit=5
    /// - Full control: ?fullQuery=@Quantity:[1000 +inf] RETURN 3 Whiteboard_ID Quantity Price LIMIT 0 2
    ///
    /// ⚠️  REFERENCE CODE ONLY - NOT PRODUCTION READY
    /// ⚠️  Missing: Query sanitization, SQL injection protection
    /// ⚠️  Missing: Rate limiting, authentication, authorization
    /// ⚠️  Missing: Result caching, performance optimization
    /// ⚠️  Missing: Audit logging, security monitoring
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchWhiteboards(
        [FromQuery] string indexName = "idx:wbd_whiteboard",  // Redis index name - defaults to whiteboard index, can be overridden
        [FromQuery] string query = "*",                       // Redis search query - "*" returns all, "@field:value" for specific searches
        [FromQuery] int limit = 10,                          // Maximum number of results to return (LIMIT parameter)
        [FromQuery] int offset = 0,                          // Starting position for results (LIMIT offset parameter)
        [FromQuery] string? returnFields = null,             // Comma-separated fields to return (RETURN parameter) - null returns all fields
        [FromQuery] string? sortBy = null,                   // Field name to sort results by (SORTBY parameter)
        [FromQuery] string? sortOrder = "ASC",               // Sort direction: ASC (ascending) or DESC (descending)
        [FromQuery] string? fullQuery = null                 // Complete Redis FT.SEARCH query override - bypasses all other parameters
    )
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();

            RedisResult searchResult;
            string executedQuery;

            // Option 1: Full query override - user provides complete Redis FT.SEARCH command
            // Example: fullQuery = "@Quantity:[1000 +inf] RETURN 3 Whiteboard_ID Quantity Price LIMIT 0 2"
            if (!string.IsNullOrEmpty(fullQuery))
            {
                // Parse the full query properly to handle Redis syntax with spaces in ranges
                var queryParts = ParseRedisQuery(fullQuery);
                var allParams = new List<object> { indexName };
                allParams.AddRange(queryParts);

                searchResult = await db.ExecuteAsync("FT.SEARCH", allParams.ToArray());
                executedQuery = $"FT.SEARCH {indexName} {fullQuery}";
            }
            // Option 2: Build query from individual parameters
            // This constructs the Redis command: FT.SEARCH indexName query [RETURN fields] [SORTBY field order] [LIMIT offset count]
            else
            {
                var queryParams = new List<object> { "FT.SEARCH", indexName, query };

                // Add RETURN clause if specific fields requested
                // Example: returnFields="Whiteboard_ID,Quantity,Price" becomes "RETURN 3 Whiteboard_ID Quantity Price"
                if (!string.IsNullOrEmpty(returnFields))
                {
                    var fields = returnFields.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    queryParams.Add("RETURN");                    // Redis RETURN keyword
                    queryParams.Add(fields.Length.ToString());   // Number of fields to return
                    queryParams.AddRange(fields.Select(f => f.Trim())); // Field names
                }

                // Add SORTBY clause if specified
                // Example: sortBy="Price", sortOrder="DESC" becomes "SORTBY Price DESC"
                if (!string.IsNullOrEmpty(sortBy))
                {
                    queryParams.Add("SORTBY");                   // Redis SORTBY keyword
                    queryParams.Add(sortBy);                     // Field name to sort by
                    queryParams.Add(sortOrder?.ToUpper() ?? "ASC"); // Sort direction
                }

                // Add LIMIT clause for pagination
                // Example: offset=0, limit=10 becomes "LIMIT 0 10"
                queryParams.Add("LIMIT");                        // Redis LIMIT keyword
                queryParams.Add(offset.ToString());              // Starting position (0-based)
                queryParams.Add(limit.ToString());               // Number of results to return

                // Execute the constructed Redis command
                searchResult = await db.ExecuteAsync(queryParams.First().ToString()!, queryParams.Skip(1).ToArray());
                executedQuery = string.Join(" ", queryParams);   // For logging/debugging
            }

            // Parse Redis search results to extract actual document data
            var results = ParseSearchResults(searchResult);

            _logger.LogInformation("Search found {Count} results for query '{Query}' on index '{IndexName}'",
                results.Count, query, indexName);

            return Ok(new {
                indexName,
                query = !string.IsNullOrEmpty(fullQuery) ? fullQuery : query,
                executedQuery,
                totalResults = results.Count,
                documents = results,
                parameters = new {
                    limit,
                    offset,
                    returnFields,
                    sortBy,
                    sortOrder
                },
                status = "success"
            });
        }
        catch (RedisServerException ex) when (ex.Message.Contains("Unknown index name"))
        {
            return NotFound(new {
                message = $"Index '{indexName}' does not exist. Create it first using POST /api/whiteboardtable/create-index"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search error for query '{Query}' on index '{IndexName}'", query, indexName);
            return StatusCode(500, new {
                error = "Search failed",
                details = ex.Message,
                indexName,
                query
            });
        }
    }

    /// <summary>
    /// Parse Redis FT.SEARCH results into structured document data
    /// Redis returns: [count, key1, [field1, value1, field2, value2, ...], key2, [...]]
    /// </summary>
    private List<object> ParseSearchResults(RedisResult searchResult)
    {
        var documents = new List<object>();

        if (searchResult.IsNull || !searchResult.Type.HasFlag(ResultType.Array))
            return documents;

        var resultArray = (RedisResult[])searchResult;
        if (resultArray.Length < 1) return documents;

        // First element is the total count
        var totalCount = (int)resultArray[0];

        // Process each document (skip index 0 which is count)
        for (int i = 1; i < resultArray.Length; i += 2)
        {
            if (i + 1 >= resultArray.Length) break;

            var keyName = resultArray[i].ToString();
            var fieldsArray = (RedisResult[])resultArray[i + 1];

            // Convert field array to dictionary
            var document = new Dictionary<string, object> { ["_key"] = keyName };

            for (int j = 0; j < fieldsArray.Length; j += 2)
            {
                if (j + 1 < fieldsArray.Length)
                {
                    var fieldName = fieldsArray[j].ToString();
                    var fieldValue = fieldsArray[j + 1].ToString();
                    document[fieldName] = fieldValue;
                }
            }

            documents.Add(document);
        }

        return documents;
    }

    /// <summary>
    /// Parse Redis query string properly handling brackets and quotes
    /// Handles cases like: @Price:[0.5 5] @Quantity:[1000 +inf] RETURN 3 Whiteboard_ID Quantity Price
    /// </summary>
    private List<string> ParseRedisQuery(string query)
    {
        var parts = new List<string>();
        var currentPart = "";
        var inBrackets = false;
        var inQuotes = false;

        for (int i = 0; i < query.Length; i++)
        {
            char c = query[i];

            switch (c)
            {
                case '[':
                    inBrackets = true;
                    currentPart += c;
                    break;

                case ']':
                    inBrackets = false;
                    currentPart += c;
                    break;

                case '"':
                case '\'':
                    inQuotes = !inQuotes;
                    currentPart += c;
                    break;

                case ' ':
                    if (inBrackets || inQuotes)
                    {
                        // Keep spaces inside brackets or quotes
                        currentPart += c;
                    }
                    else
                    {
                        // Space outside brackets/quotes - end current part
                        if (!string.IsNullOrEmpty(currentPart))
                        {
                            parts.Add(currentPart);
                            currentPart = "";
                        }
                    }
                    break;

                default:
                    currentPart += c;
                    break;
            }
        }

        // Add the last part if not empty
        if (!string.IsNullOrEmpty(currentPart))
        {
            parts.Add(currentPart);
        }

        return parts;
    }
}


// FT.SEARCH idx:wbd_deshboard "*"

// # 2. Search by Board_Type (TAG)
// FT.SEARCH idx:wbd_deshboard "@Board_Type:{1}"

// # 3. Search by Active flag (TEXT)
// FT.SEARCH idx:wbd_deshboard "@Active:A"

// # 4. Search by Deal_Start (prefix match, since TEXT)
// FT.SEARCH idx:wbd_deshboard "@Deal_Start:2025-08-07*"

// # 5. Search by Supply_Sale (numeric range)
// FT.SEARCH idx:wbd_deshboard "@Supply_Sale:[1000 +inf]"

// # 6. Combine filters (Board_Type + Active + Supply_Sale range)
// FT.SEARCH idx:wbd_deshboard "@Board_Type:{1} @Active:A @Supply_Sale:[1000 +inf]"

// # 7. Limit and return specific fields
// FT.SEARCH idx:wbd_deshboard "@Active:A" RETURN 3 Board_Type Deal_Start Supply_Sale LIMIT 0 2