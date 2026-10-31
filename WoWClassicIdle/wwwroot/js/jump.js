// Space on the /idle page makes the living player's portrait jump.
// The next jump is possible 1 s after the animation has finished. Every jump is reported to the open
// page, which counts it for the jump achievements.
(function () {
    const JUMP_MS = 500;
    const COOLDOWN_MS = 1000;
    let readyAt = 0;
    let page = null;

    window.playerJump = {
        register: function (dotNetRef) {
            page = dotNetRef;
            return {
                dispose: () => { if (page === dotNetRef) page = null; }
            };
        }
    };

    document.addEventListener('keydown', function (e) {
        if (e.code !== 'Space' || e.ctrlKey || e.altKey || e.metaKey || e.shiftKey) return;
        if (e.target instanceof Element && e.target.closest('input, textarea, select, [contenteditable]')) return;
        // Open dialogs cover the page and keep Space for their own buttons
        if (document.querySelector('[aria-modal="true"], dialog[open], .update-overlay')) return;

        const area = document.querySelector('.player-frame .portrait-area');
        if (!area) return;

        // Also while on cooldown or held down: no page scrolling, no click on a focused button
        e.preventDefault();

        const now = performance.now();
        if (e.repeat || now < readyAt || !area.querySelector('.race-portrait')) return;

        readyAt = now + JUMP_MS + COOLDOWN_MS;
        page?.invokeMethodAsync('OnJumped').catch(() => { });   // Circuit gone: nothing left to count
        // Animates the images, not the area: spell effects recoil the area with their own transform
        for (const img of area.querySelectorAll('.race-portrait, .class-badge')) {
            img.animate(
                [
                    { transform: 'translateY(0)', easing: 'cubic-bezier(.2, .7, .4, 1)' },
                    { transform: 'translateY(-24px)', easing: 'cubic-bezier(.6, 0, .8, .3)' },
                    { transform: 'translateY(0)' }
                ],
                { duration: JUMP_MS }
            );
        }
    });
})();
