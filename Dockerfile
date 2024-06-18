# Use the following commands to build and push
# docker build -t bheemboy/filemoverservice:latest -t bheemboy/filemoverservice:2024.06.18 .
# docker push --all-tags bheemboy/filemoverservice

# Stage 1 ##############################################################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN apt-get update && apt-get install -y curl
RUN curl -fsSL https://deb.nodesource.com/gpgkey/nodesource.gpg.key | gpg --dearmor | tee /usr/share/keyrings/nodesource.gpg >/dev/null
RUN echo "deb [signed-by=/usr/share/keyrings/nodesource.gpg] https://deb.nodesource.com/node_20.x bullseye main" > /etc/apt/sources.list.d/nodesource.list
RUN apt-get update && apt-get install -y nodejs npm

WORKDIR /app
COPY . .

WORKDIR /app/FileMoverWeb
RUN npm install 
RUN npm run build

WORKDIR /app/FileMoverService
RUN dotnet publish FileMoverService.csproj -c Release -o /webapp

# Stage 2 ##############################################################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

EXPOSE 80
ENV TZ=America/Los_Angeles
ENV ASPNETCORE_URLS=http://+:80

WORKDIR /webapp
COPY --from=build /webapp .

VOLUME /webapp/config

CMD ["dotnet", "/webapp/FileMoverService.dll"]

HEALTHCHECK CMD curl -f http://localhost/ || exit 1
