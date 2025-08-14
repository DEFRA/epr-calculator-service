FROM defradigital/dotnetcore-development:dotnet8.0 AS base

USER root
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root

# Install dotnet-ef tool
RUN dotnet tool install dotnet-ef --version 6.* --tool-path "/root/.dotnet/tools"

# Add the tools folder to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src
COPY ["EPR.Calculator.Service.Function/EPR.Calculator.Service.Function.csproj", "EPR.Calculator.Service.Function/"]
COPY ["EPR.Calculator.Service.Common/EPR.Calculator.Service.Common.csproj", "EPR.Calculator.Service.Common/"]
COPY ["EPR.Calculator.Service.Data/EPR.Calculator.Service.Data.csproj", "EPR.Calculator.Service.Data/"]
RUN dotnet restore "EPR.Payment.Service/EPR.Payment.Service.csproj"

COPY EPR.Calculator.Service.Function/. ./EPR.Calculator.Service.Function/.
COPY EPR.Calculator.Service.Common/. ./EPR.Calculator.Service.Common/.
COPY EPR.Calculator.Service.Data/. ./EPR.Calculator.Service.Data/.

WORKDIR "/src/EPR.Calculator.Service.Function"
RUN dotnet build "EPR.Calculator.Service.Function.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
RUN dotnet publish "EPR.Calculator.Service.Function.csproj" -c Release /p:AzureBuild=false -o /app/publish

FROM base AS final

# Switch to the non-root user
USER dotnet

WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "EPR.Payment.Service.dll"]