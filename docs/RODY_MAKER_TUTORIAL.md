# Rody Maker — Créer et partager une histoire

Guide de la version en préparation, relu le **9 septembre 2026**. Les fonctions
récentes restent à vérifier dans une version navigateur publiée. Les anciennes
vidéos et captures ne décrivent plus la sauvegarde ni le nouvel atelier vocal.
Le [document de conception](GAME_DESIGN.md) explique le produit et distingue
les améliorations proposées du fonctionnement actuel.

## Commencer

- **Nouveau :** dans la collection, choisissez la création d'histoire, entrez un
  titre et, si vous le souhaitez, importez une couverture. Une scène est créée.
- **Modifier un original :** sélectionnez sa couverture, puis **Dupliquer**.
  Vous travaillez sur une copie personnelle. Sauvegardez-la avant de changer d'histoire.
- **Reprendre :** sélectionnez une histoire personnelle, puis **Éditer**.
- **Importer :** utilisez **Importer** et choisissez un fichier `.rody.json`.
  Les anciens dossiers de jeu ne sont pas le format d'import actuel.

Pour l'instant, donnez des titres distincts à vos créations et gardez un export
avant de réimporter une histoire déjà présente : les conflits peuvent remplacer
un fichier existant. Le pinceau en pleine partie ne protège pas encore les
originaux de la même manière que **Dupliquer**.

## Se repérer dans l'éditeur

Les vignettes permettent de choisir une scène. L'image sélectionnée occupe
l'aperçu principal, avec les outils à droite.

| Outil | Usage actuel |
|---|---|
| **INTRO** | Titre de scène, textes, dialogues et musique |
| **IMG** | Image principale et images d'animation |
| **Objets** | Indices et zones cliquables |
| **Disquette** | Enregistrer l'histoire localement dans ce navigateur |
| **Test** | Quitter l'éditeur pour jouer le contenu déjà appliqué à l'histoire |
| **Reset** | Recharger la scène en mémoire ; voir ses limites ci-dessous |

L'image de titre est distincte de la couverture du menu. Elle n'a pas de dialogue
ni d'objectif à éditer. Les vignettes proposent aussi l'ajout de scènes ; les
règles d'ajout/suppression actuelles sont encore irrégulières. La promesse de
l'ancien tutoriel « de 16 à 29 scènes » ne décrit pas une limite fiable aujourd'hui.

## Images

Préparez les images dans votre outil de dessin, puis importez-les avec **IMG**.
Utilisez **320 × 130** pour une scène, **320 × 200** pour le titre ou la couverture,
et la [palette Rody](bonus/paletteRody.png) pour prévoir les couleurs du résultat.
L'import ajuste les dimensions et les couleurs ; aucun nom de fichier spécial
n'est nécessaire.

L'image principale représente la scène au repos. Les images d'animation montrent
les personnages lorsqu'ils parlent. L'éditeur présente deux groupes de trois
images, mais la création de toutes les positions et la sauvegarde des séquences
ont encore des limites. Vérifiez le résultat après avoir enregistré et rouvert.

## Introduction, texte et musique

Dans **INTRO**, choisissez le titre, les dialogues ou la musique.

![Ancienne illustration du menu Intro](tutorial-screenshots/03-intro-menu.png)

*Illustration historique : les pictogrammes aident à se repérer ; elle ne prouve
pas le comportement de la version actuelle.*

Les boutons **1**, **2**, **3** donnent accès aux trois répliques d'introduction.
Pour chacune, éditez séparément le texte affiché et la voix. L'interrupteur Mastico
choisit si Mastico s'anime ou si les images du décor accompagnent la réplique.
La police rétro ne contient pas tous les caractères : regardez le texte dans son
aperçu avant de finaliser la scène.

Pour la musique, **L1** choisit l'ouverture et **L2** la musique répétée.
Le bouton d'écoute permet de préécouter les pistes fournies.

## Écrire une voix

Ouvrez l'outil vocal depuis une réplique ou un indice. Le bouton **Voix** de la
collection ouvre le même atelier pour une utilisation indépendante.

1. Écrivez la phrase en français et écoutez sa proposition de prononciation.
2. Sélectionnez un mot qui sonne mal et ajustez sa prononciation dans le champ prévu.
3. Réécoutez le mot ou le passage dans son contexte. Les exemples de sons servent
   à essayer et insérer les sons de la voix rétro.
4. Réglez la hauteur de voix lorsqu'elle est disponible. Les indices restent dits
   par Mastico ; leur hauteur est fixe.
5. Choisissez **Utiliser ce dialogue** pour l'appliquer, ou **Annuler** pour revenir
   sans modifier la réplique. Validez aussi le panneau de dialogue, puis enregistrez
   la scène avec la disquette.

Les virgules créent une pause courte ; les points et la ponctuation de fin de phrase
une pause longue. Les espaces séparent les mots sans imposer une pause.
Les mots inventés peuvent nécessiter une correction. La conversion peut afficher
une erreur : écoutez et corrigez avant de valider.

Les phrases françaises enregistrées conservent leurs corrections pour la reprise.
Les anciennes répliques écrites directement en phonèmes restent éditables sans
inventer leur texte français. En mode indépendant, **Copier les phonèmes** copie
la partition sonore, pas un fichier audio ni l'ensemble du texte et des corrections.
La [référence vocale](SPEECH_ENGINE.md) détaille la notation pour les usages avancés.

## Dessiner un objectif

Dans **Objets**, choisissez l'objectif principal, **New Game Plus** ou **FromSoftware**.
Chacun possède un texte d'indice, une voix et deux rectangles.

1. Éditez le texte et la voix de l'indice.
2. Ouvrez le dessin des zones et faites glisser pour dessiner la zone proche.
3. Utilisez le bouton de zone pour passer à la cible exacte.
4. Dessinez cette cible à l'intérieur de la zone proche, puis terminez avec le bouton.
5. Revenez aux outils de la scène et enregistrez.

![Ancienne illustration des régions proche et exacte](tutorial-screenshots/08-object-zones.png)

**Utilisez une seule paire de zones par objectif.** L'interface conserve des messages
sur l'ajout de plusieurs zones, mais la sauvegarde actuelle ne conserve que la première.
Le clic droit ne doit donc pas être présenté comme une commande d'effacement.

## Enregistrer, tester et exporter

1. Validez le panneau dans lequel vous travaillez pour revenir à l'éditeur principal.
2. Cliquez sur la **disquette** et attendez le message de réussite. Ce bouton garde
   l'histoire dans ce navigateur ; il ne télécharge pas de fichier, malgré son ancienne infobulle.
3. Utilisez **Test** pour jouer. Enregistrez auparavant les modifications que vous
   voulez tester : le retour au même brouillon n'est pas encore garanti.
4. Revenez à la collection, sélectionnez votre histoire personnelle puis **Exporter**.
5. Conservez le fichier `.rody.json` téléchargé : il contient les scènes, images,
   textes et voix et peut être envoyé à un ami, qui l'ouvrira avec **Importer**.

**Reset n'est pas encore un retour complet à la dernière sauvegarde.** Une image
importée peut rester après Reset. Les avertissements de sortie ne couvrent pas
uniformément les modifications. Pour un travail important, enregistrez puis
exportez une version avant d'expérimenter davantage.

L'enregistrement local vise la reprise dans le même navigateur et sur le même site.
L'export permet de conserver une copie ailleurs ; un rechargement, un changement
de navigateur ou l'effacement de ses données ne doit pas être votre méthode de backup.

## Ressources et anciens supports

- [Palette PNG](bonus/paletteRody.png) et [palette ACT](bonus/paletteRody.ACT) pour votre outil de dessin.
- [Police Rody](bonus/Rody.ttf), recréée pixel par pixel, avec une couverture de caractères limitée.
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
