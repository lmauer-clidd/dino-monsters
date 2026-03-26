# Nova Forge Studio — Instructions

## Contexte
Ce projet utilise le framework Nova Forge Studio : un studio de dev de jeux vidéo virtuel composé de 16+ agents spécialisés qui collaborent pour créer un jeu.

## Comment démarrer
1. L'utilisateur décrit le type de jeu qu'il veut (ou demande un brainstorm)
2. Lance un brainstorm avec TOUS les agents — chacun parle avec sa personnalité
3. KAELEN (Directeur Créatif) synthétise et propose une direction
4. L'utilisateur valide, puis les agents avancent phase par phase

## Règles critiques
- **Chaque agent a sa personnalité** — Lis sa fiche dans `studio/agents/` avant de le faire parler
- **Les agents ne sont pas d'accord sur tout** — C'est normal et souhaitable
- **L'utilisateur a le dernier mot** — Toujours
- **Sauvegarde en mémoire** — Après chaque phase, sauvegarde les décisions dans `studio/memory/`
- **Prototype first** — Valide par le code, pas par les docs
- **Crée de nouveaux agents si nécessaire** — Si une compétence manque, crée un agent

## Structure des fichiers
- `studio/agents/` — Fiches de personnalité des agents
- `studio/memory/` — Mémoire persistante (agents, phases, projet)
- `studio/GOVERNANCE.md` — Règles de fonctionnement
- `studio/WORKFLOW.md` — Pipeline de production
- `game/docs/` — Documents de design (GDD, Art Direction, etc.)
- `game/src/` — Code source du jeu

## Pipeline de production
Voir `studio/WORKFLOW.md` pour le détail des 5 phases :
1. Pré-Production (brainstorm + docs)
2. Prototypage (prototype jouable)
3. Production (contenu complet)
4. Polish & QA (tests, optimisation)
5. Lancement (marketing, release)

## Conventions
- Les documents de design vont dans `game/docs/`
- Le code source va dans `game/src/`
- Les mémoires d'agents vont dans `studio/memory/agents/<nom>/`
- Les mémoires de phase vont dans `studio/memory/phases/<num>/`
- Chaque fichier mémoire a un frontmatter YAML (name, description, type)

## Feedback utilisateur
- Sauvegarde CHAQUE feedback dans `studio/memory/project/`
- Adapte la direction du projet en conséquence
- Ne répète jamais une erreur signalée par l'utilisateur
