#!/bin/bash

# Navigate to the script's directory
cd "$(dirname "$0")"

echo "Starting RabbitMQ and SQL Server containers..."
docker-compose up -d

echo "Starting Backend Microservices in the background..."

# Run each service in the background and redirect output to logs
mkdir -p logs

echo "Starting Identity Service..."
dotnet run --project services/SmartSure.IdentityService/SmartSure.IdentityService.csproj > logs/identity.log 2>&1 &
IDENTITY_PID=$!

echo "Starting Policy Service..."
dotnet run --project services/SmartSure.PolicyService/SmartSure.PolicyService.csproj > logs/policy.log 2>&1 &
POLICY_PID=$!

echo "Starting Claims Service..."
dotnet run --project services/SmartSure.ClaimsService/SmartSure.ClaimsService.csproj > logs/claims.log 2>&1 &
CLAIMS_PID=$!

echo "Starting Admin Service..."
dotnet run --project services/SmartSure.AdminService/SmartSure.AdminService.csproj > logs/admin.log 2>&1 &
ADMIN_PID=$!

echo "Starting API Gateway..."
dotnet run --project gateway/SmartSure.Gateway/SmartSure.Gateway.csproj > logs/gateway.log 2>&1 &
GATEWAY_PID=$!

echo ""
echo "============================================================"
echo "  SmartSure Backend - All services are starting up!"
echo "============================================================"
echo ""
echo "  API Gateway (main entry point for frontend & testing):"
echo "     http://localhost:5057"
echo ""
echo "  Swagger UI (test individual services directly):"
echo "     Gateway (aggregated):  http://localhost:5057/swagger"
echo "     Identity Service:      http://localhost:5168/swagger"
echo "     Policy Service:        http://localhost:5014/swagger"
echo "     Claims Service:        http://localhost:5265/swagger"
echo "     Admin Service:         http://localhost:5232/swagger"
echo ""
echo "  RabbitMQ Management UI:"
echo "     http://localhost:15672  (guest / guest)"
echo ""
echo "  Log files (use 'tail -f' to watch live):"
echo "     tail -f logs/gateway.log"
echo "     tail -f logs/identity.log"
echo "     tail -f logs/policy.log"
echo "     tail -f logs/claims.log"
echo "     tail -f logs/admin.log"
echo ""
echo "  NOTE: Services may take 30-60s to fully start."
echo "        Check logs if a service is not responding."
echo "============================================================"
echo "Press Ctrl+C to stop all services."

# Trap SIGINT (Ctrl+C) to kill all background processes
trap "echo 'Stopping all services...'; kill $IDENTITY_PID $POLICY_PID $CLAIMS_PID $ADMIN_PID $GATEWAY_PID; exit" SIGINT

# Wait indefinitely until Ctrl+C is pressed
wait
