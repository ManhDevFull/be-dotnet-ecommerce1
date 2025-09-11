# Dùng image chính thức của .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy file solution và project để restore
COPY *.sln .
COPY dotnet.csproj ./   # <--- ở đây tên project chính của bạn

RUN dotnet restore dotnet.csproj

# copy toàn bộ source và build
COPY . .
RUN dotnet publish dotnet.csproj -c Release -o /app/publish

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dotnet.dll"]   # <--- thay bằng đúng tên DLL sau khi build
