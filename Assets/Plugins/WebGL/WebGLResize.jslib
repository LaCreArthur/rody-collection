mergeInto(LibraryManager.library, {
    TriggerResize: function() {
        // Dispatch resize event to force Unity to recalculate canvas size
        window.dispatchEvent(new Event('resize'));
    }
});
