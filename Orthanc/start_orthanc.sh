#!/bin/bash

# Start Orthanc DICOM Server using Docker Compose
echo "Starting Orthanc DICOM Server with Docker Compose..."

# Check if docker-compose.yml exists
if [ ! -f docker-compose.yml ]; then
  echo "Error: docker-compose.yml not found!"
  exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
  echo "Error: Docker is not installed. Please install Docker to proceed."
  exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
  echo "Error: Docker Compose is not installed. Please install Docker Compose to proceed."
  exit 1
fi

# Start the Orthanc server
echo "Starting Orthanc services..."
docker-compose up -d

echo ""
echo "Orthanc DICOM Server started."
echo "You can access the Orthanc web interface at http://localhost:8042"
echo "Default credentials: Username: admin, Password: admin"
echo ""
