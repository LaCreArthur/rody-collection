var StandaloneFileBrowserWebGLPlugin = {
    // Show browser confirm dialog
    // Returns: true if user confirmed, false if cancelled
    ShowConfirmDialog: function(messagePtr) {
        var message = UTF8ToString(messagePtr);
        return confirm(message);
    },

    // Set flag for beforeunload warning
    // hasUnsaved: 1 = show warning, 0 = no warning
    SetUnsavedWorkFlag: function(hasUnsaved) {
        window._rodyHasUnsavedWork = (hasUnsaved === 1);
    },

    // Initialize beforeunload handler (call once at startup)
    InitBeforeUnloadHandler: function() {
        if (!window._rodyBeforeUnloadInitialized) {
            window._rodyBeforeUnloadInitialized = true;
            window.addEventListener('beforeunload', function(e) {
                if (window._rodyHasUnsavedWork) {
                    e.preventDefault();
                    e.returnValue = '';
                }
            });
        }
    },

    // Open file and return actual file content as text (for JSON import).
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filter: Filter files (e.g., ".json")
    // Returns: File content as text string via SendMessage
    UploadFileContent: function(gameObjectNamePtr, methodNamePtr, filterPtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filter = UTF8ToString(filterPtr);

        // Delete if element exists (safe removal via parentNode)
        var fileInput = document.getElementById(gameObjectName + '_content');
        if (fileInput && fileInput.parentNode) {
            fileInput.parentNode.removeChild(fileInput);
        }

        fileInput = document.createElement('input');
        fileInput.setAttribute('id', gameObjectName + '_content');
        fileInput.setAttribute('type', 'file');
        fileInput.setAttribute('style', 'display:none;');
        if (filter) {
            fileInput.setAttribute('accept', filter);
        }
        fileInput.onclick = function(event) {
            this.value = null;
        };
        fileInput.onchange = function(event) {
            if (event.target.files.length === 0) {
                SendMessage(gameObjectName, methodName, '');
                return;
            }
            var file = event.target.files[0];
            var reader = new FileReader();
            reader.onload = function(e) {
                SendMessage(gameObjectName, methodName, e.target.result);
            };
            reader.onerror = function(e) {
                console.error('FileReader error:', e);
                SendMessage(gameObjectName, methodName, '');
            };
            reader.readAsText(file);
            if (fileInput.parentNode) fileInput.parentNode.removeChild(fileInput);
        };
        document.body.appendChild(fileInput);

        // Try direct click first (works if user gesture context is preserved)
        // Fall back to next mouseup if blocked by browser security
        try {
            fileInput.click();
        } catch (e) {
            document.addEventListener('mouseup', function handler() {
                fileInput.click();
                document.removeEventListener('mouseup', handler);
            }, { once: true });
        }
    },

    // Open file and return content as base64 data URL (for image import)
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filter: Filter files (e.g., "image/png,image/jpeg")
    // Returns: Data URL (e.g., "data:image/png;base64,iVBORw0...") via SendMessage
    UploadFileAsBase64: function(gameObjectNamePtr, methodNamePtr, filterPtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filter = UTF8ToString(filterPtr);

        // Delete if element exists (safe removal via parentNode)
        var fileInput = document.getElementById(gameObjectName + '_base64');
        if (fileInput && fileInput.parentNode) {
            fileInput.parentNode.removeChild(fileInput);
        }

        fileInput = document.createElement('input');
        fileInput.setAttribute('id', gameObjectName + '_base64');
        fileInput.setAttribute('type', 'file');
        fileInput.setAttribute('style', 'display:none;');
        if (filter) {
            fileInput.setAttribute('accept', filter);
        }
        fileInput.onclick = function(event) {
            this.value = null;
        };
        fileInput.onchange = function(event) {
            if (event.target.files.length === 0) {
                SendMessage(gameObjectName, methodName, '');
                return;
            }
            var file = event.target.files[0];
            var reader = new FileReader();
            reader.onload = function(e) {
                // Returns data URL: "data:image/png;base64,iVBORw0..."
                SendMessage(gameObjectName, methodName, e.target.result);
            };
            reader.onerror = function(e) {
                console.error('FileReader error:', e);
                SendMessage(gameObjectName, methodName, '');
            };
            reader.readAsDataURL(file);
            if (fileInput.parentNode) fileInput.parentNode.removeChild(fileInput);
        };
        document.body.appendChild(fileInput);

        // Try direct click first (works if user gesture context is preserved)
        // Fall back to next mouseup if blocked by browser security
        try {
            fileInput.click();
        } catch (e) {
            document.addEventListener('mouseup', function handler() {
                fileInput.click();
                document.removeEventListener('mouseup', handler);
            }, { once: true });
        }
    },

    // Open file (legacy - returns blob URLs, not content).
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filter: Filter files. Example filters:
    //     Match all image files: "image/*"
    //     Match all video files: "video/*"
    //     Match all audio files: "audio/*"
    //     Custom: ".plist, .xml, .yaml"
    // multiselect: Allows multiple file selection
    UploadFile: function(gameObjectNamePtr, methodNamePtr, filterPtr, multiselect) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filter = UTF8ToString(filterPtr);

        // Delete if element exists (safe removal via parentNode)
        var fileInput = document.getElementById(gameObjectName)
        if (fileInput && fileInput.parentNode) {
            fileInput.parentNode.removeChild(fileInput);
        }

        fileInput = document.createElement('input');
        fileInput.setAttribute('id', gameObjectName);
        fileInput.setAttribute('type', 'file');
        fileInput.setAttribute('style','display:none;');
        fileInput.setAttribute('style','visibility:hidden;');
        if (multiselect) {
            fileInput.setAttribute('multiple', '');
        }
        if (filter) {
            fileInput.setAttribute('accept', filter);
        }
        fileInput.onclick = function (event) {
            // File dialog opened
            this.value = null;
        };
        fileInput.onchange = function (event) {
            // multiselect works
            var urls = [];
            for (var i = 0; i < event.target.files.length; i++) {
                urls.push(URL.createObjectURL(event.target.files[i]));
            }
            // File selected
            SendMessage(gameObjectName, methodName, urls.join());

            // Remove after file selected (safe removal via parentNode)
            if (fileInput.parentNode) fileInput.parentNode.removeChild(fileInput);
        }
        document.body.appendChild(fileInput);

        // Try direct click first (works if user gesture context is preserved)
        // Fall back to next mouseup if blocked by browser security
        try {
            fileInput.click();
        } catch (e) {
            document.addEventListener('mouseup', function handler() {
                fileInput.click();
                document.removeEventListener('mouseup', handler);
            }, { once: true });
        }
    },

    // Save file
    // DownloadFile method does not open SaveFileDialog like standalone builds, its just allows user to download file
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filenamePtr: Filename with extension
    // byteArray: byte[]
    // byteArraySize: byte[].Length
    DownloadFile: function(gameObjectNamePtr, methodNamePtr, filenamePtr, byteArray, byteArraySize) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filename = UTF8ToString(filenamePtr);

        var bytes = new Uint8Array(byteArraySize);
        for (var i = 0; i < byteArraySize; i++) {
            bytes[i] = HEAPU8[byteArray + i];
        }

        var downloader = window.document.createElement('a');
        downloader.setAttribute('id', gameObjectName);
        downloader.href = window.URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }));
        downloader.download = filename;
        document.body.appendChild(downloader);

        // Try direct click first, fall back to mouseup
        var doDownload = function() {
            downloader.click();
            if (downloader.parentNode) downloader.parentNode.removeChild(downloader);
            SendMessage(gameObjectName, methodName);
        };

        try {
            doDownload();
        } catch (e) {
            document.addEventListener('mouseup', function handler() {
                doDownload();
                document.removeEventListener('mouseup', handler);
            }, { once: true });
        }
    }
};

mergeInto(LibraryManager.library, StandaloneFileBrowserWebGLPlugin);