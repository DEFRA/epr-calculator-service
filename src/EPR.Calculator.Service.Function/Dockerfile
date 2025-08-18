FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Set working directory
WORKDIR /src

# Install Azure Artifacts Credential Provider
RUN curl -sSL https://aka.ms/install-artifacts-credprovider.sh | bash

# Copy solution files
COPY NuGet.Config ./
COPY stylecop.json ./

COPY EPR.Calculator.Service.Common/ EPR.Calculator.Service.Common/
COPY EPR.Calculator.Service.Function/ EPR.Calculator.Service.Function/
COPY EPR.Calculator.Service.Data/ EPR.Calculator.Service.Data/

# Restore and publish
RUN dotnet restore EPR.Calculator.Service.Function/EPR.Calculator.Service.Function.csproj
RUN dotnet publish EPR.Calculator.Service.Function/EPR.Calculator.Service.Function.csproj -c Release -o /home/site/wwwroot


# Use the Azure Functions image as the final image
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env /home/site/wwwroot /home/site/wwwroot
