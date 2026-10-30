// Global navigation hotkeys. A key only works while the nav menu shows the matching link,
// so pages that are still locked (talents, professions, no character yet) stay out of reach.
(function () {
    const routes = {
        b: 'inventory',
        c: 'inventory',
        f: 'idle',
        k: 'professions',
        n: 'talents'
    };

    document.addEventListener('keydown', function (e) {
        // Keep browser shortcuts (Ctrl+F, Ctrl+C ...) and typing in form fields intact
        if (e.repeat || e.ctrlKey || e.altKey || e.metaKey || e.shiftKey) return;
        if (e.target instanceof Element && e.target.closest('input, textarea, select, [contenteditable]')) return;

        const route = routes[e.key.toLowerCase()];
        if (route && document.querySelector(`.nav-link[href="${route}"]`)) {
            e.preventDefault();
            Blazor.navigateTo('/' + route);
        }
    });
})();
