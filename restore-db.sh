#!/bin/bash

container_id=$1
sql_file_to_restore=$2

cat $sql_file_to_restore | docker exec -i $container_id psql -U rcbenevoles rcbenevoles

echo "Pensez à modifier le mot de passe si nécessaire"
echo "ALTER USER rcbenevoles PASSWORD '<PASSWORD>'"