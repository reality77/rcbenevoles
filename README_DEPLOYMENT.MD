# Déploiement via docker en mode test

## Construction de l'image

`docker build -t rcbenevoles .`

## Execution

- Avant l'éxécution, lancer le container postgresql de test (base vide)

`docker run -it -e POSTGRES_USER=rcbenevoles -e POSTGRES_PASSWORD=rcbenevoles -p 5432:5432 postgres`

- Exécution de notre container web

`docker run -it -p 8080:80 -e APP_DB_SERVER=IP_OR_SERVER_NAME -e APP_DB_PASSWORD=rcbenevoles rcbenevoles`

option `-it` pour démarrer en interactif
option `-d`pour démarrer en détaché

- Le serveur web va utiliser les variables d'environnements APP_DB_SERVER et APP_DB_PASSWORD pour déterminer le nom du serveur de base de données

## Test

- http://localhost:8080/

# Mode déploiement

## Démarrer avec un docker swarm

Pour déployer le swarm :

`docker swarm init`

`docker stack deploy -c docker-compose.yml rcb`

Pour le quitter :

`docker swarm leave --force`

