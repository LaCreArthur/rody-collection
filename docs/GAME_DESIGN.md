# Rody Collection — Game Design & Product Specification

**Updated: 9 September 2026.** A single product reference for Rody Collection,
Rody stories, and Rody Maker. It describes the experience, its rules, its creative
possibilities and the intended relationship between its parts.

**Reading the status:** Sections 1–8 describe the established product and current
capabilities. Section 9 proposes a consistent editing experience; it is a
recommendation awaiting agreement, not a claim that every interaction already
works that way. Section 10 names the remaining product gaps. None of these sections
certifies that a feature has been checked in the latest published browser version.

## At a glance

- **Collection:** a library for playing originals, personal stories and bonuses.
- **Story:** a linear sequence of illustrated scenes, voices and object searches.
- **Maker:** assemble those scenes and remix existing stories without programming.
- **Voix:** write, listen to and correct deliberately robotic French speech.
- **Next UX decision:** one whole-story draft, explicit Save, safe exit and portable Export.

For product orientation, read sections 1–3; for story/creator rules, 4–8; for the
proposed UX and unresolved scope, 9–10. Bonuses and success criteria close the document.

## 1. What Rody Collection is

Rody Collection is a French retro adventure collection and creation tool. Players
browse illustrated stories, listen to the unmistakably robotic voices of Rody's
world, and find objects hidden in the scenery. The same application lets them
make new scenes, rewrite dialogue, change pictures and turn a story into their own.

It brings together three closely connected experiences:

| Experience | The person’s goal | What they leave with |
|---|---|---|
| **Rody Collection** | Discover, choose and revisit adventures | A library of original and personal stories they can play |
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

Personal stories appear after the built-ins. Several saved personal stories can
belong to the library; one story is selected for play or editing at a time.
The present ordering places the most recently saved personal stories last.

### Browsing and actions

Stories appear as covers in a horizontal carousel. Selecting a cover focuses the
story; clicking the focused cover launches it. The user can drag the carousel or
move through it with left/right controls.

A shared action area changes with the selected story:

- **Dupliquer** on a built-in opens a personal editable copy.
- **Éditer** on a personal story opens that story in Maker.
- **Exporter** on a personal story downloads its saved version for sharing.
- **Importer** brings a received story into the library.
- **New story** asks for a title and optional cover, then opens a new adventure.
- **Voix** opens the standalone voice workbench.

User-story deletion exists through a keyboard action and confirmation. Making
this action discoverable without knowing a shortcut remains a UX gap.

### Built-in versus personal

Built-ins are the reference collection. They should remain available unchanged
when someone experiments. Editing one produces a personal copy, recognisable by
its copy title. Imported and newly created stories are personal stories.

A copy is independent: changing its picture or dialogue should not modify the
built-in or another personal story. Two stories with the same visible title must
still be able to exist without one silently replacing the other. The existing
experience has gaps in these protections; this is a required distinction in the
product, not a reason to burden creators with technical identifiers.

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
The current reliably saved authoring shape is one near region and one exact region
for each of the three objectives.

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
at three times that size. Story illustrations occupy **320 × 130**; title and cover
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
The conceptual experience is the same: edit a personal adventure while preserving
the reference originals. The current paintbrush route still needs that protection
made consistent with the other entry points.

A new story begins with a title and one scene. It does not require sixteen scenes
before it can exist. The creator supplies a cover if desired, then develops the
title image and the scenes.

### The main workspace

Scene thumbnails provide navigation. The selected scene remains the focus of the
large preview, with tools grouped by the part of the scene being edited:

- **INTRO:** scene title, displayed introduction text, spoken lines and music.
- **IMG:** main illustration and character animation pictures.
- **Objects:** clues and the clickable regions of the three searches.
- **Test:** experience the scene as a player.
- **Save:** keep the current work locally.
- **Reset:** the earlier intended action was to abandon the current unsaved scene edit;
  section 9 proposes a clearer, story-wide restore action instead.

The title image is a presentation screen; it does not have the ordinary scene's
introductory dialogue and object-search tools. New scene creation and later-scene
deletion exist, but the current controls have inconsistent limits. They should not
be advertised as unrestricted scene management.

### Editing art

Creators prepare illustrations outside Maker and import them. Maker fits the
pictures to the scene or title area and the Rody palette. It is an assembly and
story-authoring tool, not an in-app painting program.

The main picture is the resting scene. Additional pictures create simple talking
animation. A creator should be able to see exactly which picture they are replacing,
preview the sequence, and keep or discard the change through the same saving rules
as dialogue. Current frame-creation and save behaviour need refinement before all
animation affordances can be considered dependable.

### Editing dialogue and objectives

The introduction offers three sequential dialogue positions. For each, the creator
edits the visible line, the spoken line, character pitch where available, and whether
Mastico or the illustration animates.

Each objective has its own visible clue, spoken clue and pair of regions. The
creator draws the near region, then a smaller target within it. Different regions
can make the optional challenges harder without changing the illustration.
Multiple targets within one objective were advertised in older versions, but the
current save behaviour does not safely preserve that promise. The present design
must say one pair until a deliberate decision changes it.

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
to that line; **Cancel** leaves the line as it was. The containing scene still needs
Save to keep the edit. A saved French-authored dialogue retains its French source,
word corrections and chosen expression for later editing.

Already-authored phonetic dialogue remains editable even when its original French
text is unavailable. The workbench should not invent a supposed source sentence.

Standalone Voix can play and copy a score for reuse. It does not itself create a
saved story, and a downloadable audio recording is not a current feature. Copying
a score is also not a substitute for saving the full French-authored dialogue in
a story.

## 9. Recommended saving and editing experience — proposed

### One simple distinction

| Term | Meaning for the creator |
|---|---|
| **Draft** | All changes they are trying in one open story, across its scenes |
| **Save** | Keep the whole edited story in this browser so it can be reopened |
| **Export** | Download the saved story as a portable backup or a file to share |
| **Restore saved story** | Discard all changes since this story’s last successful Save |

The existing project direction is explicit Save, with export as a separate action.
The recommendation is **one draft for the whole story**. Pictures, animation, zones,
text, voice and added/deleted scenes all belong to that draft. Moving between scenes
keeps the draft without asking the creator to save at every click.

This changes the older scene-only Reset proposal. Replace that ambiguous action
with **Rétablir l’histoire enregistrée**, whose confirmation explicitly says it will
discard changes across the story. There is one Save boundary and one complete
restore boundary; an image import must not bypass them while a text edit obeys them.
This is a proposed change, not a previously accepted decision.

### Action contract

| What the creator does | What should happen |
|---|---|
| Saves | Show `Enregistrement…`, then `Enregistré dans ce navigateur` only after success. Keep the draft and offer retry if saving fails. |
| Restores the saved story | Name the story and ask before discarding all changes. Restore its last successful save, including pictures and scenes added or deleted since then. Disable the action if nothing changed. |
| Changes scene or opens another editor panel | Keep all draft edits and navigate without a save prompt. |
| Changes story or leaves the editor | If there are changes, offer `Enregistrer / Ne pas enregistrer / Rester`. Discard restores the saved story; an unsaved copy is dropped. If unchanged, go directly. A failed save keeps the creator in place. |
| Tests a scene | Play the draft and return to the same draft. Testing should neither save it nor lose it. This is a proposed change to the current Test behaviour. |
| Edits a built-in | Work in a personal copy; keep the original intact. Create its library entry on the first successful Save. Keep the existing copy-title style without a compulsory name prompt. |
| Imports a conflicting story | Offer a new copy, replacement of the named personal story, or cancellation. Default to a new copy; never silently replace an original. |
| Exports | Download the selected saved story. Say `Téléchargement lancé`, since the application cannot guarantee that the person kept the file. |
| Deletes a personal story | Name the story in the confirmation and remove it only after the deletion succeeds. |

Closing a browser tab is a separate boundary: the application cannot promise a
custom three-button dialog there. It should make the unsaved state visible before
that moment; neither hiding nor closing a tab should secretly save a draft that
the creator intended to discard.

### New stories and structural edits

Creating a new story saves its initial title, optional cover and first scene before
opening the editor. If that initial save fails, retain the entered information and
offer retry; do not claim the story was created successfully. A built-in copy is
kept as a draft until its first Save. Before that first save, there is no saved
version to restore, so Restore is unavailable. Discarding the copy when leaving
creates no library entry; the original remains available to duplicate again.

Adding or deleting a scene edits the same story draft. Save commits the resulting
story; restoring brings back the saved scene list and scene content. Confirm deletion
of a named scene, but do not secretly save that deletion on its own. Minimum/maximum
scene counts still require the product decision identified below.

### Feedback and help

Show **Modifications non enregistrées** alongside the story identity when
there is a draft. Once saved, the creator should not be repeatedly told that the
story will disappear just because they have not exported it.

A brief first-save explanation is enough:

> Conservé dans ce navigateur. Exporte un fichier pour le partager ou le garder ailleurs.

Browser-local storage is tied to that browser and site; clearing it or changing
device does not move the library. A single downloaded `.rody.json` story file contains
the pictures and authored dialogue as well as the scenes; it is the way to carry a
story elsewhere. There is no account-based cloud library or simultaneous collaborative
editing in the current scope.

A backup indicator can show that the saved version has been offered for download,
but must not imply a guaranteed external backup. Use readable words alongside any
colour cue. Keep Save's tooltip about saving, Export's about downloading, and Restore's
about discarding the whole story draft; the same terms should appear throughout the
collection and editor.

### Priority

First make saving, restoring, story identity and error messages trustworthy. Next
make navigation and testing use those same rules. Then add concise labels and
button help. A new visual theme or a more elaborate menu hierarchy would not solve
the present uncertainty about what happens to an edit.

## 10. Product gaps and decisions still open

These are gaps to resolve, not new features silently added to the release:

- **Trust in saved work:** complete image/text rollback, safe copies, collision handling,
  truthful failure messages and dependable reopening in the browser.
- **Draft scope, Test and safe exits:** agree the whole-story draft, story-wide restore,
  draft preview and Save / Discard / Stay proposal before changing those interactions.
  It replaces the earlier scene-only Reset intention.
- **Scene management:** choose a clear capacity and consistent add/delete rules;
  decide whether the sixteen-slot story menu should remain the intentional retro limit.
- **Animation authoring:** make creating, replacing and removing all displayed frame
  positions dependable, and preserve the exact chosen sequence when saving.
- **Object count:** retain one target pair per objective unless there is a concrete
  creative need to restore multiple targets without losing them.
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
while still understanding which edits are drafts, which are saved and which have
been exported. All of this should feel like Rody: colourful, odd, funny and unmistakably
robotic, with reliable controls underneath that personality.
