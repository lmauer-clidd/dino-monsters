# Nova Forge Studio — Systeme de Skills Agents

## Principe

Un **skill** est une methodologie replicable qu'un agent applique pour garantir la qualite de son travail. Chaque skill encode les best practices, les contraintes (tech, business, produit, design), et les etapes de validation pour un type de tache specifique.

Les skills sont le **savoir-faire industrialise** du studio. Un agent sans skills improvise. Un agent avec skills produit de la qualite constante.

---

## Structure d'un Skill

Chaque skill est un fichier Markdown dans `studio/skills/` avec ce format :

```markdown
---
name: nom-du-skill
description: Ce que le skill permet de faire
owner: AGENT_PRINCIPAL
domain: tech | design | art | narrative | balance | qa | animation
triggers: Quand ce skill est active
---

# [Nom du Skill]

## Objectif
Quel probleme ce skill resout.

## Quand l'utiliser
Conditions d'activation (triggers).

## Methodologie

### Etape 1 : Analyse
- Ce qu'il faut examiner avant de commencer
- Donnees a collecter
- Questions a se poser

### Etape 2 : Resolution / Production
- Processus pas a pas
- Best practices a respecter
- Contraintes a verifier (tech, business, produit, design)

### Etape 3 : Validation
- Tests a executer
- Criteres de qualite
- Checklist de sortie

### Etape 4 : Deploiement
- Comment integrer le livrable
- Qui doit reviewer
- Quoi documenter

## Contraintes
- **Tech** : (performances, architecture, compatibilite)
- **Business** : (pas de dark patterns, respect du joueur)
- **Produit** : (coherence avec la vision Pokemon-like)
- **Design** : (fun, equilibre, accessibilite)

## Anti-patterns
Ce qu'il ne faut PAS faire (erreurs courantes).

## Exemples
Cas concrets d'application du skill.

## Lecons apprises
(Rempli au fil du temps par l'agent — voir section Memoire)
```

---

## Skills par departement

### TECH
| Skill | Owner | Description |
|-------|-------|-------------|
| `implement-feature` | CIPHER | Pipeline complet pour implementer une feature (spec → code → test → review → merge) |
| `debug-and-fix` | CIPHER | Methodologie de diagnostic (reproduire → isoler → corriger → test regression) |
| `data-migration` | GRID | Modifier un schema JSON sans casser les references (backup → migrate → validate → deploy) |
| `performance-audit` | PULSE | Mesurer et optimiser les performances (profiling → bottleneck → fix → benchmark) |
| `prototype-rapid` | SPARK | Creer un POC en < 1h (hypothesis → minimal code → measure → decide) |

### DESIGN / BALANCE
| Skill | Owner | Description |
|-------|-------|-------------|
| `create-dino` | FOSSIL | Creer un nouveau dino (nom → types → stats → learnset → description → review) |
| `create-move` | CLAW | Creer une nouvelle attaque (tier → type → power → pp → effect → balance check) |
| `balance-check` | SCALE | Verifier l'equilibre d'un changement (POC simulation → metrics → adjust → validate) |
| `design-gym` | ARENA | Designer une arene (type → equipe → strategie → difficulte → review) |
| `map-encounters` | COMPASS | Definir les encounters d'une zone (level range → species → weights → xp validation) |

### QA
| Skill | Owner | Description |
|-------|-------|-------------|
| `audit-mechanic` | OAK | Auditer une mecanique de jeu (checklist → code review → test → verdict) |
| `generate-tests` | SHERLOCK | Generer des tests pour un module (identify cases → write tests → run → coverage report) |
| `regression-test` | SHERLOCK | Apres un bug fix, creer le test qui empeche la regression |

### ANIMATION
| Skill | Owner | Description |
|-------|-------|-------------|
| `animate-combat` | FLICKER | Creer une animation de combat (spec timing → keyframes → particles → audio sync) |
| `create-vfx` | EMBER | Creer un effet visuel (palette → particle config → intensity levels → review) |

### NARRATIVE
| Skill | Owner | Description |
|-------|-------|-------------|
| `write-event` | ORION | Ecrire un evenement narratif (contexte → dialogues → choix → flags → integration) |
| `name-entity` | LYRA | Nommer un dino/lieu/personnage (lore check → 3 propositions → validation) |

---

## Comment creer un nouveau Skill

Quand un agent realise qu'il repete un processus, il doit le formaliser en skill :

1. **Identifier le pattern** : "Je fais toujours les memes etapes quand je cree un dino"
2. **Documenter les etapes** : Analyse → Production → Validation → Deploiement
3. **Ajouter les contraintes** : Tech, Business, Produit, Design
4. **Ajouter les anti-patterns** : Ce qui a echoue dans le passe
5. **Sauvegarder** dans `studio/skills/[nom-du-skill].md`
6. **Referencer** dans la fiche agent (section "Skills")

---

## Integration avec la generation d'agents

Quand un nouvel agent est cree (via la regle "Creation d'agents" de GOVERNANCE.md), il DOIT :

1. **Heriter des skills de son departement** — Les skills du departement sont automatiquement disponibles
2. **Creer ses skills specifiques** — Si son role necessite une methodologie unique
3. **Documenter ses skills dans sa fiche** — Nouvelle section "## Skills" apres "## Protocoles d'invocation"

Format dans la fiche agent :
```markdown
## Skills
- `implement-feature` — Pipeline complet pour implementer une feature
- `mon-skill-specifique` — Description courte
```

---

## Integration avec la memoire

Les skills evoluent grace a la memoire des agents. Apres chaque utilisation d'un skill, l'agent DOIT mettre a jour :

1. **Le skill lui-meme** (section "Lecons apprises") si une nouvelle best practice est decouverte
2. **Sa memoire personnelle** (`studio/memory/agents/<nom>/`) avec le contexte specifique

Voir `studio/MEMORY.md` pour le protocole complet de memoire.
