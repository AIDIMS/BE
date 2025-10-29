#!/bin/bash

# Stop Orthanc DICOM Server using Docker Compose
echo "Stopping Orthanc DICOM Server with Docker Compose..."
docker-compose down
echo ""
echo "Orthanc DICOM Server stopped."
