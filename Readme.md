# Experimental Dotnet Docker

## Build Image
docker build -t kdc-experimental-image:latest .

## Create Container
docker create --name kdc-experimental kdc-experimental-image:latest
