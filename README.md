# Rody Collection

Recréation des jeux d'aventure Rody & Mastico de l'Atari ST avec un éditeur de niveaux intégré.

**Jouer maintenant :** https://lacrearthur.github.io/rody-collection/

## C'est quoi ?

Rody Collection regroupe les 6 histoires originales de la série Rody & Mastico, et une aventure exclusive : **Rody à Ibiza**. Le tout jouable directement dans le navigateur. L'éditeur Rody Maker permet de créer et partager ses propres histoires.

### Fonctionnalités

- **6 histoires originales** - Toutes les aventures classiques de Rody & Mastico (I à VI)
- **Rody à Ibiza** - Une nouvelle aventure exclusive créée pour cette collection
- **Éditeur intégré** - Créez vos propres jeux avec Rody Maker
- **Import/Export** - Partagez vos histoires en fichiers `.rody.json`
- **Synthèse vocale** - Le système TTS par phonèmes qui donne le vrai feeling rétro
- **Multi-plateforme** - WebGL (navigateur) et builds desktop
- **DOOMastico** - Un Doom-like dans l'univers de Rody à Ibiza

## Pour les développeurs

Voir [CLAUDE.md](CLAUDE.md) pour les détails d'architecture.

### Démarrage rapide

1. Ouvrir dans Unity 6 (6000.3.2f1)
2. Charger la scène `0_MenuCollection`
3. Appuyer sur Play

### Build

```bash
# Build WebGL via Unity Editor
File > Build Settings > WebGL > Build

# Le CI déploie automatiquement sur GitHub Pages à chaque push sur master
```

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

## Liens

- **Jouer :** https://lacrearthur.github.io/rody-collection/
- **itch.io :** https://lacrearthur.itch.io/rody-maker
- **Site original :** https://lacrearthur.github.io/RodyAIbiza/
