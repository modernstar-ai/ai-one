#!/bin/bash

RESOURCE_GROUP_NAME="<RSG>"
TAG_FILE="<TAGSFILE>"
az group update --name $RESOURCE_GROUP_NAME --set tags=@$TAG_FILE