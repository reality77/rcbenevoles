# Gestion des bénévoles

L'application Gestion des bénévoles permet d'effectuer des pointages pour les bénévoles de l'association Les Restaurants du Coeur par des responsables de centres afin d'editer des fiches d'abandon de remboursement des frais de déplacement servant de justificatifs à l'administration fiscale.

## Technologies employées

L'application a été développée en C# (ASP.NET MVC Core) et Entity Framework Core.
Le moteur de base de données utilisé est PostgreSQL

L'application a été optimisée afin d'être facilement embarquable dans un environnement docker.

## Versions

| Indicateur | master | develop |
|------------|--------|---------|
| Travis     | [![Build Status](https://travis-ci.org/reality77/rcbenevoles.svg?branch=master)](https://travis-ci.org/reality77/rcbenevoles) |  [![Build Status](https://travis-ci.org/reality77/rcbenevoles.svg?branch=develop)](https://travis-ci.org/reality77/rcbenevoles) |
| Sonar      | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=reality77_rcbenevoles&metric=alert_status)](https://sonarcloud.io/dashboard?id=reality77_rcbenevoles) | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?branch=develop&project=reality77_rcbenevoles&metric=alert_status)](https://sonarcloud.io/dashboard?id=reality77_rcbenevoles&branch=develop) [![Greenkeeper badge](https://badges.greenkeeper.io/reality77/rcbenevoles.svg)](https://greenkeeper.io/)

## Variables d'environnement

**APP_DB_SERVER** : Nom ou adresse IP du serveur de base de données

**APP_DB_PASSWORD** : Mot de passe de l'utilisateur rcbenevoles sur la base de données rcbenevoles

**APP_ADMIN_PASSWORD** : Mot de passe de l'administrateur du site lors de la création de celui-ci

**APP_GENERATE_DEV_DATA** : Permet de générer des données de test (si valeur = 1)