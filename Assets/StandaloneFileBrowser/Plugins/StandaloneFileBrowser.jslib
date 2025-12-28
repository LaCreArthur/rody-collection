var StandaloneFileBrowserWebGLPlugin = {
    // Open file and return actual file content as text (for JSON import).
    // gameObjectNamePtr: Unique GameObject name. Required for calling back unity with SendMessage.
    // methodNamePtr: Callback method name on given GameObject.
    // filter: Filter files (e.g., ".json")
    // Returns: File content as text string via SendMessage
    UploadFileContent: function(gameObjectNamePtr, methodNamePtr, filterPtr) {
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var methodName = UTF8ToString(methodNamePtr);
        var filter = UTF8ToString(filterPtr);

        // Delete if element exists
        var fileInput = document.getElementById(gameObjectName + '_content');
        if (fileInput) {
            document.body.removeChild(fileInput);
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
            document.body.removeChild(fileInput);
        };
        document.body.appendChild(fileInput);

        document.onmouseup = function() {
            fileInput.click();
            document.onmouseup = null;
        };
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
        gameObjectName = Pointer_stringify(gameObjectNamePtr);
        methodName = Pointer_stringify(methodNamePtr);
        filter = Pointer_stringify(filterPtr);

        // Delete if element exist
        var fileInput = document.getElementById(gameObjectName)
        if (fileInput) {
            document.body.removeChild(fileInput);
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

            // Remove after file selected
            document.body.removeChild(fileInput);
        }
        document.body.appendChild(fileInput);

        document.onmouseup = function() {
            fileInput.click();
            document.onmouseup = null;
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
        gameObjectName = Pointer_stringify(gameObjectNamePtr);
        methodName = Pointer_stringify(methodNamePtr);
        filename = Pointer_stringify(filenamePtr);

        var bytes = new Uint8Array(byteArraySize);
        for (var i = 0; i < byteArraySize; i++) {
            bytes[i] = HEAPU8[byteArray + i];
        }

        var downloader = window.document.createElement('a');
        downloader.setAttribute('id', gameObjectName);
        downloader.href = window.URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }));
        downloader.download = filename;
        document.body.appendChild(downloader);

        document.onmouseup = function() {
            downloader.click();
            document.body.removeChild(downloader);
        	document.onmouseup = null;

            SendMessage(gameObjectName, methodName);
        }
    }
};

mergeInto(LibraryManager.library, StandaloneFileBrowserWebGLPlugin);