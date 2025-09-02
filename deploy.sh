#!/bin/bash

# Redis Search API Deployment Script
# This script helps initialize and push the code to GitHub repository

echo "🚀 Redis Search API - GitHub Deployment Script"
echo "=============================================="

# Check if git is installed
if ! command -v git &> /dev/null; then
    echo "❌ Git is not installed. Please install Git first."
    exit 1
fi

# Check if we're in a git repository
if [ ! -d ".git" ]; then
    echo "📁 Initializing Git repository..."
    git init
    
    echo "🔗 Adding remote repository..."
    git remote add origin https://github.com/naresh29s/naresh29s-Redis-Search-stackexchange-dotnet.git
else
    echo "✅ Git repository already initialized"
fi

# Add all files
echo "📝 Adding files to Git..."
git add .

# Check if there are changes to commit
if git diff --staged --quiet; then
    echo "ℹ️  No changes to commit"
else
    # Commit changes
    echo "💾 Committing changes..."
    git commit -m "feat: Redis Search API with ASP.NET Core and StackExchange.Redis

- Implemented WhiteboardTable controller with search index creation
- Added advanced Redis FT.SEARCH functionality with flexible parameters
- Support for TAG, TEXT, and NUMERIC field types
- Custom query parser for complex Redis syntax
- Comprehensive error handling and logging
- Production-ready configuration with credential management
- Complete API documentation and examples"

    # Push to GitHub
    echo "🌐 Pushing to GitHub..."
    git branch -M main
    git push -u origin main

    echo "✅ Successfully deployed to GitHub!"
    echo "🔗 Repository: https://github.com/naresh29s/naresh29s-Redis-Search-stackexchange-dotnet"
fi

echo ""
echo "🎉 Deployment completed!"
echo "📚 Next steps:"
echo "   1. Visit your GitHub repository"
echo "   2. Update Redis connection settings in appsettings.json"
echo "   3. Run 'dotnet restore && dotnet run' to start the application"
echo "   4. Access Swagger UI at http://localhost:5001/swagger"
