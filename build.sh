#!/usr/bin/env bash

#exit if any command fails
set -e
cd ./src/Projection.Utils
dotnet publish -o ./pub
source="${pwd}/pub"
image="eventstorecontrib/eventstore-projection-utils"
versiontag="${image}:${APPVEYOR_BUILD_VERSION}"
latesttag="${image}:latest"

echo $source
echo $latesttag
echo $versiontag

docker build --build-arg source=$source -t $latesttag -t $versiontag . 

docker login -u=$DOCKER_USER -p=$DOCKER_PASS
docker push $versiontag
docker push $latesttag