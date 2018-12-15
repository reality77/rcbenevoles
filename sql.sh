# $1 = container id

docker exec -tiu postgres $1 psql -U rcbenevoles rcbenevoles
