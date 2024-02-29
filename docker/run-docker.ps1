$dockerComposeFile="./docker/docker-compose-webappexample.yml"

# delete all containers
docker-compose -f $dockerComposeFile rm -f
# pull latest images
docker-compose -f $dockerComposeFile pull
# build images
docker-compose -f $dockerComposeFile build --no-cache 
# run
docker-compose -f $dockerComposeFile config 
docker-compose -f $dockerComposeFile up 