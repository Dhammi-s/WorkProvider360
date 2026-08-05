FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

WORKDIR /home/site/wwwroot

COPY . .

RUN dotnet publish -c Release -o /home/site/wwwroot
