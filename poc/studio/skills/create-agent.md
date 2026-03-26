---
name: create-agent
description: Pipeline pour creer un nouvel agent specialise dans le studio (analyse -> fiche -> skills -> memoire -> integration)
owner: KAELEN
domain: tech
triggers: Quand une competence manque dans le studio et qu'aucun agent existant ne peut la couvrir
---

# Create Agent

## Objectif
Ajouter un agent specialise au studio quand une competence manquante est identifiee, sans creer de redondance.

## Quand l'utiliser
- Competence necessaire non couverte par un agent existant
- Charge de travail trop importante sur un agent existant (split)
- Nouveau domaine du jeu qui necessite une expertise dediee

## Methodologie

### Etape 1 : Analyse
- Identifier precisement la competence manquante
- Verifier qu'aucun agent existant ne couvre deja ce role (consulter GOVERNANCE.md)
- Definir le departement d'appartenance (Tech, Design, Art, Narrative, QA, Animation)
- Lister les interactions prevues avec les agents existants

### Etape 2 : Production — Fiche Agent
Creer `studio/agents/[NOM].md` avec TOUTES les sections obligatoires :
- **Personnalite** : Traits de caractere distinctifs, maniere de s'exprimer, valeurs
- **Competences** : Domaines d'expertise specifiques
- **References** : Inspirations professionnelles (vrais studios, vrais experts)
- **Role** : Responsabilite principale en une phrase
- **Ce qu'il defend** : Valeurs non-negociables (min 3)
- **Red flags** : Ce qui le fait reagir negativement (min 3)
- **Collaborations** : Avec quels agents il travaille et comment
- **Protocoles d'invocation** : INVOKE (min 2) et REVIEW (min 1) protocols

### Etape 3 : Skills
- Heriter des skills du departement (automatique)
- Creer les skills specifiques au role unique de l'agent
- Documenter les skills dans la section "## Skills" de la fiche

### Etape 4 : Memoire
- Creer le repertoire `studio/memory/agents/<nom>/`
- Creer `lessons.md` (vide, sera rempli au fil du temps)
- Creer `preferences.md` (vide, sera rempli au fil du temps)

### Etape 5 : Integration
- Ajouter l'agent dans GOVERNANCE.md (listing equipe + autorite de decision)
- Mettre a jour DISPATCH.md si l'agent gere un nouveau type de tache
- Presenter l'agent aux agents avec lesquels il collaborera

### Etape 6 : Review
- KAELEN valide la coherence creative (personnalite distinctive, pas de doublon)
- CIPHER valide la coherence technique (protocols bien definis, pas de conflit)

## Contraintes
- **Tech** : Minimum 2 protocoles INVOKE, 1 obligation REVIEW, 3 red flags
- **Business** : L'agent doit avoir une valeur ajoutee claire et unique
- **Produit** : Coherence avec la culture du studio (desaccords sains, collaboration)
- **Design** : Personnalite memorable et distincte des agents existants

## Anti-patterns
- Agent sans personnalite (une fiche technique froide)
- Agent qui chevauche completement un agent existant (redondance)
- Agent sans skills (il ne sait rien faire de methodique)
- Agent isole (pas de collaborations definies)
- Agent fourre-tout (trop de responsabilites differentes)

## Lecons apprises
- Un bon agent a une opinion forte sur son domaine — les desaccords entre agents produisent de meilleures decisions.
- La personnalite n'est pas decorative : elle guide les decisions de l'agent quand les regles ne couvrent pas un cas.
