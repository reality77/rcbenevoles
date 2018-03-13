# Gestion des bénévoles

L'application Gestion des bénévoles permet d'effectuer des pointages pour les bénévoles de l'association Les Restaurants du Coeur par des responsables de centres afin d'editer des fiches d'abandon de remboursement des frais de déplacement servant de justificatifs à l'administration fiscale.

## Technologies employées

L'application a été développée en C# (ASP.NET MVC Core) et Entity Framework Core.
Le base de données utilisée est une base PostgreSQL

L'application a été optimisée afin d'être facilement packageable dans un environnement docker.

## Variables d'environnement

APP_DB_SERVER : Nom ou adresse IP du serveur de base de données
APP_DB_PASSWORD : Mot de passe de l'utilisateur rcbenevoles sur la base de données rcbenevoles

APP_ADMIN_PASSWORD : Mot de passe de l'administrateur du site lors de la création de celui-ci
APP_GENERATE_DEV_DATA : Permet de générer des données de test (si valeur = 1)