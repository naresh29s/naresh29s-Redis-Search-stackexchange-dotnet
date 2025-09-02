# Redis Search API with ASP.NET Core & StackExchange.Redis

A comprehensive ASP.NET Core Web API application that demonstrates advanced Redis Search functionality using StackExchange.Redis client library. This application provides a powerful interface for creating search indexes and performing complex queries on Redis hash data.

## ⚠️ **IMPORTANT DISCLAIMER**

**THIS CODE IS FOR REFERENCE AND LEARNING PURPOSES ONLY**

🚨 **NOT PRODUCTION READY** - This code is intended for educational purposes and demonstrations only. It should **NOT** be used in production environments without significant security enhancements and proper code review.

### Missing Production Requirements:
- ❌ **Authentication & Authorization** - No user authentication or API security
- ❌ **Input Validation & Sanitization** - Vulnerable to injection attacks
- ❌ **Rate Limiting** - No protection against abuse or DoS attacks
- ❌ **Security Headers** - Missing CORS, CSP, and other security headers
- ❌ **Audit Logging** - No security event logging or monitoring
- ❌ **Error Handling** - Basic error handling, may leak sensitive information
- ❌ **Connection Management** - No connection pooling or retry policies
- ❌ **Data Validation** - Minimal validation of Redis queries and parameters
- ❌ **Performance Optimization** - No caching, pagination limits, or query optimization
- ❌ **Monitoring & Health Checks** - Basic health checks only

### Before Production Use:
1. **Security Review** - Conduct thorough security assessment
2. **Authentication** - Implement proper API authentication (JWT, OAuth, etc.)
3. **Authorization** - Add role-based access control
4. **Input Validation** - Implement comprehensive input sanitization
5. **Rate Limiting** - Add API rate limiting and throttling
6. **Monitoring** - Implement proper logging, metrics, and alerting
7. **Testing** - Add comprehensive unit, integration, and security tests
8. **Documentation** - Create proper API documentation and security guidelines

## 🚀 Features

### Core Functionality
- **Redis Search Index Creation**: Automated FT.CREATE command execution with proper field type mapping
- **Advanced Search Capabilities**: Support for complex Redis search queries with multiple parameters
- **Flexible Query Interface**: Both parameter-based and raw Redis query support
- **Real-time Data Parsing**: Converts Redis search results into structured JSON responses
- **Production-Ready**: Comprehensive error handling, logging, and configuration management

### Search Capabilities
- **Field-Specific Searches**: TAG, TEXT, and NUMERIC field type support
- **Range Queries**: Numeric and date range filtering (`@Price:[0.5 5]`, `@Quantity:[1000 +inf]`)
- **Exact Matches**: TAG field searches for precise value matching
- **Text Searches**: Full-text search on TEXT fields
- **Advanced Features**: RETURN, SORTBY, LIMIT, and pagination support
- **Custom Query Override**: Direct Redis FT.SEARCH command execution

## 🛠️ Technology Stack

### Framework & Runtime
- **ASP.NET Core**: 9.0
- **.NET Runtime**: 9.0.8
- **Language**: C# 12

### Dependencies & Versions
```xml
<PackageReference Include="StackExchange.Redis" Version="2.7.33" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
```

### Redis Requirements
- **Redis Stack**: 6.2+ (with RediSearch module)
- **Redis Search Module**: For FT.CREATE and FT.SEARCH commands
- **Supported Redis Deployments**: Local, Cloud (Redis Labs, AWS ElastiCache, etc.)

## 📋 Prerequisites

### Development Environment
1. **.NET 9.0 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)
2. **Redis Stack** with Search module enabled
3. **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
4. **Git** for version control

### Redis Setup Options

#### Option 1: Local Redis Stack (Recommended for Development)
```bash
# macOS using Homebrew
brew tap redis-stack/redis-stack
brew install redis-stack
redis-stack-server

# Ubuntu/Debian
curl -fsSL https://packages.redis.io/gpg | sudo gpg --dearmor -o /usr/share/keyrings/redis-archive-keyring.gpg
echo "deb [signed-by=/usr/share/keyrings/redis-archive-keyring.gpg] https://packages.redis.io/deb $(lsb_release -cs) main" | sudo tee /etc/apt/sources.list.d/redis.list
sudo apt-get update
sudo apt-get install redis-stack-server

# Windows
# Download Redis Stack from https://redis.io/download
```

#### Option 2: Docker (Quick Setup)
```bash
docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

#### Option 3: Cloud Redis (Production)
- Redis Labs Cloud
- AWS ElastiCache for Redis
- Azure Cache for Redis
- Google Cloud Memorystore

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/naresh29s/naresh29s-Redis-Search-stackexchange-dotnet.git
cd naresh29s-Redis-Search-stackexchange-dotnet
```

### 2. Configure Redis Connection
Update `appsettings.json` with your Redis connection details:

```json
{
  "ConnectionStrings": {
    "RedisHost": "your-redis-host.com",
    "RedisPort": "6379",
    "RedisUser": "default",
    "RedisPassword": "your-redis-password"
  }
}
```

For local development:
```json
{
  "ConnectionStrings": {
    "RedisHost": "localhost",
    "RedisPort": "6379",
    "RedisUser": "default",
    "RedisPassword": ""
  }
}
```

### 3. Build and Run
```bash
# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

### 4. Access the Application
- **API Base URL**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/api/redis/health

## 📚 API Documentation

### WhiteboardTable Controller

#### Create Search Index
**POST** `/api/whiteboardtable/create-index`

Creates a Redis search index for WBD_WHITEBOARD keys with optimized field types:
- `Whiteboard_ID`: TAG SORTABLE (exact matches)
- `Deal_Start`: TEXT SORTABLE (date strings)
- `Deal_End`: TEXT SORTABLE (date strings)
- `Quantity`: NUMERIC SORTABLE (range queries)
- `Price`: NUMERIC SORTABLE (range queries)
- `startdate_ing`: NUMERIC SORTABLE (timestamp queries)

#### Advanced Search
**GET** `/api/whiteboardtable/search`

**Parameters:**
- `indexName` (optional): Redis index name (default: "idx:wbd_whiteboard")
- `query` (optional): Redis search query (default: "*")
- `limit` (optional): Maximum results (default: 10)
- `offset` (optional): Result offset (default: 0)
- `returnFields` (optional): Comma-separated fields to return
- `sortBy` (optional): Field to sort by
- `sortOrder` (optional): ASC or DESC
- `fullQuery` (optional): Complete Redis FT.SEARCH command override

### Redis Controller (Basic Operations)
- **POST** `/api/redis/set/{key}` - Set string value
- **GET** `/api/redis/get/{key}` - Get string value
- **DELETE** `/api/redis/delete/{key}` - Delete key
- **GET** `/api/redis/health` - Connection health check

## 🔍 Query Examples

### Basic Searches
```bash
# Get all documents
GET /api/whiteboardtable/search?query=*

# Search by Whiteboard ID
GET /api/whiteboardtable/search?query=@Whiteboard_ID:{6777127.0}

# Price range query
GET /api/whiteboardtable/search?query=@Price:[0.5 5]

# Quantity with infinity
GET /api/whiteboardtable/search?query=@Quantity:[1000 +inf]
```

### Advanced Parameter Usage
```bash
# Return specific fields only
GET /api/whiteboardtable/search?query=*&returnFields=Whiteboard_ID,Quantity,Price

# Sort by price descending
GET /api/whiteboardtable/search?query=*&sortBy=Price&sortOrder=DESC

# Pagination
GET /api/whiteboardtable/search?query=*&offset=10&limit=5
```

### Full Query Control
```bash
# Complete Redis command override
GET /api/whiteboardtable/search?fullQuery=@Quantity:[1000 +inf] RETURN 3 Whiteboard_ID Quantity Price LIMIT 0 2

# Multiple conditions
GET /api/whiteboardtable/search?fullQuery=@Whiteboard_ID:{6777127.0} @Price:[0.5 5] SORTBY Price DESC
```

## 🏗️ Architecture & Design

### Data Type Strategy
The application uses optimized Redis field types for maximum search performance:

- **TAG**: For exact matches (Whiteboard_ID)
- **TEXT**: For text search and date strings (Deal_Start, Deal_End)
- **NUMERIC**: For range queries and sorting (Quantity, Price, startdate_ing)

### Query Parser
Custom `ParseRedisQuery` method handles complex Redis syntax:
- Preserves spaces within brackets: `@Price:[0.5 5]`
- Handles infinity notation: `@Quantity:[1000 +inf]`
- Supports quoted strings and nested syntax

### Error Handling
- Comprehensive exception handling for Redis connection issues
- Structured error responses with detailed messages
- Logging integration for debugging and monitoring

## 🔧 Configuration

### Environment Variables
```bash
# For production deployment
export REDIS_HOST="your-redis-host.com"
export REDIS_PORT="6379"
export REDIS_USER="default"
export REDIS_PASSWORD="your-secure-password"
```

### Docker Configuration
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
EXPOSE 5001
ENTRYPOINT ["dotnet", "RedisApp.dll"]
```

## 🧪 Testing

### Manual Testing with curl
```bash
# Create index
curl -X POST "http://localhost:5001/api/whiteboardtable/create-index"

# Search all
curl "http://localhost:5001/api/whiteboardtable/search?query=*"

# Advanced search
curl "http://localhost:5001/api/whiteboardtable/search?fullQuery=@Price:[0.5 5] RETURN 2 Whiteboard_ID Price"
```

### Using Swagger UI
1. Navigate to http://localhost:5001/swagger
2. Expand WhiteboardTable endpoints
3. Use "Try it out" feature for interactive testing

## 🚨 Troubleshooting

### Common Issues

#### Redis Connection Failed
```
Error: "Failed to connect to Redis"
Solution: Verify Redis is running and connection string is correct
```

#### Index Creation Failed
```
Error: "Unknown index name"
Solution: Ensure Redis has RediSearch module loaded
```

#### Search Returns No Results
```
Issue: Data exists but search returns empty
Solution: Recreate index after data insertion, or use reindexing
```

### Debug Commands
```bash
# Check Redis connection
redis-cli ping

# Verify RediSearch module
redis-cli MODULE LIST

# Check existing indexes
redis-cli FT._LIST
```

## 📈 Performance Considerations

### Index Optimization
- Use appropriate field types (TAG vs TEXT vs NUMERIC)
- Consider field weights for relevance scoring
- Monitor index size and memory usage

### Query Optimization
- Use specific field queries instead of wildcard searches
- Implement pagination for large result sets
- Consider caching frequently accessed data

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **StackExchange.Redis** team for the excellent Redis client
- **Redis Labs** for Redis Stack and RediSearch module
- **Microsoft** for ASP.NET Core framework
- **Community** contributors and testers

## 📞 Support

For questions, issues, or contributions:
- **GitHub Issues**: [Create an issue](https://github.com/naresh29s/naresh29s-Redis-Search-stackexchange-dotnet/issues)
- **Documentation**: Check this README and inline code comments
- **Redis Documentation**: [Redis Search Documentation](https://redis.io/docs/stack/search/)

---

**Built with ❤️ using ASP.NET Core 9.0 and Redis Stack**
