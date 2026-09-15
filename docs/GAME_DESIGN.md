# Rody Collection — Game Design & Product Specification

**Updated: 10 September 2026.** A single product reference for Rody Collection,
Rody stories, and Rody Maker. It describes the experience, its rules, its creative
possibilities and the intended relationship between its parts.

**Reading the status:** This document defines the agreed product, including the
single personal workspace accepted on September 9. Section 9 owns its saving and
replacement rules; section 10 names remaining product decisions. Implementation
and browser acceptance are tracked separately. This specification does not certify
that the latest published version already provides every described behavior.

## At a glance

- **Collection:** the originals, one personal story and bonuses.
- **Story:** a linear sequence of illustrated scenes, voices and object searches.
- **Maker:** assemble those scenes and remix existing stories without programming.
- **Voix:** write, listen to and correct deliberately robotic French speech.
- **Editing:** originals plus one personal story; Save downloads; Discard restores; automatic browser recovery.

For product orientation, read sections 1–3; for story/creator rules, 4–8; for the
saving rules and unresolved scope, 9–10. Bonuses and success criteria close the document.

## 1. What Rody Collection is

Rody Collection is a French retro adventure collection and creation tool. Players
browse illustrated stories, listen to the unmistakably robotic voices of Rody's
world, and find objects hidden in the scenery. The same application lets them
make new scenes, rewrite dialogue, change pictures and turn a story into their own.

It brings together three closely connected experiences:

| Experience | The person’s goal | What they leave with |
|---|---|---|
| **Rody Collection** | Discover, choose and revisit adventures | The original adventures and their one personal story to play |
| **A Rody story** | Follow a small adventure and solve its visual searches | A completed sequence of scenes, with optional harder discoveries |
| **Rody Maker** | Make or remix an adventure without programming | A playable personal story and a file they can share |

A standalone **Voix** workbench also lets someone experiment with the retro voice
without first making a story. Bonus games and hidden interactions give the
collection a playful identity beyond its library.

The central promise is: **play a strange little adventure, discover how it works,
and make one yourself.**

## 2. Identity, audience and tone

### Origins

The foundation is the original Rody & Mastico series on Atari ST. Its bright,
childlike imagery, illustrated situations, simple searches and synthetic French
speech are the source of the collection's visual and sonic language.

The revival also comes from Benzaie's streams, which framed those children's games
with adult, psychedelic humour. Arthur created **Rody à Ibiza** in that spirit,
and Benzaie played it on stream. Rody Collection grew from that personal project
into a way to revisit the originals and create new adventures.

It is both a fan tribute and an authored, eccentric work. The purpose of polishing
it is to make that personality accessible, rather than to erase its oddness.

### Audience

The main audience is French-speaking retro fans, viewers familiar with the humour,
and a small community of curious creators. A creator may want to change a single
joke or voice line, make an adventure for friends, or build an entire episode.
The product must accommodate that small first edit before asking someone to learn
the whole editor.

The original stories and Ibiza do not have the same tone. The collection should
not be presented as a uniformly child-oriented or educational product; its parody
and action bonuses also serve an adult audience.

### Design pillars

- **A recognisable voice.** Robotic pronunciation, character pitch and deliberate
  awkwardness are part of the performance, not defects to smooth into natural narration.
- **Pictures lead interaction.** Covers, tableaux, character faces and illustrated
  controls give the experience its identity.
- **Simple actions, expressive results.** A scene is easy to understand, but text,
  imagery, timing, hidden targets and delivery allow an author to make it their own.
- **Play and creation stay close.** The player can move from an adventure to editing
  it, and a creator can listen or test without a lengthy setup.
- **Personal work is trustworthy.** Originals stay intact, edits can be discarded,
  saved work can be reopened, and sharing is explicit.

The project is a free, non-commercial fan work. Original characters and reused
assets remain credited to their respective creators. Community stories can be
shared with Arthur for possible inclusion; that is a personal curation process,
not an automatic publication promise.

## 3. The Collection: the library and its navigation

### Included stories

The collection contains seven built-in adventures. Noël is the fourth original
story, not an eighth adventure added to six numbered originals.

| Order | Story | Place in the collection |
|---|---|---|
| 1 | Rody et Mastico | First original adventure |
| 2 | Rody et Mastico II | Second original adventure |
| 3 | Rody et Mastico III | Third original adventure |
| 4 | Rody Noël | Christmas adventure, occupying the fourth position |
| 5 | Rody et Mastico V | Fifth original adventure |
| 6 | Rody et Mastico VI | Sixth original adventure |
| 7 | Rody à Ibiza | Arthur's fan-made episode, after the originals |

One **Mon histoire** slot follows the originals. It shows the current personal
story, or offers creation/import when empty. Replacing that story follows section 9.

### Browsing and actions

Stories appear as covers in a horizontal carousel. Selecting a cover focuses the
story; clicking the focused cover launches it. The user can drag the carousel or
move through it with left/right controls.

The shared action area changes with the selected story:

- **Dupliquer** on a built-in opens a personal editable copy.
- **Éditer** on a personal story opens that story in Maker.
- **Enregistrer** on the personal story downloads its complete current draft.
- **Importer** opens a received story in the personal workspace.
- **New story** asks for a title and optional cover, then opens a new adventure.
- **Voix** opens the standalone voice workbench.

There is no personal-library management or story-deletion shortcut. New, Duplicate
and Import replace the personal workspace; scene deletion is a separate authoring tool.

### Built-in versus personal

Built-ins are the reference collection. They should remain available unchanged
when someone experiments. Editing one produces a personal copy, recognisable by
its copy title. Imported and newly created stories are personal stories.

A copy is independent: changing its picture or dialogue should not modify the
built-in. Importing a story with the same title as an original must never replace
that original. Replacing the one personal workspace follows the explicit choices
in section 9; creators do not need to manage technical identifiers.

## 4. What a Rody story contains

A story is a titled, self-contained adventure composed of an ordered sequence of
illustrated scenes. Its presentation includes a library cover, a title image,
scene content, music choices, dialogue and credits.

**The cover and title image serve different purposes.** The cover identifies the
adventure in the collection. The title image introduces it after launch. Changing
one should not unexpectedly change the other.

The normal journey is:

**Choose story → title presentation → story menu → illustrated scenes → ending / credits.**

The story menu offers scene selection, editing and a return to the collection.
Its existing Atari-style selection grid exposes sixteen scene positions. Stories
can be longer; later scenes remain reachable by progressing through the adventure.
The menu is not currently a complete paginated chapter browser.

### Anatomy of a scene

| Element | Role |
|---|---|
| Scene title | Gives the situation a name or establishes a joke |
| Main illustration | The setting and the visual search space |
| Optional animation pictures | Bring a speaking character in the illustration to life |
| Up to three introduction lines | Set up the situation, exchange or punchline in sequence |
| Displayed text | What the player reads |
| Spoken dialogue | What the character actually says, with its chosen delivery |
| Introduction music | Establishes the scene's opening |
| Scene music | Repeats around the scene's ongoing interaction |
| Main objective | The first object search |
| New Game Plus objective | An optional harder search after the main objective |
| FromSoftware objective | A further optional search, deliberately more obscure |

Displayed writing and spoken delivery are distinct authoring choices. They may
say the same thing, but a creator can intentionally use a difference for timing,
pronunciation or humour. Editing one must not silently rewrite the other.

Stories use a linear scene sequence. The core creation model is an illustrated
search adventure, not a branching conversation, inventory-combination or quest
scripting editor. This constraint keeps a complete story within reach of a casual
creator.

## 5. Playing a scene

### The basic loop

1. The scene appears with its illustration, title and opening music.
2. Up to three spoken introduction lines establish what is happening.
3. The player enters the main search and receives a spoken and written clue.
4. They click a location in the picture and receive feedback.
5. Finding the main object allows progression or the next optional challenge.
6. The player continues to the following scene and eventually reaches the ending.

The player can repeat dialogue or an objective's clue through the available
controls. Progression controls appear at the appropriate point in the scene.
A random click is not a universal dialogue-skip command.

### Finding an object

Each objective associates its clue with two nested regions in the illustration:

| Click location | Feedback | Purpose |
|---|---|---|
| Inside the exact target | Mastico reacts positively; the objective is found | Clear success |
| Inside the surrounding near region | Mastico indicates the player is close | A useful hint without revealing the exact answer |
| Elsewhere | A negative or retry response | Encourage another attempt |

The surrounding region is a softer clue. It should guide someone toward the
answer rather than make success depend on guessing an arbitrary invisible pixel.
Each of the three objectives has one near region and one exact target region.

### Difficulty and optional play

The main objective provides the basic route through the adventure. After finding
it, the player can continue or choose **New Game Plus**. Completing that optional
search makes **FromSoftware** available.

These names describe increasingly difficult searches within the scene. New Game
Plus is not a separate campaign restart, and FromSoftware is not an alternative
ending. Finding both optional objectives is not required to move to the next scene.
The reward is discovery, Mastico's reaction and the extra joke or challenge; the
specification does not promise a points economy or leaderboard.

The normal story loop is forgiving: a wrong click leads to feedback and another
attempt. Difficulty comes from observation and interpretation, rather than combat
or a countdown.

## 6. Presentation: pictures, sound and character performance

### Visual language

The reference canvas is **320 × 200**, in a **16:10** proportion, commonly presented
at three times that size. This is a hard pixel budget: enlargement adds no design pixels. Story illustrations occupy **320 × 130**; title and cover
art use **320 × 200**. A sixteen-colour palette, hard pixel edges and the recreated
Rody lettering maintain the Atari character.

Maker reuses that language: illustrated buttons, coloured panels, the scene itself
as the main preview, and small thumbnails for navigation. The style should remain
recognisable when clarity improves. Labels, focus and feedback should help users
understand the existing pictograms without turning the editor into a generic web
dashboard.

Resizing should preserve proportions and keep visible objects aligned with their
clickable regions. Reading text and placing a target must remain practical at the
actual displayed size. Colour alone should not carry saving or selection status.
The pixel font has limited character coverage; creators need a legible indication
when text cannot be displayed as entered.

### Voice and animation

The speech is deliberately robotic and rooted in the 1988 sound. Authenticity is
a matter of pronunciation, rhythm, pauses and expression, not merely a retro filter.
Perfect reproduction of every aspect of the original hardware is not a product claim.

Each introduction line can use Mastico's animated presence or a character depicted
in the scene. Character pitch changes the delivery. The scene animation returns
to its base picture when the character stops speaking, with story-specific ending
behaviour possible. The editor presents animation pictures in two groups of three;
this supports small character performances rather than a general animation timeline.
The object clues use Mastico's voice.

Music is chosen from the supplied library. The creator can preview tracks and
choose the scene's opening and repeating music. Importing new music is not part of
the current Maker workflow.

## 7. Rody Maker: building an adventure

### Ways in

A creator can start a blank adventure, import a friend's story, duplicate a built-in
from the collection, or enter editing from a story menu or the gameplay paintbrush.
Every entry edits a personal adventure while preserving the reference originals
and follows the same replacement rules.

A new story begins with a title and one scene. It does not require sixteen scenes
before it can exist. The creator supplies a cover if desired, then develops the
title image and the scenes.

### The main workspace

The scene title and introduction occupy the original game's text panel. All three
introduction passages remain visible together; clicking one selects and edits it.
The bottom selectors choose an introduction passage or objective, including empty
positions. The selected objective shows its clue and target together. The native
picture size, illustrated tool positions and pixel font remain unchanged.


The native Maker affordances distinguish selection from action: the active passage
has a small side mark; hovering an editable passage highlights it and uses a writing
cursor. Selecting an objective briefly explains dragging to draw its target, with a
crosshair over the picture. During drawing, the selector row shows the proximity
value; two illustrated buttons enlarge/reduce it. Scene navigation has distinct
storyboard and paper-arrow pictograms. The initial help teaches both direct gestures.


- **Scenes:** temporarily open image-only thumbnails, with titles in tooltips, a scrollbar and adjacent-scene arrows.
- **Music:** choose the scene's introduction and loop tracks.
- **Images:** import the illustration and talking animation pictures.
- **Test:** play the current draft in the actual game, then return to the same scene and passage.
- **Save:** download the complete personal story and advance its restore point.
- **Restore:** restore the whole story to its initial or latest saved version, even after Test.

The title image is a presentation screen without dialogue, objectives or scene music.
Scene creation stops at29 narrative scenes; all existing imported scenes remain visible.
Deletion is offered only on the selected scene from position18 onward, with confirmation.
These retained authoring limits do not imply unrestricted scene management.

### Editing art

Creators prepare illustrations outside Maker and import them. Maker fits the
pictures to the scene or title area and the Rody palette. It is an assembly and
story-authoring tool, not an in-app painting program.

The main picture is the resting scene. Additional pictures create simple talking
animation. A creator should be able to see exactly which picture they are replacing,
preview the sequence, and keep or discard the change through the same saving rules
as dialogue. Existing images and the next empty position can be imported; individual
additional frames can be previewed and removed. Saving keeps the exact authored sequence.

### Editing dialogue and objectives

The introduction offers three sequential dialogue positions. Visible and spoken
text remain independent. Each passage's voice tool retains its pronunciation,
character pitch where available, and Mastico/illustration speaker choice.
Click the title or a visible passage to edit it directly; the active passage is
highlighted while its neighbours remain visible. Done, Return and Escape retain
accepted typing. Voice changes keep their own explicit Apply/Cancel choice.

Story text must fit the fixed text panel using the actual font, wrapping and line
breaks. There is no scrolling or shrinking. An insertion or paste is automatically
cropped to the prefix that fits, preserving the existing surrounding text and other
passages. Opening an existing story never crops its stored text.

Each objective has its own visible clue, spoken clue and exactly one near/target
pair. Selecting it immediately arms drawing on the picture. Drag anywhere to
replace the exact target; the near region follows with an adjustable pixel margin.
The +/− controls change that margin; Validate accepts the complete pair, while
Cancel or Escape keeps the previous pair. A plain click opens these controls
without altering the rectangles. Existing free-form regions retain their shape
until the creator accepts an edit. The picture never opens the image tools.

## 8. The voice workbench

The workbench serves two connected uses: prepare a line for a story, or play with
the retro voice independently through **Voix**.

### The authoring loop

**Write French → listen → select a word → correct its pronunciation → listen again.**

French entry proposes a pronunciation for the whole sentence. Unknown words and
invented names receive a guess that the author can correct. The important result
is the line the author hears and chooses, not blind acceptance of the conversion.
The tool is not a spelling assistant and cannot promise a useful result for every
language or unusual symbol.

Commas create short pauses and sentence-ending punctuation creates longer pauses.
Ordinary spaces separate words without forcing a silence. This keeps speech from
becoming a series of disconnected word recordings.

The creator can edit the phonetic score directly, use sound examples, insert sounds,
change permitted pitch, and audition a selected passage. Experienced authors can
shape duration, emphasis, volume and pauses while keeping the same voice character.
Original passages provide examples of that expression.

### Keeping a line

When opened from a story, **Use dialogue** applies the chosen speech and delivery
to that line; **Cancel** leaves the line as it was. Applying the line immediately
keeps it in the story draft; Save downloads it with
the whole story. A saved French-authored dialogue retains its French source,
word corrections and chosen expression for later editing.

Already-authored phonetic dialogue remains editable even when its original French
text is unavailable. The workbench should not invent a supposed source sentence.

Standalone Voix can play and copy a score for reuse. It does not itself create a
saved story, and a downloadable audio recording is not a current feature. Copying
a score is also not a substitute for saving the full French-authored dialogue in
a story.

## 9. Saving and editing

Arthur accepted this direction on September 9: **a document-style editor, with the
original collection always available and one personal “My story” workspace**.
This replaces the multiple-personal-story library and separate Save/Export model.

### One story, one Save, one restore point

| Term | Meaning for the creator |
|---|---|
| **My story / Mon histoire** | The one personal story open for editing and play |
| **Draft** | All changes in that story, across every scene and kind of content |
| **Save / Enregistrer** | Download the complete story file and keep editing |
| **Restore point** | The initial version opened for editing, then the version captured by the latest Save |
| **Discard changes / Annuler les modifications** | Return the whole story to its restore point |

Text, voice, pictures, animation, targets, metadata and added/deleted scenes all
belong to the same draft. Save advances the restore point, so Discard never returns
to a version older than the latest Save. There is no scene-only save or reset.
Save is also available before any edits, to download an unchanged story.

### Action contract

| What the creator does | What happens |
|---|---|
| Creates, duplicates or imports | Opens the one editable story. If the existing draft has changes, first offer **Enregistrer / Ne pas enregistrer / Annuler** before replacing it. |
| Chooses Save in that prompt | Download the existing draft through the normal Save action, then open the replacement. An observed save error leaves the existing work and choice intact. |
| Chooses Discard in that prompt | Replace the existing personal story with the prepared new one. |
| Chooses Cancel in that prompt | Keep the existing story, changes and place in the editor. |
| Changes scene or editor panel | Keep all accepted changes and navigate without a save prompt. |
| Tests | Play the current draft; return to the same draft and editor scene, without saving it. |
| Leaves the editor, browses or plays an original, or opens standalone Voix | Preserve the personal story and its changes. None of these actions replaces it. |
| Edits an original | Duplicate it into the personal workspace, using the same replacement choice if needed. Keep the original intact, including through the gameplay paintbrush. |
| Saves | Download the complete draft, keep editing and advance its restore point. Show **Téléchargement lancé**. |
| Discards changes | Confirm that all changes in the story will be discarded; restore the initial/latest saved version, including pictures and scenes. Disable when unchanged. |
| Cancels an import or chooses an invalid file | Leave the previous story and all its changes untouched. Validate the incoming story before offering replacement. |
| Adds or deletes a scene | Change the same draft. Confirm a named scene's deletion; whole-story Discard can bring it back. |

Only New, Duplicate and Import replace the personal workspace. The collection
contains the original covers plus one personal slot with its current title and
cover; there are no library ordering, personal-story deletion or collision choices.
New still asks for a title and optional cover; Duplicate uses the existing copy
name without a mandatory renaming step. Editing story presentation remains separate
scope. Cover and title image retain their distinct meanings.

### Automatic recovery and honest feedback

The browser automatically remembers this one workspace, including its draft and
restore point, so a refresh can recover both. Recovery never turns draft changes
into a Save or advances the restore point. If recovery storage fails, retain the
open work, explain the failure, keep retry available and leave file Save available. A write interrupted
by closing/crashing the browser may not recover its latest changes.

Show **Modifications non enregistrées** beside the personal story when it contains
edits since its restore point. Use readable words, not colour alone. There is no
separate Export button, local-save status or external-backup badge.

A brief explanation introduces the single action:

> Enregistrer télécharge ton histoire. Ce navigateur garde automatiquement ton travail pour le retrouver ici.

Save starts a browser download; the application cannot certify that the person
kept the file on disk. An observed download error preserves the draft and restore
point. Cancellation later in browser/operating-system download controls may be
unobservable, including when Save precedes replacing the personal story.

Browser recovery belongs to that browser and site. Clearing site data or changing
device does not carry the workspace elsewhere. The downloaded story contains its
scenes, pictures and complete authored dialogue; it is the portable copy to keep
or share. No cloud library, revision history or simultaneous collaborative editing
is part of this direction.

## 10. Product gaps and decisions still open

These are gaps to resolve, not new features silently added to the release:

- **Browser release acceptance:** demonstrate that the section 9 contract holds
  through real downloads, replacement failures and browser refresh, alongside the
  original stories and voice workbench.
- **Existing personal work:** prepare preservation of stories that only exist in the
  older browser library before publicly replacing it. Do not silently erase them.
- **Scene management:** choose a clear capacity and consistent add/delete rules;
  decide whether the sixteen-slot story menu should remain the intentional retro limit.
- **Expanded creative capacity:** additional animation positions or multiple targets
  need a concrete creative use before expanding the existing controls and one-pair rule.
- **Story presentation editing:** make changing a personal story's title, cover and
  credits understandable. The story can carry these, but the editor does not yet
  offer a complete, consistent management surface for them.
- **Clarity at different display sizes:** keep text readable and drawn targets aligned
  with the scene, with understandable keyboard focus and actionable errors.

## 11. Bonuses and boundaries

**DOOMastico** is a separate action bonus in the Collection: a retro first-person
shooter tied to the Ibiza universe, with weapons, enemies, objectives and win/lose
flow. It is not another object-search story, and Rody Maker does not author its levels.
Its violence and humour also reinforce that the whole collection is not a children's
product simply because the originals were children's games.

Other hidden interactions and minigames, including **RollToInfinity**, are playful
extras. Discovery can remain part of their appeal; a creator should not need to
understand them to make a Rody adventure.

The release target is an accessible browser experience. Older downloadable editions
are historical products and should not be used to promise that today's editing or
voice features work identically on every platform. Publishing and community curation
remain deliberate actions by Arthur, separate from a creator saving a personal story.

## 12. What a successful experience looks like

A new player can choose an adventure, understand the clue, learn from near/miss
feedback, find an object and continue without learning the editor.

A curious player can make one personal change without damaging the original.
A new creator can build a scene with an illustration, a spoken introduction and a
findable object, test it, save it, reopen it and send it to a friend.

An experienced creator can make the delivery and timing distinctly their own,
while still understanding the current draft, what Discard will restore, and how
to keep or share the story with Save. All of this should feel like Rody: colourful,
odd, funny and unmistakably robotic, with reliable controls underneath that personality.
