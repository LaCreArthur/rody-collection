mergeInto(LibraryManager.library, {
    $rodyFrenchEngine: null,
    RodyFrench_Convert__deps: ['$rodyFrenchEngine'],
    RodyFrench_Convert: function(textPtr, requestId, receiverPtr, moduleUrlPtr) {
        var text = UTF8ToString(textPtr);
        var receiver = UTF8ToString(receiverPtr);
        var moduleUrl = new URL(UTF8ToString(moduleUrlPtr), document.baseURI).href;
        if (!rodyFrenchEngine) {
            rodyFrenchEngine = import(moduleUrl).then(function(module) {
                return module.default(module.roa);
            }).then(function(engine) {
                engine.setVoice('fr');
                return engine;
            }).catch(function(error) {
                rodyFrenchEngine = null;
                throw error;
            });
        }
        rodyFrenchEngine.then(function(engine) {
            var result = engine.textToIpaWithSourceMap(text);
            SendMessage(receiver, 'RodyFrenchConverted', JSON.stringify({
                requestId: requestId,
                ipa: result.ipa,
                sourceMap: result.sourceMap.map(function(pair) {
                    return { source: pair[0], ipa: pair[1] };
                }),
                error: ''
            }));
        }).catch(function(error) {
            SendMessage(receiver, 'RodyFrenchConverted', JSON.stringify({
                requestId: requestId, ipa: '', sourceMap: [],
                error: error.message || String(error)
            }));
        });
    }
});
