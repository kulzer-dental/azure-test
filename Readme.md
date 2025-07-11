# Experimental Dotnet Docker

## Build Image
docker build -t kdc-experimental-image:latest .

## Create Container
docker create --name kdc-experimental kdc-experimental-image:latest

## Push

export CR_PAT=YOUR_TOKEN
echo $CR_PAT | docker login ghcr.io -u maxkbul --password-stdin
docker push kdc-experimental-image:latest