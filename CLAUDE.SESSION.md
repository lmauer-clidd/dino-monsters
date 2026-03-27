# Session Context — Dino Monsters

> **INSTRUCTION CRITIQUE** : Si c'est une nouvelle session, lis TOUS les fichiers listés dans la section "Fichiers de contexte" ci-dessous AVANT de répondre. Après chaque commit, mets à jour le journal de bord.

## Fichiers de contexte à lire au démarrage

```
1. CLAUDE.md                          — Instructions du projet (Nova Forge Studio)
2. CLAUDE.SESSION.md                  — CE FICHIER (contexte global)
3. context/HISTORY.md                 — Journal chronologique de tout ce qu'on a fait
4. context/DECISIONS.md               — Décisions techniques et pourquoi
5. context/LESSONS.md                 — Ce qui a marché / pas marché
6. context/CURRENT_STATE.md           — État actuel exact du projet
7. studio/memory/project/feedback.md  — Tous les feedbacks utilisateur
8. studio/memory/project/conventions.md — Conventions techniques découvertes
```

## Comment utiliser ce système

### Nouvelle session
```
1. Lis CLAUDE.md + CLAUDE.SESSION.md
2. Lis context/CURRENT_STATE.md pour savoir où on en est
3. Lis context/HISTORY.md pour le contexte complet
4. Lis context/DECISIONS.md et LESSONS.md si besoin de détails
5. Tu es prêt à continuer le travail
```

### Pendant le travail
```
- Après chaque commit : mets à jour context/CURRENT_STATE.md
- Après chaque décision importante : ajoute dans context/DECISIONS.md
- Après chaque échec/succès notable : ajoute dans context/LESSONS.md
- Après chaque phase complétée : ajoute dans context/HISTORY.md
```

### Merge/PR
```
- Avant merge : résumé de la branche dans HISTORY.md
- Après merge : mise à jour de CURRENT_STATE.md
```
