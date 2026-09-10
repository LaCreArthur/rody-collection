# Rody Collection

Recréation des aventures Rody & Mastico de l'Atari ST, avec Rody à Ibiza,
l'éditeur intégré Rody Maker et des bonus dont DOOMastico.

**[Jouer dans le navigateur](https://lacrearthur.github.io/rody-collection/)** ·
[Page itch.io](https://lacrearthur.itch.io/rody-mastico-collection)

Ce dépôt prépare la prochaine version. Le [roadmap](docs/ROADMAP.md) distingue
les fonctions présentes dans le code des vérifications et de la publication restantes.

## Comprendre le projet

**[Game Design & Product Specification](docs/GAME_DESIGN.md)** explique en un seul
document la Collection, les histoires, Rody Maker et l'atelier vocal, sans référence
au code. Il définit notamment le travail sur une histoire personnelle, son enregistrement
et sa restauration ; le roadmap suit leur validation et leur publication.

| Besoin | Document |
|---|---|
| Jouer | [Guide du joueur](docs/PLAYER_GUIDE.md) |
| Créer et partager une histoire | [Tutoriel Rody Maker](docs/RODY_MAKER_TUTORIAL.md) |
| Reprendre le travail avec un agent | [Point d'entrée](CLAUDE.md) |
| Connaître les priorités | [Roadmap](docs/ROADMAP.md) |
| Comprendre les voix et leur validation | [Référence vocale](docs/SPEECH_ENGINE.md) |
| Retrouver une ancienne session | [Historique](DEVLOG.md) |

Pour écrire de la notation avec un agent, la
[compétence français → phonèmes Rody](.claude/skills/french-to-rody-phonemes/SKILL.md)
est dans le dépôt. L'atelier intégré accepte également le français directement.

## Démarrage développeur

1. Utiliser la version Unity indiquée dans [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
2. Ouvrir `Assets/Scenes/0_MenuCollection.unity` et lancer Play pour le parcours général.
3. Vérifier les fonctions navigateur dans une version WebGL ; les sélecteurs de
   fichiers navigateur ne fonctionnent pas dans le lecteur de l'Editor.

Les scènes, formats et outils d'export sont décrits dans
[l'architecture actuelle](docs/unify/ARCHITECTURE.md).
Le [workflow de publication](.github/workflows/deploy-pages.yml) construit et déploie
sur GitHub Pages à chaque push sur `master` : publier reste une action explicite.

## Crédits

### Rody Maker
- **Code/UI Design :** Arthur Scheidel
- **Assistance Code :** Lugioli
- **Assistance UI Design/PixelArt :** Nicolas Legay & Rose Luxey

### Rody à Ibiza (Original)
- **Code/Synthèse vocale/Scénario/PixelArt :** Arthur Scheidel
- **Scénario/PixelArt++/Animations :** Rose Luxey
- **Scénario/PixelArt/Animations :** Guillaume Fleck

### DOOMastico
- **Code/Design :** Arthur Scheidel

Rody & Mastico et les éléments originaux sont crédités à Lankhor ; les éléments
réutilisés de DOOM à id Software. Ce projet est un hommage gratuit et non commercial.
Dans sa présentation d'origine, Arthur s'excuse auprès des créateurs d'avoir utilisé
leurs noms et leurs assets avant de leur avoir demandé leur avis.

## Liens

- [Source](https://github.com/LaCreArthur/rody-collection)
- [Rody à Ibiza, site d'origine](https://lacrearthur.github.io/RodyAIbiza/)
- [DOOMastico : référence et idées de gameplay](docs/DOOM_FPS.md)
- [Récupération de références et migrations Unity](docs/MIGRATION_GUIDE.md)
- [Texte de page itch.io à relire avant publication](docs/itch-pages/ITCH_RODY_COLLECTION.md)
