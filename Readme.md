# Experimental Dotnet Docker

## Build Image
docker build -t kdc-experimental-image:latest .

## Create Container
docker create --name kdc-experimental kdc-experimental-image:latest

## Push

export CR_PAT=YOUR_TOKEN
echo $CR_PAT | docker login ghcr.io -u maxkbul --password-stdin
docker tag kdc-experimental-image:latest ghcr.io/kulzer-dental/kdc-experimental-image:latest
docker push ghcr.io/kulzer-dental/kdc-experimental-image:latest