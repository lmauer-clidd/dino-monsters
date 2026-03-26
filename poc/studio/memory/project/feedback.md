---
name: feedback
description: Historique de tous les feedbacks utilisateur
type: project
---

# Feedback Utilisateur

### 2026-03-23 — "le graphisme joue un role majeur dans la qualite du jeu"
**Interpretation** : L'art direction est une priorite, pas un "nice to have"
**Impact** : Toutes les decisions visuelles doivent viser la qualite Pokemon moderne
**Action** : Feedback integre comme contrainte permanente
**Statut** : RESOLU — Direction artistique renforcee

### 2026-03-23 — "le style est trop pixelise, les textes a peine lisibles"
**Interpretation** : Viser qualite Pokemon moderne, pas GBA brut. Resolution plus haute, textes lisibles
**Impact** : Augmenter les tailles de police, ameliorer le rendu des dinos
**Action** : BattleHUD et DialogueBox ameliores
**Statut** : RESOLU

### 2026-03-23 — "pyrex s'affiche en bleu"
**Interpretation** : Bug de mapping types JSON→Code (type 2 = Water en code mais Fire en JSON)
**Impact** : Tous les dinos avaient potentiellement le mauvais type affiche
**Action** : Documente dans TYPE_MAPPING.md, restoreDoors() cree, validation GRID
**Statut** : RESOLU

### 2026-03-23 — "mon dino feu ne fait presque aucun degats au dino eau"
**Interpretation** : Les fallback stats etaient asymetriques (ennemi 2x plus de defense que joueur)
**Impact** : Combats completement desequilibres
**Action** : buildTrainerDino() + wild dino avec vrais baseStats + defaultStat() symetrique
**Statut** : RESOLU

### 2026-03-23 — "les combats ne sont pas equilibres"
**Interpretation** : Move power trop haute, HP trop bas, pas de split physique/special
**Impact** : Combats en 1-2 tours, one-shots constants
**Action** : Move power -30%, HP +40%, Def +20%, physical/special split, 134 tests automatises
**Statut** : RESOLU

### 2026-03-23 — "on ne peut pas rentrer dans les maisons, pas de portes"
**Interpretation** : Les chemins ecrasaient les tiles de porte, rendant les portes invisibles
**Impact** : Impossible d'acceder aux batiments
**Action** : restoreDoors() + entree auto en marchant sur porte + rendu porte ameliore
**Statut** : RESOLU

### 2026-03-23 — "il faut des skills pour les agents et qu'ils apprennent de leurs experiences"
**Interpretation** : Industrialiser la qualite via des methodologies formalisees et de la memoire
**Impact** : Systeme de skills + protocole de memoire + integration dans la generation d'agents
**Action** : SKILLS.md + MEMORY.md + 6 skills crees + GOVERNANCE mise a jour
**Statut** : RESOLU

### 2026-03-23 — "cycle d'amelioration continue jusqu'a qualite release"
**Interpretation** : Audit complet → priorisation P0/P1/P2/P3 → fix par vagues paralleles → validation
**Impact** : 13 issues corrigees en 3 vagues (P0→P1→P2+P3), 0 regression
**Action** : PP sync, evolution guard, status icons, capture animation, HP bar colors, XP animation, item descriptions, battle fade, damage numbers, audio safety, Dinodex counter deja present, battle intro deja present
**Statut** : RESOLU — 0 P0, 0 P1 restants
