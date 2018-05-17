#!/usr/bin/env bash

#exit if any command fails
set -e
cd ./src/Projection.Utils
dotnet publish -o ./pub
source="$pwd/pub"
docker build --build-arg source=$source . 

