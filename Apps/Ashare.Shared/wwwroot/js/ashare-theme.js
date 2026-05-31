// AshareTheme: applies remote CSS variable overrides delivered via AppConfig
// snapshot. The single <style id> ensures repeated calls replace, never append.
(function () {
    "use strict";

    if (window.AshareTheme) { return; }

    window.AshareTheme = {
        applyRemote: function (elementId, css) {
            if (!elementId || typeof css !== "string") return;

            var el = document.getElementById(elementId);
            if (!el) {
                el = document.createElement("style");
                el.id = elementId;
                el.setAttribute("data-source", "appconfig");
                document.head.appendChild(el);
            }

            // Reassigning textContent is the cheapest way to replace.
            if (el.textContent !== css) {
                el.textContent = css;
            }
        },

        clearRemote: function (elementId) {
            var el = document.getElementById(elementId);
            if (el && el.parentNode) el.parentNode.removeChild(el);
        }
    };
})();
