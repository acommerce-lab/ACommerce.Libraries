// Nadena.Web media picker: opens a file dialog and returns picked images as base64.
window.nadenaMediaPicker = {
    pick: function (accept, multiple, capture) {
        return new Promise(function (resolve) {
            var input = document.createElement('input');
            input.type = 'file';
            input.accept = accept || 'image/*';
            if (multiple) input.multiple = true;
            if (capture) input.setAttribute('capture', 'environment');
            input.style.position = 'fixed';
            input.style.left = '-10000px';
            document.body.appendChild(input);

            var settled = false;
            function cleanup() { try { document.body.removeChild(input); } catch (e) { } }

            function toBase64(buffer) {
                var bytes = new Uint8Array(buffer);
                var binary = '';
                var chunk = 0x8000;
                for (var i = 0; i < bytes.length; i += chunk) {
                    binary += String.fromCharCode.apply(null, bytes.subarray(i, i + chunk));
                }
                return btoa(binary);
            }

            input.addEventListener('change', function () {
                settled = true;
                var files = Array.prototype.slice.call(input.files || []);
                Promise.all(files.map(function (f) {
                    return f.arrayBuffer().then(function (buf) {
                        return { fileName: f.name, contentType: f.type || 'image/jpeg', base64: toBase64(buf) };
                    });
                })).then(function (results) {
                    cleanup();
                    resolve(results);
                }).catch(function () {
                    cleanup();
                    resolve([]);
                });
            }, { once: true });

            // Fallback: if the user cancels, resolve empty once focus returns.
            function onFocus() {
                setTimeout(function () {
                    if (!settled) { cleanup(); window.removeEventListener('focus', onFocus); resolve([]); }
                }, 600);
            }
            window.addEventListener('focus', onFocus);

            input.click();
        });
    }
};
