# Rody Collection - Guide du Joueur

> L'ultime collection des Rody & Mastico : jouez a tous les jeux originaux, creez vos propres histoires et decouvrez les secrets caches.

**Jouer en ligne** : https://lacrearthur.github.io/rody-collection/

---

## C'est quoi Rody Collection ?

Rody Collection est une recreation fidele des jeux d'aventure educatifs **Rody et Mastico** sortis sur Atari ST par Lankhor (1988-1991). Cette collection comprend :

- **Les 6 Rody & Mastico originaux** (I a VI)
- **Rody Noel** - Le special de Noel
- **Rody a Ibiza** - Une aventure fan-made
- **DOOMastico** - Un mini-jeu bonus style DOOM
- **Rody Maker** - Un editeur de niveaux complet pour creer vos propres histoires

### Pourquoi Rody Collection ?

Vous pourriez telecharger l'emulateur Atari ST et les ROMs sur atarimania et jouer aux Rody simplement. Mais Rody Collection c'est quand meme vachement plus simple :

- **Acces simple** - Pas besoin de configurer un emulateur
- **Multi-plateforme** - Windows, macOS et navigateur WebGL
- **Editeur integre** - Modifiez n'importe quelle scene ou creez vos propres jeux
- **Objectifs bonus** - Chaque scene a un objectif cache "New Game+"
- **Controles modernes** - Souris, clavier et tactile

C'est un logiciel fait sur Unity, compatible sur toutes les plateformes et super simple d'acces. On a acces aux differents Rody et Mastico tres rapidement et surtout il y a l'integration de Rody Maker qui permet de modifier toutes les scenes, de rajouter des scenes, de rajouter des objectifs, de tout customiser et de creer ses propres jeux.

---

## Controles

### Navigation dans les menus

| Touche | Action |
|--------|--------|
| **Souris** | Cliquer pour selectionner, glisser pour defiler |
| **Fleches** (Gauche/Droite) | Naviguer entre les jeux |
| **Entree** | Confirmer la selection |
| **Echap** | Retourner au menu principal |
| **Double-clic** | Lancer le jeu selectionne |

### En jeu

| Touche | Action |
|--------|--------|
| **Clic** | Interagir / Trouver des objets |
| **Clic n'importe ou** | Passer les dialogues |
| **Livre ferme (3e bouton)** | Retourner au menu Rody Collection |
| **Pinceau** | Entrer dans Rody Maker |

### Mode DOOMastico

| Touche | Action |
|--------|--------|
| **Z / S** | Avancer / Reculer |
| **Q / D** | Se deplacer a gauche / droite |
| **Souris** | Regarder autour / Viser |

---

## Comment jouer

### Le gameplay

Rody & Mastico est un jeu d'aventure point-and-click ou vous aidez Rody et son ami Mastico a trouver des objets caches dans chaque scene.

1. **Regardez l'introduction** - Mastico raconte l'histoire avec la synthese vocale
2. **Trouvez l'objet principal** - Cliquez sur la bonne zone decrite par Mastico
3. **Trouvez l'objet secondaire (NGP)** - "Near Game Piece" pour des points bonus
4. **Trouvez le troisieme objet (FSW)** - "Final Scene Win" - l'objectif bonus New Game+
5. **Passez a la scene suivante** - Completez l'histoire !

### Astuces

- Ecoutez bien les indices de Mastico
- Les objets sont caches dans des zones cliquables specifiques
- Chaque scene a 3 objectifs possibles a trouver
- L'objectif bonus (FSW) est disponible dans chaque scene de chaque jeu

---

## Menu de selection des jeux

Le menu principal affiche tous les jeux disponibles dans un carrousel defilant. Je me suis inspire du menu des Mines pour faire ce menu-la. On peut le deplacer a la souris ou aux fleches.

### Jeux disponibles

| Jeu | Description |
|-----|-------------|
| **Rody et Mastico I** | L'aventure originale (1988) |
| **Rody et Mastico II** | Deuxieme chapitre |
| **Rody et Mastico III** | Troisieme chapitre |
| **Rody et Mastico IV** | Rody Noel - Special de Noel |
| **Rody et Mastico V** | Cinquieme aventure (assez style !) |
| **Rody et Mastico VI** | Le chapitre final |
| **Rody a Ibiza** | Aventure fan-made par la communaute |
| **DOOMastico** | Jeu bonus style DOOM |

### Actions du menu

- **Selectionner un jeu** - Simple clic sur la couverture
- **Jouer a un jeu** - Double-clic sur la couverture
- **Creer un nouveau jeu** - Cliquer sur le bouton "+"
- **Importer un jeu** - Cliquer sur le bouton dossier
- **Editer un jeu** - Entrer dans le jeu, puis cliquer sur le pinceau

---

## Creer et partager des jeux

### Creer un nouveau jeu

Je voulais vraiment simplifier le systeme d'importation et de creation de jeux. Rody Maker etait un peu complique pour selectionner le bon dossier de jeu qui devait avoir les bons dossiers dedans. Du coup j'ai simplifie ca avec deux boutons :

1. Cliquez sur le bouton **"Creer nouveau"** dans le menu
2. Entrez un titre pour votre jeu (par exemple "Le Manoir de Mortevielle" pour la reference a un autre jeu de Lankhor !)
3. Choisissez une image de couverture (celle qui apparaitra dans le menu)
4. Mastico confirme que le jeu a bien ete cree
5. Vous etes automatiquement emmene dans Rody Maker pour commencer l'edition

Et quand on clique sur enregistrer, ca va directement enregistrer dans le jeu qu'on vient de creer. On n'a plus besoin de se compliquer la vie a manipuler les dossiers a la main !

### Importer des jeux

1. Cliquez sur le bouton **"Importer"** (icone dossier)
2. Selectionnez un dossier de jeu ou un fichier `.rody.json`
3. Le jeu apparait dans votre collection

### Partager vos jeux

Les jeux sont stockes dans le dossier `StreamingAssets/` :

1. Allez dans `RodyCollection/StreamingAssets/`
2. Trouvez votre dossier de jeu (ex: "Ma Nouvelle Histoire")
3. Partagez le dossier entier avec d'autres personnes
4. Ils peuvent l'importer avec le bouton Importer

**Ou utilisez le format portable :**
- Exportez en fichier `.rody.json` (inclut toutes les images en base64)
- Un seul fichier, facile a partager par email ou cloud

### Partager vos histoires

Si vous faites des histoires, des nouvelles histoires, dites-le moi sur Twitter, dites-le moi en commentaires YouTube, dites-le moi sur Facebook, n'importe ! Et si les histoires sont vraiment bien, je les integrerai directement dans le jeu. Comme ca toutes les prochaines personnes qui le telechargent auront acces a votre jeu.

On peut aussi faire un Dropbox/Google Drive collaboratif ou on met tous nos dossiers et comme ca on peut telecharger les dossiers des autres facilement.

---

## Secrets caches

La collection contient plusieurs easter eggs et fonctionnalites cachees :
- Des interactions secretes dans le menu principal
- Des objectifs bonus dans chaque scene
- Des references cachees a d'autres jeux de Lankhor

*Je vais pas vous en dire trop, je vous laisserai decouvrir les petits secrets !*

---

## Informations techniques

### Resolution
- **Native** : 320x200 (mode basse resolution Atari ST)
- **Affichage** : 960x600 (mise a l'echelle 3x)
- Le plein ecran preserve le ratio 16:10 avec des bandes noires

Je vous deconseille le mode plein ecran car le jeu ne fonctionne que sur les resolutions en 16:10.

### Synthese vocale par phonemes

Le jeu utilise un systeme TTS personnalise qui recree la synthese vocale de l'Atari ST en concatenant des phonemes francais pre-enregistres. Ca donne le vrai feeling retro des jeux originaux.

---

## Credits

**Rody Collection** developpe par Arthur Scheidel (LaCreArthur / Bretzel Studio)

- **Code & UI Design** : Arthur Scheidel
- **Rody a Ibiza** : Arthur Scheidel, Rose Luxey, Guillaume Fleck
- **Rody & Mastico originaux** : Lankhor (1988-1991)

Ca represente environ trois ans de travail depuis Rody a Ibiza avec Rody Maker et jusqu'a Rody Collection. C'est quelque chose dont je suis assez fier !

### Note legale

C'est un projet de fan. Le nom et les assets de Rody & Mastico appartiennent aux createurs originaux. Je suis desole aux createurs des Rody & Mastico d'avoir utilise leurs noms et leurs assets un peu avant de leur avoir vraiment demande leur avis. Mais c'est pour ca que le projet reste gratuit et open source.

---

## Liens

- **Jouer en ligne** : https://lacrearthur.github.io/rody-collection/
- **Telecharger** : https://lacrearthur.itch.io/rody-mastico-collection
- **Code source** : https://github.com/LaCreArthur/rody-collection
- **Tutoriel Rody Maker** : Voir [RODY_MAKER_TUTORIAL.md](RODY_MAKER_TUTORIAL.md)

---

## Soutenir le projet

Rody Collection est gratuit parce que je n'ai simplement pas le droit de le vendre puisque j'utilise le nom et les assets de la serie Rody & Mastico qui ne m'appartiennent pas. Les donations sont le seul moyen de me soutenir et de me remercier pour ce travail.

Si le projet vous plait :
- **Donnez** ce que vous voulez sur itch.io
- **Partagez** le jeu avec vos amis
- **Creez** et partagez vos propres histoires
- **Signalez** les bugs et suggerez des fonctionnalites

Plus on sera nombreux a creer et partager des histoires, plus ce sera drole. Il y aura plein d'histoires a jouer, plein de choses a decouvrir !
