---
name: brainstorm_initial
description: Résultat du brainstorm initial — direction créative validée pour Jurassic Trainers
type: project
---

# Brainstorm Initial — Jurassic Trainers

## Direction validée par KAELEN

### Concept
Clone Pokémon avec des dinosaures, style GBA Fire Red. Thème central : l'extinction.

### Mécaniques clés
- Core loop : Explorer → Combattre → Capturer → Évoluer → Progresser
- 150 dinos (50 lignées × 3 stades : Bébé/Jeune/Adulte)
- 14 types : Roche, Eau, Feu, Plante, Glace, Vol, Terre, Foudre, Poison, Acier, Ombre, Lumière, Sable, Fossile
- Combats tour par tour, 4 attaques actives
- Jurassic Balls pour capturer (basé PV + tempérament)
- Niveau max 50, évolutions entre niv 14-20 et 30-38
- Terrain de combat influence les types

### Starters
- PYREX (Feu → Feu/Roche)
- AQUADON (Eau → Eau/Glace)
- FLORASAUR (Plante → Plante/Terre)

### Monde
- Région : Pangaea
- 10 villes, 8 arènes + Ligue des 4
- Biomes : Plaines, Forêt, Côte, Montagnes, Volcan, Toundra, Marais, Désert, Plateau

### Villes et arènes
1. Bourg-Nid (départ)
2. Ville-Fougère (Plante)
3. Port-Coquille (Eau)
4. Roche-Haute (Roche)
5. Volcanville (Feu)
6. Cryo-Cité (Glace)
7. Electropolis (Foudre)
8. Marais-Noir (Poison)
9. Ciel-Haut (Vol)
10. Paléo-Capital (Fossile + Ligue des 4)

### Narrative
- Professeur Paléo confie le premier dino + Dinodex
- Rival : REX (petit-fils du Prof, arrogant puis humble)
- Méchants : Escadron Météore (veut provoquer l'extinction avec la Pierre d'Extinction)
- Commandant Impact (ancien dresseur, motivé par une perte personnelle)
- Légendaire : PANGAEON (Fossile/Lumière, gardien de l'équilibre)
- Arc en 3 actes

### Tech
- Phaser 3 + TypeScript
- Résolution 240×160 upscalée
- Maps via Tiled → JSON
- Données en JSON (dinos, attaques, items)

### Business
- Paiement unique, zéro microtransactions
- Durée : 25-35h principale, 50h+ complet

### Arbitrage types
- 14 types au lieu de 18 (fusion Vent→Vol, Marais→Poison/Eau, Magma→Feu/Roche, Cristal→Roche/Lumière)

## Prochaines étapes
Phase 1 Pré-Production : GDD, liste 150 dinos, table des types, carte monde, bible narrative, direction artistique, architecture technique, prototype v0.1
