# Rody Maker — Créer et partager une histoire

Guide de la version en préparation, relu le **14 septembre 2026**. Les fonctions
récentes restent à vérifier dans une version navigateur publiée. Les anciennes
vidéos et captures ne décrivent plus la sauvegarde ni le nouvel atelier vocal.
Le [document de conception](GAME_DESIGN.md) explique le produit et distingue
les règles de création et de sauvegarde de cette version.

## Commencer

- **Nouveau :** dans la collection, choisissez la création d'histoire, entrez un
  titre et, si vous le souhaitez, importez une couverture. Une scène est créée.
- **Modifier un original :** sélectionnez sa couverture, puis **Dupliquer**.
  La copie devient **Mon histoire**, votre seul espace personnel de création.
- **Reprendre :** sélectionnez **Mon histoire**, puis **Éditer**.
- **Importer :** utilisez **Importer** et choisissez un fichier `.rody.json`.
  Les anciens dossiers de jeu ne sont pas le format d'import actuel.

Créer, dupliquer ou importer remplace **Mon histoire**. Si vous l'avez modifiée,
choisissez **Enregistrer**, **Ne pas enregistrer** ou **Annuler**. Enregistrer
télécharge d'abord l'histoire actuelle ; Annuler la garde ouverte. Un import
annulé ou illisible conserve aussi votre travail. Jouer un original ne le remplace
pas, et le pinceau du jeu protège les originaux de la même manière que Dupliquer.

## Se repérer dans l'éditeur

Le titre et les textes occupent le panneau du jeu original. Cliquez directement
sur le titre ou une réplique pour l’éditer. Le survol éclaire le texte et change
le curseur ; un petit repère latéral indique le passage sélectionné. Les trois répliques d’introduction
restent affichées ensemble ; celle que vous éditez est surlignée.

| Outil | Usage |
|---|---|
| **Scènes** | Ouvrir les vignettes (titres en infobulle) ; les flèches choisissent la scène précédente/suivante |
| **Musique** | Choisir et écouter les pistes de la scène |
| **Images** | Image principale et images d’animation |
| **INTRO 1 2 3** | Sélectionner une réplique, même encore vide |
| **OBJETS 1 2 3** | Montrer ensemble l’indice et les zones de l’objectif |
| **Disquette / Enregistrer** | Télécharger l’histoire complète |
| **Test** | Jouer le brouillon actuel sans l’enregistrer |
| **Rétablir** | Restaurer toute l’histoire à son point de reprise |

L’image de titre est distincte de la couverture du menu. Elle n’a pas de dialogue,
d’objectif ni de musique de scène. **+ Scène** permet d’ajouter jusqu’à29 scènes.
La suppression reste réservée aux scènes à partir de la18e : sélectionnez la scène,
rouvrez Scènes et utilisez sa croix, puis confirmez.

## Images

Préparez les images dans votre outil de dessin, puis importez-les avec **IMG**.
Utilisez **320 × 130** pour une scène, **320 × 200** pour le titre ou la couverture,
et la [palette Rody](bonus/paletteRody.png) pour prévoir les couleurs du résultat.
L'import ajuste les dimensions et les couleurs ; aucun nom de fichier spécial
n'est nécessaire.

L'image principale représente la scène au repos. Les images d'animation montrent
les personnages lorsqu'ils parlent. L'éditeur présente deux groupes de trois
images. Importez une image existante pour la remplacer, ou la prochaine position
vide pour ajouter une image. Les commandes de prévisualisation et **Retirer**
concernent l'image correspondante. Enregistrer garde cette séquence sans ajouter
de copies de l'image principale.

## Introduction, texte et musique

Cliquez sur le titre ou une réplique pour écrire. Les sélecteurs **INTRO 1 2 3**
donnent aussi accès aux passages vides. **Terminer**, **Retour** ou Échap gardent
le texte saisi. La voix suit le texte : chaque modification refait la voix de la
réplique. Le bouton du personnage passe de Mastico à cinq hauteurs de voix d’un
personnage du décor, puis revient à Mastico, et fait écouter la réplique.

Le texte doit tenir dans le panneau fixe, retours à la ligne compris. Si vous
collez trop de texte, seule la partie qui tient est insérée ; les autres passages
et le texte autour de votre sélection sont conservés. Un bref message explique
la coupe. Aucun défilement ni réduction de police ne masque un débordement.
La police rétro contient les lettres françaises et la ponctuation courante ;
relisez l’aperçu si vous utilisez un signe rare.

Pour la musique, **L1** choisit l'ouverture et **L2** la musique répétée.
Le bouton d'écoute permet de préécouter les pistes fournies.

## Corriger la prononciation

La voix est créée à partir du texte français. Si un mot sonne mal :

1. Pendant l’édition d’une réplique ou d’un indice, cliquez sur le bouton d’écoute :
   la réplique est dite.
2. Cliquez sur le mot qui sonne mal : il est dit seul et un petit panneau s’ouvre.
3. Dans **ÉCRIS « mot » COMME TU LE DIS**, écrivez le mot comme il se prononce,
   par exemple « badeudon » pour « Badedon ». Le bouton d’écoute du panneau dit
   le mot ; celui du haut dit toute la réplique.
4. **Valider** garde la correction ; **Retour** ou Échap l’abandonne.

Une correction vaut pour ce mot dans toute l’histoire, y compris dans les répliques
écrites ensuite. Les mots corrigés sont soulignés. Videz le champ pour retirer une
correction. Une correction reste un seul mot.

Les virgules créent une pause courte ; les points et la ponctuation de fin de phrase
une pause longue. Les espaces séparent les mots sans imposer une pause.
La conversion peut afficher une erreur : écoutez et corrigez avant de valider.

Les répliques originales de 1988 gardent leur voix d’origine : le bouton d’écoute
la fait entendre. Modifier leur texte la remplace par une voix créée à partir du texte.

Le bouton **Voix** de la collection ouvre l’atelier vocal indépendant, qui montre la
partition sonore complète pour les usages avancés. **Copier les phonèmes** copie
cette partition, pas un fichier audio. La [référence vocale](SPEECH_ENGINE.md)
détaille la notation.

## Dessiner un objectif

Choisissez **OBJETS 1**, **2** ou **3** : objectif principal, New Game Plus ou
FromSoftware. Son indice, sa voix et ses rectangles appartiennent à la même sélection.

1. Cliquez sur l’indice pour l’éditer ; sa voix suit le texte.
2. Glissez directement dans le décor pour dessiner la cible exacte. Le dessin est déjà actif.
3. La zone proche l’entoure automatiquement. Réglez sa marge avec **+** et **−**.
4. **Valider** garde les deux rectangles ; **Annuler** ou Échap abandonne le changement.

Un clic sans glisser ouvre les réglages sans remplacer la cible. Chaque objectif
garde une seule paire de zones. Les anciennes zones irrégulières restent intactes
tant que vous ne validez pas de modification ; leur marge apparaît comme **—**.
Cliquer dans le décor n’ouvre jamais le menu d’images.

## Enregistrer, tester et reprendre

Les textes saisis et les modifications acceptées restent dans le même brouillon,
même en changeant de scène, en testant ou en allant jouer un original. L'atelier
vocal conserve son choix explicite **Utiliser ce dialogue / Annuler**.

- **Test** joue le contenu actuel. Revenir à l'éditeur retrouve votre scène de travail et le passage sélectionné,
  même si vous avez progressé dans l'histoire pendant le test.
- **Enregistrer** télécharge un fichier `.rody.json` contenant toute l'histoire :
  scènes, images, textes et voix. Il reste disponible même sans modifications.
- **Rétablir** demande confirmation puis restaure toute l'histoire,
  y compris les images et les scènes ajoutées/supprimées, à sa version initiale ou
  au dernier Enregistrer. Le bouton est désactivé lorsqu'il n'y a pas de changements.

**Téléchargement lancé** signifie que le navigateur a reçu la demande. Vérifiez que
vous avez gardé le fichier ; le jeu ne peut pas savoir si vous annulez ensuite son
enregistrement dans les commandes du navigateur ou du système. Un échec détecté
conserve le brouillon et son point de reprise.

Le navigateur conserve automatiquement **Mon histoire**, avec ses changements et
son point de reprise, pour la retrouver après rechargement. Cela ne remplace pas
le fichier téléchargé. **Modifications non enregistrées** signifie que vous avez
changé l'histoire depuis son point de reprise, même si le navigateur garde ce travail.

Une erreur de récupération laisse le travail ouvert et Enregistrer disponible.
**Réessayer** permet de relancer la récupération automatique. Une écriture
interrompue par une fermeture ou un plantage peut perdre les derniers changements.
Changer de navigateur ou d'appareil, ou effacer les données du site, ne transporte
pas votre espace de travail : utilisez le fichier `.rody.json` et **Importer**.
Ce même fichier permet de partager l'histoire avec un ami.

## Ressources et anciens supports

- [Palette PNG](bonus/paletteRody.png) et [palette ACT](bonus/paletteRody.ACT) pour votre outil de dessin.
- [Police Rody](bonus/Rody.ttf), recréée pixel par pixel : lettres françaises (majuscules accentuées comprises),
  guillemets « » et ponctuation courante.
- [Ancienne vidéo](https://www.youtube.com/watch?v=1vx8D2irVLI) : repère visuel historique,
  pas une procédure actuelle pour importer, enregistrer ou écrire une voix.
- [Ancien menu de scènes](tutorial-screenshots/01-hub-scenes.png),
  [ancien éditeur principal](tutorial-screenshots/02-editor-main.png),
  [trois répliques](tutorial-screenshots/04-three-speakers.png),
  [ancien panneau de dialogue](tutorial-screenshots/05-dialogue-edit.png),
  [musique](tutorial-screenshots/06-music-selection.png),
  [trois objectifs](tutorial-screenshots/07-object-slots.png),
  [pinceau](tutorial-screenshots/09-paintbrush-edit.png).
  Ces images sont conservées comme références historiques. Leurs annotations sur
  les dossiers de sauvegarde et l'ancien clavier vocal sont obsolètes.

Pour signaler un problème ou proposer une histoire, utilisez la
[page itch.io de la collection](https://lacrearthur.itch.io/rody-mastico-collection).


Pendant le dessin d’une cible, les numéros de passage laissent place à
**Proximité : … px**. Les deux boutons illustrés élargissent ou réduisent cette
zone autour de la cible ; validez pour conserver le changement ou annulez pour
retrouver la zone précédente.
