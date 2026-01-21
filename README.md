# Exercice — Application console .NET (Ocea Smart Building)

## Contexte
Ocea Smart Building intervient sur des opérations de pose de compteurs. L’objectif de cet exercice est de développer une application console .NET (C#) qui lit des ordres de travail depuis un fichier CSV, applique une règle métier simple, enrichit les données via un géocodage public, puis simule l’envoi HTTP du résultat.

Durée cible : ~2 heures.

## Livrable
- Le code source complet.
- Un `README` expliquant comment exécuter l’application (pré-requis, commande, exemple).
- Le tout poussé sur Git dans le repository :
  `https://github.com/david-ocea/TestIngeIntegrationOcea.git`

---

## Entrée

### Fichier
- Nom : `input.csv`
- Format : UTF-8, séparateur `;`, en-tête obligatoire

### Colonnes (6)
`WorkOrderId;MeterSerial;Street;PostalCode;City;PlannedDate`

- `PlannedDate` : format `YYYY-MM-DD`

### Exemple `input.csv`
```csv
WorkOrderId;MeterSerial;Street;PostalCode;City;PlannedDate
WO-2026-0101;MTR-69002-0101;12 Rue de la République;69002;Lyon;2026-01-22
WO-2026-0102;MTR-69003-0102;85 Cours Lafayette;69003;Lyon;2026-01-23
WO-2026-0103;MTR-69006-0103;14 Boulevard des Belges;69006;Lyon;2026-01-24
WO-2026-0104;MTR-69007-0104;27 Avenue Jean Jaurès;69007;Lyon;2026-01-21
WO-2026-0105;MTR-69008-0105;33 Avenue des Frères Lumière;69008;Lyon;2026-01-25
WO-2026-0106;MTR-69004-0106;10 Boulevard de la Croix-Rousse;69004;Lyon;2026-01-26
WO-2026-0107;MTR-69100-0107;112 Rue du 4 Août 1789;69100;Villeurbanne;2026-01-22
WO-2026-0108;MTR-69100-0108;6 Avenue Henri Barbusse;69100;Villeurbanne;2026-01-23
WO-2026-0109;MTR-69500-0109;3 Avenue Franklin Roosevelt;69500;Bron;2026-01-24
WO-2026-0110;MTR-69300-0110;28 Avenue du Général de Gaulle;69300;Caluire-et-Cuire;2026-01-25
WO-2026-0111;MTR-69600-0111;20 Grande Rue;69600;Oullins;2026-01-26
WO-2026-0112;;8 Rue Garibaldi;69003;Lyon;2026-01-23
```

## User stories

### US-01 — Import et validation
En tant que planificateur, je veux importer un CSV d’ordres de travail et rejeter proprement les lignes invalides.

**Critères d’acceptation**
- Lecture du CSV `;` avec en-tête obligatoire.
- Validation : champs obligatoires non vides, `PostalCode` = 5 chiffres, `PlannedDate` parseable.
- Génération d’un `rejected.csv` contenant la ligne d’origine + une colonne `RejectReason`.
- Affichage en console d’un récapitulatif : nombre de lignes lues / valides / rejetées.

### US-02 — Calcul des champs métier
En tant que planificateur, je veux calculer des champs dérivés pour prioriser les interventions de pose.

**Critères d’acceptation**
- `AddressLabel = "{Street}, {PostalCode} {City}"` après `Trim()` sur chaque champ et réduction des espaces multiples.
- `Department = PostalCode.Substring(0, 2)` (ex: `69003` → `69`).
- `DaysToPlan = (PlannedDate.Date - Today.Date).Days` (différence en jours calendaires, entier).
- `Priority` :
  - `P1` si `DaysToPlan <= 1`
  - `P2` si `DaysToPlan` est `2` ou `3`
  - `P3` si `DaysToPlan >= 4`
- Le `output.json` contient, pour chaque ligne valide, les champs d’entrée + `AddressLabel`, `Department`, `DaysToPlan`, `Priority`.

### US-03 — Enrichissement géocodage
En tant que planificateur, je veux des coordonnées GPS pour exploiter les interventions par zone.

**Critères d’acceptation**
- Appel géocodage sur `AddressLabel` via `GET https://data.geopf.fr/geocodage/search?q={AddressLabel}&limit=1`.
- Ajout de `Latitude`, `Longitude`, `GeoScore` dans `output.json`.
- En cas d’échec HTTP/time-out ou de 0 résultat : `Latitude/Longitude/GeoScore = null` et le traitement continue.

### US-04 — Simulation d’envoi HTTP
En tant qu’intégrateur, je veux simuler l’envoi du résultat vers un endpoint HTTP public.

**Critères d’acceptation**
- Construction d’un payload JSON (liste des interventions enrichies) correspondant à `output.json`.
- `POST https://httpbin.org/post` avec `Content-Type: application/json`.
- Affichage en console : code HTTP + taille du payload envoyé (octets).

**Exemple de payload**
```json
[
  {
    "workOrderId": "WO-2026-0101",
    "meterSerial": "MTR-69002-0101",
    "street": "12 Rue de la République",
    "postalCode": "69002",
    "city": "Lyon",
    "plannedDate": "2026-01-22",
    "addressLabel": "12 Rue de la République, 69002 Lyon",
    "department": "69",
    "daysToPlan": 1,
    "priority": "P1",
    "latitude": 45.7579,
    "longitude": 4.8321,
    "geoScore": 0.86
  }
]
