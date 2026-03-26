---
name: generate-tests
description: Methodologie pour ecrire des tests de qualite (analyse -> structure -> implementation -> coverage)
owner: SHERLOCK
domain: qa
triggers: Quand une mecanique doit etre testee ou quand un module manque de couverture
---

# Generate Tests

## Objectif
Ecrire des tests fiables, deterministes et maintenables qui detectent les vrais bugs sans generer de faux positifs.

## Quand l'utiliser
- Nouvelle feature implementee (INVOKE par CIPHER)
- Mecanique P0 de la checklist OAK sans couverture
- Apres un bugfix (test de regression)
- Refactoring d'un module existant

## Methodologie

### Etape 1 : Analyse
- Identifier la mecanique a tester : quels sont les inputs, outputs, effets de bord ?
- Lister les cas nominaux (happy path)
- Lister les cas limites (edge cases) : valeurs 0, max, negatives, types incompatibles
- Lister les cas d'erreur : inputs invalides, etats impossibles

### Etape 2 : Structure
- Minimum 3 tests par mecanique :
  1. **Happy path** : le cas normal fonctionne
  2. **Edge case** : les limites sont gerees correctement
  3. **Error case** : les erreurs sont gerees gracieusement
- Grouper par describe() logique (par mecanique, pas par fichier)
- Noms de tests descriptifs : "should deal 1.5x damage with STAB bonus" (pas "test1")

### Etape 3 : Implementation
- Syntaxe vitest (describe, it, expect)
- Fonctions pures UNIQUEMENT — aucune dependance a Phaser ou au DOM
- Tests deterministes : pas de Math.random(), pas de Date.now()
- Si la mecanique utilise du random, mocker le RNG avec une seed fixe
- Chaque test est independant : pas de dependance a l'ordre d'execution
- Setup/teardown via beforeEach si necessaire

### Etape 4 : Validation
- Tous les tests passent (`npx vitest run`)
- Verifier que les tests echouent quand on casse la feature (mutation testing mental)
- Chaque mecanique P0 de la checklist OAK a une couverture
- Pas de tests redondants (tester le meme cas 2 fois)

## Contraintes
- **Tech** : Pure functions only, vitest, deterministe, pas de Phaser
- **Business** : Tester le comportement (ce que le joueur voit), pas l'implementation
- **Produit** : Couverture de toutes les mecaniques P0
- **Design** : Les noms de tests servent de documentation vivante

## Anti-patterns
- Tests qui passent toujours (expect(true).toBe(true))
- Tests dependants de l'ordre d'execution
- Tester l'implementation au lieu du comportement (ex: "appelle calculateDamage 1 fois")
- Assertions trop precises sur des calculs avec arrondi (utiliser des ranges)
- Tests flaky (qui echouent aleatoirement)
- Un seul test geant au lieu de tests unitaires cibles

## Lecons apprises
- Les tests de taux de capture echouaient a cause d'arrondis floor() qui differaient de 1. Utiliser des assertions par range (toBeGreaterThan/toBeLessThan) pour les calculs avec arrondis.
- Tester le comportement observable, pas les details d'implementation — les tests survivent aux refactorings.
