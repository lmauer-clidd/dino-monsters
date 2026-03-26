# Nova Forge Studio — Protocole de Memoire Agent

## Principe

Les agents **apprennent de leurs experiences**. Chaque agent maintient une memoire personnelle qui l'aide a travailler mieux et plus vite. La memoire n'est pas optionnelle — c'est une obligation de qualite.

> "Un agent qui repete la meme erreur deux fois n'a pas de memoire. Un studio qui repete la meme erreur deux fois n'a pas de culture."

---

## Structure de la memoire

```
studio/memory/
  agents/
    <NOM>/
      lessons.md       — Lecons apprises (erreurs, decouvertes, best practices)
      preferences.md   — Preferences et decisions recurrentes
      context.md       — Contexte du projet (etat actuel, decisions passees)
  phases/
    <NUM>/
      decisions.md     — Decisions collectives de la phase
      retrospective.md — Ce qui a marche / echoue
  project/
      vision.md        — Vision du projet (mise a jour par KAELEN)
      feedback.md      — Feedback utilisateur (TOUJOURS sauvegarde)
      conventions.md   — Conventions de code/data/naming
```

---

## Quand ecrire en memoire

### OBLIGATOIRE (apres chaque tache)

| Evenement | Quoi sauvegarder | Ou |
|-----------|-----------------|-----|
| Bug corrige | Cause racine + fix + comment eviter | `agents/<nom>/lessons.md` |
| Erreur commise | Ce qui s'est passe + pourquoi + prevention | `agents/<nom>/lessons.md` |
| Decision prise | Choix + alternatives rejetees + raison | `agents/<nom>/preferences.md` |
| Feedback utilisateur | Citation exacte + interpretation + action | `project/feedback.md` |
| Fin de phase | Retrospective collective | `phases/<num>/retrospective.md` |

### RECOMMANDE (quand pertinent)

| Evenement | Quoi sauvegarder | Ou |
|-----------|-----------------|-----|
| Astuce decouverte | Technique ou raccourci utile | `agents/<nom>/lessons.md` |
| Pattern identifie | Processus repetitif a formaliser en skill | `agents/<nom>/lessons.md` |
| Preference clarifiee | Choix esthetique, technique ou design | `agents/<nom>/preferences.md` |
| Contexte change | Nouvelle contrainte ou direction | `agents/<nom>/context.md` |

---

## Format d'une entree memoire

```markdown
### [DATE] — [TITRE COURT]

**Contexte** : Ce sur quoi je travaillais
**Evenement** : Ce qui s'est passe
**Lecon** : Ce que j'ai appris
**Action** : Ce que je ferai differemment la prochaine fois
**Tags** : #bug #balance #data #architecture #ux ...
```

### Exemple (GRID)
```markdown
### 2026-03-23 — Les chemins ecrasent les portes

**Contexte** : Audit des batiments dans les villes
**Evenement** : Les tiles Door etaient placees par buildBuilding() mais les chemins (Path) traces APRES ecrasaient les portes. Toutes les portes de Bourg-Nid etaient invisibles.
**Lecon** : L'ordre de placement des tiles est CRITIQUE. Les elements importants (portes, signs) doivent etre places EN DERNIER ou proteges par une passe finale.
**Action** : Cree restoreDoors() qui repose toutes les portes apres les chemins. Appele automatiquement au chargement de ALL_MAPS.
**Tags** : #bug #maps #tiles #doors #ordre-de-placement
```

### Exemple (SCALE)
```markdown
### 2026-03-23 — Les fallback stats etaient asymetriques

**Contexte** : Le joueur trouvait que Pyrex etait trop faible contre les ennemis
**Evenement** : Les stats par defaut des ennemis etaient gonflees (def = 7 + level*2 = 17) alors que le joueur avait ses vraies stats calculees (def = 9). L'ennemi avait 2x plus de defense.
**Lecon** : Ne JAMAIS utiliser des formules de fallback differentes pour le joueur et l'ennemi. Utiliser la meme formule (formule Pokemon) pour les deux cotes.
**Action** : Remplace les fallbacks par defaultStat() symetrique + buildTrainerDino() avec les vrais baseStats.
**Tags** : #bug #balance #stats #asymetrie #formule
```

---

## Comment un agent utilise sa memoire

### Au debut d'une tache
1. **Relire** ses `lessons.md` et `preferences.md` pertinentes
2. **Chercher** si un probleme similaire a deja ete rencontre
3. **Appliquer** les lecons apprises pour eviter les memes erreurs

### Pendant une tache
4. **Noter** les decisions prises et pourquoi (meme les petites)
5. **Documenter** les problemes rencontres en temps reel

### Apres une tache
6. **Ecrire** la lecon apprise dans `lessons.md`
7. **Mettre a jour** les skills si une best practice est decouverte
8. **Partager** avec l'equipe si la lecon est transversale (→ `project/conventions.md`)

---

## Memoire collective (projet)

### `project/feedback.md`
CHAQUE feedback utilisateur est sauvegarde ici. Format :
```markdown
### [DATE] — Feedback: "[citation exacte]"
**Interpretation** : Ce que l'utilisateur veut dire
**Impact** : Ce qu'on doit changer
**Action** : Qui fait quoi
**Statut** : OUVERT | EN COURS | RESOLU
```

### `project/conventions.md`
Les conventions decouvertes au fil du temps :
- "Les types JSON et les types code utilisent des numerotations differentes → toujours passer par mapType()"
- "Les chemins doivent etre traces AVANT les batiments, et restoreDoors() doit etre appele a la fin"
- "Pas de move type avant le niveau 7 pour les starters"
- etc.

### `project/vision.md`
La vision du projet mise a jour par KAELEN :
- Clone Pokemon avec des dinosaures
- Style GBA mais qualite moderne (lisible, soigne)
- 150 dinos, 8 arenes, Ligue des 4, histoire complete
- Priorite sur le gameplay et l'equilibre

---

## Integration avec les Skills

Les skills et la memoire sont lies :

```
Agent execute une tache
  → Applique le skill correspondant
  → Rencontre un probleme / decouvre une astuce
  → Ecrit dans sa memoire (lessons.md)
  → Met a jour le skill (section "Lecons apprises")
  → La prochaine fois, le skill est meilleur
```

C'est un **cycle d'amelioration continue**. Les skills encodent le "comment", la memoire encode le "pourquoi" et les "exceptions".
