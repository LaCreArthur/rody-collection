# Firebase WebGL Research: Level Editor for RodyMaker

## Executive Summary

**Is it possible to read, load, and write story data from Firebase on WebGL?**

**YES** - It is absolutely possible, and you already have most of the infrastructure in place. Your existing `FirebaseStoryProvider` using the REST API approach is the correct solution for WebGL.

---

## Current State Analysis

### What You Already Have

| Component | Status | Notes |
|-----------|--------|-------|
| `FirebaseStoryProvider.cs` | Implemented | Full REST API client with read/write capabilities |
| `FirebaseConfig.cs` | Configured | Firestore + Storage endpoints ready |
| `IStoryProvider` interface | Complete | Abstraction layer for swappable backends |
| `StoryProviderManager` | Working | Platform detection (WebGL vs Desktop) |
| Read operations (WebGL) | Working | Stories, scenes, sprites load from Firebase |
| Write operations (WebGL) | **IMPLEMENTED** | `SaveGameAsync`, `SaveSpritesAsync`, `SaveSceneAsync`, `UploadSpriteAsync` all connected |

### Phase 1 Implementation Status: COMPLETE

The Level Editor now supports WebGL saving via Firebase:

**New methods added to `RM_SaveLoad.cs`:**
- `SaveGameAsync()` - Main async save entry point
- `SaveSpritesAsync()` - Uploads all sprites to Firebase Storage
- `SaveCoverSpriteAsync()` - Handles scene 0 cover image
- `UpdateSceneCountAsync()` - Updates story metadata
- `GameManagerToSceneData()` - Converts editor state to data model
- `MakeTextureReadable()` - Creates readable texture copy for PNG encoding

**Modified `RM_MainLayout.cs`:**
- `SaveClick()` now uses platform detection (`#if UNITY_WEBGL`)
- `SaveClickAsync()` added for WebGL builds

### Remaining Setup Required

Before testing on WebGL, you need to:
1. **Configure CORS** on Firebase Storage bucket (see CORS section below)
2. **Test in WebGL build** - The code compiles but needs runtime testing

---

## Web Research Findings

### Firebase Unity SDK Does NOT Support WebGL

The official Firebase Unity SDK does not support WebGL builds. This is a known limitation.

> "Firebase does not provide support for WebGL games natively."
> - [Unity Discussions](https://discussions.unity.com/t/unity-webgl-build-using-firebase/881082)

### Three Viable Approaches

#### 1. REST API (Recommended - You're Using This)

**Pros:**
- Works on ALL platforms including WebGL
- No external dependencies
- Full control over requests
- Already implemented in your codebase

**Cons:**
- More verbose than SDK
- Manual JSON parsing required
- No real-time listeners (must poll)

> "Tested on Android and WebGL platform. Should work well on other platforms too since most of the implementation is only a simple http REST request."
> - [unity-firebase-realtime-database](https://github.com/edricwilliem/unity-firebase-realtime-database)

#### 2. JavaScript SDK Bridge via `.jslib`

**Pros:**
- Full Firebase SDK features
- Real-time listeners possible
- Google officially supports this approach

**Cons:**
- Requires maintaining C#/JavaScript bridge
- More complex setup
- Platform-specific code paths

> "Writing a WebGL plugin that uses the JavaScript SDK is considered the most stable path forward."
> - [Firebase Google Groups](https://groups.google.com/g/firebase-talk/c/GDi5R63SAMk)

The [FirebaseWebGL](https://github.com/rotolonico/FirebaseWebGL) package provides:
- Realtime Database
- Authentication
- Cloud Functions
- Storage
- Firestore

#### 3. Third-Party Assets

[WebGL API for Firebase](https://assetstore.unity.com/packages/tools/utilities/webgl-api-for-firebase-272421) on Unity Asset Store provides a pre-built solution.

---

## Firebase REST API Details

### Firestore REST API

**Base URL:** `https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents`

| Operation | Method | Endpoint |
|-----------|--------|----------|
| Get Document | GET | `/{collection}/{docId}?key={API_KEY}` |
| Create/Update | PATCH | `/{collection}/{docId}?key={API_KEY}` |
| Create New | POST | `/{collection}?key={API_KEY}` |
| Delete | DELETE | `/{collection}/{docId}?key={API_KEY}` |
| Query | POST | `:runQuery?key={API_KEY}` |

**Authentication Options:**
1. **API Key** (current approach) - Works for public read/write with permissive security rules
2. **Firebase ID Token** - For authenticated users (recommended for production)
3. **OAuth 2.0 Token** - For server-to-server communication

> Source: [Firebase Firestore REST API Docs](https://firebase.google.com/docs/firestore/use-rest-api)

### Firebase Storage REST API

**Base URL:** `https://firebasestorage.googleapis.com/v0/b/{bucket}/o`

| Operation | Method | Endpoint |
|-----------|--------|----------|
| Download | GET | `/{path}?alt=media` |
| Upload | POST | `/{path}` |
| Get Metadata | GET | `/{path}` |
| Delete | DELETE | `/{path}` |

**Important for WebGL:** You need to configure CORS on your Storage bucket for browser uploads to work.

> Source: [Firebase Storage REST API](https://firebase.google.com/docs/storage/web/download-files)

---

## Implementation Plan

### Phase 1: Connect Level Editor to Firebase (Core)

**Goal:** Make `RM_SaveLoad.SaveGame()` work on WebGL by using `FirebaseStoryProvider`

#### Step 1.1: Create `SaveGameAsync` Method

Add a new async method in `RM_SaveLoad.cs`:

```csharp
public static void SaveGameAsync(RM_GameManager gm, Action onComplete, Action<string> onError)
{
    // Convert RM_GameManager state to SceneData
    SceneData scene = GameManagerToSceneData(gm);

    // Save scene data to Firebase
    var provider = StoryProviderManager.FirebaseProvider;
    provider.SaveSceneAsync(storyId, gm.currentScene, scene,
        () => {
            // Save sprites...
            SaveSpritesAsync(gm, onComplete, onError);
        },
        onError
    );
}
```

#### Step 1.2: Create `SaveSpritesAsync` Method

Convert the current sprite saving logic to use Firebase Storage:

```csharp
private static void SaveSpritesAsync(RM_GameManager gm, Action onComplete, Action<string> onError)
{
    var provider = StoryProviderManager.FirebaseProvider;

    // Get sprite PNG bytes
    byte[] pngData = gm.scenePanel.GetComponent<SpriteRenderer>().sprite.texture.EncodeToPNG();

    // Upload to Firebase Storage
    string spriteName = $"{gm.currentScene}.1.png";
    provider.UploadSpriteAsync(storyId, spriteName, pngData,
        url => onComplete?.Invoke(),
        onError
    );
}
```

#### Step 1.3: Create `GameManagerToSceneData` Conversion

Add method to convert `RM_GameManager` state to `SceneData`:

```csharp
private static SceneData GameManagerToSceneData(RM_GameManager gm)
{
    return new SceneData
    {
        dialogues = new PhonemeDialogues {
            intro1 = gm.introDial1,
            intro2 = gm.introDial2,
            intro3 = gm.introDial3,
            obj = gm.objDial,
            ngp = gm.ngpDial,
            fsw = gm.fswDial
        },
        texts = new DisplayTexts {
            title = gm.titleText,
            intro = gm.introText,
            obj = gm.objText,
            ngp = gm.ngpText,
            fsw = gm.fswText
        },
        music = new MusicSettings {
            introMusic = gm.musicIntro,
            sceneMusic = gm.musicLoop
        },
        voice = new VoiceSettings {
            pitch1 = gm.pitch1,
            pitch2 = gm.pitch2,
            pitch3 = gm.pitch3,
            isMastico1 = gm.isMastico1,
            isMastico2 = gm.isMastico2,
            isMastico3 = gm.isMastico3,
            isZambla = gm.isZambla
        },
        objects = new ObjectZones {
            obj = new ObjectZone {
                positionRaw = objListToString(gm.obj),
                sizeRaw = objListToString(gm.obj, true),
                nearPositionRaw = objListToString(gm.objNear),
                nearSizeRaw = objListToString(gm.objNear, true)
            },
            // ... similar for ngp, fsw
        }
    };
}
```

#### Step 1.4: Add Platform-Specific Save Logic

Modify the save button handler to use the appropriate method:

```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
    RM_SaveLoad.SaveGameAsync(gm, OnSaveComplete, OnSaveError);
#else
    RM_SaveLoad.SaveGame(gm);
#endif
```

### Phase 2: Story/Project Management (Optional)

**Goal:** Allow creating new stories, renaming, deleting from WebGL

#### Step 2.1: Add `CreateStoryAsync` Method

```csharp
public void CreateStoryAsync(string title, Action<string> onComplete, Action<string> onError)
{
    // Generate story ID
    string storyId = GenerateStoryId(title);

    // Create story document in Firestore
    var metadata = new StoryMetadata(storyId) { title = title, sceneCount = 0 };
    // POST to stories collection...
}
```

#### Step 2.2: Add `DeleteStoryAsync` Method

```csharp
public void DeleteStoryAsync(string storyId, Action onComplete, Action<string> onError)
{
    // Delete all scenes in subcollection first
    // Delete story document
    // Delete storage files
}
```

### Phase 3: Authentication (Production)

**Goal:** Secure the Firebase data with user authentication

#### Step 3.1: Implement Anonymous Auth

For simplicity, use Firebase Anonymous Authentication:
- User gets a unique UID without sign-in
- Stories can be tied to their UID
- Token passed in REST API headers

#### Step 3.2: Update Security Rules

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /stories/{storyId} {
      allow read: if true;
      allow write: if request.auth != null && request.auth.uid == resource.data.ownerId;
    }
  }
}
```

---

## File Changes Required

### Files to Modify

| File | Changes |
|------|---------|
| `RM_SaveLoad.cs` | Add `SaveGameAsync`, `SaveSpritesAsync`, `GameManagerToSceneData` |
| `FirebaseStoryProvider.cs` | Add `CreateStoryAsync`, `DeleteStoryAsync`, `UpdateSceneCountAsync` |
| `RM_GameManager.cs` or UI handlers | Add platform-specific save button logic |
| `RM_MainLayout.cs` | Add async scene management UI handlers |

### Files to Create (Optional)

| File | Purpose |
|------|---------|
| `FirebaseAuth.cs` | Handle authentication (if implementing Phase 3) |

---

## CORS Configuration

For Firebase Storage uploads to work from WebGL, you need to configure CORS:

1. Create `cors.json`:
```json
[
  {
    "origin": ["*"],
    "method": ["GET", "POST", "PUT", "DELETE"],
    "maxAgeSeconds": 3600
  }
]
```

2. Apply using gsutil:
```bash
gsutil cors set cors.json gs://rody-maker.firebasestorage.app
```

---

## Estimated Work Breakdown

| Phase | Effort | Priority |
|-------|--------|----------|
| Phase 1.1-1.3: Core Save Logic | 2-3 hours | High |
| Phase 1.4: Platform Detection | 30 min | High |
| Phase 2: Story Management | 2-3 hours | Medium |
| Phase 3: Authentication | 4-6 hours | Low (production) |
| CORS Setup | 15 min | High |

**Total Minimum (Core functionality):** ~3-4 hours

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API Key exposed in WebGL build | Medium | Implement security rules; consider authentication |
| CORS not configured | High | Must configure before testing |
| Network failures | Medium | Add retry logic, save queue |
| Concurrent edits | Low | Add optimistic locking or last-write-wins |

---

## Recommended Approach

1. **Start with Phase 1** - Get basic save working on WebGL
2. **Test thoroughly** on WebGL build
3. **Add Phase 2** if you need story management from browser
4. **Skip Phase 3** unless publishing publicly

Your existing REST API implementation is solid. The main work is connecting the Level Editor's save logic to use `FirebaseStoryProvider` instead of file system operations.

---

## Sources

- [Firebase Firestore REST API](https://firebase.google.com/docs/firestore/use-rest-api)
- [Firebase Storage REST API](https://firebase.google.com/docs/storage/web/download-files)
- [FirebaseWebGL GitHub](https://github.com/rotolonico/FirebaseWebGL)
- [Firebase with Unity (even in WebGL Build!)](https://medium.com/firebase-developers/firebase-with-unity-even-in-webgl-build-8891e6f9b33c)
- [Unity WebGL Firebase Discussions](https://discussions.unity.com/t/unity-webgl-build-using-firebase/881082)
- [Firebase Talk Google Groups](https://groups.google.com/g/firebase-talk/c/GDi5R63SAMk)
- [WebGL API for Firebase Asset](https://assetstore.unity.com/packages/tools/utilities/webgl-api-for-firebase-272421)
