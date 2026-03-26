# Nova Forge Studio — Template

Studio de developpement de jeux video avec 16+ agents specialises.

## Comment utiliser

1. Copier ce dossier dans un nouveau repertoire de projet
2. Ouvrir une session Claude Code dans ce repertoire
3. Dire : "Lance le studio Nova Forge pour creer un nouveau jeu"
4. Les agents feront un brainstorm, puis avanceront phase par phase

## Structure

```
studio-template/
  studio/
    agents/          # Fiches des agents (personnalite, competences, role)
      direction/     # KAELEN (Directeur Creatif)
      design/        # YUKI, MARCUS, SANA (Game Design)
      art/           # ZEPHYR, NOVA, ATLAS, PIXEL (Art & UI/UX)
      narrative/     # ORION, LYRA (Narration & Worldbuilding)
      tech/          # CIPHER, RUNE, SPARK, HARMONY (Tech & Dev)
      business/      # VESPER, ECHO (Business & Marketing)
    memory/
      agents/        # Memoire personnelle de chaque agent (vide au depart)
      phases/        # Memoire par phase du projet (remplie au fur et a mesure)
      project/       # Memoire globale du projet
    GOVERNANCE.md    # Regles de fonctionnement du studio
    WORKFLOW.md      # Pipeline de production (phases, livrables, validation)
  game/
    docs/            # Documents de design (GDD, Art Direction, etc.)
    src/             # Code source du jeu (vide au depart)
  CLAUDE.md          # Instructions pour Claude Code
```

## Phases de production

1. **Pre-Production** — Brainstorm, GCD, GDD, Art Direction, Narrative Bible, Tech Architecture
2. **Prototypage** — Prototype jouable validant le core gameplay
3. **Production** — Contenu complet, niveaux, systemes
4. **Polish & QA** — Bug fixes, optimisation, juice
5. **Lancement** — Marketing, release, community

## Agents disponibles

| Agent | Role | Specialite |
|-------|------|-----------|
| KAELEN | Directeur Creatif | Vision, arbitrage, coherence |
| YUKI | Lead Game Designer | Core loop, game feel, fun |
| MARCUS | System Designer | Progression, economie, equilibrage |
| SANA | Level Designer | Espaces, puzzles, flow |
| ORION | Lead Narratif | Histoire, dialogues, arcs |
| LYRA | Worldbuilder | Univers, lore, coherence monde |
| ZEPHYR | Directeur Artistique | Style visuel, palette, identite |
| NOVA | Concept Artist | Creatures, personnages, props |
| ATLAS | Environment Artist | Biomes, decors, ambiance |
| PIXEL | UI/UX Designer | Interface, accessibilite, menus |
| CIPHER | Lead Architect / CTO | Architecture, moteur, performance |
| RUNE | Dev Gameplay | Systemes, game feel, prototypage |
| SPARK | Prototypeur Rapide | MVP, iteration rapide |
| HARMONY | Sound Designer | Musique procedurale, SFX |
| VESPER | Product Manager | Monetisation, business model |
| ECHO | Community & Marketing | Strategie marketing, communaute |
