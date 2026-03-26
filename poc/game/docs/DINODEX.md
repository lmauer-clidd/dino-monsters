---
name: DINODEX
description: Complete database of all 150 dinosaurs for Jurassic Trainers
type: game-design
authors: NOVA (Concept Artist), MARCUS (System Designer)
version: 1.0
date: 2026-03-22
---

# DINODEX — Jurassic Trainers

> *"Chaque dinosaure raconte une histoire de millions d'annees. A toi de la decouvrir."*
> — Professeur Paleo, Introduction au Dinodex

## Table des matieres

- Lignes #001-009 : Starters (Feu, Eau, Plante)
- Lignes #010-018 : Routes initiales (Roche, Vol, Poison)
- Lignes #019-027 : Foret ancienne (Plante, Terre, Vol)
- Lignes #028-036 : Grottes & Volcans (Feu, Roche, Acier)
- Lignes #037-048 : Zones intermediaires (Eau, Foudre, Glace, Sable)
- Lignes #049-060 : Mi-jeu (Terre, Poison, Ombre, Vol)
- Lignes #061-078 : Zones avancees (Acier, Lumiere, Fossile, Sable)
- Lignes #079-096 : Fin de jeu (Glace, Ombre, Foudre, Feu)
- Lignes #097-141 : Zones secretes & speciales
- Lignes #142-150 : Legendaires & Pseudo-legendaires

## Capacites passives (Abilities)

| # | Nom | Effet |
|---|-----|-------|
| 1 | Peau Dure | Reduit les degats de contact de 15% |
| 2 | Torrent | Les attaques Eau sont boostees de 50% quand PV < 33% |
| 3 | Flamme Vive | Les attaques Feu sont boostees de 50% quand PV < 33% |
| 4 | Vegetal | Les attaques Plante sont boostees de 50% quand PV < 33% |
| 5 | Machoire | Les attaques de type morsure infligent 30% de degats en plus |
| 6 | Carapace | Reduit la chance de coup critique adverse de 50% |
| 7 | Velocite | Agit toujours en premier quand PV = 100% |
| 8 | Fossile Ancien | Les attaques Fossile infligent 20% de degats en plus |
| 9 | Ecailles Epaisses | Reduit les degats speciaux recus de 15% |
| 10 | Sang Froid | Immunise contre la Brulure |
| 11 | Nage Rapide | Vitesse doublee sous la pluie |
| 12 | Regeneration | Recupere 1/16 de ses PV max a chaque tour |
| 13 | Intimidation | Reduit l'Attaque adverse de 1 cran a l'entree en combat |
| 14 | Cuirasse | Les attaques physiques recoivent un malus de 20% |
| 15 | Charge Statique | 30% de chance de paralyser l'adversaire au contact |
| 16 | Venin | 20% de chance d'empoisonner l'adversaire au contact |
| 17 | Ailes Coupantes | Les attaques Vol infligent 20% de degats en plus |
| 18 | Ombre Furtive | Precision augmentee de 20% |
| 19 | Photosynthese | Recupere 1/8 PV max par tour au soleil |
| 20 | Armure Naturelle | Ignore les baisses de Defense |
| 21 | Griffe Aceree | Les attaques de griffes ont +15% de chance de critique |
| 22 | Tempete de Sable | Invoque une tempete de sable a l'entree en combat |
| 23 | Instinct Predateur | Attaque augmentee de 1 cran apres chaque KO |
| 24 | Aura Lumineuse | Les attaques Lumiere infligent 20% de degats en plus |
| 25 | Gel Eternel | Les attaques Glace ont 20% de chance de geler |

---

## LIGNE 1 — PYREX (Starter Feu)

### #001 — PYREX
- **Types** : Feu
- **Stage** : Bebe (Ligne : Pyrex)
- **Evolution** : Evolue en Pyrovore au niveau 16
- **Base Stats** : PV 45 / Atk 55 / Def 35 / AtkSp 50 / DefSp 35 / Vit 60 (Total : 280)
- **Abilities** : Flamme Vive / Machoire (cachee)
- **Dino reel inspire** : Compsognathus
- **Design** : Petit theropode bipede au corps lisse et chaud, avec une crete de flammes miniatures sur la tete. Sa queue se termine par une petite flamme orange vif. Ses yeux sont grands et expressifs, avec des pupilles en fente.
- **Dinodex Entry** : "La flamme au bout de sa queue indique son humeur. Quand Pyrex est heureux, elle brille d'un orange eclatant."
- **Habitat** : Starter (Laboratoire du Professeur Paleo)
- **Rarete** : Starter
- **XP Group** : Moyen

### #002 — PYROVORE
- **Types** : Feu
- **Stage** : Jeune (Ligne : Pyrex)
- **Evolution** : De Pyrex au niveau 16 / Evolue en Volcanorex au niveau 36
- **Base Stats** : PV 65 / Atk 80 / Def 50 / AtkSp 70 / DefSp 50 / Vit 65 (Total : 380)
- **Abilities** : Flamme Vive / Machoire (cachee)
- **Dino reel inspire** : Allosaurus
- **Design** : Theropode elance aux ecailles rouge sombre, avec des veines incandescentes visibles le long de ses flancs. Ses bras courts portent des griffes noires fumantes. Sa crete de flammes s'est allongee en une criniere de feu.
- **Dinodex Entry** : "Pyrovore chasse en embuscade, cachant ses flammes sous ses ecailles avant de bondir sur sa proie dans une eruption de chaleur."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

### #003 — VOLCANOREX
- **Types** : Feu / Roche
- **Stage** : Adulte (Ligne : Pyrex)
- **Evolution** : De Pyrovore au niveau 36
- **Base Stats** : PV 90 / Atk 110 / Def 75 / AtkSp 95 / DefSp 65 / Vit 40 (Total : 475)
- **Abilities** : Flamme Vive / Machoire (cachee)
- **Dino reel inspire** : Tyrannosaurus Rex
- **Design** : Tyrannosaure colossal dont le dos est couvert de plaques de roche volcanique incandescente. De la lave coule entre les fissures de son armure naturelle. Sa gueule enorme degage une fumee permanente et ses yeux brillent comme des braises. Sa queue massive se termine par un amas de roche en fusion.
- **Dinodex Entry** : "On dit que Volcanorex peut provoquer des eruptions miniatures en frappant le sol. Les anciens peuples le veneraient comme le gardien des volcans."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

---

## LIGNE 2 — AQUADON (Starter Eau)

### #004 — AQUADON
- **Types** : Eau
- **Stage** : Bebe (Ligne : Aquadon)
- **Evolution** : Evolue en Marexis au niveau 16
- **Base Stats** : PV 50 / Atk 40 / Def 45 / AtkSp 55 / DefSp 45 / Vit 45 (Total : 280)
- **Abilities** : Torrent / Nage Rapide (cachee)
- **Dino reel inspire** : Plesiosaurus (juvenile)
- **Design** : Petit plesiosaure aux proportions adorables, avec un long cou fin et une tete ronde aux grands yeux bleus. Sa peau est bleu clair avec des taches blanches rappelant l'ecume. Ses nageoires sont petites et arrondies.
- **Dinodex Entry** : "Aquadon adore jouer dans les vagues. Il utilise son long cou pour attraper des poissons avec une precision surprenante."
- **Habitat** : Starter (Laboratoire du Professeur Paleo)
- **Rarete** : Starter
- **XP Group** : Moyen

### #005 — MAREXIS
- **Types** : Eau
- **Stage** : Jeune (Ligne : Aquadon)
- **Evolution** : De Aquadon au niveau 16 / Evolue en Abyssaure au niveau 36
- **Base Stats** : PV 70 / Atk 55 / Def 65 / AtkSp 75 / DefSp 65 / Vit 50 (Total : 380)
- **Abilities** : Torrent / Nage Rapide (cachee)
- **Dino reel inspire** : Mosasaurus (juvenile)
- **Design** : Reptile marin elance au corps hydrodynamique bleu nuit. Des nageoires tranchantes ornent son dos et sa queue. Sa machoire allongee porte des rangees de dents fines. Des motifs bioluminescents apparaissent le long de ses flancs.
- **Dinodex Entry** : "Marexis peut plonger a plus de 200 metres. Les lumieres sur ses flancs servent a communiquer avec ses congeneres dans les profondeurs."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

### #006 — ABYSSAURE
- **Types** : Eau / Glace
- **Stage** : Adulte (Ligne : Aquadon)
- **Evolution** : De Marexis au niveau 36
- **Base Stats** : PV 95 / Atk 65 / Def 80 / AtkSp 110 / DefSp 90 / Vit 35 (Total : 475)
- **Abilities** : Torrent / Nage Rapide (cachee)
- **Dino reel inspire** : Mosasaurus (adulte) + elements abyssaux
- **Design** : Leviathan marin titanesque au corps bleu abyssal presque noir. Des cristaux de glace forment une couronne autour de sa tete. Ses yeux brillent d'un bleu glacial. D'immenses nageoires s'etendent comme des ailes sous-marines, et sa queue se termine par une nageoire caudale ornee de givre permanent.
- **Dinodex Entry** : "Abyssaure regne sur les abysses les plus froids. Quand il remonte a la surface, la temperature chute brutalement et la mer gele sur des kilometres."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

---

## LIGNE 3 — FLORASAUR (Starter Plante)

### #007 — FLORASAUR
- **Types** : Plante
- **Stage** : Bebe (Ligne : Florasaur)
- **Evolution** : Evolue en Sylvacolle au niveau 16
- **Base Stats** : PV 50 / Atk 45 / Def 50 / AtkSp 50 / DefSp 50 / Vit 35 (Total : 280)
- **Abilities** : Vegetal / Photosynthese (cachee)
- **Dino reel inspire** : Apatosaurus (juvenile)
- **Design** : Petit sauropode trapu avec des fleurs colorees qui poussent le long de son dos. Sa peau est vert tendre avec des motifs de feuilles. Il a de grands yeux noisette et un sourire naturellement doux. De petites pousses sortent de sa tete.
- **Dinodex Entry** : "Les fleurs sur le dos de Florasaur changent de couleur avec les saisons. Au printemps, elles repandent un parfum qui attire les papillons."
- **Habitat** : Starter (Laboratoire du Professeur Paleo)
- **Rarete** : Starter
- **XP Group** : Moyen

### #008 — SYLVACOLLE
- **Types** : Plante
- **Stage** : Jeune (Ligne : Florasaur)
- **Evolution** : De Florasaur au niveau 16 / Evolue en Titanarbore au niveau 36
- **Base Stats** : PV 70 / Atk 60 / Def 70 / AtkSp 65 / DefSp 70 / Vit 45 (Total : 380)
- **Abilities** : Vegetal / Photosynthese (cachee)
- **Dino reel inspire** : Brachiosaurus (juvenile)
- **Design** : Sauropode de taille moyenne dont le dos porte de jeunes arbres et buissons. Son cou allonge est entoure de lierre. Sa peau est vert foret avec des taches brun-mousse. Des champignons lumineux poussent a la base de sa queue.
- **Dinodex Entry** : "Sylvacolle sert de refuge ambulant pour de nombreux petits animaux. Les arbres sur son dos produisent des fruits nutritifs toute l'annee."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

### #009 — TITANARBORE
- **Types** : Plante / Terre
- **Stage** : Adulte (Ligne : Florasaur)
- **Evolution** : De Sylvacolle au niveau 36
- **Base Stats** : PV 100 / Atk 80 / Def 95 / AtkSp 85 / DefSp 85 / Vit 30 (Total : 475)
- **Abilities** : Vegetal / Photosynthese (cachee)
- **Dino reel inspire** : Argentinosaurus
- **Design** : Sauropode titanesque dont le corps entier est un ecosysteme vivant. D'immenses arbres centenaires poussent sur son dos, formant une petite foret. Son cou est couvert de mousse epaisse et de lianes. Ses pattes massives ressemblent a des troncs d'arbres, et chaque pas fait pousser de l'herbe sous ses pieds.
- **Dinodex Entry** : "Titanarbore est si ancien que certaines forets entieres sont nees de ses passages. On dit qu'il porte sur son dos l'equilibre de la nature elle-meme."
- **Habitat** : Starter evolution
- **Rarete** : Starter
- **XP Group** : Moyen

---

## LIGNE 4 — CAILLEX (Route 1 — Roche)

### #010 — CAILLEX
- **Types** : Roche
- **Stage** : Bebe (Ligne : Caillex)
- **Evolution** : Evolue en Graviroc au niveau 18
- **Base Stats** : PV 40 / Atk 45 / Def 55 / AtkSp 25 / DefSp 30 / Vit 35 (Total : 230)
- **Abilities** : Peau Dure / Carapace (cachee)
- **Dino reel inspire** : Protoceratops (juvenile)
- **Design** : Petit ceratopsien trapu couvert de cailloux et de gravier incrustes dans sa peau. Sa collerette est une plaque de roche plate et rugueuse. Ses yeux ronds et curieux depassent d'un visage gris-brun.
- **Dinodex Entry** : "Caillex adore se rouler dans les graviers. Plus il accumule de pierres sur sa peau, plus il se sent en securite."
- **Habitat** : Route 1 — Sentier des Fossiles
- **Rarete** : Commun
- **XP Group** : Rapide

### #011 — GRAVIROC
- **Types** : Roche
- **Stage** : Jeune (Ligne : Caillex)
- **Evolution** : De Caillex au niveau 18 / Evolue en Monolisaure au niveau 34
- **Base Stats** : PV 60 / Atk 65 / Def 80 / AtkSp 35 / DefSp 45 / Vit 30 (Total : 315)
- **Abilities** : Peau Dure / Carapace (cachee)
- **Dino reel inspire** : Protoceratops (adulte)
- **Design** : Ceratopsien de taille moyenne dont la collerette s'est transformee en un bouclier de granit. Des strates geologiques sont visibles le long de ses flancs. Sa peau est gris ardoise, avec des cristaux de quartz incrustes.
- **Dinodex Entry** : "Graviroc peut identifier l'age d'une roche rien qu'en la touchant du museau. Les geologues l'utilisent parfois comme assistant de terrain."
- **Habitat** : Route 1 — Sentier des Fossiles / Grottes de Gneiss
- **Rarete** : Commun
- **XP Group** : Rapide

### #012 — MONOLISAURE
- **Types** : Roche / Acier
- **Stage** : Adulte (Ligne : Caillex)
- **Evolution** : De Graviroc au niveau 34
- **Base Stats** : PV 85 / Atk 90 / Def 120 / AtkSp 40 / DefSp 60 / Vit 25 (Total : 420)
- **Abilities** : Peau Dure / Armure Naturelle (cachee)
- **Dino reel inspire** : Triceratops
- **Design** : Ceratopsien massif dont la collerette est un monolithe de roche polie. Ses trois cornes sont en acier naturel, brillant sous le soleil. Son corps est couvert de plaques tectoniques qui bougent lentement quand il marche. Des filons metalliques parcourent sa peau de roche.
- **Dinodex Entry** : "Monolisaure est aussi immobile qu'une montagne. Les voyageurs le confondent parfois avec un rocher avant qu'il ne se leve en faisant trembler le sol."
- **Habitat** : Grottes de Gneiss / Mont Ardoise
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 5 — PLUMEX (Route 1 — Vol)

### #013 — PLUMEX
- **Types** : Vol
- **Stage** : Bebe (Ligne : Plumex)
- **Evolution** : Evolue en Aeroraptor au niveau 15
- **Base Stats** : PV 35 / Atk 40 / Def 25 / AtkSp 35 / DefSp 30 / Vit 60 (Total : 225)
- **Abilities** : Ailes Coupantes / Velocite (cachee)
- **Dino reel inspire** : Microraptor (juvenile)
- **Design** : Minuscule raptor a quatre ailes couvertes de plumes iridescentes bleu-vert. Ses grands yeux curieux occupent la moitie de son visage. Sa queue plumeuse est aussi longue que son corps.
- **Dinodex Entry** : "Plumex ne sait pas encore bien voler. Il saute d'arbre en arbre en battant frenetiquement ses quatre ailes, laissant des plumes dans son sillage."
- **Habitat** : Route 1 — Sentier des Fossiles / Foret du Cretace
- **Rarete** : Commun
- **XP Group** : Rapide

### #014 — AERORAPTOR
- **Types** : Vol
- **Stage** : Jeune (Ligne : Plumex)
- **Evolution** : De Plumex au niveau 15 / Evolue en Cieloptere au niveau 32
- **Base Stats** : PV 50 / Atk 60 / Def 35 / AtkSp 50 / DefSp 40 / Vit 80 (Total : 315)
- **Abilities** : Ailes Coupantes / Velocite (cachee)
- **Dino reel inspire** : Microraptor (adulte)
- **Design** : Raptor agile aux quatre ailes deployees, couvertes de plumes bleu metallique. Son corps est svelte et aerodynamique. Des griffes acerees ornent ses pattes arriere, et sa crete de plumes forme un casque naturel.
- **Dinodex Entry** : "Aeroraptor chasse en plein vol avec une precision chirurgicale. Il peut changer de direction instantanement grace a ses quatre ailes independantes."
- **Habitat** : Foret du Cretace / Falaises Venteuses
- **Rarete** : Commun
- **XP Group** : Rapide

### #015 — CIELOPTERE
- **Types** : Vol / Foudre
- **Stage** : Adulte (Ligne : Plumex)
- **Evolution** : De Aeroraptor au niveau 32
- **Base Stats** : PV 70 / Atk 85 / Def 50 / AtkSp 75 / DefSp 55 / Vit 105 (Total : 440)
- **Abilities** : Ailes Coupantes / Charge Statique (cachee)
- **Dino reel inspire** : Quetzalcoatlus
- **Design** : Pterosaure majestueux aux ailes immenses chargees d'electricite statique. Ses plumes sont bleu electrique avec des eclats dores. Des arcs electriques creptitent entre ses ailes pendant le vol. Son bec long et pointu brille comme un paratonnerre.
- **Dinodex Entry** : "Cieloptere vole si haut qu'il traverse les nuages d'orage. Il se nourrit de la foudre elle-meme, canalisant l'electricite dans ses plumes."
- **Habitat** : Falaises Venteuses / Pic Tonnerre
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 6 — TOXIDON (Route 2 — Poison)

### #016 — TOXIDON
- **Types** : Poison
- **Stage** : Bebe (Ligne : Toxidon)
- **Evolution** : Evolue en Venomex au niveau 17
- **Base Stats** : PV 40 / Atk 35 / Def 30 / AtkSp 50 / DefSp 40 / Vit 40 (Total : 235)
- **Abilities** : Venin / Sang Froid (cachee)
- **Dino reel inspire** : Sinornithosaurus
- **Design** : Petit theropode a plumes violettes et vertes toxiques. Des glandes a venin sont visibles sous ses yeux, formant des poches gonflees. Sa langue bifide depasse souvent de sa bouche. Ses griffes suintent d'un liquide violet.
- **Dinodex Entry** : "Toxidon est l'un des rares dinosaures venimeux. Une seule egratignure de ses griffes peut paralyser un adversaire pendant des heures."
- **Habitat** : Route 2 — Marais Toxiques
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #017 — VENOMEX
- **Types** : Poison
- **Stage** : Jeune (Ligne : Toxidon)
- **Evolution** : De Toxidon au niveau 17 / Evolue en Pandemonium au niveau 35
- **Base Stats** : PV 55 / Atk 50 / Def 45 / AtkSp 70 / DefSp 55 / Vit 50 (Total : 325)
- **Abilities** : Venin / Sang Froid (cachee)
- **Dino reel inspire** : Dilophosaurus
- **Design** : Theropode elance aux deux cretes craniales qui secretent un venin volatil violet. Sa peau est vert sombre avec des motifs d'avertissement jaunes et noirs. Une collerette de peau toxique peut se deployer autour de son cou.
- **Dinodex Entry** : "Venomex projette un brouillard toxique depuis ses cretes pour etourdir ses proies. Les chasseurs evitent les zones ou son odeur acre persiste."
- **Habitat** : Marais Toxiques / Jungle Nocturne
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #018 — PANDEMONIUM
- **Types** : Poison / Ombre
- **Stage** : Adulte (Ligne : Toxidon)
- **Evolution** : De Venomex au niveau 35
- **Base Stats** : PV 75 / Atk 65 / Def 60 / AtkSp 100 / DefSp 75 / Vit 65 (Total : 440)
- **Abilities** : Venin / Ombre Furtive (cachee)
- **Dino reel inspire** : Dilophosaurus (gigantesque)
- **Design** : Grand theropode sinistre dont le corps emane une brume toxique permanente. Ses cretes craniales se sont transformees en cornes tordues violet-noir. Ses yeux brillent d'une lueur violette malveillante. Des ombres semblent s'accrocher a lui comme des tentacules.
- **Dinodex Entry** : "Pandemonium se deplace dans un nuage de poison si dense qu'il obscurcit la lumiere. Les rares temoins de son passage ne se souviennent que d'un cauchemar violet."
- **Habitat** : Jungle Nocturne / Abime Corrompue
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 7 — FERNEX (Foret — Plante)

### #019 — FERNEX
- **Types** : Plante
- **Stage** : Bebe (Ligne : Fernex)
- **Evolution** : Evolue en Fougeraptor au niveau 18
- **Base Stats** : PV 45 / Atk 50 / Def 40 / AtkSp 35 / DefSp 35 / Vit 50 (Total : 255)
- **Abilities** : Vegetal / Griffe Aceree (cachee)
- **Dino reel inspire** : Velociraptor (avec fougeres)
- **Design** : Petit raptor agile dont le corps est couvert de fougeres miniatures. Des frondes vertes poussent le long de ses bras et de sa queue. Ses yeux verts sont vifs et intelligents. Ses griffes sont en bois durci.
- **Dinodex Entry** : "Fernex se camoufle si bien dans les fougeres qu'il est presque invisible. Seul le bruissement de ses pas trahit sa presence."
- **Habitat** : Foret du Cretace
- **Rarete** : Commun
- **XP Group** : Rapide

### #020 — FOUGERAPTOR
- **Types** : Plante
- **Stage** : Jeune (Ligne : Fernex)
- **Evolution** : De Fernex au niveau 18 / Evolue en Junglorex au niveau 34
- **Base Stats** : PV 60 / Atk 70 / Def 55 / AtkSp 50 / DefSp 45 / Vit 60 (Total : 340)
- **Abilities** : Vegetal / Griffe Aceree (cachee)
- **Dino reel inspire** : Deinonychus
- **Design** : Raptor de taille moyenne dont les bras sont couverts de longues frondes de fougeres arborescentes. Sa peau est verte avec des stries brunes. Ses griffes falciformes sont en bois petrifie, aussi dures que l'acier.
- **Dinodex Entry** : "Fougeraptor taille les arbres avec ses griffes pour marquer son territoire. Les forets qu'il habite sont etrangement bien entretenues."
- **Habitat** : Foret du Cretace / Vallee des Fougeres
- **Rarete** : Commun
- **XP Group** : Rapide

### #021 — JUNGLOREX
- **Types** : Plante / Poison
- **Stage** : Adulte (Ligne : Fernex)
- **Evolution** : De Fougeraptor au niveau 34
- **Base Stats** : PV 80 / Atk 95 / Def 70 / AtkSp 65 / DefSp 60 / Vit 60 (Total : 430)
- **Abilities** : Vegetal / Venin (cachee)
- **Dino reel inspire** : Utahraptor
- **Design** : Grand raptor feroce dont le corps est recouvert de vegetation dense et toxique. Des lianes empoisonnees pendent de ses bras comme des fouets. Ses griffes falciformes secretent une seve corrosive verte. Des fleurs carnivores poussent sur ses epaules.
- **Dinodex Entry** : "Junglorex est le predateur supreme de la jungle. Sa vegetation toxique paralyse quiconque s'approche trop pres, et ses griffes empoisonnees ne laissent aucune chance."
- **Habitat** : Vallee des Fougeres / Jungle Primordiale
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 8 — TERRAVORE (Foret — Terre)

### #022 — TERRAVORE
- **Types** : Terre
- **Stage** : Bebe (Ligne : Terravore)
- **Evolution** : Evolue en Seismops au niveau 20
- **Base Stats** : PV 50 / Atk 45 / Def 50 / AtkSp 30 / DefSp 40 / Vit 30 (Total : 245)
- **Abilities** : Peau Dure / Armure Naturelle (cachee)
- **Dino reel inspire** : Ankylosaurus (juvenile)
- **Design** : Petit ankylosaure trapu au corps couvert de plaques de terre seche. Sa queue possede une boule de terre compactee. Ses courtes pattes creusent constamment le sol. Ses yeux sont a moitie caches par des plaques protectrices.
- **Dinodex Entry** : "Terravore passe la majeure partie de sa journee a creuser des terriers. Il peut s'enterrer completement en quelques secondes."
- **Habitat** : Foret du Cretace / Plaines de Boue
- **Rarete** : Commun
- **XP Group** : Lent

### #023 — SEISMOPS
- **Types** : Terre
- **Stage** : Jeune (Ligne : Terravore)
- **Evolution** : De Terravore au niveau 20 / Evolue en Tectonyx au niveau 36
- **Base Stats** : PV 70 / Atk 65 / Def 75 / AtkSp 40 / DefSp 55 / Vit 30 (Total : 335)
- **Abilities** : Peau Dure / Armure Naturelle (cachee)
- **Dino reel inspire** : Ankylosaurus (sub-adulte)
- **Design** : Ankylosaure robuste dont les plaques dorsales sont devenues des dalles de terre compactee. Des fissures sismiques parcourent son corps. Sa queue est une masse de terre dense capable de provoquer des mini-seismes.
- **Dinodex Entry** : "Seismops communique avec les autres de son espece par des vibrations dans le sol. Il peut sentir un intrus a plus d'un kilometre."
- **Habitat** : Plaines de Boue / Gorge Tectonique
- **Rarete** : Commun
- **XP Group** : Lent

### #024 — TECTONYX
- **Types** : Terre / Roche
- **Stage** : Adulte (Ligne : Terravore)
- **Evolution** : De Seismops au niveau 36
- **Base Stats** : PV 100 / Atk 90 / Def 120 / AtkSp 45 / DefSp 70 / Vit 20 (Total : 445)
- **Abilities** : Peau Dure / Cuirasse (cachee)
- **Dino reel inspire** : Ankylosaurus (gigantesque)
- **Design** : Ankylosaure titanesque dont le corps est une veritable plaque tectonique mobile. Des strates de roche et de terre s'empilent sur son dos comme un paysage miniature. Sa queue est un maillet sismique capable de fissurer le sol. De petites plantes poussent dans les crevasses de son armure.
- **Dinodex Entry** : "Tectonyx est si lourd que chacun de ses pas fait trembler la terre. Les geologues etudient les failles qu'il laisse derriere lui pour comprendre la structure du sous-sol."
- **Habitat** : Gorge Tectonique / Mont Ardoise
- **Rarete** : Peu commun
- **XP Group** : Lent

---

## LIGNE 9 — ZEPHYROS (Foret — Vol)

### #025 — ZEPHYROS
- **Types** : Vol
- **Stage** : Bebe (Ligne : Zephyros)
- **Evolution** : Evolue en Galewing au niveau 19
- **Base Stats** : PV 35 / Atk 30 / Def 30 / AtkSp 45 / DefSp 40 / Vit 55 (Total : 235)
- **Abilities** : Ailes Coupantes / Regeneration (cachee)
- **Dino reel inspire** : Archaeopteryx (juvenile)
- **Design** : Petit dinosaure aviaire aux plumes multicolores comme un perroquet prehistorique. Ses ailes sont encore courtes et duvetees. Il a un petit bec dente et de grands yeux dores curieux.
- **Dinodex Entry** : "Zephyros est plus a l'aise a sautiller qu'a voler. Ses plumes colorees le rendent populaire aupres des eleveurs debutants."
- **Habitat** : Foret du Cretace / Canopee Ancienne
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #026 — GALEWING
- **Types** : Vol
- **Stage** : Jeune (Ligne : Zephyros)
- **Evolution** : De Zephyros au niveau 19 / Evolue en Tempestail au niveau 35
- **Base Stats** : PV 50 / Atk 45 / Def 45 / AtkSp 65 / DefSp 55 / Vit 70 (Total : 330)
- **Abilities** : Ailes Coupantes / Regeneration (cachee)
- **Dino reel inspire** : Archaeopteryx (adulte)
- **Design** : Oiseau prehistorique gracieux aux longues plumes vertes et bleues. Ses ailes sont devenues assez grandes pour un vol soutenu. Une queue de plumes en eventail lui sert de gouvernail aerien.
- **Dinodex Entry** : "Galewing danse dans les courants ascendants avec une grace hypnotisante. On dit que voir sa danse porte bonheur aux voyageurs."
- **Habitat** : Canopee Ancienne / Col des Vents
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #027 — TEMPESTAIL
- **Types** : Vol / Eau
- **Stage** : Adulte (Ligne : Zephyros)
- **Evolution** : De Galewing au niveau 35
- **Base Stats** : PV 70 / Atk 60 / Def 60 / AtkSp 90 / DefSp 75 / Vit 85 (Total : 440)
- **Abilities** : Ailes Coupantes / Nage Rapide (cachee)
- **Dino reel inspire** : Hesperornis + elements de tempete
- **Design** : Grand pterosaure-oiseau aux plumes bleu tempete et argent. Ses ailes immenses sont bordees de gouttelettes d'eau permanentes. Il peut voler a travers les tempetes sans effort. Sa queue longue et fluide ondule comme de l'eau dans le vent.
- **Dinodex Entry** : "Tempestail est le messager des tempetes. Les marins le redoutent, car son apparition annonce toujours un ouragan imminent."
- **Habitat** : Col des Vents / Cote des Tempetes
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 10 — IGNITOPS (Grottes — Feu)

### #028 — IGNITOPS
- **Types** : Feu
- **Stage** : Bebe (Ligne : Ignitops)
- **Evolution** : Evolue en Lavacorne au niveau 20
- **Base Stats** : PV 45 / Atk 50 / Def 45 / AtkSp 40 / DefSp 35 / Vit 35 (Total : 250)
- **Abilities** : Flamme Vive / Peau Dure (cachee)
- **Dino reel inspire** : Styracosaurus (juvenile)
- **Design** : Petit ceratopsien dont les cornes sont des flammes solidifiees orange. Son corps est rouge brique avec des fissures incandescentes. Sa collerette brille d'une chaleur douce. Ses yeux sont ambre chaud.
- **Dinodex Entry** : "Ignitops rechauffe les grottes froides avec sa collerette incandescente. Les mineurs le considerent comme un porte-bonheur souterrain."
- **Habitat** : Grottes Embrasees
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #029 — LAVACORNE
- **Types** : Feu
- **Stage** : Jeune (Ligne : Ignitops)
- **Evolution** : De Ignitops au niveau 20 / Evolue en Magmatops au niveau 36
- **Base Stats** : PV 65 / Atk 75 / Def 65 / AtkSp 55 / DefSp 50 / Vit 35 (Total : 345)
- **Abilities** : Flamme Vive / Peau Dure (cachee)
- **Dino reel inspire** : Styracosaurus
- **Design** : Ceratopsien robuste dont les nombreuses cornes sont devenues des coulees de lave figee. Sa collerette est un bouclier de magma durci parseme de braises. Sa peau craquele revele de la roche en fusion en dessous.
- **Dinodex Entry** : "Lavacorne charge avec la force d'une coulee de lave. Ses cornes de magma percent tout sur leur passage et cauterisent les blessures instantanement."
- **Habitat** : Grottes Embrasees / Caldeira Ardente
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #030 — MAGMATOPS
- **Types** : Feu / Roche
- **Stage** : Adulte (Ligne : Ignitops)
- **Evolution** : De Lavacorne au niveau 36
- **Base Stats** : PV 90 / Atk 105 / Def 90 / AtkSp 65 / DefSp 60 / Vit 30 (Total : 440)
- **Abilities** : Flamme Vive / Cuirasse (cachee)
- **Dino reel inspire** : Pachyrhinosaurus + volcan
- **Design** : Ceratopsien massif dont le corps entier est une forge volcanique. Sa collerette est un cratere miniature d'ou jaillissent des flammes. Ses cornes sont des colonnes de basalte en fusion. Des rivieres de lave coulent le long de ses flancs, et son souffle cree des ondulations de chaleur.
- **Dinodex Entry** : "Magmatops vit au coeur des volcans actifs. On raconte que sa colere est si puissante qu'elle peut reveiller un volcan endormi."
- **Habitat** : Caldeira Ardente / Cratere du Jurassique
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 11 — STALAGMOR (Grottes — Roche)

### #031 — STALAGMOR
- **Types** : Roche
- **Stage** : Bebe (Ligne : Stalagmor)
- **Evolution** : Evolue en Calcitaure au niveau 22
- **Base Stats** : PV 50 / Atk 35 / Def 60 / AtkSp 35 / DefSp 45 / Vit 20 (Total : 245)
- **Abilities** : Carapace / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Nodosaurus (juvenile)
- **Design** : Petit dinosaure cuirasse dont le dos est herisse de stalagmites naturelles. Son corps est gris perle avec des reflets cristallins. Il se deplace lentement, laissant des traces de calcaire derriere lui.
- **Dinodex Entry** : "Stalagmor grandit si lentement que les stalactites ont le temps de se former sur son dos. Certains specimens portent des cristaux vieux de mille ans."
- **Habitat** : Grottes de Gneiss / Caverne de Cristal
- **Rarete** : Peu commun
- **XP Group** : Lent

### #032 — CALCITAURE
- **Types** : Roche
- **Stage** : Jeune (Ligne : Stalagmor)
- **Evolution** : De Stalagmor au niveau 22 / Evolue en Geodonte au niveau 38
- **Base Stats** : PV 70 / Atk 50 / Def 85 / AtkSp 50 / DefSp 60 / Vit 20 (Total : 335)
- **Abilities** : Carapace / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Nodosaurus
- **Design** : Dinosaure cuirasse de taille moyenne dont les stalagmites dorsales se sont fusionnees en colonnes de calcite. Des cristaux translucides poussent entre ses plaques d'armure. Son regard est stoique et imperturbable.
- **Dinodex Entry** : "Calcitaure peut rester immobile pendant des semaines entieres. Les explorateurs imprudents le confondent avec une formation rocheuse naturelle."
- **Habitat** : Caverne de Cristal
- **Rarete** : Peu commun
- **XP Group** : Lent

### #033 — GEODONTE
- **Types** : Roche / Lumiere
- **Stage** : Adulte (Ligne : Stalagmor)
- **Evolution** : De Calcitaure au niveau 38
- **Base Stats** : PV 95 / Atk 60 / Def 115 / AtkSp 70 / DefSp 85 / Vit 20 (Total : 445)
- **Abilities** : Carapace / Aura Lumineuse (cachee)
- **Dino reel inspire** : Sauropelta geante
- **Design** : Dinosaure forteresse dont le dos est un paysage de geodes cristallines eclatantes. Chaque cristal emet une lumiere douce et multicolore. Son corps est un amalgame de mineraux precieux. Quand il se defend, les cristaux brillent d'une lumiere aveuglante.
- **Dinodex Entry** : "Geodonte est une merveille geologique vivante. Les cristaux sur son dos refractent la lumiere en arcs-en-ciel souterrains qui illuminent les grottes les plus sombres."
- **Habitat** : Caverne de Cristal / Sanctuaire Souterrain
- **Rarete** : Rare
- **XP Group** : Lent

---

## LIGNE 12 — FORGERON (Grottes — Acier)

### #034 — FORGERON
- **Types** : Acier
- **Stage** : Bebe (Ligne : Forgeron)
- **Evolution** : Evolue en Blindorex au niveau 22
- **Base Stats** : PV 40 / Atk 50 / Def 55 / AtkSp 30 / DefSp 40 / Vit 35 (Total : 250)
- **Abilities** : Cuirasse / Intimidation (cachee)
- **Dino reel inspire** : Stegosaurus (juvenile)
- **Design** : Petit stegosaure dont les plaques dorsales sont en metal brut gris-argent. Sa peau a un eclat metallique. Ses pattes sont lourdes et clignotent comme du metal fraichement forge.
- **Dinodex Entry** : "Forgeron aiguise ses plaques metalliques en les frottant contre les rochers. Le bruit qu'il produit rappelle le martellement d'un forgeron."
- **Habitat** : Grottes de Gneiss / Mine Abandonnee
- **Rarete** : Rare
- **XP Group** : Lent

### #035 — BLINDOREX
- **Types** : Acier
- **Stage** : Jeune (Ligne : Forgeron)
- **Evolution** : De Forgeron au niveau 22 / Evolue en Titanacier au niveau 38
- **Base Stats** : PV 60 / Atk 75 / Def 80 / AtkSp 40 / DefSp 55 / Vit 30 (Total : 340)
- **Abilities** : Cuirasse / Intimidation (cachee)
- **Dino reel inspire** : Stegosaurus
- **Design** : Stegosaure robuste dont les plaques dorsales sont devenues des lames d'acier poli. Son corps est recouvert d'un blindage metallique naturel gris-chrome. Sa queue porte des pointes d'acier trempe.
- **Dinodex Entry** : "Blindorex est si bien blinde que meme les predateurs les plus feroces renoncent a l'attaquer. Ses plaques d'acier deflectent presque tous les coups."
- **Habitat** : Mine Abandonnee / Forge Naturelle
- **Rarete** : Rare
- **XP Group** : Lent

### #036 — TITANACIER
- **Types** : Acier / Feu
- **Stage** : Adulte (Ligne : Forgeron)
- **Evolution** : De Blindorex au niveau 38
- **Base Stats** : PV 85 / Atk 100 / Def 110 / AtkSp 55 / DefSp 70 / Vit 25 (Total : 445)
- **Abilities** : Cuirasse / Peau Dure (cachee)
- **Dino reel inspire** : Stegosaurus titanesque
- **Design** : Stegosaure colossal dont le corps est une forge vivante. Ses plaques dorsales sont des enclumes de metal en fusion, rougeoyantes de chaleur. De la fumee s'echappe entre ses plaques. Sa queue herissee de pointes d'acier ardent peut trancher le metal. Son regard est dur et determine.
- **Dinodex Entry** : "Titanacier forge son propre blindage en absorbant le metal de son environnement. On dit que son corps est plus dur que n'importe quel alliage connu."
- **Habitat** : Forge Naturelle / Coeur de la Montagne
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 13 — ONDIVORE (Zones intermediaires — Eau)

### #037 — ONDIVORE
- **Types** : Eau
- **Stage** : Bebe (Ligne : Ondivore)
- **Evolution** : Evolue en Courantin au niveau 18
- **Base Stats** : PV 45 / Atk 35 / Def 35 / AtkSp 50 / DefSp 40 / Vit 45 (Total : 250)
- **Abilities** : Torrent / Nage Rapide (cachee)
- **Dino reel inspire** : Ichthyosaurus (juvenile)
- **Design** : Petit ichthyosaure au corps en forme de dauphin, avec des nageoires bleues translucides. Ses grands yeux ronds occupent une grande partie de son crane. Sa peau est bleu clair avec des reflets argentes.
- **Dinodex Entry** : "Ondivore nage en cercles pour creer de petits tourbillons dans lesquels il piege ses proies. C'est un spectacle fascinant a observer."
- **Habitat** : Lac du Cretace / Riviere Azur
- **Rarete** : Commun
- **XP Group** : Rapide

### #038 — COURANTIN
- **Types** : Eau
- **Stage** : Jeune (Ligne : Ondivore)
- **Evolution** : De Ondivore au niveau 18 / Evolue en Maelstrom au niveau 34
- **Base Stats** : PV 60 / Atk 50 / Def 50 / AtkSp 70 / DefSp 55 / Vit 55 (Total : 340)
- **Abilities** : Torrent / Nage Rapide (cachee)
- **Dino reel inspire** : Ichthyosaurus
- **Design** : Ichthyosaure elance au corps hydrodynamique bleu marine. Des spirales d'eau tourbillonnent en permanence autour de ses nageoires. Son museau long et pointu fend l'eau sans effort.
- **Dinodex Entry** : "Courantin peut nager a contre-courant meme dans les rapides les plus violents. Sa vitesse dans l'eau rivalise avec celle d'un jet."
- **Habitat** : Riviere Azur / Cascades d'Opale
- **Rarete** : Commun
- **XP Group** : Rapide

### #039 — MAELSTROM
- **Types** : Eau / Vol
- **Stage** : Adulte (Ligne : Ondivore)
- **Evolution** : De Courantin au niveau 34
- **Base Stats** : PV 80 / Atk 65 / Def 65 / AtkSp 90 / DefSp 70 / Vit 75 (Total : 445)
- **Abilities** : Torrent / Ailes Coupantes (cachee)
- **Dino reel inspire** : Ichthyosaure + poisson volant
- **Design** : Creature marine spectaculaire capable de jaillir de l'eau et de planer sur de longues distances. Ses nageoires se sont transformees en ailes translucides bleu-argent. Un tourbillon d'eau perpetuel entoure sa queue. Ses ecailles scintillent comme la surface de l'ocean.
- **Dinodex Entry** : "Maelstrom est aussi a l'aise dans les airs que dans l'eau. Il cree des trombes marines en spiralant a grande vitesse entre ciel et mer."
- **Habitat** : Cascades d'Opale / Ocean Primordial
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 14 — FULGURE (Zones intermediaires — Foudre)

### #040 — FULGURE
- **Types** : Foudre
- **Stage** : Bebe (Ligne : Fulgure)
- **Evolution** : Evolue en Voltacroc au niveau 20
- **Base Stats** : PV 35 / Atk 40 / Def 30 / AtkSp 55 / DefSp 35 / Vit 55 (Total : 250)
- **Abilities** : Charge Statique / Velocite (cachee)
- **Dino reel inspire** : Oviraptor (juvenile)
- **Design** : Petit theropode nerveux dont les plumes sont chargees d'electricite statique jaune vif. Des etincelles jaillissent de sa crete quand il est excite. Ses yeux sont jaune electrique. Il trepigne constamment sur place.
- **Dinodex Entry** : "Fulgure est toujours en mouvement. Il accumule tant d'electricite statique que tout ce qu'il touche recoit une decharge."
- **Habitat** : Plaines Orageuses
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #041 — VOLTACROC
- **Types** : Foudre
- **Stage** : Jeune (Ligne : Fulgure)
- **Evolution** : De Fulgure au niveau 20 / Evolue en Oragriffe au niveau 36
- **Base Stats** : PV 50 / Atk 60 / Def 40 / AtkSp 75 / DefSp 50 / Vit 70 (Total : 345)
- **Abilities** : Charge Statique / Velocite (cachee)
- **Dino reel inspire** : Oviraptor
- **Design** : Theropode agile au corps couvert de plumes herissees par l'electricite. Des arcs electriques relient ses griffes entre elles. Sa crete est devenue un paratonnerre naturel qui creptite en permanence.
- **Dinodex Entry** : "Voltacroc attire la foudre pendant les orages pour recharger son corps. Les bergers l'evitent car il provoque des courts-circuits dans les clotures."
- **Habitat** : Plaines Orageuses / Pic Tonnerre
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #042 — ORAGRIFFE
- **Types** : Foudre / Acier
- **Stage** : Adulte (Ligne : Fulgure)
- **Evolution** : De Voltacroc au niveau 36
- **Base Stats** : PV 70 / Atk 85 / Def 60 / AtkSp 100 / DefSp 65 / Vit 85 (Total : 465)
- **Abilities** : Charge Statique / Griffe Aceree (cachee)
- **Dino reel inspire** : Therizinosaurus electrique
- **Design** : Grand theropode aux immenses griffes d'acier conducteur qui servent de paratonnerres. Son corps est couvert de plumes electriques bleu-jaune. Des eclairs permanents parcourent ses bras. Son regard est intense et calculateur, et l'air autour de lui est charge d'ozone.
- **Dinodex Entry** : "Oragriffe canalise la puissance de la foudre dans ses griffes d'acier. Un seul coup de ses serres peut alimenter une ville entiere pendant une journee."
- **Habitat** : Pic Tonnerre / Sommet de l'Orage
- **Rarete** : Rare
- **XP Group** : Rapide

---

## LIGNE 15 — GIVREX (Zones intermediaires — Glace)

### #043 — GIVREX
- **Types** : Glace
- **Stage** : Bebe (Ligne : Givrex)
- **Evolution** : Evolue en Cryodonte au niveau 20
- **Base Stats** : PV 45 / Atk 40 / Def 40 / AtkSp 50 / DefSp 45 / Vit 35 (Total : 255)
- **Abilities** : Gel Eternel / Sang Froid (cachee)
- **Dino reel inspire** : Cryolophosaurus (juvenile)
- **Design** : Petit theropode au corps bleu glace avec une crete de cristaux de givre. De petits flocons de neige tombent autour de lui en permanence. Ses yeux sont bleu pale, presque blancs. Son souffle est visible meme par temps chaud.
- **Dinodex Entry** : "Givrex laisse des empreintes de givre partout ou il marche. Les enfants adorent suivre sa trace pour decouvrir des motifs de glace uniques."
- **Habitat** : Col des Vents / Vallee Givre
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #044 — CRYODONTE
- **Types** : Glace
- **Stage** : Jeune (Ligne : Givrex)
- **Evolution** : De Givrex au niveau 20 / Evolue en Glaciadon au niveau 36
- **Base Stats** : PV 60 / Atk 55 / Def 55 / AtkSp 70 / DefSp 65 / Vit 40 (Total : 345)
- **Abilities** : Gel Eternel / Sang Froid (cachee)
- **Dino reel inspire** : Cryolophosaurus
- **Design** : Theropode elegant dont la crete s'est transformee en couronne de glace. Son corps est bleu fonce avec des veines de givre blanc. Des cristaux de glace flottent en orbite autour de ses pattes. Ses dents sont en glace incassable.
- **Dinodex Entry** : "Cryodonte peut geler l'eau instantanement avec son souffle. Les lacs qu'il traverse restent geles pendant des jours apres son passage."
- **Habitat** : Vallee Givree / Glacier Ancien
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #045 — GLACIADON
- **Types** : Glace / Terre
- **Stage** : Adulte (Ligne : Givrex)
- **Evolution** : De Cryodonte au niveau 36
- **Base Stats** : PV 85 / Atk 70 / Def 75 / AtkSp 100 / DefSp 80 / Vit 35 (Total : 445)
- **Abilities** : Gel Eternel / Tempete de Sable (cachee)
- **Dino reel inspire** : Cryolophosaurus geant + elements glaciaires
- **Design** : Grand theropode majestueux dont le corps est un paysage arctique vivant. Sa crete est devenue un glacier miniature. Son armure de glace est parsemee de terre gelee et de permafrost. Des avalanches miniatures glissent le long de ses flancs quand il bouge.
- **Dinodex Entry** : "Glaciadon est le souverain des terres gelees. Sa presence transforme le paysage en toundra arctique. Les glaciers se forment dans ses pas."
- **Habitat** : Glacier Ancien / Toundra Eternelle
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 16 — DUNEX (Zones intermediaires — Sable)

### #046 — DUNEX
- **Types** : Sable
- **Stage** : Bebe (Ligne : Dunex)
- **Evolution** : Evolue en Sirocco au niveau 18
- **Base Stats** : PV 40 / Atk 45 / Def 40 / AtkSp 35 / DefSp 35 / Vit 50 (Total : 245)
- **Abilities** : Tempete de Sable / Velocite (cachee)
- **Dino reel inspire** : Gallimimus (juvenile)
- **Design** : Petit ornithomimide au corps beige-dore couvert de fines particules de sable. Il est extremement rapide et laisse une trainee de sable derriere lui. Ses yeux sont ambre avec des pupilles horizontales.
- **Dinodex Entry** : "Dunex court si vite sur le sable qu'il ne s'enfonce jamais. Il glisse sur les dunes comme un surfeur sur les vagues."
- **Habitat** : Desert du Permien / Dunes d'Ambre
- **Rarete** : Commun
- **XP Group** : Rapide

### #047 — SIROCCO
- **Types** : Sable
- **Stage** : Jeune (Ligne : Dunex)
- **Evolution** : De Dunex au niveau 18 / Evolue en Tempestable au niveau 34
- **Base Stats** : PV 55 / Atk 60 / Def 55 / AtkSp 50 / DefSp 50 / Vit 70 (Total : 340)
- **Abilities** : Tempete de Sable / Velocite (cachee)
- **Dino reel inspire** : Gallimimus
- **Design** : Ornithomimide svelte au corps dore avec des motifs tourbillonnants de sable. Un voile de sable permanent flotte autour de lui. Ses longues pattes sont adaptees a la course sur terrain sableux. Sa queue est un panache de sable.
- **Dinodex Entry** : "Sirocco peut courir a la vitesse du vent du desert. Il souleve des tempetes de sable dans son sillage qui desorientent ses predateurs."
- **Habitat** : Dunes d'Ambre / Canyon Ecarlate
- **Rarete** : Commun
- **XP Group** : Rapide

### #048 — TEMPESTABLE
- **Types** : Sable / Vol
- **Stage** : Adulte (Ligne : Dunex)
- **Evolution** : De Sirocco au niveau 34
- **Base Stats** : PV 75 / Atk 80 / Def 65 / AtkSp 65 / DefSp 60 / Vit 95 (Total : 440)
- **Abilities** : Tempete de Sable / Ailes Coupantes (cachee)
- **Dino reel inspire** : Struthiomimus + elements de tempete de sable
- **Design** : Ornithomimide majestueux aux plumes dorees et cuivrees. Des ailes de sable solidifie lui permettent de planer au-dessus du desert. Il est entoure d'un tourbillon permanent de sable qui forme des motifs hypnotiques. Son regard percant surveille l'horizon.
- **Dinodex Entry** : "Tempestable commande les vents du desert. Il peut declencher des tempetes de sable demesurees d'un battement d'ailes, ensevelissant des villes entieres."
- **Habitat** : Canyon Ecarlate / Vortex du Desert
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 17 — BOURBEX (Mi-jeu — Terre)

### #049 — BOURBEX
- **Types** : Terre
- **Stage** : Bebe (Ligne : Bourbex)
- **Evolution** : Evolue en Marechal au niveau 22
- **Base Stats** : PV 50 / Atk 40 / Def 45 / AtkSp 40 / DefSp 45 / Vit 30 (Total : 250)
- **Abilities** : Peau Dure / Regeneration (cachee)
- **Dino reel inspire** : Maiasaura (juvenile)
- **Design** : Petit hadrosaure au corps couvert de boue sechee. Des traces de pattes decorent naturellement sa peau brune. Ses yeux sont doux et maternels. Sa queue courte bat joyeusement dans la boue.
- **Dinodex Entry** : "Bourbex adore se rouler dans la boue. Cette couche protectrice le garde au frais en ete et au chaud en hiver."
- **Habitat** : Plaines de Boue / Marais du Jurassique
- **Rarete** : Commun
- **XP Group** : Moyen

### #050 — MARECHAL
- **Types** : Terre / Eau
- **Stage** : Jeune (Ligne : Bourbex)
- **Evolution** : De Bourbex au niveau 22 / Evolue en Diluvion au niveau 36
- **Base Stats** : PV 70 / Atk 55 / Def 65 / AtkSp 55 / DefSp 60 / Vit 35 (Total : 340)
- **Abilities** : Peau Dure / Regeneration (cachee)
- **Dino reel inspire** : Maiasaura
- **Design** : Hadrosaure robuste dont le corps est un melange de terre humide et d'eau boueuse. Des flaques se forment autour de ses pattes. Sa crete plate sert de reservoir d'eau boueuse. Des plantes aquatiques poussent dans les plis de sa peau.
- **Dinodex Entry** : "Marechal est un dinosaure pacifique qui protege les zones humides. Il cree des mares en pietinant le sol, offrant un habitat a de nombreuses especes."
- **Habitat** : Marais du Jurassique / Delta Boueux
- **Rarete** : Commun
- **XP Group** : Moyen

### #051 — DILUVION
- **Types** : Terre / Eau
- **Stage** : Adulte (Ligne : Bourbex)
- **Evolution** : De Marechal au niveau 36
- **Base Stats** : PV 100 / Atk 70 / Def 85 / AtkSp 75 / DefSp 80 / Vit 30 (Total : 440)
- **Abilities** : Peau Dure / Nage Rapide (cachee)
- **Dino reel inspire** : Parasaurolophus geant
- **Design** : Hadrosaure imposant dont le corps est un marecage vivant. De l'eau boueuse ruisselle constamment le long de ses flancs. Sa crete allongee projette des jets de boue. Des nenuphars flottent dans les flaques sur son dos. Son cri resonne comme le grondement d'un fleuve en crue.
- **Dinodex Entry** : "Diluvion peut provoquer des inondations controlees pour proteger son territoire. Les zones marecageuses qu'il habite sont les plus fertiles du monde."
- **Habitat** : Delta Boueux / Grand Marecage
- **Rarete** : Peu commun
- **XP Group** : Moyen

---

## LIGNE 18 — NOCTUREX (Mi-jeu — Poison/Ombre)

### #052 — NOCTUREX
- **Types** : Poison
- **Stage** : Bebe (Ligne : Nocturex)
- **Evolution** : Evolue en Spectrovore au niveau 24
- **Base Stats** : PV 40 / Atk 35 / Def 35 / AtkSp 55 / DefSp 45 / Vit 45 (Total : 255)
- **Abilities** : Venin / Ombre Furtive (cachee)
- **Dino reel inspire** : Troodon (juvenile)
- **Design** : Petit theropode nocturne aux grands yeux violets luminescents. Sa peau est sombre avec des taches bioluminescentes violettes. Ses griffes fines secretent un venin subtil. Il est presque invisible dans l'obscurite.
- **Dinodex Entry** : "Nocturex ne sort que la nuit. Ses yeux brillants dans le noir sont souvent confondus avec des feux follets par les voyageurs egares."
- **Habitat** : Jungle Nocturne / Foret Obscure
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #053 — SPECTROVORE
- **Types** : Poison / Ombre
- **Stage** : Jeune (Ligne : Nocturex)
- **Evolution** : De Nocturex au niveau 24 / Evolue en Abyssinthe au niveau 38
- **Base Stats** : PV 55 / Atk 50 / Def 45 / AtkSp 75 / DefSp 60 / Vit 55 (Total : 340)
- **Abilities** : Venin / Ombre Furtive (cachee)
- **Dino reel inspire** : Troodon
- **Design** : Theropode furtif dont le corps semble semi-transparent dans la penombre. Des volutes de brume toxique emanent de sa peau sombre. Ses yeux violet vif percent les tenebres. Sa silhouette se dissout dans les ombres.
- **Dinodex Entry** : "Spectrovore se fond dans les ombres comme s'il en faisait partie. Ses victimes ne realisent sa presence que lorsque le venin commence a faire effet."
- **Habitat** : Foret Obscure / Abime Corrompue
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #054 — ABYSSINTHE
- **Types** : Poison / Ombre
- **Stage** : Adulte (Ligne : Nocturex)
- **Evolution** : De Spectrovore au niveau 38
- **Base Stats** : PV 75 / Atk 60 / Def 55 / AtkSp 105 / DefSp 80 / Vit 70 (Total : 445)
- **Abilities** : Venin / Instinct Predateur (cachee)
- **Dino reel inspire** : Troodon geant fantasmatique
- **Design** : Grand theropode spectral dont le corps est une silhouette d'ombre pure. Des yeux violet incandescent brillent dans une face sombre et indefinie. Un halo de brume toxique violet sombre l'entoure en permanence. Ses griffes sont des lames d'ombre solidifiee suintant de poison.
- **Dinodex Entry** : "Abyssinthe est le cauchemar de l'echequier nocturne. Meme les plus braves dresseurs hesitent a l'affronter, car son venin corrompt autant le corps que l'esprit."
- **Habitat** : Abime Corrompue / Neant Obscur
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 19 — PTERACIER (Mi-jeu — Vol/Acier)

### #055 — PTERACIER
- **Types** : Vol
- **Stage** : Bebe (Ligne : Pteracier)
- **Evolution** : Evolue en Aeronyx au niveau 24
- **Base Stats** : PV 40 / Atk 45 / Def 45 / AtkSp 40 / DefSp 35 / Vit 50 (Total : 255)
- **Abilities** : Ailes Coupantes / Cuirasse (cachee)
- **Dino reel inspire** : Pteranodon (juvenile)
- **Design** : Petit pterosaure aux ailes membraneuses grises avec un eclat metallique. Sa crete est courte et pointue comme une lame. Ses yeux sont vifs et percants, d'un gris acier.
- **Dinodex Entry** : "Pteracier s'entraine a voler en se jetant des falaises. Sa crete metallique lui sert de gouvernail dans les vents violents."
- **Habitat** : Falaises Venteuses / Mine Abandonnee
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #056 — AERONYX
- **Types** : Vol / Acier
- **Stage** : Jeune (Ligne : Pteracier)
- **Evolution** : De Pteracier au niveau 24 / Evolue en Chromaile au niveau 38
- **Base Stats** : PV 55 / Atk 65 / Def 65 / AtkSp 55 / DefSp 50 / Vit 60 (Total : 350)
- **Abilities** : Ailes Coupantes / Cuirasse (cachee)
- **Dino reel inspire** : Pteranodon
- **Design** : Pterosaure aux ailes renforcees de plaques metalliques. Son corps est gris chrome avec des reflets argentes. Sa crete est une lame d'acier poli. Il vole avec une precision mecanique.
- **Dinodex Entry** : "Aeronyx peut voler a travers les tempetes de grele sans une egratignure. Son blindage aerien en fait le chasseur le plus redoute du ciel."
- **Habitat** : Mine Abandonnee / Forge Naturelle / Col des Vents
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #057 — CHROMAILE
- **Types** : Vol / Acier
- **Stage** : Adulte (Ligne : Pteracier)
- **Evolution** : De Aeronyx au niveau 38
- **Base Stats** : PV 75 / Atk 90 / Def 85 / AtkSp 70 / DefSp 65 / Vit 70 (Total : 455)
- **Abilities** : Ailes Coupantes / Intimidation (cachee)
- **Dino reel inspire** : Quetzalcoatlus blinde
- **Design** : Pterosaure immense au corps entierement blinde de chrome poli. Ses ailes sont des boucliers volants aux bords tranchants. Sa crete est un sabre metallique etincelant. Son envergure projette une ombre massive et intimidante sur le paysage.
- **Dinodex Entry** : "Chromaile est un chasseur aerien inegalable. Ses ailes d'acier chrome coupent l'air avec un sifflement terrifiant. Il fond sur ses proies comme un missile vivant."
- **Habitat** : Forge Naturelle / Ciel de Chrome
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 20 — OMBRIX (Mi-jeu — Ombre)

### #058 — OMBRIX
- **Types** : Ombre
- **Stage** : Bebe (Ligne : Ombrix)
- **Evolution** : Evolue en Tenebrix au niveau 22
- **Base Stats** : PV 40 / Atk 50 / Def 30 / AtkSp 45 / DefSp 35 / Vit 55 (Total : 255)
- **Abilities** : Ombre Furtive / Instinct Predateur (cachee)
- **Dino reel inspire** : Bambiraptor (juvenile)
- **Design** : Petit raptor aux ecailles noires qui absorbent la lumiere. Ses yeux sont rouges comme des rubis dans l'obscurite. Son corps semble projeter plus d'ombre qu'il ne devrait. Sa queue est une trainee de tenebres.
- **Dinodex Entry** : "Ombrix ne projette pas d'ombre — il EST l'ombre. Les dresseurs doivent souvent verifier qu'il n'a pas disparu dans les coins sombres."
- **Habitat** : Foret Obscure / Grotte d'Ombre
- **Rarete** : Rare
- **XP Group** : Moyen

### #059 — TENEBRIX
- **Types** : Ombre
- **Stage** : Jeune (Ligne : Ombrix)
- **Evolution** : De Ombrix au niveau 22 / Evolue en Nocturnyx au niveau 36
- **Base Stats** : PV 55 / Atk 70 / Def 40 / AtkSp 60 / DefSp 50 / Vit 70 (Total : 345)
- **Abilities** : Ombre Furtive / Instinct Predateur (cachee)
- **Dino reel inspire** : Bambiraptor
- **Design** : Raptor agile dont le corps est un patchwork de tenebres et de lueurs rouges. Il peut se fondre partiellement dans les ombres. Ses griffes sont noires et presque invisibles. Ses mouvements sont silencieux comme la nuit.
- **Dinodex Entry** : "Tenebrix chasse dans l'obscurite totale avec une efficacite effrayante. Ses proies ne voient que deux yeux rouges avant qu'il ne soit trop tard."
- **Habitat** : Grotte d'Ombre / Neant Obscur
- **Rarete** : Rare
- **XP Group** : Moyen

### #060 — NOCTURNYX
- **Types** : Ombre / Vol
- **Stage** : Adulte (Ligne : Ombrix)
- **Evolution** : De Tenebrix au niveau 36
- **Base Stats** : PV 75 / Atk 95 / Def 55 / AtkSp 80 / DefSp 60 / Vit 90 (Total : 455)
- **Abilities** : Ombre Furtive / Velocite (cachee)
- **Dino reel inspire** : Balaur + creature nocturne ailee
- **Design** : Predateur aile spectral dont le corps est une masse d'ombre vivante. Ses ailes sont des voiles de tenebres qui absorbent toute lumiere. Ses yeux rouges incandescents sont les seules sources de lumiere visible. Il vole sans un bruit, laissant un sillage d'obscurite.
- **Dinodex Entry** : "Nocturnyx est le predateur ultime de la nuit. Son vol silencieux et ses ailes de tenebres le rendent invisible meme aux sens les plus aiguises."
- **Habitat** : Neant Obscur / Sommet des Ombres
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 21 — SOLAUREX (Zones avancees — Lumiere)

### #061 — SOLAUREX
- **Types** : Lumiere
- **Stage** : Bebe (Ligne : Solaurex)
- **Evolution** : Evolue en Heliodonte au niveau 24
- **Base Stats** : PV 45 / Atk 35 / Def 40 / AtkSp 55 / DefSp 50 / Vit 40 (Total : 265)
- **Abilities** : Aura Lumineuse / Regeneration (cachee)
- **Dino reel inspire** : Psittacosaurus (juvenile)
- **Design** : Petit ceratopsien au corps dore qui emet une douce lueur solaire. Des motifs de rayons de soleil ornent sa collerette. Ses ecailles sont chaudes au toucher et scintillent. Ses yeux sont d'un or pur.
- **Dinodex Entry** : "Solaurex brille d'une lumiere douce qui reconforte tous ceux qui l'entourent. Les voyageurs le cherchent dans les grottes obscures pour s'orienter."
- **Habitat** : Prairie Lumineuse / Sanctuaire Solaire
- **Rarete** : Rare
- **XP Group** : Moyen

### #062 — HELIODONTE
- **Types** : Lumiere
- **Stage** : Jeune (Ligne : Solaurex)
- **Evolution** : De Solaurex au niveau 24 / Evolue en Radianox au niveau 38
- **Base Stats** : PV 60 / Atk 50 / Def 55 / AtkSp 75 / DefSp 70 / Vit 45 (Total : 355)
- **Abilities** : Aura Lumineuse / Regeneration (cachee)
- **Dino reel inspire** : Psittacosaurus
- **Design** : Ceratopsien elegant dont la collerette est un prisme vivant qui decompose la lumiere en arcs-en-ciel. Son corps dore est orne de gemmes lumineuses naturelles. Un halo de lumiere douce l'entoure en permanence.
- **Dinodex Entry** : "Heliodonte transforme la lumiere du soleil en energie pure. Les plantes poussent deux fois plus vite dans son aura lumineuse."
- **Habitat** : Sanctuaire Solaire / Temple de l'Aube
- **Rarete** : Rare
- **XP Group** : Moyen

### #063 — RADIANOX
- **Types** : Lumiere / Feu
- **Stage** : Adulte (Ligne : Solaurex)
- **Evolution** : De Heliodonte au niveau 38
- **Base Stats** : PV 80 / Atk 65 / Def 70 / AtkSp 105 / DefSp 90 / Vit 50 (Total : 460)
- **Abilities** : Aura Lumineuse / Flamme Vive (cachee)
- **Dino reel inspire** : Psittacosaurus geant + soleil
- **Design** : Ceratopsien majestueux dont le corps entier irradie de lumiere solaire. Sa collerette est une couronne de flammes blanches. Des rayons de lumiere jaillissent entre ses plaques d'armure dorees. Il brille comme un petit soleil, et son ombre n'existe pas.
- **Dinodex Entry** : "Radianox est venere comme l'incarnation du soleil. Sa lumiere peut guerir les blessures mineures et purifier les zones corrompues par l'ombre."
- **Habitat** : Temple de l'Aube / Zenith Dore
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 22 — RELICOR (Zones avancees — Fossile)

### #064 — RELICOR
- **Types** : Fossile
- **Stage** : Bebe (Ligne : Relicor)
- **Evolution** : Evolue en Archeon au niveau 24
- **Base Stats** : PV 45 / Atk 40 / Def 50 / AtkSp 45 / DefSp 45 / Vit 30 (Total : 255)
- **Abilities** : Fossile Ancien / Carapace (cachee)
- **Dino reel inspire** : Anomalocaris (juvenile)
- **Design** : Petite creature primitive ressemblant a un arthropode fossilise. Son corps est brun-ambre avec des motifs d'empreintes fossiles. Des fragments de roche sedimentaire sont incrustes dans sa carapace. Ses yeux sont comme des pierres polies.
- **Dinodex Entry** : "Relicor est un vestige vivant d'une ere reculee. Son corps contient des fossiles d'especes disparues depuis des millions d'annees."
- **Habitat** : Site de Fouilles / Strates Anciennes
- **Rarete** : Rare
- **XP Group** : Lent

### #065 — ARCHEON
- **Types** : Fossile
- **Stage** : Jeune (Ligne : Relicor)
- **Evolution** : De Relicor au niveau 24 / Evolue en Primordax au niveau 38
- **Base Stats** : PV 65 / Atk 55 / Def 70 / AtkSp 60 / DefSp 65 / Vit 35 (Total : 350)
- **Abilities** : Fossile Ancien / Carapace (cachee)
- **Dino reel inspire** : Anomalocaris
- **Design** : Creature primitive de taille moyenne dont la carapace est une mosaique de fossiles de differentes epoques. Des ammonites et trilobites fossilises sont visibles dans son armure. Son corps porte les marques de millions d'annees d'evolution.
- **Dinodex Entry** : "Archeon porte l'histoire de la vie elle-meme sur sa carapace. Les paleontologues reve de l'etudier, mais il est incroyablement difficile a trouver."
- **Habitat** : Strates Anciennes / Falaise du Cambrien
- **Rarete** : Rare
- **XP Group** : Lent

### #066 — PRIMORDAX
- **Types** : Fossile / Terre
- **Stage** : Adulte (Ligne : Relicor)
- **Evolution** : De Archeon au niveau 38
- **Base Stats** : PV 90 / Atk 70 / Def 95 / AtkSp 80 / DefSp 85 / Vit 30 (Total : 450)
- **Abilities** : Fossile Ancien / Armure Naturelle (cachee)
- **Dino reel inspire** : Anomalocaris geant + archive fossile
- **Design** : Creature colossale dont le corps est un livre d'histoire geologique vivant. Chaque plaque de son armure contient un fossile d'une ere differente. Des strates sedimentaires s'empilent sur son dos. Son regard ancien semble contenir la sagesse de millions d'annees.
- **Dinodex Entry** : "Primordax est le gardien des archives de la Terre. On dit qu'il se souvient de chaque ere geologique et de chaque creature qui a foule ce monde."
- **Habitat** : Falaise du Cambrien / Sanctuaire des Origines
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 23 — SAHLIX (Zones avancees — Sable)

### #067 — SAHLIX
- **Types** : Sable
- **Stage** : Bebe (Ligne : Sahlix)
- **Evolution** : Evolue en Erosirex au niveau 22
- **Base Stats** : PV 45 / Atk 50 / Def 35 / AtkSp 40 / DefSp 30 / Vit 55 (Total : 255)
- **Abilities** : Tempete de Sable / Griffe Aceree (cachee)
- **Dino reel inspire** : Velociraptor mongoliensis (juvenile)
- **Design** : Petit raptor desert au pelage sableux dore. Son corps est camouflage pour les dunes. Des grains de sable tourbillonnent autour de ses pattes griffues. Ses yeux sont perçants comme ceux d'un faucon du desert.
- **Dinodex Entry** : "Sahlix s'enterre dans le sable pour surprendre ses proies. Seuls ses yeux brillants depassent de la surface des dunes."
- **Habitat** : Desert du Permien / Oasis Cachee
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #068 — EROSIREX
- **Types** : Sable / Roche
- **Stage** : Jeune (Ligne : Sahlix)
- **Evolution** : De Sahlix au niveau 22 / Evolue en Khamsinyx au niveau 36
- **Base Stats** : PV 60 / Atk 70 / Def 55 / AtkSp 55 / DefSp 45 / Vit 60 (Total : 345)
- **Abilities** : Tempete de Sable / Griffe Aceree (cachee)
- **Dino reel inspire** : Velociraptor mongoliensis
- **Design** : Raptor du desert dont le corps est couvert de gres erode par le vent. Des motifs d'erosion eolienne marquent ses flancs. Sa peau est rouge-sable avec des striures de roche polie. Ses griffes sont en gres durci.
- **Dinodex Entry** : "Erosirex sculpte le paysage avec ses griffes. Les formations rocheuses etranges du desert sont souvent son oeuvre involontaire."
- **Habitat** : Oasis Cachee / Canyon Ecarlate
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #069 — KHAMSINYX
- **Types** : Sable / Roche
- **Stage** : Adulte (Ligne : Sahlix)
- **Evolution** : De Erosirex au niveau 36
- **Base Stats** : PV 80 / Atk 95 / Def 70 / AtkSp 70 / DefSp 55 / Vit 75 (Total : 445)
- **Abilities** : Tempete de Sable / Instinct Predateur (cachee)
- **Dino reel inspire** : Tarbosaurus + tempete khamsin
- **Design** : Grand theropode du desert dont le corps est un tourbillon de sable et de roche sculpte en forme de predateur. Des formations de gres forment une armure naturelle. Il est entoure d'un vent brulant permanent. Ses yeux sont des agates du desert, striees de rouge et d'or.
- **Dinodex Entry** : "Khamsinyx est nomme d'apres le vent brulant du desert. Son passage transforme les plaines fertiles en dunes arides, et ses rugissements declenchent des tempetes."
- **Habitat** : Canyon Ecarlate / Vortex du Desert
- **Rarete** : Rare
- **XP Group** : Rapide

---

## LIGNE 24 — AQUATHORN (Zones avancees — Eau)

### #070 — AQUATHORN
- **Types** : Eau
- **Stage** : Bebe (Ligne : Aquathorn)
- **Evolution** : Evolue en Marevore au niveau 20
- **Base Stats** : PV 50 / Atk 45 / Def 50 / AtkSp 40 / DefSp 40 / Vit 30 (Total : 255)
- **Abilities** : Torrent / Peau Dure (cachee)
- **Dino reel inspire** : Spinosaurus (juvenile)
- **Design** : Petit spinosaure au corps bleu sarcelle avec une voile dorsale translucide comme une nageoire. Des epines d'eau solidifiee poussent le long de son dos. Ses pattes palmees sont faites pour nager.
- **Dinodex Entry** : "Aquathorn utilise sa voile dorsale comme un filet de peche naturel. Il reste immobile dans l'eau peu profonde, attendant que les poissons se prennent dans ses epines."
- **Habitat** : Riviere Azur / Mangrove Ancestrale
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #071 — MAREVORE
- **Types** : Eau
- **Stage** : Jeune (Ligne : Aquathorn)
- **Evolution** : De Aquathorn au niveau 20 / Evolue en Leviaspine au niveau 36
- **Base Stats** : PV 70 / Atk 65 / Def 65 / AtkSp 55 / DefSp 55 / Vit 35 (Total : 345)
- **Abilities** : Torrent / Peau Dure (cachee)
- **Dino reel inspire** : Spinosaurus
- **Design** : Spinosaure semi-aquatique au corps bleu marine avec une voile dorsale majestueuse striee de bleu et de blanc. Ses griffes puissantes sont adaptees a la peche. Sa machoire crocodilienne est herissee de dents crochues.
- **Dinodex Entry** : "Marevore est un predateur semi-aquatique redoutable. Sa voile lui sert a la fois de thermoregulateur et de signal d'intimidation envers les rivaux."
- **Habitat** : Mangrove Ancestrale / Estuaire du Trias
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #072 — LEVIASPINE
- **Types** : Eau / Poison
- **Stage** : Adulte (Ligne : Aquathorn)
- **Evolution** : De Marevore au niveau 36
- **Base Stats** : PV 95 / Atk 90 / Def 80 / AtkSp 70 / DefSp 65 / Vit 40 (Total : 440)
- **Abilities** : Torrent / Venin (cachee)
- **Dino reel inspire** : Spinosaurus geant
- **Design** : Spinosaure titanesque dont la voile dorsale est herissee d'epines venimeuses bleues et violettes. Son corps est bleu abyssal avec des motifs de poisson-lion venimeux. Sa machoire geante secrete un venin paralysant. Des bulles toxiques flottent autour de lui.
- **Dinodex Entry** : "Leviaspine est le predateur supreme des eaux profondes. Ses epines venimeuses dissuadent meme les plus grands predateurs de l'approcher."
- **Habitat** : Estuaire du Trias / Ocean Primordial
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 25 — LUMINEX (Zones avancees — Lumiere — 2 stages)

### #073 — LUMINEX
- **Types** : Lumiere
- **Stage** : Bebe (Ligne : Luminex)
- **Evolution** : Evolue en Aurorex au niveau 30 (Evolution speciale : necessite la Pierre d'Aube)
- **Base Stats** : PV 50 / Atk 40 / Def 40 / AtkSp 65 / DefSp 55 / Vit 55 (Total : 305)
- **Abilities** : Aura Lumineuse / Photosynthese (cachee)
- **Dino reel inspire** : Leptoceratops
- **Design** : Petit ceratopsien gracieux dont le corps emet une lumiere doree naturelle. Ses ecailles sont comme de petits miroirs qui refletent et amplifient la lumiere. Un halo de particules lumineuses flotte autour de lui.
- **Dinodex Entry** : "Luminex est si brillant qu'il sert de phare naturel dans les grottes. Les mineurs l'emmenent comme compagnon pour eclairer les tunnels les plus sombres."
- **Habitat** : Temple de l'Aube / Prairie Lumineuse
- **Rarete** : Rare
- **XP Group** : Moyen

### #074 — AUROREX
- **Types** : Lumiere / Glace
- **Stage** : Adulte (Ligne : Luminex)
- **Evolution** : De Luminex avec Pierre d'Aube
- **Base Stats** : PV 75 / Atk 55 / Def 60 / AtkSp 100 / DefSp 85 / Vit 70 (Total : 445)
- **Abilities** : Aura Lumineuse / Gel Eternel (cachee)
- **Dino reel inspire** : Leptoceratops + aurore boreale
- **Design** : Ceratopsien etheree dont le corps projette les couleurs d'une aurore boreale. Des voiles de lumiere verte, bleue et violette ondulent autour de lui. Sa collerette est un ecran d'aurore polaire vivante. Des cristaux de glace captent et refractent sa lumiere en spectacles celestes.
- **Dinodex Entry** : "Aurorex n'apparait que dans les regions polaires, pendant les nuits les plus froides. Sa lumiere est si belle que les peuples du Nord le considerent comme un messager divin."
- **Habitat** : Toundra Eternelle / Zenith Dore
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 26 — EMBRASAURE (Fin de jeu — Feu — 2 stages)

### #075 — EMBRASAURE
- **Types** : Feu
- **Stage** : Bebe (Ligne : Embrasaure)
- **Evolution** : Evolue en Infernadon au niveau 32 (Evolution speciale : necessite d'etre dans la Caldeira Ardente)
- **Base Stats** : PV 55 / Atk 60 / Def 40 / AtkSp 55 / DefSp 35 / Vit 55 (Total : 300)
- **Abilities** : Flamme Vive / Intimidation (cachee)
- **Dino reel inspire** : Carnotaurus (juvenile)
- **Design** : Petit carnotaure aux cornes de flamme bleue. Son corps est rouge cerise avec des ecailles qui luisent comme des braises. Des flammes bleues dansent sur ses epaules. Malgre ses bras minuscules, son regard est feroce.
- **Dinodex Entry** : "Embrasaure possede des flammes si chaudes qu'elles sont bleues. Les forgerons antiques recherchaient sa chaleur pour fondre les metaux les plus resistants."
- **Habitat** : Caldeira Ardente / Cratere du Jurassique
- **Rarete** : Rare
- **XP Group** : Lent

### #076 — INFERNADON
- **Types** : Feu / Ombre
- **Stage** : Adulte (Ligne : Embrasaure)
- **Evolution** : De Embrasaure au niveau 32 dans la Caldeira Ardente
- **Base Stats** : PV 80 / Atk 100 / Def 65 / AtkSp 90 / DefSp 55 / Vit 70 (Total : 460)
- **Abilities** : Flamme Vive / Instinct Predateur (cachee)
- **Dino reel inspire** : Carnotaurus
- **Design** : Carnotaure terrifiant dont le corps est un brasier vivant de flammes noires et bleues. Ses cornes sont des colonnes de feu infernal. Ses yeux sont des fournaises orange dans un visage sombre. L'air autour de lui ondule de chaleur intense, et des ombres dansent dans ses flammes.
- **Dinodex Entry** : "Infernadon brule d'un feu si sombre qu'il semble venir des profondeurs de la terre. Les dresseurs assez braves pour le maitriser gagnent un respect universel."
- **Habitat** : Cratere du Jurassique / Coeur de la Montagne
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 27 — GLACIAIRE (Fin de jeu — Glace)

### #077 — GLACIAIRE
- **Types** : Glace
- **Stage** : Bebe (Ligne : Glaciaire)
- **Evolution** : Evolue en Frosthorn au niveau 22
- **Base Stats** : PV 50 / Atk 35 / Def 50 / AtkSp 50 / DefSp 55 / Vit 25 (Total : 265)
- **Abilities** : Gel Eternel / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Einiosaurus (juvenile)
- **Design** : Petit ceratopsien bleu glace dont la corne nasale est un pic de glace. Sa collerette est bordee de stalactites de givre. Son souffle est un brouillard glace. Ses yeux sont d'un bleu profond.
- **Dinodex Entry** : "Glaciaire dort dans la neige, invisible sous une couverture de givre. Seul un petit nuage de buee trahit sa presence les jours de grand froid."
- **Habitat** : Vallee Givree / Glacier Ancien
- **Rarete** : Peu commun
- **XP Group** : Lent

### #078 — FROSTHORN
- **Types** : Glace
- **Stage** : Jeune (Ligne : Glaciaire)
- **Evolution** : De Glaciaire au niveau 22 / Evolue en Cryotitan au niveau 38
- **Base Stats** : PV 70 / Atk 50 / Def 70 / AtkSp 65 / DefSp 75 / Vit 25 (Total : 355)
- **Abilities** : Gel Eternel / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Einiosaurus
- **Design** : Ceratopsien robuste dont les cornes sont des stalagmites de glace eternelle. Sa collerette est un mur de permafrost. Des cristaux de neige scintillent sur tout son corps bleu-blanc. Le sol gele sous ses pas.
- **Dinodex Entry** : "Frosthorn peut survivre dans des temperatures de -60 degres. Sa corne de glace ne fond jamais, meme au coeur de l'ete."
- **Habitat** : Glacier Ancien / Toundra Eternelle
- **Rarete** : Peu commun
- **XP Group** : Lent

### #079 — CRYOTITAN
- **Types** : Glace / Acier
- **Stage** : Adulte (Ligne : Glaciaire)
- **Evolution** : De Frosthorn au niveau 38
- **Base Stats** : PV 95 / Atk 65 / Def 100 / AtkSp 80 / DefSp 95 / Vit 20 (Total : 455)
- **Abilities** : Gel Eternel / Cuirasse (cachee)
- **Dino reel inspire** : Pentaceratops + forteresse de glace
- **Design** : Ceratopsien forteresse dont le corps est un glacier vivant renforce d'acier. Ses cinq cornes sont des piliers de glace metallique. Sa collerette est un rempart de permafrost et d'acier givre. Il avance comme un glacier — lent, implacable, indestructible.
- **Dinodex Entry** : "Cryotitan est une forteresse ambulante de glace et d'acier. Il a traverse des epoques entieres sans jamais fondre, temoignage vivant de l'ere glaciaire."
- **Habitat** : Toundra Eternelle / Forteresse Givre
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 28 — OMBRASAURE (Fin de jeu — Ombre)

### #080 — OMBRASAURE
- **Types** : Ombre
- **Stage** : Bebe (Ligne : Ombrasaure)
- **Evolution** : Evolue en Crepuscor au niveau 24
- **Base Stats** : PV 40 / Atk 35 / Def 35 / AtkSp 55 / DefSp 50 / Vit 50 (Total : 265)
- **Abilities** : Ombre Furtive / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Saurornitholestes (juvenile)
- **Design** : Petit raptor a la peau d'un violet si sombre qu'il semble absorber la lumiere. Des constellations de points lumineux parsement son dos comme un ciel etoile inverse. Ses yeux sont des etoiles argentees dans un visage sombre.
- **Dinodex Entry** : "Ombrasaure porte la nuit etoilee sur son dos. Les astronomes le cherchent car les constellations sur sa peau changent avec les saisons."
- **Habitat** : Foret Obscure / Clairiere Lunaire
- **Rarete** : Rare
- **XP Group** : Moyen

### #081 — CREPUSCOR
- **Types** : Ombre
- **Stage** : Jeune (Ligne : Ombrasaure)
- **Evolution** : De Ombrasaure au niveau 24 / Evolue en Eclipsadon au niveau 38
- **Base Stats** : PV 55 / Atk 50 / Def 50 / AtkSp 75 / DefSp 65 / Vit 60 (Total : 355)
- **Abilities** : Ombre Furtive / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Saurornitholestes
- **Design** : Raptor elegant dont la peau est un crepuscule vivant — gradient de violet, orange et noir. Des ombres dansent autour de lui comme des aurores boreales sombres. Ses griffes emettent une lueur mauve.
- **Dinodex Entry** : "Crepuscor n'apparait qu'au crepuscule, dans le moment fragile entre jour et nuit. Sa presence annonce la tombee de la nuit meme en plein jour."
- **Habitat** : Clairiere Lunaire / Neant Obscur
- **Rarete** : Rare
- **XP Group** : Moyen

### #082 — ECLIPSADON
- **Types** : Ombre / Lumiere
- **Stage** : Adulte (Ligne : Ombrasaure)
- **Evolution** : De Crepuscor au niveau 38
- **Base Stats** : PV 75 / Atk 65 / Def 65 / AtkSp 100 / DefSp 85 / Vit 70 (Total : 460)
- **Abilities** : Ombre Furtive / Aura Lumineuse (cachee)
- **Dino reel inspire** : Dakotaraptor + eclipse solaire
- **Design** : Grand raptor majestueux dont le corps est un equilibre parfait entre ombre et lumiere. Une moitie est dore eclatant, l'autre est ombre pure. Un anneau de lumiere solaire entoure sa tete sombre comme une couronne d'eclipse. Ses yeux sont l'un dore, l'autre violet.
- **Dinodex Entry** : "Eclipsadon incarne l'equilibre entre lumiere et tenebres. Sa presence provoque des eclipses locales, plongeant la zone dans un crepuscule surnaturel."
- **Habitat** : Neant Obscur / Zenith Dore
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 29 — FULGOREX (Fin de jeu — Foudre)

### #083 — FULGOREX
- **Types** : Foudre
- **Stage** : Bebe (Ligne : Fulgorex)
- **Evolution** : Evolue en Electrodonte au niveau 24
- **Base Stats** : PV 40 / Atk 45 / Def 35 / AtkSp 55 / DefSp 40 / Vit 55 (Total : 270)
- **Abilities** : Charge Statique / Velocite (cachee)
- **Dino reel inspire** : Dromaeosaurus (juvenile)
- **Design** : Petit raptor couvert de plumes jaune electrique striees de noir. Des etincelles parcourent ses plumes en permanence. Sa queue est une bobine Tesla naturelle. Il trepigne d'energie nerveuse.
- **Dinodex Entry** : "Fulgorex genere tellement d'electricite qu'il ne peut pas rester immobile. Il court en cercles pour decharger son trop-plein d'energie."
- **Habitat** : Plaines Orageuses / Steppe Electrique
- **Rarete** : Rare
- **XP Group** : Rapide

### #084 — ELECTRODONTE
- **Types** : Foudre
- **Stage** : Jeune (Ligne : Fulgorex)
- **Evolution** : De Fulgorex au niveau 24 / Evolue en Megavolt au niveau 38
- **Base Stats** : PV 55 / Atk 60 / Def 50 / AtkSp 75 / DefSp 55 / Vit 70 (Total : 365)
- **Abilities** : Charge Statique / Velocite (cachee)
- **Dino reel inspire** : Dromaeosaurus
- **Design** : Raptor electrique au corps couvert de plumes conductives bleu-jaune. Des eclairs miniatures relient ses griffes au sol. Sa queue est un generateur electrique organique. L'air crepite d'electricite autour de lui.
- **Dinodex Entry** : "Electrodonte peut alimenter un village entier en electricite pendant une semaine. Les ingenieurs etudient ses organes pour ameliorer les generateurs."
- **Habitat** : Steppe Electrique / Pic Tonnerre
- **Rarete** : Rare
- **XP Group** : Rapide

### #085 — MEGAVOLT
- **Types** : Foudre / Vol
- **Stage** : Adulte (Ligne : Fulgorex)
- **Evolution** : De Electrodonte au niveau 38
- **Base Stats** : PV 75 / Atk 80 / Def 60 / AtkSp 105 / DefSp 65 / Vit 85 (Total : 470)
- **Abilities** : Charge Statique / Ailes Coupantes (cachee)
- **Dino reel inspire** : Rahonavis geant electrique
- **Design** : Grand dinosaure avien dont les ailes sont des champs electriques vivants. Des eclairs parcourent ses plumes bleues et jaunes en permanence. Son corps entier est un condensateur vivant. Quand il bat des ailes, des arcs electriques illuminent le ciel.
- **Dinodex Entry** : "Megavolt est une tempete vivante. Son vol genere des eclairs qui illuminent le ciel nocturne, et son cri est un roulement de tonnerre."
- **Habitat** : Sommet de l'Orage / Ciel de Chrome
- **Rarete** : Tres rare
- **XP Group** : Rapide

---

## LIGNE 30 — CORALOX (Fin de jeu — Eau — 2 stages)

### #086 — CORALOX
- **Types** : Eau
- **Stage** : Bebe (Ligne : Coralox)
- **Evolution** : Evolue en Recifadon au niveau 30 (Evolution speciale : necessite la Pierre Corail)
- **Base Stats** : PV 55 / Atk 35 / Def 60 / AtkSp 55 / DefSp 55 / Vit 30 (Total : 290)
- **Abilities** : Torrent / Regeneration (cachee)
- **Dino reel inspire** : Tanystropheus (juvenile, aquatique)
- **Design** : Petit reptile marin au corps couvert de coraux vivants multicolores. Sa peau rose et orange est parsemee de polypes. Des anémones de mer poussent sur son dos. Il ressemble a un recif en miniature.
- **Dinodex Entry** : "Coralox porte un ecosysteme entier sur son dos. De minuscules poissons nagent entre les coraux qui poussent sur lui, formant un recif mobile."
- **Habitat** : Ocean Primordial / Recif Arc-en-Ciel
- **Rarete** : Rare
- **XP Group** : Lent

### #087 — RECIFADON
- **Types** : Eau / Lumiere
- **Stage** : Adulte (Ligne : Coralox)
- **Evolution** : De Coralox avec Pierre Corail
- **Base Stats** : PV 90 / Atk 50 / Def 90 / AtkSp 85 / DefSp 85 / Vit 35 (Total : 435)
- **Abilities** : Torrent / Aura Lumineuse (cachee)
- **Dino reel inspire** : Tanystropheus geant + recif corallien
- **Design** : Grand reptile marin dont le corps entier est un recif corallien fluorescent vivant. Des coraux luminescents de toutes les couleurs poussent sur chaque surface de son corps. Des bancs de poissons tropicaux nagent dans son sillage. Il brille sous l'eau comme un jardin sous-marin enchanté.
- **Dinodex Entry** : "Recifadon est un sanctuaire marin vivant. Les oceans ou il vit sont les plus riches en biodiversite. Les pecheurs le protegent comme un tresor."
- **Habitat** : Recif Arc-en-Ciel / Sanctuaire Marin
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 31 — EPINOX (Zones secretes — Plante/Poison — Sans evolution)

### #088 — EPINOX
- **Types** : Plante / Poison
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 85 / Atk 50 / Def 85 / AtkSp 90 / DefSp 80 / Vit 40 (Total : 430)
- **Abilities** : Vegetal / Venin (cachee)
- **Dino reel inspire** : Kentrosaurus + cactus venimeux
- **Design** : Dinosaure solitaire dont le corps est un cactus geant herisee d'epines venimeuses. Des fleurs toxiques multicolores eclosent entre ses epines. Son corps vert sombre suinte d'une seve empoisonnee. Ses yeux sont caches derriere un masque d'epines.
- **Dinodex Entry** : "Epinox est un piege vivant. Ses fleurs attirent les imprudents avec leur beaute, mais ses epines venimeuses punissent quiconque s'approche trop pres."
- **Habitat** : Desert du Permien / Vallee des Epines
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 32 — MYTHOREX (Zones secretes — Fossile/Ombre — Sans evolution)

### #089 — MYTHOREX
- **Types** : Fossile / Ombre
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 80 / Atk 85 / Def 70 / AtkSp 80 / DefSp 75 / Vit 50 (Total : 440)
- **Abilities** : Fossile Ancien / Ombre Furtive (cachee)
- **Dino reel inspire** : Dunkleosteus
- **Design** : Poisson cuirasse prehistorique dont la carapace est faite d'ossements fossilises noircis. Ses yeux brillent d'une lueur spectrale bleue. Des ombres anciennes emanent de ses plaques osseuses. Sa machoire de cisaille peut trancher n'importe quel materiau.
- **Dinodex Entry** : "Mythorex est un survivant d'une ere oubliee. Il hante les eaux profondes comme un fantome du passe, et les pecheurs le considèrent comme un mauvais presage."
- **Habitat** : Abysses du Devonien / Fosse Ancestrale
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 33 — VOLTAILE (Zones secretes — Foudre/Vol — Sans evolution)

### #090 — VOLTAILE
- **Types** : Foudre / Vol
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 70 / Atk 75 / Def 55 / AtkSp 90 / DefSp 60 / Vit 95 (Total : 445)
- **Abilities** : Charge Statique / Ailes Coupantes (cachee)
- **Dino reel inspire** : Dimorphodon + eclair en boule
- **Design** : Pterosaure rapide entoure d'un champ electrique permanent. Ses ailes translucides creptitent d'electricite blanche. Son corps compact est bleu metallique avec des marques de foudre jaunes. Des eclairs en boule orbitent autour de lui pendant le vol.
- **Dinodex Entry** : "Voltaile est plus rapide que la foudre qu'il genere. Il traverse le ciel comme un eclair vivant, laissant un trail electrique dans son sillage."
- **Habitat** : Sommet de l'Orage / Ciel de Chrome
- **Rarete** : Tres rare
- **XP Group** : Rapide

---

## LIGNE 34 — RACIVORE (Zones secretes — Plante — 2 stages)

### #091 — RACIVORE
- **Types** : Plante
- **Stage** : Bebe (Ligne : Racivore)
- **Evolution** : Evolue en Sequoiax au niveau 35 (Evolution speciale : necessite un niveau d'amitie maximal)
- **Base Stats** : PV 55 / Atk 40 / Def 55 / AtkSp 50 / DefSp 50 / Vit 35 (Total : 285)
- **Abilities** : Photosynthese / Regeneration (cachee)
- **Dino reel inspire** : Mussaurus (juvenile)
- **Design** : Petit sauropode dont le corps est enracine dans le sol. Des racines poussent de ses pattes et s'enfoncent dans la terre a chaque pas. Sa peau est brune comme l'ecorce, avec des pousses vertes sur le dos.
- **Dinodex Entry** : "Racivore s'enracine la ou il dort. Au matin, il arrache ses racines du sol et reprend sa route, laissant de petits arbrisseaux pousser a sa place."
- **Habitat** : Jungle Primordiale / Foret Ancestrale
- **Rarete** : Rare
- **XP Group** : Lent

### #092 — SEQUOIAX
- **Types** : Plante / Roche
- **Stage** : Adulte (Ligne : Racivore)
- **Evolution** : De Racivore au niveau 35 avec amitie max
- **Base Stats** : PV 100 / Atk 60 / Def 90 / AtkSp 75 / DefSp 80 / Vit 30 (Total : 435)
- **Abilities** : Photosynthese / Armure Naturelle (cachee)
- **Dino reel inspire** : Diplodocus + sequoia geant
- **Design** : Sauropode dont le corps est un sequoia vivant. Son cou est un tronc massif, ses pattes sont des racines anciennes, et sa tete emerge d'un canope de feuillage. Des strates de roche sont visibles entre ses anneaux d'ecorce. Des oiseaux nichent dans ses branches.
- **Dinodex Entry** : "Sequoiax vit depuis si longtemps que personne ne connait son age exact. Les forets les plus anciennes se sont formees autour des lieux ou il s'est repose."
- **Habitat** : Foret Ancestrale / Sanctuaire des Origines
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 35 — SABLONIX (Zones secretes — Sable/Fossile — 2 stages)

### #093 — SABLONIX
- **Types** : Sable
- **Stage** : Bebe (Ligne : Sablonix)
- **Evolution** : Evolue en Sphinx au niveau 34 (Evolution speciale : necessite la Pierre de Sable)
- **Base Stats** : PV 45 / Atk 40 / Def 45 / AtkSp 50 / DefSp 50 / Vit 40 (Total : 270)
- **Abilities** : Tempete de Sable / Fossile Ancien (cachee)
- **Dino reel inspire** : Psittacosaurus mongoliensis (juvenile)
- **Design** : Petit ceratopsien au corps de gres dore. Des hieroglyphes naturels ornent ses flancs. Son visage rappelle un masque egyptien miniature. Du sable fin coule entre ses ecailles.
- **Dinodex Entry** : "Sablonix porte des motifs mysterieux que personne n'a jamais pu dechiffrer. Certains pensent qu'ils racontent l'histoire d'une civilisation oubliee."
- **Habitat** : Desert du Permien / Ruines Englouties
- **Rarete** : Rare
- **XP Group** : Moyen

### #094 — SPHINX
- **Types** : Sable / Fossile
- **Stage** : Adulte (Ligne : Sablonix)
- **Evolution** : De Sablonix avec Pierre de Sable
- **Base Stats** : PV 80 / Atk 60 / Def 75 / AtkSp 85 / DefSp 80 / Vit 55 (Total : 435)
- **Abilities** : Tempete de Sable / Fossile Ancien (cachee)
- **Dino reel inspire** : Protoceratops geant + sphinx
- **Design** : Ceratopsien majestueux dont le corps est un monument de gres sculpte. Sa posture rappelle un sphinx, avec des pattes avant etendues et un regard enigmatique. Des hieroglyphes fossilises couvrent chaque surface de son corps. Des tempetes de sable miniatures tourbillonnent entre ses cornes.
- **Dinodex Entry** : "Sphinx garde les secrets du desert depuis des millenaires. On dit qu'il pose une enigme a quiconque veut passer, et que seuls les sages peuvent le dompter."
- **Habitat** : Ruines Englouties / Temple du Desert
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 36 — VERDOX (Zones avancees — Plante)

### #095 — VERDOX
- **Types** : Plante
- **Stage** : Bebe (Ligne : Verdox)
- **Evolution** : Evolue en Chlorodon au niveau 20
- **Base Stats** : PV 45 / Atk 40 / Def 45 / AtkSp 50 / DefSp 45 / Vit 30 (Total : 255)
- **Abilities** : Photosynthese / Vegetal (cachee)
- **Dino reel inspire** : Iguanodon (juvenile)
- **Design** : Petit iguanodonte au corps vert pomme parseme de feuilles. Son pouce epineux est une epine de rosier. Des bourgeeons fleurissent le long de sa colonne vertebrale. Ses yeux sont verts comme des emeraudes.
- **Dinodex Entry** : "Verdox plante des graines partout ou il passe. Les sentiers qu'il emprunte regulierement deviennent des chemins fleuris en quelques semaines."
- **Habitat** : Prairie Lumineuse / Vallee des Fougeres
- **Rarete** : Commun
- **XP Group** : Moyen

### #096 — CHLORODON
- **Types** : Plante
- **Stage** : Jeune (Ligne : Verdox)
- **Evolution** : De Verdox au niveau 20 / Evolue en Gaiasaure au niveau 36
- **Base Stats** : PV 65 / Atk 55 / Def 60 / AtkSp 70 / DefSp 60 / Vit 35 (Total : 345)
- **Abilities** : Photosynthese / Vegetal (cachee)
- **Dino reel inspire** : Iguanodon
- **Design** : Iguanodonte de taille moyenne au corps vert foret couvert de mousse et de lichen. Son pouce est devenu une lame de feuille geante. Des vrilles vegetales s'etendent de ses bras. Sa peau a la texture de l'ecorce.
- **Dinodex Entry** : "Chlorodon convertit la lumiere du soleil en energie avec une efficacite remarquable. Il peut rester des mois sans manger tant que le soleil brille."
- **Habitat** : Vallee des Fougeres / Jungle Primordiale
- **Rarete** : Commun
- **XP Group** : Moyen

### #097 — GAIASAURE
- **Types** : Plante / Lumiere
- **Stage** : Adulte (Ligne : Verdox)
- **Evolution** : De Chlorodon au niveau 36
- **Base Stats** : PV 90 / Atk 70 / Def 80 / AtkSp 90 / DefSp 80 / Vit 30 (Total : 440)
- **Abilities** : Photosynthese / Aura Lumineuse (cachee)
- **Dino reel inspire** : Iguanodon geant + deesse terre
- **Design** : Grand iguanodonte majestueux dont le corps est une garden sacree vivante. Des fleurs lumineuses eclosent a chaque pas. Son corps vert emeraude irradie d'une lumiere doree naturelle. Des papillons de lumiere l'accompagnent en permanence. Son regard est bienveillant et sage.
- **Dinodex Entry** : "Gaiasaure est le gardien des forets sacrees. Sa presence fait fleurir les terres les plus arides et guerit les ecosystemes endommages."
- **Habitat** : Jungle Primordiale / Sanctuaire Vegetal
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 37 — MAGMAX (Zones avancees — Feu)

### #098 — MAGMAX
- **Types** : Feu
- **Stage** : Bebe (Ligne : Magmax)
- **Evolution** : Evolue en Pyroclade au niveau 22
- **Base Stats** : PV 45 / Atk 55 / Def 35 / AtkSp 45 / DefSp 30 / Vit 50 (Total : 260)
- **Abilities** : Flamme Vive / Griffe Aceree (cachee)
- **Dino reel inspire** : Coelophysis (juvenile)
- **Design** : Petit theropode svelte au corps rouge-orange avec des veines de magma visible sous sa peau translucide. Des geysers miniatures eruditent de son dos quand il est excite. Ses griffes sont en obsidienne chaude.
- **Dinodex Entry** : "Magmax est si chaud qu'il fait fondre la neige dans un rayon de deux metres. Les randonneurs l'apprecient comme compagnon de montagne en hiver."
- **Habitat** : Caldeira Ardente / Champs de Lave
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #099 — PYROCLADE
- **Types** : Feu
- **Stage** : Jeune (Ligne : Magmax)
- **Evolution** : De Magmax au niveau 22 / Evolue en Erupsion au niveau 36
- **Base Stats** : PV 60 / Atk 75 / Def 50 / AtkSp 60 / DefSp 45 / Vit 55 (Total : 345)
- **Abilities** : Flamme Vive / Griffe Aceree (cachee)
- **Dino reel inspire** : Coelophysis
- **Design** : Theropode agile au corps rouge sombre avec des coulees de lave qui dessinent des motifs sur ses flancs. Des eclats pyroclastiques flottent autour de lui. Ses yeux sont des braises ardentes. Il laisse des traces brulantes derriere lui.
- **Dinodex Entry** : "Pyroclade est nomme d'apres les nuees ardentes. Sa vitesse et sa chaleur le rendent aussi dangereux et imprevisible qu'une eruption volcanique."
- **Habitat** : Champs de Lave / Cratere du Jurassique
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #100 — ERUPSION
- **Types** : Feu / Terre
- **Stage** : Adulte (Ligne : Magmax)
- **Evolution** : De Pyroclade au niveau 36
- **Base Stats** : PV 80 / Atk 100 / Def 65 / AtkSp 75 / DefSp 55 / Vit 60 (Total : 435)
- **Abilities** : Flamme Vive / Intimidation (cachee)
- **Dino reel inspire** : Ceratosaurus + eruption volcanique
- **Design** : Grand theropode dont le corps est une eruption volcanique en cours. De la lave jaillit de son dos comme un volcan miniature. Sa peau craquele revele un interieur de magma. Chaque pas fait trembler le sol et cracher des etincelles. Sa corne nasale est un bouchon volcanique pret a exploser.
- **Dinodex Entry** : "Erupsion est une catastrophe naturelle sur pattes. Son rugissement declenche des eruptions mineures, et les villes a proximite de son territoire sont construites avec des abris volcaniques."
- **Habitat** : Cratere du Jurassique / Coeur de la Montagne
- **Rarete** : Rare
- **XP Group** : Rapide

---

## LIGNE 38 — BLINDOSAURE (Zones avancees — Acier/Terre)

### #101 — BLINDOSAURE
- **Types** : Acier
- **Stage** : Bebe (Ligne : Blindosaure)
- **Evolution** : Evolue en Cuiravore au niveau 24
- **Base Stats** : PV 50 / Atk 40 / Def 60 / AtkSp 30 / DefSp 50 / Vit 25 (Total : 255)
- **Abilities** : Cuirasse / Armure Naturelle (cachee)
- **Dino reel inspire** : Euoplocephalus (juvenile)
- **Design** : Petit ankylosaure au corps recouvert de plaques d'acier naturel. Sa queue se termine par un marteau metallique. Ses yeux sont proteges par des visors d'acier. Il est trapu et compact comme un petit tank.
- **Dinodex Entry** : "Blindosaure est si bien protege qu'il dort a decouvert sans crainte. Les predateurs se cassent les dents sur son armure d'acier naturel."
- **Habitat** : Mine Abandonnee / Forge Naturelle
- **Rarete** : Rare
- **XP Group** : Lent

### #102 — CUIRAVORE
- **Types** : Acier
- **Stage** : Jeune (Ligne : Blindosaure)
- **Evolution** : De Blindosaure au niveau 24 / Evolue en Bastiosaure au niveau 38
- **Base Stats** : PV 70 / Atk 55 / Def 85 / AtkSp 40 / DefSp 70 / Vit 25 (Total : 345)
- **Abilities** : Cuirasse / Armure Naturelle (cachee)
- **Dino reel inspire** : Euoplocephalus
- **Design** : Ankylosaure robuste au blindage d'acier poli. Ses plaques dorsales sont des boucliers metalliques rivetes. Sa queue est un fleau d'armes lourd. Des motifs graves ornent son armure comme des decorations militaires.
- **Dinodex Entry** : "Cuiravore est un tank vivant. Son armure peut resister a l'impact d'un meteorite, et sa queue est une arme de siege a elle seule."
- **Habitat** : Forge Naturelle / Coeur de la Montagne
- **Rarete** : Rare
- **XP Group** : Lent

### #103 — BASTIOSAURE
- **Types** : Acier / Terre
- **Stage** : Adulte (Ligne : Blindosaure)
- **Evolution** : De Cuiravore au niveau 38
- **Base Stats** : PV 95 / Atk 70 / Def 125 / AtkSp 50 / DefSp 90 / Vit 20 (Total : 450)
- **Abilities** : Cuirasse / Peau Dure (cachee)
- **Dino reel inspire** : Euoplocephalus titanesque + forteresse
- **Design** : Ankylosaure colossal dont le corps est une forteresse d'acier et de terre. Des tourelles de metal ornent son dos. Son armure est composee de couches d'acier et de roche compactee. Sa queue est un boulet de demolition massif. Il est immobile et imperturbable comme un chateau fort.
- **Dinodex Entry** : "Bastiosaure est la defense ultime. Les armees anciennes construisaient leurs fortifications en s'inspirant de son armure. Rien ne peut percer sa cuirasse."
- **Habitat** : Coeur de la Montagne / Citadelle Fossile
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 39 — BOREALIS (Zones secretes — Lumiere/Glace — 2 stages)

### #104 — BOREALIS
- **Types** : Lumiere
- **Stage** : Bebe (Ligne : Borealis)
- **Evolution** : Evolue en Celestiax au niveau 35 (Evolution speciale : la nuit, avec amitie elevee)
- **Base Stats** : PV 45 / Atk 35 / Def 45 / AtkSp 60 / DefSp 55 / Vit 45 (Total : 285)
- **Abilities** : Aura Lumineuse / Gel Eternel (cachee)
- **Dino reel inspire** : Leaellynasaura
- **Design** : Petit dinosaure gracieux aux grands yeux lumineux. Son corps emet une lueur douce et changeante comme une aurore boreale. Sa longue queue est un ruban de lumiere multicolore. Ses plumes sont translucides et iridescentes.
- **Dinodex Entry** : "Borealis vit dans les regions polaires ou la nuit dure des mois. Sa lumiere guide les creatures perdues dans l'obscurite arctique."
- **Habitat** : Toundra Eternelle / Grotte de l'Aurore
- **Rarete** : Tres rare
- **XP Group** : Lent

### #105 — CELESTIAX
- **Types** : Lumiere / Glace
- **Stage** : Adulte (Ligne : Borealis)
- **Evolution** : De Borealis la nuit avec amitie elevee
- **Base Stats** : PV 70 / Atk 50 / Def 65 / AtkSp 100 / DefSp 90 / Vit 60 (Total : 435)
- **Abilities** : Aura Lumineuse / Gel Eternel (cachee)
- **Dino reel inspire** : Leaellynasaura + aurore boreale celeste
- **Design** : Dinosaure etheree dont le corps est un spectacle celeste vivant. Des aurores boreales ondulent en permanence autour de lui. Sa queue est un voile de lumiere arctique. Des cristaux de glace flottent dans son aura lumineuse. Son regard est d'une serenite surnaturelle.
- **Dinodex Entry** : "Celestiax est considere comme l'esprit de l'hiver. Son apparition pendant les nuits polaires est un evenement que les peuples du Nord celebrent avec des festivals de lumiere."
- **Habitat** : Grotte de l'Aurore / Zenith Dore
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 40 — CORRODEX (Zones avancees — Poison)

### #106 — CORRODEX
- **Types** : Poison
- **Stage** : Bebe (Ligne : Corrodex)
- **Evolution** : Evolue en Acidonte au niveau 22
- **Base Stats** : PV 45 / Atk 40 / Def 40 / AtkSp 50 / DefSp 45 / Vit 40 (Total : 260)
- **Abilities** : Venin / Sang Froid (cachee)
- **Dino reel inspire** : Compsognathus + creature acide
- **Design** : Petit theropode au corps vert-acide avec des taches jaune toxique. Sa peau suinte d'un liquide corrosif. Ses griffes dissolvent tout ce qu'elles touchent. Son sourire revele des dents couvertes d'acide.
- **Dinodex Entry** : "Corrodex dissout sa nourriture avant meme de la manger. Les cages metalliques ne resistent pas longtemps a son acide — les dresseurs utilisent du verre special."
- **Habitat** : Marais Toxiques / Bassin Acide
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #107 — ACIDONTE
- **Types** : Poison
- **Stage** : Jeune (Ligne : Corrodex)
- **Evolution** : De Corrodex au niveau 22 / Evolue en Dissolvor au niveau 36
- **Base Stats** : PV 60 / Atk 55 / Def 55 / AtkSp 70 / DefSp 60 / Vit 50 (Total : 350)
- **Abilities** : Venin / Sang Froid (cachee)
- **Dino reel inspire** : Herrerasaurus + acide
- **Design** : Theropode de taille moyenne dont la peau secrete un acide puissant jaune-vert. Des fumerolles acides s'elevent de son corps. Sa machoire degouline de fluide corrosif. Le sol se dissout la ou il marche.
- **Dinodex Entry** : "Acidonte peut dissoudre la roche en quelques minutes. Les grottes et tunnels du monde souterrain ont souvent ete creuses par ses ancetres."
- **Habitat** : Bassin Acide / Abime Corrompue
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #108 — DISSOLVOR
- **Types** : Poison / Eau
- **Stage** : Adulte (Ligne : Corrodex)
- **Evolution** : De Acidonte au niveau 36
- **Base Stats** : PV 80 / Atk 70 / Def 65 / AtkSp 95 / DefSp 75 / Vit 55 (Total : 440)
- **Abilities** : Venin / Nage Rapide (cachee)
- **Dino reel inspire** : Herrerasaurus geant + maree acide
- **Design** : Grand theropode semi-aquatique dont le corps est un reservoir d'acide vivant. Son corps vert-noir suinte en permanence. Il nage dans des lacs d'acide comme d'autres nagent dans l'eau. Des bulles toxiques eclatent a la surface de sa peau. Ses yeux jaune acide brillent dans l'obscurite.
- **Dinodex Entry** : "Dissolvor transforme les lacs en bassins d'acide. Les ecologistes tentent de limiter sa population pour proteger les ecosystemes aquatiques."
- **Habitat** : Abime Corrompue / Lac Corrosif
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 41 — TERROPS (Zones avancees — Terre)

### #109 — TERROPS
- **Types** : Terre
- **Stage** : Bebe (Ligne : Terrops)
- **Evolution** : Evolue en Lithocrane au niveau 20
- **Base Stats** : PV 45 / Atk 55 / Def 40 / AtkSp 30 / DefSp 35 / Vit 45 (Total : 250)
- **Abilities** : Peau Dure / Machoire (cachee)
- **Dino reel inspire** : Pachycephalosaurus (juvenile)
- **Design** : Petit dinosaure bipede au crane epais couvert de terre compactee. Son dome cranien est orne de petits cailloux incrustes. Il se cogne la tete partout avec enthousiasme. Ses yeux sont determines et obstines.
- **Dinodex Entry** : "Terrops teste la durete de tout en se cognant la tete dessus. Les murs, les arbres, les rochers — rien ne resiste a sa curiosite destructrice."
- **Habitat** : Plaines de Boue / Gorge Tectonique
- **Rarete** : Commun
- **XP Group** : Rapide

### #110 — LITHOCRANE
- **Types** : Terre
- **Stage** : Jeune (Ligne : Terrops)
- **Evolution** : De Terrops au niveau 20 / Evolue en Seismocrane au niveau 34
- **Base Stats** : PV 60 / Atk 75 / Def 55 / AtkSp 40 / DefSp 45 / Vit 50 (Total : 325)
- **Abilities** : Peau Dure / Machoire (cachee)
- **Dino reel inspire** : Pachycephalosaurus
- **Design** : Pachycephalosaure robuste au dome cranien de roche sedimentaire. Des strates sont visibles dans son crane epais. Son corps est brun-ocre avec des fissures sismiques. Il charge tete baissee avec une force devastatrice.
- **Dinodex Entry** : "Lithocrane peut briser un mur de pierre d'un seul coup de tete. Les demolisseurs l'emploient parfois pour abattre les vieux batiments."
- **Habitat** : Gorge Tectonique / Plateau Brise
- **Rarete** : Commun
- **XP Group** : Rapide

### #111 — SEISMOCRANE
- **Types** : Terre / Foudre
- **Stage** : Adulte (Ligne : Terrops)
- **Evolution** : De Lithocrane au niveau 34
- **Base Stats** : PV 80 / Atk 100 / Def 70 / AtkSp 55 / DefSp 55 / Vit 65 (Total : 425)
- **Abilities** : Peau Dure / Charge Statique (cachee)
- **Dino reel inspire** : Pachycephalosaurus geant + seisme electrique
- **Design** : Grand pachycephalosaure dont le dome cranien est un epicentre sismique. Des ondes de choc electriques emanent de sa tete a chaque impact. Son corps brun est parcouru de lignes de faille electrifiees. Quand il charge, le sol tremble et des eclairs jaillissent du point d'impact.
- **Dinodex Entry** : "Seismocrane combine la force brute de la terre avec la puissance de la foudre. Ses charges sont des seismes electriques qui devastent tout sur leur passage."
- **Habitat** : Plateau Brise / Sommet de l'Orage
- **Rarete** : Peu commun
- **XP Group** : Rapide

---

## LIGNE 42 — CRESTAIL (Zones avancees — Vol)

### #112 — CRESTAIL
- **Types** : Vol
- **Stage** : Bebe (Ligne : Crestail)
- **Evolution** : Evolue en Aerocrete au niveau 22
- **Base Stats** : PV 35 / Atk 35 / Def 30 / AtkSp 50 / DefSp 40 / Vit 65 (Total : 255)
- **Abilities** : Ailes Coupantes / Velocite (cachee)
- **Dino reel inspire** : Tupuxuara (juvenile)
- **Design** : Petit pterosaure a la crete coloree disproportionnee. Sa crete est un eventail de couleurs vives — rouge, bleu, jaune. Son corps est petit et frele sous cette crete enorme. Ses ailes sont courtes mais rapides.
- **Dinodex Entry** : "Crestail utilise sa crete coloree pour impressionner les autres. Plus la crete est grande et coloree, plus le Crestail est respecte dans la colonie."
- **Habitat** : Falaises Venteuses / Nid des Vents
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #113 — AEROCRETE
- **Types** : Vol
- **Stage** : Jeune (Ligne : Crestail)
- **Evolution** : De Crestail au niveau 22 / Evolue en Panachex au niveau 36
- **Base Stats** : PV 50 / Atk 50 / Def 40 / AtkSp 65 / DefSp 55 / Vit 80 (Total : 340)
- **Abilities** : Ailes Coupantes / Velocite (cachee)
- **Dino reel inspire** : Tupuxuara
- **Design** : Pterosaure elegant a la crete majestueuse multicolore. Ses ailes sont devenues longues et effilees. Des plumes decoratives pendent de sa crete comme des bannieres. Son vol est acrobatique et gracieux.
- **Dinodex Entry** : "Aerocrete execute des acrobaties aeriennes complexes pour seduire ses partenaires. Ses danses de vol sont parmi les plus belles du monde prehistorique."
- **Habitat** : Nid des Vents / Col des Vents
- **Rarete** : Peu commun
- **XP Group** : Rapide

### #114 — PANACHEX
- **Types** : Vol / Lumiere
- **Stage** : Adulte (Ligne : Crestail)
- **Evolution** : De Aerocrete au niveau 36
- **Base Stats** : PV 70 / Atk 65 / Def 55 / AtkSp 85 / DefSp 70 / Vit 100 (Total : 445)
- **Abilities** : Ailes Coupantes / Aura Lumineuse (cachee)
- **Dino reel inspire** : Tupuxuara geant + oiseau de paradis
- **Design** : Pterosaure spectaculaire dont la crete est un prisme de lumiere vivant. Des plumes luminescentes de toutes les couleurs forment une queue de paon. Ses ailes projettent des arcs-en-ciel quand elles captent le soleil. Son vol est un spectacle de lumiere qui eblouit les spectateurs.
- **Dinodex Entry** : "Panachex est considere comme le plus beau dinosaure du monde. Son vol lumineux est visible a des kilometres, attirant les foules et les photographes."
- **Habitat** : Col des Vents / Zenith Dore
- **Rarete** : Rare
- **XP Group** : Rapide

---

## LIGNE 43 — ROCALVE (Zones secretes — Roche)

### #115 — ROCALVE
- **Types** : Roche
- **Stage** : Bebe (Ligne : Rocalve)
- **Evolution** : Evolue en Basaltex au niveau 24
- **Base Stats** : PV 50 / Atk 50 / Def 55 / AtkSp 30 / DefSp 40 / Vit 25 (Total : 250)
- **Abilities** : Peau Dure / Carapace (cachee)
- **Dino reel inspire** : Scutellosaurus (juvenile)
- **Design** : Petit dinosaure cuirasse dont les plaques sont en basalte volcanique noir. Ses ecailles rugueuses sont chaudes au toucher. De petits cristaux de roche ornent sa queue. Ses yeux sont gris comme la pierre.
- **Dinodex Entry** : "Rocalve adore se chauffer au soleil sur les rochers volcaniques. Sa peau de basalte absorbe la chaleur et la redistribue pendant la nuit froide."
- **Habitat** : Mont Ardoise / Champs de Lave
- **Rarete** : Peu commun
- **XP Group** : Lent

### #116 — BASALTEX
- **Types** : Roche
- **Stage** : Jeune (Ligne : Rocalve)
- **Evolution** : De Rocalve au niveau 24 / Evolue en Obsidianyx au niveau 38
- **Base Stats** : PV 70 / Atk 70 / Def 75 / AtkSp 40 / DefSp 55 / Vit 25 (Total : 335)
- **Abilities** : Peau Dure / Carapace (cachee)
- **Dino reel inspire** : Scutellosaurus
- **Design** : Dinosaure cuirasse au corps de basalte sombre avec des colonnes hexagonales naturelles sur son dos. Des veines de roche en fusion sont parfois visibles entre les plaques. Son armure est geometriquement parfaite.
- **Dinodex Entry** : "Basaltex est un geologue ne. Il choisit instinctivement les roches les plus dures pour renforcer son armure naturelle."
- **Habitat** : Champs de Lave / Colonne de Basalte
- **Rarete** : Peu commun
- **XP Group** : Lent

### #117 — OBSIDIANYX
- **Types** : Roche / Ombre
- **Stage** : Adulte (Ligne : Rocalve)
- **Evolution** : De Basaltex au niveau 38
- **Base Stats** : PV 90 / Atk 95 / Def 100 / AtkSp 50 / DefSp 65 / Vit 25 (Total : 425)
- **Abilities** : Peau Dure / Ombre Furtive (cachee)
- **Dino reel inspire** : Scelidosaurus geant + obsidienne
- **Design** : Grand dinosaure cuirasse dont le corps est entierement en obsidienne noire brillante. Sa surface reflete le monde environnant comme un miroir sombre. Des eclats d'obsidienne tranchants herissent son dos. Dans l'obscurite, il est presque invisible sauf pour ses yeux de braise rouge.
- **Dinodex Entry** : "Obsidianyx est un predateur embusque dont le corps d'obsidienne reflete son environnement. Les voyageurs ne le voient qu'au dernier moment, quand ses yeux rouges s'ouvrent."
- **Habitat** : Colonne de Basalte / Neant Obscur
- **Rarete** : Rare
- **XP Group** : Lent

---

## LIGNE 44 — MOSSAURE (Zones secretes — Plante/Eau)

### #118 — MOSSAURE
- **Types** : Plante
- **Stage** : Bebe (Ligne : Mossaure)
- **Evolution** : Evolue en Limnofern au niveau 20
- **Base Stats** : PV 50 / Atk 35 / Def 45 / AtkSp 50 / DefSp 45 / Vit 30 (Total : 255)
- **Abilities** : Vegetal / Nage Rapide (cachee)
- **Dino reel inspire** : Amargasaurus (juvenile)
- **Design** : Petit sauropode au corps couvert de mousse aquatique verte. Des nenuphars miniatures poussent sur son dos. Ses pattes courtes sont palmees. Il degage une odeur de foret humide.
- **Dinodex Entry** : "Mossaure vit a moitie dans l'eau, a moitie sur terre. La mousse sur son dos filtre l'eau des mares ou il se baigne."
- **Habitat** : Marais du Jurassique / Mangrove Ancestrale
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #119 — LIMNOFERN
- **Types** : Plante / Eau
- **Stage** : Jeune (Ligne : Mossaure)
- **Evolution** : De Mossaure au niveau 20 / Evolue en Paludex au niveau 34
- **Base Stats** : PV 70 / Atk 50 / Def 60 / AtkSp 65 / DefSp 60 / Vit 35 (Total : 340)
- **Abilities** : Vegetal / Nage Rapide (cachee)
- **Dino reel inspire** : Amargasaurus
- **Design** : Sauropode semi-aquatique dont les epines dorsales sont des roseaux vivants. Des fougeres aquatiques pendent de son cou. Son corps est vert-brun avec des reflets d'eau. Des poissons nagent dans les flaques sur son dos.
- **Dinodex Entry** : "Limnofern est l'ecosysteme ambulant des marais. Sa presence enrichit les zones humides et favorise la biodiversite aquatique."
- **Habitat** : Mangrove Ancestrale / Delta Boueux
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #120 — PALUDEX
- **Types** : Plante / Eau
- **Stage** : Adulte (Ligne : Mossaure)
- **Evolution** : De Limnofern au niveau 34
- **Base Stats** : PV 95 / Atk 65 / Def 80 / AtkSp 80 / DefSp 75 / Vit 30 (Total : 425)
- **Abilities** : Vegetal / Regeneration (cachee)
- **Dino reel inspire** : Amargasaurus geant + marais
- **Design** : Grand sauropode dont le corps est un marais vivant complet. Des arbres poussent sur son dos, des mares se forment entre ses vertebres, et des grenouilles coassent sur ses flancs. Son cou est une cascade de fougeres. Il se deplace comme une ile flottante.
- **Dinodex Entry** : "Paludex est un marais ambulant. Les ecologistes le suivent pour cartographier les ecosystemes, car partout ou il passe, la nature s'epanouit."
- **Habitat** : Delta Boueux / Grand Marecage
- **Rarete** : Rare
- **XP Group** : Moyen

---

## LIGNE 45 — OSSAREX (Zones secretes — Fossile)

### #121 — OSSAREX
- **Types** : Fossile
- **Stage** : Bebe (Ligne : Ossarex)
- **Evolution** : Evolue en Necrodonte au niveau 24
- **Base Stats** : PV 45 / Atk 55 / Def 45 / AtkSp 40 / DefSp 35 / Vit 35 (Total : 255)
- **Abilities** : Fossile Ancien / Machoire (cachee)
- **Dino reel inspire** : Herrerasaurus (fossilise)
- **Design** : Petit theropode dont la peau est si fine qu'on voit ses os fossilises en dessous. Des fragments d'os anciens depassent de sa peau. Ses yeux sont des ammonites fossilisees. Il a l'air fragile mais solide.
- **Dinodex Entry** : "Ossarex est un puzzle vivant de fossiles. Les paleontologues decouvrent regulierement de nouvelles especes en examinant les os que sa peau revele."
- **Habitat** : Site de Fouilles / Cimetiere des Titans
- **Rarete** : Rare
- **XP Group** : Moyen

### #122 — NECRODONTE
- **Types** : Fossile
- **Stage** : Jeune (Ligne : Ossarex)
- **Evolution** : De Ossarex au niveau 24 / Evolue en Paleorex au niveau 38
- **Base Stats** : PV 60 / Atk 75 / Def 60 / AtkSp 55 / DefSp 50 / Vit 40 (Total : 340)
- **Abilities** : Fossile Ancien / Machoire (cachee)
- **Dino reel inspire** : Theropode squelettique
- **Design** : Theropode dont la moitie du corps est exposee sous forme de squelette fossilise. L'autre moitie est couverte de peau d'ambre. Ses dents sont des fossiles d'anciennes creatures. Il se deplace avec une grace spectrale.
- **Dinodex Entry** : "Necrodonte est un pont entre les vivants et les fossiles. Chacun de ses os raconte l'histoire d'une creature disparue qu'il a absorbe."
- **Habitat** : Cimetiere des Titans / Strates Anciennes
- **Rarete** : Rare
- **XP Group** : Moyen

### #123 — PALEOREX
- **Types** : Fossile / Roche
- **Stage** : Adulte (Ligne : Ossarex)
- **Evolution** : De Necrodonte au niveau 38
- **Base Stats** : PV 85 / Atk 100 / Def 80 / AtkSp 65 / DefSp 60 / Vit 50 (Total : 440)
- **Abilities** : Fossile Ancien / Intimidation (cachee)
- **Dino reel inspire** : T-Rex fossilise vivant
- **Design** : Grand theropode dont le corps est un assemblage de fossiles de dizaines d'especes differentes. Son crane est celui d'un T-Rex reconstitue. Ses bras portent des os de pterosaures. Ses jambes sont en os de sauropode. C'est un monstre de Frankenstein paleontologique vivant.
- **Dinodex Entry** : "Paleorex est l'archive vivante de toute la prehistoire. Il porte les restes de creatures qui ont vecu des millions d'annees avant lui. Son rugissement est un echo de toutes les eres passees."
- **Habitat** : Strates Anciennes / Citadelle Fossile
- **Rarete** : Tres rare
- **XP Group** : Moyen

---

## LIGNE 46 — SIFFLEUR (Zones secretes — Sable/Ombre — Sans evolution)

### #124 — SIFFLEUR
- **Types** : Sable / Ombre
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 70 / Atk 80 / Def 55 / AtkSp 75 / DefSp 55 / Vit 100 (Total : 435)
- **Abilities** : Tempete de Sable / Ombre Furtive (cachee)
- **Dino reel inspire** : Ornithomimus + djinn du desert
- **Design** : Dinosaure svelte et fantomatique au corps sable-noir. Il se deplace comme un mirage, oscillant entre solide et ombre. Un sifflement constant emane de lui quand le vent passe a travers ses plumes spectrales. Ses yeux sont des trous noirs dans un visage de sable.
- **Dinodex Entry** : "Siffleur est un mirage vivant du desert. Les voyageurs l'entendent siffler avant de le voir, mais quand ils se retournent, il a deja disparu comme une ombre dans le sable."
- **Habitat** : Vortex du Desert / Temple du Desert
- **Rarete** : Tres rare
- **XP Group** : Rapide

---

## LIGNE 47 — TITANOX (Zones secretes — Roche/Terre)

### #125 — TITANOX
- **Types** : Roche
- **Stage** : Bebe (Ligne : Titanox)
- **Evolution** : Evolue en Colossaure au niveau 28
- **Base Stats** : PV 55 / Atk 50 / Def 60 / AtkSp 30 / DefSp 45 / Vit 20 (Total : 260)
- **Abilities** : Peau Dure / Armure Naturelle (cachee)
- **Dino reel inspire** : Titanosaurus (juvenile)
- **Design** : Petit sauropode trapu au corps de granit gris. Malgre sa taille modeste, il est incroyablement lourd. Ses pattes sont des colonnes de pierre. Des mineraux scintillent dans sa peau rugueuse.
- **Dinodex Entry** : "Titanox est si lourd pour sa taille que les balances ordinaires se cassent. Les dresseurs doivent renforcer le sol de leur maison avant de l'accueillir."
- **Habitat** : Mont Ardoise / Citadelle Fossile
- **Rarete** : Rare
- **XP Group** : Lent

### #126 — COLOSSAURE
- **Types** : Roche / Terre
- **Stage** : Jeune (Ligne : Titanox)
- **Evolution** : De Titanox au niveau 28 / Evolue en Megalithon au niveau 40 (le plus tardif du jeu)
- **Base Stats** : PV 80 / Atk 70 / Def 85 / AtkSp 40 / DefSp 60 / Vit 20 (Total : 355)
- **Abilities** : Peau Dure / Armure Naturelle (cachee)
- **Dino reel inspire** : Titanosaurus
- **Design** : Sauropode massif au corps de granit et de terre compactee. Des strates rocheuses sont visibles le long de son cou immense. Ses pas font trembler le sol. Des plantes rupestres poussent dans les crevasses de sa peau de pierre.
- **Dinodex Entry** : "Colossaure grandit extremement lentement mais sans jamais s'arreter. Les plus vieux specimens sont confondus avec des montagnes."
- **Habitat** : Citadelle Fossile / Sanctuaire des Origines
- **Rarete** : Rare
- **XP Group** : Lent

### #127 — MEGALITHON
- **Types** : Roche / Terre
- **Stage** : Adulte (Ligne : Titanox)
- **Evolution** : De Colossaure au niveau 40
- **Base Stats** : PV 120 / Atk 90 / Def 110 / AtkSp 50 / DefSp 75 / Vit 20 (Total : 465)
- **Abilities** : Peau Dure / Cuirasse (cachee)
- **Dino reel inspire** : Dreadnoughtus + megalithique
- **Design** : Sauropode titanesque dont le corps est un paysage montagneux vivant. Des formations megalithiques — menhirs et dolmens — poussent sur son dos. Son cou est une chaine de montagnes. Ses pattes sont des piliers tectoniques. Il est si grand que des nuages se forment autour de sa tete.
- **Dinodex Entry** : "Megalithon est la creature la plus massive jamais observee. Les civilisations anciennes construisaient leurs monuments sacres sur son dos endormi, croyant qu'il etait une montagne."
- **Habitat** : Sanctuaire des Origines / Sommet du Monde
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## LIGNE 48 — CHRONOX (Pre-legendaire — Fossile/Foudre — Pseudo-legendaire)

### #128 — CHRONOX
- **Types** : Fossile
- **Stage** : Bebe (Ligne : Chronox)
- **Evolution** : Evolue en Temporex au niveau 30
- **Base Stats** : PV 50 / Atk 45 / Def 50 / AtkSp 55 / DefSp 50 / Vit 50 (Total : 300)
- **Abilities** : Fossile Ancien / Velocite (cachee)
- **Dino reel inspire** : Creature du Precambrien
- **Design** : Petit dinosaure etheree dont le corps semble faire de cristaux temporels ambre. Des rouages fossilises sont visibles dans sa peau translucide. Ses yeux sont des sabliers miniatures. Le temps semble ralentir autour de lui.
- **Dinodex Entry** : "Chronox existe a la frontiere entre le passe et le present. Ses mouvements semblent decales dans le temps, comme s'il voyait quelques secondes dans le futur."
- **Habitat** : Faille Temporelle (evenement special)
- **Rarete** : Tres rare
- **XP Group** : Lent

### #129 — TEMPOREX
- **Types** : Fossile / Foudre
- **Stage** : Jeune (Ligne : Chronox)
- **Evolution** : De Chronox au niveau 30 / Evolue en Eternadon au niveau 50 (niveau max)
- **Base Stats** : PV 70 / Atk 60 / Def 65 / AtkSp 80 / DefSp 70 / Vit 65 (Total : 410)
- **Abilities** : Fossile Ancien / Velocite (cachee)
- **Dino reel inspire** : Dinosaure temporel
- **Design** : Theropode elegant dont le corps est un mecanisme d'horloge vivant. Des engrenages de cristal fossilise tournent dans sa peau translucide. Des arcs electriques temporels relient ses mouvements passes et futurs, creant des images fantomes autour de lui.
- **Dinodex Entry** : "Temporex manipule le flux du temps a une echelle locale. Les dresseurs rapportent que les combats contre lui semblent durer des heures en seulement quelques minutes."
- **Habitat** : Faille Temporelle
- **Rarete** : Tres rare
- **XP Group** : Lent

### #130 — ETERNADON
- **Types** : Fossile / Foudre
- **Stage** : Adulte (Ligne : Chronox)
- **Evolution** : De Temporex au niveau 50
- **Base Stats** : PV 95 / Atk 75 / Def 80 / AtkSp 110 / DefSp 85 / Vit 85 (Total : 530)
- **Abilities** : Fossile Ancien / Velocite (cachee)
- **Dino reel inspire** : Creature temporelle transcendante
- **Design** : Grand dinosaure spectaculaire dont le corps est un tourbillon de temps cristallise. Des scenes du passe et du futur sont visibles comme des hologrammes dans sa peau transparente. Des eclairs temporels le relient a toutes les epoques simultanement. Son regard contient l'infini.
- **Dinodex Entry** : "Eternadon transcende le temps lui-meme. On dit qu'il a assiste a la naissance et a la mort de chaque ere. Son pouvoir temporel est si grand qu'il pourrait remodeler l'histoire."
- **Habitat** : Faille Temporelle (apres la Ligue)
- **Rarete** : Legendaire
- **XP Group** : Lent

---

## LIGNE 49 — GARDIENS ELEMENTAIRES (Legendaires — Trio)

### #131 — PYRONYX
- **Types** : Feu / Fossile
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 90 / Atk 105 / Def 80 / AtkSp 95 / DefSp 75 / Vit 55 (Total : 500)
- **Abilities** : Flamme Vive / Fossile Ancien (cachee)
- **Dino reel inspire** : Spinosaurus legendaire + flamme primordiale
- **Design** : Spinosaure legendaire dont la voile dorsale est une muraille de flammes primordiales. Son corps est rouge magma avec des motifs de fossiles incandescents. Des flammes anciennes brulent dans ses orbites. Il degage une chaleur qui fait fondre la roche.
- **Dinodex Entry** : "Pyronyx est le Gardien du Feu Primordial, la premiere flamme qui a jailli de la Terre. Il protege le coeur volcanique du monde depuis l'aube des temps."
- **Habitat** : Sanctuaire du Feu (legendaire)
- **Rarete** : Legendaire
- **XP Group** : Lent

### #132 — GLACIORNYX
- **Types** : Glace / Fossile
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 90 / Atk 75 / Def 95 / AtkSp 105 / DefSp 90 / Vit 45 (Total : 500)
- **Abilities** : Gel Eternel / Fossile Ancien (cachee)
- **Dino reel inspire** : Sauropode legendaire de glace
- **Design** : Sauropode de glace legendaire dont le corps est un glacier vivant primordial. Des cristaux de glace anciens forment son armure. Son souffle cree des eres glaciaires. Des aurores boreales emanent de son corps. Son regard contient la sagesse des ages de glace.
- **Dinodex Entry** : "Glaciornyx est le Gardien du Froid Primordial. C'est lui qui a apporte les eres glaciaires pour equilibrer la chaleur du monde."
- **Habitat** : Sanctuaire de Glace (legendaire)
- **Rarete** : Legendaire
- **XP Group** : Lent

### #133 — GALVORNYX
- **Types** : Foudre / Fossile
- **Stage** : Adulte (Pas d'evolution)
- **Evolution** : Aucune
- **Base Stats** : PV 90 / Atk 80 / Def 75 / AtkSp 100 / DefSp 80 / Vit 75 (Total : 500)
- **Abilities** : Charge Statique / Fossile Ancien (cachee)
- **Dino reel inspire** : Raptor legendaire electrique
- **Design** : Grand raptor legendaire dont le corps est un reseau de foudre primordiale fossilisee. Des eclairs permanents parcourent ses plumes d'ambre electrique. Ses griffes sont des eclairs solidifies. L'ozone emane de lui en permanence. Son cri est un coup de tonnerre.
- **Dinodex Entry** : "Galvornyx est le Gardien de la Foudre Primordiale. C'est lui qui a allume la premiere etincelle de vie dans les oceans primitifs."
- **Habitat** : Sanctuaire de la Foudre (legendaire)
- **Rarete** : Legendaire
- **XP Group** : Lent

---

## LIGNE 50 — LIGNES FINALES (#134-#150)

### #134 — AQUAVEIL
- **Types** : Eau
- **Stage** : Bebe (Ligne : Aquaveil)
- **Evolution** : Evolue en Tidalos au niveau 18
- **Base Stats** : PV 50 / Atk 40 / Def 40 / AtkSp 50 / DefSp 45 / Vit 40 (Total : 265)
- **Abilities** : Torrent / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Elasmosaurus (juvenile)
- **Design** : Petit plesiosaure au corps bleu pastel avec un voile d'eau translucide qui l'entoure comme une cape. Son long cou est gracieux et fin. Ses nageoires sont douces et arrondies.
- **Dinodex Entry** : "Aquaveil se cache sous un voile d'eau qui le rend presque invisible dans les lacs. Les pecheurs pensent voir un simple remous quand il passe."
- **Habitat** : Lac du Cretace / Riviere Azur
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #135 — TIDALOS
- **Types** : Eau
- **Stage** : Jeune (Ligne : Aquaveil)
- **Evolution** : De Aquaveil au niveau 18 / Evolue en Tsunamex au niveau 34
- **Base Stats** : PV 65 / Atk 55 / Def 55 / AtkSp 70 / DefSp 60 / Vit 50 (Total : 355)
- **Abilities** : Torrent / Ecailles Epaisses (cachee)
- **Dino reel inspire** : Elasmosaurus
- **Design** : Plesiosaure elegant au corps bleu profond avec des motifs de vagues. Des courants d'eau tourbillonnent autour de ses nageoires. Son cou se dresse avec majeste au-dessus des flots.
- **Dinodex Entry** : "Tidalos controle les marees avec les mouvements de ses nageoires. Les ports de peche prosperent la ou il elut domicile."
- **Habitat** : Riviere Azur / Ocean Primordial
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #136 — TSUNAMEX
- **Types** : Eau / Terre
- **Stage** : Adulte (Ligne : Aquaveil)
- **Evolution** : De Tidalos au niveau 34
- **Base Stats** : PV 95 / Atk 70 / Def 75 / AtkSp 90 / DefSp 75 / Vit 40 (Total : 445)
- **Abilities** : Torrent / Peau Dure (cachee)
- **Dino reel inspire** : Elasmosaurus geant + tsunami
- **Design** : Plesiosaure colossal dont la presence provoque des vagues immenses. Son corps bleu-noir est couvert de sediments marins. Des coraux et rochers se sont agglomeres sur son ventre. Ses mouvements dans l'eau generent des courants sous-marins puissants.
- **Dinodex Entry** : "Tsunamex est la force des oceans incarnee. Ses plongees profondes provoquent des tsunamis, et les civilisations cotieres l'ont autrefois venere et craint."
- **Habitat** : Ocean Primordial / Fosse Abyssale
- **Rarete** : Rare
- **XP Group** : Moyen

### #137 — LUMIVORE
- **Types** : Lumiere
- **Stage** : Bebe (Ligne : Lumivore)
- **Evolution** : Evolue en Solardonte au niveau 22
- **Base Stats** : PV 40 / Atk 35 / Def 35 / AtkSp 55 / DefSp 50 / Vit 50 (Total : 265)
- **Abilities** : Aura Lumineuse / Regeneration (cachee)
- **Dino reel inspire** : Jeholosaurus (juvenile)
- **Design** : Petit dinosaure bipede au corps dore clair qui emet une lumiere chaude. Des motifs en forme de soleil ornent ses flancs. Ses yeux sont doux et lumineux comme des lanternes.
- **Dinodex Entry** : "Lumivore mange la lumiere du soleil pour grandir. Les jours nuageux le rendent triste et faible, tandis que les jours ensoleilles le remplissent d'energie."
- **Habitat** : Prairie Lumineuse / Sanctuaire Solaire
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #138 — SOLARDONTE
- **Types** : Lumiere
- **Stage** : Jeune (Ligne : Lumivore)
- **Evolution** : De Lumivore au niveau 22 / Evolue en Novaflare au niveau 38
- **Base Stats** : PV 55 / Atk 50 / Def 50 / AtkSp 75 / DefSp 65 / Vit 55 (Total : 350)
- **Abilities** : Aura Lumineuse / Regeneration (cachee)
- **Dino reel inspire** : Jeholosaurus
- **Design** : Dinosaure bipede gracieux au corps d'or pur qui brille d'une lumiere solaire intense. Des courbes de lumiere tracent des motifs spirales sur sa peau. Une aura chaude et reconfortante emane de lui.
- **Dinodex Entry** : "Solardonte stocke tellement de lumiere que meme la nuit, il brille comme un petit soleil. Les villages proches de son habitat n'ont jamais besoin de torches."
- **Habitat** : Sanctuaire Solaire / Temple de l'Aube
- **Rarete** : Peu commun
- **XP Group** : Moyen

### #139 — NOVAFLARE
- **Types** : Lumiere / Feu
- **Stage** : Adulte (Ligne : Lumivore)
- **Evolution** : De Solardonte au niveau 38
- **Base Stats** : PV 80 / Atk 65 / Def 65 / AtkSp 105 / DefSp 85 / Vit 60 (Total : 460)
- **Abilities** : Aura Lumineuse / Flamme Vive (cachee)
- **Dino reel inspire** : Jeholosaurus geant + nova stellaire
- **Design** : Grand dinosaure dont le corps est une etoile miniature vivante. Des eruptions de lumiere solaire jaillissent de sa peau doree. Sa temperature de surface est si elevee que l'air se deforme autour de lui. Il brille d'une lumiere si intense qu'il est difficile de le regarder directement.
- **Dinodex Entry** : "Novaflare est un soleil en miniature. Sa lumiere atteint des temperatures stellaires, et les scientifiques l'etudient pour comprendre les reactions thermonucleaires."
- **Habitat** : Zenith Dore / Temple de l'Aube
- **Rarete** : Tres rare
- **XP Group** : Moyen

### #140 — TERRADAX
- **Types** : Terre / Sable
- **Stage** : Bebe (Ligne : Terradax)
- **Evolution** : Evolue en Erosidon au niveau 24
- **Base Stats** : PV 50 / Atk 50 / Def 50 / AtkSp 35 / DefSp 40 / Vit 35 (Total : 260)
- **Abilities** : Tempete de Sable / Peau Dure (cachee)
- **Dino reel inspire** : Protoceratops andrewsi (juvenile)
- **Design** : Petit ceratopsien au corps de terre et de sable melange, comme de l'argile crue. Sa collerette est un eventail de terre sechee craquelle. Ses pattes creusent instinctivement le sol.
- **Dinodex Entry** : "Terradax change de couleur selon le sol ou il vit — brun dans les plaines, rouge dans les canyons, dore dans le desert."
- **Habitat** : Desert du Permien / Plaines de Boue
- **Rarete** : Commun
- **XP Group** : Moyen

### #141 — EROSIDON
- **Types** : Terre / Sable
- **Stage** : Jeune (Ligne : Terradax)
- **Evolution** : De Terradax au niveau 24 / Evolue en Geomantyx au niveau 38
- **Base Stats** : PV 70 / Atk 70 / Def 65 / AtkSp 50 / DefSp 55 / Vit 40 (Total : 350)
- **Abilities** : Tempete de Sable / Peau Dure (cachee)
- **Dino reel inspire** : Protoceratops andrewsi
- **Design** : Ceratopsien robuste dont le corps est un paysage d'erosion. Des arches naturelles de gres se forment sur son dos. Sa collerette est sculptee par le vent en formations spectaculaires. Du sable coule entre ses plaques.
- **Dinodex Entry** : "Erosidon sculpte le paysage malgre lui. Les formations rocheuses spectaculaires du desert sont souvent le resultat de ses passages repetes."
- **Habitat** : Canyon Ecarlate / Plateau Brise
- **Rarete** : Commun
- **XP Group** : Moyen

### #142 — GEOMANTYX
- **Types** : Terre / Sable
- **Stage** : Adulte (Ligne : Terradax)
- **Evolution** : De Erosidon au niveau 38
- **Base Stats** : PV 95 / Atk 95 / Def 85 / AtkSp 60 / DefSp 70 / Vit 45 (Total : 450)
- **Abilities** : Tempete de Sable / Intimidation (cachee)
- **Dino reel inspire** : Grand ceratopsien + geomancie
- **Design** : Grand ceratopsien majestueux dont le corps est un monument geologique vivant. Des formations terrestres spectaculaires — arches, piliers, mesas — ornent son dos. Sa collerette est une carte topographique vivante du monde. Il commande la terre et le sable avec une autorite absolue.
- **Dinodex Entry** : "Geomantyx est le maitre de la terre et du sable. Il peut remodeler les paysages d'un coup de patte, creant des montagnes et des vallees a volonte."
- **Habitat** : Vortex du Desert / Sommet du Monde
- **Rarete** : Rare
- **XP Group** : Moyen

### #143 — VENIVOLT
- **Types** : Poison / Foudre
- **Stage** : Bebe (Ligne : Venivolt)
- **Evolution** : Evolue en Toxeclair au niveau 26
- **Base Stats** : PV 40 / Atk 45 / Def 35 / AtkSp 55 / DefSp 40 / Vit 55 (Total : 270)
- **Abilities** : Venin / Charge Statique (cachee)
- **Dino reel inspire** : Sinosauropteryx (juvenile)
- **Design** : Petit theropode a plumes jaune-vert electrique. Des etincelles empoisonnees creptitent entre ses plumes toxiques. Son bec est charge de venin electrifie. Il est nerveux et impredictible.
- **Dinodex Entry** : "Venivolt est un danger ambulant : son venin est electrifie. Une seule piqure provoque a la fois un empoisonnement et une paralysie electrique."
- **Habitat** : Marais Toxiques / Steppe Electrique
- **Rarete** : Rare
- **XP Group** : Rapide

### #144 — TOXECLAIR
- **Types** : Poison / Foudre
- **Stage** : Jeune (Ligne : Venivolt)
- **Evolution** : De Venivolt au niveau 26 / Evolue en Fulguvenin au niveau 40
- **Base Stats** : PV 55 / Atk 60 / Def 50 / AtkSp 75 / DefSp 55 / Vit 70 (Total : 365)
- **Abilities** : Venin / Charge Statique (cachee)
- **Dino reel inspire** : Sinosauropteryx
- **Design** : Theropode agile aux plumes vert acide striees d'eclairs jaunes. Des arcs electriques toxiques relient ses plumes. Son regard est calculateur et dangereux. Un halo de brume electrique empoisonnee l'entoure.
- **Dinodex Entry** : "Toxeclair combine la foudre et le poison dans des attaques devastatrices. Les dresseurs qui le maitrisent sont craints dans les tournois."
- **Habitat** : Steppe Electrique / Bassin Acide
- **Rarete** : Rare
- **XP Group** : Rapide

### #145 — FULGUVENIN
- **Types** : Poison / Foudre
- **Stage** : Adulte (Ligne : Venivolt)
- **Evolution** : De Toxeclair au niveau 40
- **Base Stats** : PV 75 / Atk 80 / Def 60 / AtkSp 100 / DefSp 70 / Vit 85 (Total : 470)
- **Abilities** : Venin / Velocite (cachee)
- **Dino reel inspire** : Grand theropode electro-toxique
- **Design** : Grand theropode dont le corps est un reacteur de venin electrique. Des eclairs vert poison parcourent ses plumes herissees. Ses griffes suintent d'un venin qui crepite d'electricite. L'air autour de lui est charge d'ozone toxique. Son cri provoque des decharges empoisonnees.
- **Dinodex Entry** : "Fulguvenin est la fusion parfaite du poison et de la foudre. Son venin electrique est si puissant qu'il peut paralyser et empoisonner simultanement un adversaire dix fois plus grand."
- **Habitat** : Sommet de l'Orage / Lac Corrosif
- **Rarete** : Tres rare
- **XP Group** : Rapide

### #146 — DRACONYX
- **Types** : Ombre / Feu
- **Stage** : Bebe (Ligne : Draconyx — Pseudo-legendaire)
- **Evolution** : Evolue en Wyverion au niveau 30
- **Base Stats** : PV 50 / Atk 55 / Def 45 / AtkSp 55 / DefSp 45 / Vit 50 (Total : 300)
- **Abilities** : Ombre Furtive / Flamme Vive (cachee)
- **Dino reel inspire** : Yi qi (dinosaure aile a ailes de chauve-souris)
- **Design** : Petit dinosaure draconique aux ailes membraneuses sombres bordees de flammes. Son corps est noir avec des veines de feu. Ses yeux sont rouge rubis incandescents. Sa queue est un fouet de flammes noires.
- **Dinodex Entry** : "Draconyx est le seul dinosaure qui ressemble a un dragon des legendes. Les anciens peuples ont cree les mythes draconiques en observant ses ancetres."
- **Habitat** : Abime Corrompue / Spire Draconique (zone post-jeu)
- **Rarete** : Tres rare
- **XP Group** : Lent

### #147 — WYVERION
- **Types** : Ombre / Feu
- **Stage** : Jeune (Ligne : Draconyx — Pseudo-legendaire)
- **Evolution** : De Draconyx au niveau 30 / Evolue en Tyranombre au niveau 50 (niveau max)
- **Base Stats** : PV 70 / Atk 75 / Def 60 / AtkSp 80 / DefSp 60 / Vit 65 (Total : 410)
- **Abilities** : Ombre Furtive / Flamme Vive (cachee)
- **Dino reel inspire** : Yi qi (adulte) + wyverne
- **Design** : Dinosaure draconique de taille moyenne aux ailes noires enflammees. Son corps sombre est parcouru de rivieres de feu. Sa crete est une couronne de flammes noires. Il plane dans les cieux comme une ombre ardente.
- **Dinodex Entry** : "Wyverion chasse dans les cieux nocturnes, fondant sur ses proies comme une etoile filante sombre. Son feu noir est plus chaud que les flammes ordinaires."
- **Habitat** : Spire Draconique
- **Rarete** : Tres rare
- **XP Group** : Lent

### #148 — TYRANOMBRE
- **Types** : Ombre / Feu
- **Stage** : Adulte (Ligne : Draconyx — Pseudo-legendaire)
- **Evolution** : De Wyverion au niveau 50
- **Base Stats** : PV 95 / Atk 100 / Def 75 / AtkSp 105 / DefSp 75 / Vit 80 (Total : 530)
- **Abilities** : Ombre Furtive / Instinct Predateur (cachee)
- **Dino reel inspire** : Grand dinosaure draconique mythique
- **Design** : Dinosaure draconique titanesque dont le corps est un chaos de tenebres et de flammes infernales. Ses immenses ailes membraneuses sont des voiles de nuit enflammee. Son corps noir absorbe la lumiere, mais des fissures de feu revelent un interieur de magma sombre. Ses yeux sont des supernovae rouges dans un visage de tenebres. Son rugissement eclaire la nuit de flammes noires.
- **Dinodex Entry** : "Tyranombre est la creature la plus redoutee du monde. Mi-ombre mi-flamme, il incarne la terreur primordiale. Les legendes disent qu'il est le gardien de la frontiere entre le monde des vivants et l'au-dela."
- **Habitat** : Spire Draconique / Neant Obscur
- **Rarete** : Tres rare
- **XP Group** : Lent

---

## PANGAEON — Le Legendaire Ultime

### #149 — PROTOVIE
- **Types** : Fossile / Lumiere
- **Stage** : Bebe (Ligne : speciale, evenement)
- **Evolution** : Evolue en Pangaeon au niveau 50 (Evolution speciale : necessite les trois Reliques Primordiales)
- **Base Stats** : PV 60 / Atk 50 / Def 55 / AtkSp 60 / DefSp 55 / Vit 50 (Total : 330)
- **Abilities** : Fossile Ancien / Aura Lumineuse (cachee)
- **Dino reel inspire** : Premiere forme de vie complexe
- **Design** : Petite creature primordiale dont le corps est un cristal de lumiere fossilisee. On distingue a l'interieur les formes de toutes les creatures qui ont jamais existe. Son corps pulse d'une lumiere douce et ancienne. Il est fragile et etheree.
- **Dinodex Entry** : "Protovie est le germe de toute vie. Il contient en lui le potentiel de chaque creature qui a jamais foule la Terre. Le proteger, c'est proteger l'avenir."
- **Habitat** : Sanctuaire des Origines (evenement post-jeu)
- **Rarete** : Legendaire
- **XP Group** : Lent

### #150 — PANGAEON
- **Types** : Fossile / Lumiere
- **Stage** : Adulte (Ligne : Protovie)
- **Evolution** : De Protovie au niveau 50 avec les trois Reliques Primordiales
- **Base Stats** : PV 100 / Atk 90 / Def 90 / AtkSp 100 / DefSp 90 / Vit 80 (Total : 550)
- **Abilities** : Fossile Ancien / Aura Lumineuse (cachee)
- **Dino reel inspire** : Pangee + toute la vie prehistorique
- **Design** : Creature legendaire majestueuse dont le corps est la Terre elle-meme en miniature. Des continents fossilises forment son armure, des oceans de lumiere coulent entre ses plaques tectoniques. Toutes les eres geologiques sont representees sur sa peau. Des hologrammes de creatures disparues apparaissent dans son aura. Il est a la fois ancien et eternel, portant l'histoire entiere de la vie sur ses epaules. Sa presence inspire un respect absolu.
- **Dinodex Entry** : "Pangaeon est l'incarnation de la Terre primordiale, la Pangee vivante. Il porte l'histoire de toute la vie, de la premiere cellule au dernier dinosaure. Le rencontrer, c'est contempler l'infini de l'evolution elle-meme."
- **Habitat** : Sommet du Monde (evenement unique post-Ligue)
- **Rarete** : Legendaire
- **XP Group** : Lent

---

## Annexes

### Recapitulatif par type primaire

| Type | Lignes | Nombre de dinos |
|------|--------|-----------------|
| Feu | Pyrex, Ignitops, Embrasaure, Magmax | 11 |
| Eau | Aquadon, Ondivore, Aquathorn, Coralox, Aquaveil | 14 |
| Plante | Florasaur, Fernex, Verdox, Racivore, Mossaure | 14 |
| Roche | Caillex, Stalagmor, Rocalve, Titanox | 12 |
| Vol | Plumex, Zephyros, Pteracier, Crestail | 12 |
| Terre | Terravore, Bourbex, Terrops, Terradax | 12 |
| Glace | Givrex, Glaciaire | 6 |
| Foudre | Fulgure, Fulgorex | 6 |
| Poison | Toxidon, Nocturex, Corrodex, Venivolt | 11 |
| Acier | Forgeron, Blindosaure | 6 |
| Ombre | Ombrix, Ombrasaure, Draconyx | 9 |
| Lumiere | Solaurex, Luminex, Borealis, Lumivore | 11 |
| Sable | Dunex, Sahlix, Sablonix | 8 |
| Fossile | Relicor, Ossarex, Chronox, Gardiens, Protovie | 11 |
| **Multi (sans evolution)** | Epinox, Mythorex, Voltaile, Siffleur | 4 |

### Dinos sans evolution (4)
- #088 Epinox (Plante/Poison)
- #089 Mythorex (Fossile/Ombre)
- #090 Voltaile (Foudre/Vol)
- #124 Siffleur (Sable/Ombre)

### Dinos a 2 stades (6)
- Luminex (#073-074) — Pierre d'Aube
- Embrasaure (#075-076) — Lieu Caldeira Ardente
- Coralox (#086-087) — Pierre Corail
- Racivore (#091-092) — Amitie max
- Sablonix (#093-094) — Pierre de Sable
- Borealis (#104-105) — Nuit + Amitie

### Legendaires (7 dinos legendaires)
- #130 Eternadon (Pseudo-legendaire, total 530)
- #131 Pyronyx (Gardien du Feu, total 500)
- #132 Glaciornyx (Gardien de la Glace, total 500)
- #133 Galvornyx (Gardien de la Foudre, total 500)
- #148 Tyranombre (Pseudo-legendaire, total 530)
- #149 Protovie (Pre-evolution legendaire)
- #150 Pangaeon (Legendaire ultime, total 550)

### Evolution speciales
| Dino | Condition |
|------|-----------|
| Luminex → Aurorex | Pierre d'Aube |
| Embrasaure → Infernadon | Niveau 32 dans la Caldeira Ardente |
| Coralox → Recifadon | Pierre Corail |
| Racivore → Sequoiax | Amitie maximale + niveau 35 |
| Sablonix → Sphinx | Pierre de Sable |
| Borealis → Celestiax | Nuit + amitie elevee |
| Protovie → Pangaeon | Niveau 50 + trois Reliques Primordiales |

---

*Document redige par NOVA (Concept Artist) et MARCUS (System Designer) pour Nova Forge Studio.*
*Version 1.0 — Mars 2026*
