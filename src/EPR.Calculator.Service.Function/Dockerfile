FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

COPY EPR.Calculator.Service.Common/. ./EPR.Calculator.Service.Common/.
COPY EPR.Calculator.Service.Function/. ./EPR.Calculator.Service.Function/.
COPY EPR.Calculator.Service.Data/. ./EPR.Calculator.Service.Data/.
COPY stylecop.json ./

# Publish 
RUN dotnet publish EPR.Calculator.Service.Function/*.csproj --output /home/site/wwwroot

COPY --chown=dotnet:dotnet EPR.Calculator.Service.Data/. ./EPR.Calculator.Service.Data/.

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

COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
