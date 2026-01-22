# Ocea Smart Building — Application console .NET

## Prérequis

* .NET SDK 8.0 (ou version supérieure compatible). Téléchargeable depuis <https://dotnet.microsoft.com/download>.
* Une connexion internet pour le géocodage et la simulation d’envoi HTTP.

## Compilation et exécution

1. Cloner le dépôt ou copier les fichiers dans un répertoire local.
2. Assurez‑vous que `input.csv` se trouve dans le dossier Data (ou spécifiez son chemin en argument).
3. Dans un terminal, positionnez‑vous à la racine du projet et exécutez :

```bash
# Restaure les dépendances et compile l’application et les tests
dotnet build

# Exécution de l’application avec le fichier d’entrée par défaut (input.csv)
dotnet run 

# Ou exécution en spécifiant un chemin personnalisé
dotnet run -- /chemin/vers/mon/fichier.csv
```

Lors de l’exécution, l’application :

* lit le fichier CSV `input.csv` (séparateur `;` et en‑tête obligatoire) ;
* valide chaque ligne (champs requis non vides, code postal à 5 chiffres, date planifiée au format `YYYY-MM-DD`) ;
* génère un fichier `rejected.csv` contenant les lignes invalides et une colonne `RejectReason` décrivant le motif ;
* affiche un récapitulatif des lignes lues, valides et rejetées ;
* calcule pour chaque ordre de travail valide :
  * `AddressLabel` : concaténation normalisée de la rue, du code postal et de la ville ;
  * `Department` : deux premiers caractères du code postal ;
  * `DaysToPlan` : différence en jours entre la date planifiée et la date du jour (entier, positif ou négatif) ;
  * `Priority` : P1 (≤ 1 jour), P2 (2 ou 3 jours) ou P3 (≥ 4 jours) ;
* appelle le service public de géocodage (<https://data.geopf.fr/geocodage/search>) pour enrichir l’adresse avec `Latitude`, `Longitude` et `GeoScore` (null en cas d’échec) ;
* écrit l’ensemble des ordres enrichis dans `output.json` (format JSON avec indentations) ;

## Tests unitaires

TODO

## Limitations

* Le parseur CSV intégré se contente de séparer les colonnes sur `;` et ne gère pas les guillemets ni les champs contenant des points‑virgules. Pour des cas plus complexes, l’utilisation d’une bibliothèque dédiée comme `CsvHelper` serait recommandée.

## Licence

Ce projet est fourni à titre d’exemple pour l’exercice technique Ocea Smart Building.