#!/usr/bin/env bash

set -euo pipefail

if ! command -v reportgenerator >/dev/null 2>&1; then
    echo "Installing ReportGenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
fi

rm -rf src/EPR.Calculator.Service.Function.UnitTests/TestResults
rm -rf coveragereport

dotnet test src \
  --collect:"XPlat Code Coverage" \
  --settings coverage.runsettings

reportgenerator \
  -reports:"src/**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:"Html"

echo "Open coveragereport/index.html"
