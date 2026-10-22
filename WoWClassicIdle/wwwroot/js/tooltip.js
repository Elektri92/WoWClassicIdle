// Positions WoW-style tooltips (position: fixed) next to the hovered element, clamped to the viewport.
window.wowTooltip = {
    // Host-Element ohne eigene Box (display: contents) liefert ein leeres Rechteck –
    // dann dient das erste sichtbare Kind-Element (der Slot) als Anker.
    anchor: function (host) {
        const r = host.getBoundingClientRect();
        if (r.width > 0 || r.height > 0) return r;
        for (const child of host.children) {
            if (child.classList.contains('wow-tooltip')) continue;
            const cr = child.getBoundingClientRect();
            if (cr.width > 0 || cr.height > 0) return cr;
        }
        return r;
    },
    show: function (host) {
        const tt = host.querySelector(':scope > .wow-tooltip');
        if (!tt) return;
        tt.classList.add('visible');

        const margin = 8;
        const r = wowTooltip.anchor(host);
        const w = tt.offsetWidth;
        const h = tt.offsetHeight;

        let left = r.right + margin;
        if (left + w > window.innerWidth - margin) left = r.left - w - margin;
        left = Math.max(margin, left);

        let top = r.top;
        if (top + h > window.innerHeight - margin) top = window.innerHeight - h - margin;
        top = Math.max(margin, top);

        tt.style.left = left + 'px';
        tt.style.top = top + 'px';
    },
    hide: function (host) {
        const tt = host.querySelector(':scope > .wow-tooltip');
        if (tt) tt.classList.remove('visible');
    }
};
