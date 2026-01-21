# Rody Collection

Recréation des jeux d'aventure Rody & Mastico de l'Atari ST avec un éditeur de niveaux intégré et de nombreux bonus.

**Jouer maintenant :** https://lacrearthur.github.io/rody-collection/

## C'est quoi ?

Rody Collection regroupe les 6 histoires originales de la série Rody & Mastico, et l'aventure exclusive : **Rody à Ibiza**. Le tout jouable directement dans le navigateur. L'éditeur Rody Maker permet de créer et partager ses propres histoires.

### Fonctionnalités

- **6 histoires originales** - Toutes les aventures classiques de Rody & Mastico (I à VI) avec de nouveaux objectifs bonus
- **Rody à Ibiza** - Une nouvelle aventure exclusive créée pour l'occasion
- **Éditeur intégré** - Créez vos propres jeux avec l'éditeur d'histoires Rody Maker 
- **Import/Export** - Partagez vos histoires en fichiers `.rody.json`
- **Synthèse vocale** - Le système TTS par phonèmes recréé la voix Atari ST originale
- **DOOMastico** - Un Doom-like dans l'univers de Rody à Ibiza

## Documentation

### Pour les joueurs (FR)

| Document | Description |
|----------|-------------|
| [Player Guide](docs/PLAYER_GUIDE.md) | Comment jouer |
| [Rody Maker Tutorial](docs/RODY_MAKER_TUTORIAL.md) | Guide de l'éditeur |

### Pour les développeurs

| Document | Description |
|----------|-------------|
| [CLAUDE.md](CLAUDE.md) | Architecture et référence principale |
| [Development Log](DEVLOG.md) | Historique des sessions |
| [Roadmap](docs/ROADMAP.md) | Progression et travail restant |
| [Save Awareness Plan](docs/SAVE_AWARENESS_PLAN.md) | UX pour éviter la perte de données |

### Sous-projets

| Document | Description |
|----------|-------------|
| [DOOM FPS Module](docs/DOOM_FPS.md) | Documentation du minigame FPS |
| [Doomastico Gameplay Audit](docs/DOOMASTICO_GAMEPLAY_AUDIT.md) | Améliorations gameplay futures |

### Référence (réutilisable)

| Document | Description |
|----------|-------------|
| [Migration Guide](docs/MIGRATION_GUIDE.md) | Toolkit migration BetterEvent/Odin |
| [Learnings](docs/LEARNINGS.md) | Meta-knowledge des migrations passées |

## Démarrage rapide

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
- **Source :** https://github.com/LaCreArthur/rody-collection
- **Site original :** https://lacrearthur.github.io/RodyAIbiza/
