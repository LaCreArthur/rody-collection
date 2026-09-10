var StandaloneFileBrowserWebGLPlugin = {
    // Clipboard access stays in the browser and reports the actual permission result.
    RodyCopyText: function(receiverPtr, textPtr) {
        var receiver = UTF8ToString(receiverPtr);
        if (!navigator.clipboard || !navigator.clipboard.writeText) {
            SendMessage(receiver, 'ClipboardCopyFailed', '');
            return;
        }
        navigator.clipboard.writeText(UTF8ToString(textPtr)).then(function() {
            SendMessage(receiver, 'ClipboardCopied', '');
        }, function() { SendMessage(receiver, 'ClipboardCopyFailed', ''); });
    },
    RodyPasteText: function(receiverPtr) {
        var receiver = UTF8ToString(receiverPtr);
        if (!navigator.clipboard || !navigator.clipboard.readText) {
            SendMessage(receiver, 'ClipboardFailed', '');
            return;
        }
        navigator.clipboard.readText().then(function(text) {
            SendMessage(receiver, 'ClipboardPasted', text);
        }, function() { SendMessage(receiver, 'ClipboardFailed', ''); });
    },
    // One native picker for text and image input. A new element permits reopening
    // the same file; one JSON result distinguishes cancellation from empty content.
    UploadFileContent: function(gameObjectNamePtr, methodNamePtr, filterPtr, asDataUrl) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filter = UTF8ToString(filterPtr);
        var fileInput = null;
        var reader = null;
        var completed = false;

        var finish = function(content, error) {
            if (completed) return;
            completed = true;
            if (fileInput) {
                fileInput.onchange = fileInput.oncancel = null;
                fileInput.remove();
            }
            if (reader) reader.onload = reader.onerror = reader.onabort = null;
            SendMessage(gameObjectName, methodName, JSON.stringify({ content: content, error: error }));
        };

        try {
            fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.style.display = 'none';
            fileInput.accept = filter;
            fileInput.oncancel = function() { finish(null, null); };
            fileInput.onchange = function() {
                var file = fileInput.files[0];
                if (!file) {
                    finish(null, null);
                    return;
                }
                fileInput.onchange = fileInput.oncancel = null;
                fileInput.remove();
                try {
                    reader = new FileReader();
                    reader.onload = function() { finish(reader.result, null); };
                    reader.onerror = function() {
                        finish(null, "Impossible de lire le fichier : " + (reader.error ? reader.error.message : "erreur de lecture."));
                    };
                    reader.onabort = function() { finish(null, "La lecture du fichier a été interrompue."); };
                    if (asDataUrl) reader.readAsDataURL(file);
                    else reader.readAsText(file);
                } catch (error) {
                    finish(null, "Impossible de lire le fichier : " + (error.message || String(error)));
                }
            };
            document.body.appendChild(fileInput);
            // Unlike click(), showPicker() reports missing user activation.
            fileInput.showPicker();
        } catch (error) {
            finish(null, "Impossible d'ouvrir le choix de fichier : " + (error.message || String(error)));
        }
    },

    // Success means the direct browser download request was dispatched. Whether
    // the person keeps the file after that handoff is not observable by this API.
    DownloadFile: function(gameObjectNamePtr, methodNamePtr, filenamePtr, byteArray, byteArraySize) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filename = UTF8ToString(filenamePtr);
        var downloader = null;
        var url = null;
        var error = '';

        try {
            if (!navigator.userActivation.isActive)
                throw new Error("Cliquez à nouveau sur Enregistrer pour lancer le téléchargement.");
            var bytes = HEAPU8.slice(byteArray, byteArray + byteArraySize);
            url = URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }));
            downloader = document.createElement('a');
            downloader.href = url;
            downloader.download = filename;
            downloader.style.display = 'none';
            document.body.appendChild(downloader);
            downloader.click();
        } catch (failure) {
            error = failure.message || String(failure);
        } finally {
            if (downloader) downloader.remove();
            if (url) {
                if (error) URL.revokeObjectURL(url);
                // Keep the Blob alive long enough for the browser to consume the
                // queued navigation, then release it even when no further action occurs.
                else setTimeout(function() { URL.revokeObjectURL(url); }, 60000);
            }
        }
        SendMessage(gameObjectName, methodName, error);
    },

    // Persist persistentDataPath (IDBFS virtual FS) to IndexedDB.
    // Unity does NOT auto-flush file writes; call this after writing a story.
    // Async: reports completion via SendMessage ('' on success, error message on failure).
    RodySyncFs: function(gameObjectNamePtr, methodNamePtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        FS.syncfs(false, function(err) {
            SendMessage(gameObjectName, methodName, err ? (err.message || String(err)) : '');
        });
    },

    // Hydrate persistentDataPath (IDBFS) FROM IndexedDB.
    // Call once at startup before the first user-story read; IDBFS is empty until synced in.
    // Async: reports completion via SendMessage ('' on success, error message on failure).
    RodySyncFsHydrate: function(gameObjectNamePtr, methodNamePtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        FS.syncfs(true, function(err) {
            SendMessage(gameObjectName, methodName, err ? (err.message || String(err)) : '');
        });
    }
};

mergeInto(LibraryManager.library, StandaloneFileBrowserWebGLPlugin);