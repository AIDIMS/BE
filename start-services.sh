#!/bin/bash

echo "🚀 Starting AIDIMS Backend Services..."

# Build images first
echo "📦 Building Docker images..."
docker-compose build

# Start databases first
echo "🗄️ Starting databases..."
docker-compose up -d postgres postgres-web

# Wait for databases to be ready
echo "⏳ Waiting for databases to be ready..."
sleep 15

# Run migrations
echo "🔄 Running database migrations..."
docker-compose up migration

# Check if migration was successful
if [ $? -eq 0 ]; then
    echo "✅ Migration completed successfully"

    # Start all services
    echo "🚀 Starting all services..."
    docker-compose up -d

    echo ""
    echo "🎉 AIDIMS Backend is now running!"
    echo ""
    echo "📋 Service URLs:"
    echo "  • Backend API: http://localhost:5104"
    echo "  • Swagger UI: http://localhost:5104/swagger"
    echo "  • Orthanc Web UI: http://localhost:8042 (admin/admin)"
    echo "  • PostgreSQL (Web): localhost:5433"
    echo "  • PostgreSQL (Orthanc): localhost:5434"
    echo ""
    echo "📊 To view logs:"
    echo "  docker-compose logs -f [service-name]"
    echo ""
    echo "🛑 To stop all services:"
    echo "  docker-compose down"

else
    echo "❌ Migration failed!"
    echo "📋 Check logs with: docker-compose logs migration"
    exit 1
fi
