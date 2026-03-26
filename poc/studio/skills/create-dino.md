---
name: create-dino
description: Pipeline complet pour creer une nouvelle espece de dino (nom -> types -> stats -> learnset -> review)
owner: FOSSIL
domain: design
triggers: Quand une nouvelle espece de dino doit etre ajoutee au jeu
---

# Create Dino

## Objectif
Creer un dino equilibre, coherent et memorable qui enrichit la diversite du roster.

## Quand l'utiliser
- Ajout d'une nouvelle espece au Dinodex
- Completion d'une ligne d'evolution existante
- Remplissage d'un gap dans la distribution des types

## Methodologie

### Etape 1 : Analyse
- Verifier la distribution des types existants — eviter la surrepresentation
- Verifier les lignes d'evolution incompletes — prioriser leur completion
- Identifier le niche ecologique du dino (early game, mid game, late game, legendaire)
- Definir le concept visuel/thematique avant les stats

### Etape 2 : Production
- **Nom** : Evocateur du type/theme, pronounceable, memorable (3 propositions via LYRA)
- **Types** : INVOKE PRISM pour valider la combinaison de types (synergie, originalite)
- **Base Stats** : INVOKE SCALE pour verifier l'equilibre (total, distribution, comparaison aux pairs)
- **Learnset** : INVOKE STRIPE pour definir les moves appris
  - Charge + Grondement obligatoires au Lv1
  - Pas de moves types avant Lv7
  - Minimum 4 moves dans le learnset
- **Description** : Texte lore coherent avec le monde et le type
- **Evolution** : Chaine d'evolution avec niveaux (la transformation doit avoir un sens visuel)

### Etape 3 : Validation
- Tous les moveIds du learnset existent dans moves.json
- Toutes les cibles d'evolution existent dans dinos.json
- Stats dans un range raisonnable pour le tier du dino
- Charge + Grondement presents au Lv1
- Pas de move type avant Lv7

### Etape 4 : Deploiement
- Ajouter l'entree dans dinos.json avec le bon format
- GRID valide l'integrite referentielle
- SCALE lance une simulation d'equilibre avec le nouveau dino
- Mettre a jour le Dinodex si necessaire

## Contraintes
- **Tech** : Utiliser la numerotation de types JSON (0=Fossil, 1=Water, 2=Fire, etc.), PAS le DinoType enum du code
- **Business** : Chaque dino doit etre potentiellement le favori d'un joueur
- **Produit** : Coherence Pokemon-like (3 stades d'evolution max, stats qui progressent)
- **Design** : Minimum 4 moves, progression logique du learnset

## Anti-patterns
- Stats identiques a un autre dino existant (manque d'identite)
- Nom qui n'evoque pas le type ou le theme du dino
- Evolution qui n'a pas de sens visuel (ex: poisson -> oiseau sans logique)
- Utiliser le DinoType enum au lieu de la numerotation JSON
- Learnset trop court (< 4 moves)

## Lecons apprises
- La numerotation des types dans le JSON est differente de l'enum dans le code. TOUJOURS utiliser la numerotation JSON dans dinos.json (0=Fossil, 1=Water, 2=Fire, etc.)
- Verifier les references croisees (moveIds, evolution targets) AVANT de commit
