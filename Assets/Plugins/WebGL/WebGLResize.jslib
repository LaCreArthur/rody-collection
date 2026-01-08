mergeInto(LibraryManager.library, {
    TriggerResize: function() {
        var canvas = document.querySelector("#unity-canvas");
        var container = document.querySelector("#unity-container");
        if (canvas && container) {
            // Get container's actual CSS dimensions
            var style = window.getComputedStyle(container);
            var width = parseInt(style.width, 10);
            var height = parseInt(style.height, 10);

            // Reset canvas rendering resolution to match container
            // This forces Unity to re-render at the correct size
            canvas.width = width;
            canvas.height = height;

            console.log('[WebGLResize] Canvas reset to:', width, 'x', height);
        }

        // Dispatch resize event for any other listeners
        window.dispatchEvent(new Event('resize'));
    }
});
