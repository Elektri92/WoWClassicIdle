// Combat effects – buffs on the player (rune, light pillar, particles) and debuffs on the enemy
// (circling motes, drops, stun stars); the icon then flies into the buff/debuff bar.
(() => {
    const { PLAYER, colorsFor, centerOf, spawn, animateAndRemove, flash, ring, sparks } = window.spellFx.fx;
    const HEAL = ['#9dff8a', '#24b02a', '#f0fff0'];
    const SHIELD = ['#a8dcff', '#2f7fe0', '#ffffff'];
    const DEBUFF_DEFAULT = ['#ff6a5a', '#8a1010', '#ffd0c8'];  // physical debuffs (Rend, Sunder Armor)

    /** Icon of the buff/debuff in the frame's bar (alt = ability name). */
    function findBarIcon(barSelector, name) {
        for (const img of document.querySelectorAll(`${barSelector} img`)) {
            if (img.alt !== name) continue;
            const r = img.getBoundingClientRect();
            if (r.width > 0) return { el: img, x: r.left + r.width / 2, y: r.top + r.height / 2 };
        }
        return null;
    }

    /** Icon pops up, pauses briefly and then flies into the buff/debuff bar. */
    function badgeToBar(x, y, colors, opts, barSelector, { startScale = 0, holdMs = 450 } = {}) {
        if (!opts.icon) return;
        const badge = spawn('spellfx-badge', x, y, colors);
        const img = document.createElement('img');
        img.src = opts.icon;
        img.alt = '';
        img.onerror = () => { img.style.visibility = 'hidden'; };
        badge.appendChild(img);
        badge.animate([
            { transform: `translate(-50%, -50%) scale(${startScale})`, opacity: 0 },
            { transform: 'translate(-50%, -50%) scale(1.25)', opacity: 1, offset: 0.3 },
            { transform: 'translate(-50%, -50%) scale(1)', opacity: 1 },
        ], { duration: 380, fill: 'forwards', easing: 'ease-out' });

        setTimeout(() => {
            // The bar is only updated with the next UI render → only look for the target now
            const dest = findBarIcon(barSelector, opts.name);
            const tx = dest ? dest.x - x : 0, ty = dest ? dest.y - y : -40;
            animateAndRemove(badge, [
                { transform: 'translate(-50%, -50%) scale(1)', opacity: 1 },
                { transform: `translate(calc(-50% + ${tx}px), calc(-50% + ${ty}px)) scale(${dest ? 0.6 : 0.8})`, opacity: dest ? 0.9 : 0 },
            ], { duration: 420, easing: 'cubic-bezier(.5,0,.75,.6)' });
            if (dest) setTimeout(() => {
                ring(dest.x, dest.y, colors, 0.55, 320);
                dest.el.animate([
                    { filter: 'brightness(1)' },
                    { filter: `brightness(2) drop-shadow(0 0 6px ${colors[0]})`, offset: 0.3 },
                    { filter: 'brightness(1)' },
                ], { duration: 420, easing: 'ease-out' });
            }, 420);
        }, holdMs);
    }

    /** Particles drifting upwards (dy < 0) or downwards (dy > 0), started staggered. */
    function drift(x, y, colors, cls, count, { spreadX = 30, rise = 70, delayStep = 40, life = 800 } = {}) {
        for (let i = 0; i < count; i++) {
            const sx = x + (Math.random() - 0.5) * 2 * spreadX;
            const sy = y + (Math.random() - 0.5) * 16;
            const el = spawn(cls, sx, sy, colors);
            el.style.opacity = '0';
            const dist = rise * (0.7 + Math.random() * 0.5);
            const wobble = (Math.random() - 0.5) * 16;
            animateAndRemove(el, [
                { transform: 'translate(-50%, -50%) scale(.4)', opacity: 0 },
                { transform: `translate(calc(-50% + ${wobble / 2}px), calc(-50% + ${-dist * 0.2}px)) scale(1)`, opacity: 1, offset: 0.2 },
                { transform: `translate(calc(-50% + ${wobble}px), calc(-50% + ${-dist}px)) scale(.5)`, opacity: 0 },
            ], { duration: life + Math.random() * 200, delay: i * delayStep, easing: 'ease-out' });
        }
    }

    /**
     * Buff/Heilung/Schild/Gestalt am Spieler.
     * opts: { icon, name, kind: buff | heal | shield | form }
     */
    function buffFx(opts) {
        const t = centerOf(PLAYER);
        if (!t) return;
        const colors = opts.kind === 'heal' ? HEAL
            : opts.kind === 'shield' ? SHIELD
            : colorsFor(opts.icon);
        const { x, y } = t;

        // Portrait lights up
        t.el.animate([
            { filter: 'brightness(1)' },
            { filter: `brightness(1.6) drop-shadow(0 0 14px ${colors[0]})`, offset: 0.3 },
            { filter: 'brightness(1)' },
        ], { duration: 700, easing: 'ease-out' });

        // Rotierender Runenkreis
        animateAndRemove(spawn('spellfx-rune', x, y, colors), [
            { transform: 'translate(-50%, -50%) scale(.4) rotate(0deg)', opacity: 0 },
            { transform: 'translate(-50%, -50%) scale(1.15) rotate(70deg)', opacity: 1, offset: 0.35 },
            { transform: 'translate(-50%, -50%) scale(1.3) rotate(160deg)', opacity: 0 },
        ], { duration: 950, easing: 'ease-out' });

        // Light pillar from the bottom of the portrait
        animateAndRemove(spawn('spellfx-pillar', x, y + 34, colors), [
            { transform: 'translate(-50%, -100%) scaleY(0)', opacity: 0.2 },
            { transform: 'translate(-50%, -100%) scaleY(1)', opacity: 0.9, offset: 0.3 },
            { transform: 'translate(-50%, -100%) scaleY(1.1) scaleX(.4)', opacity: 0 },
        ], { duration: 750, easing: 'ease-out' });

        switch (opts.kind) {
            case 'heal':
                drift(x, y + 20, colors, 'spellfx-plus', 9, { rise: 80, delayStep: 55 });
                break;
            case 'shield':
                animateAndRemove(spawn('spellfx-bubble', x, y, colors), [
                    { transform: 'translate(-50%, -50%) scale(.5)', opacity: 0 },
                    { transform: 'translate(-50%, -50%) scale(1.08)', opacity: 1, offset: 0.25 },
                    { transform: 'translate(-50%, -50%) scale(1)', opacity: 0.85, offset: 0.7 },
                    { transform: 'translate(-50%, -50%) scale(1.15)', opacity: 0 },
                ], { duration: 1200, easing: 'ease-out' });
                drift(x, y + 10, colors, 'spellfx-mote', 6, { rise: 40 });
                break;
            case 'form':   // Gestaltwandel: Rauchwolke + Blitz
                flash(x, y, colors, 1.6, 420);
                sparks(x, y, colors, 14, 25, 30, null, 'spellfx-dust');
                drift(x, y + 20, colors, 'spellfx-mote', 8, { rise: 60 });
                break;
            default:
                drift(x, y + 24, colors, 'spellfx-mote', 12, { rise: 75 });
                break;
        }

        badgeToBar(x, y - 46, colors, opts, '.player-frame .buff-bar:not(.debuff-bar)');
    }

    /** Debuff lands on the enemy: circling motes, falling drops, icon stamp → debuff bar. */
    function debuffFx(target, opts) {
        const { x, y } = target;
        const colors = colorsFor(opts.icon, DEBUFF_DEFAULT);

        // Circling motes that pull into the target
        const orbit = spawn('spellfx-orbit', x, y, colors);
        for (let i = 0; i < 3; i++) {
            const m = document.createElement('div');
            m.className = 'spellfx-orbit-mote';
            m.style.transform = `rotate(${i * 120}deg) translateX(40px)`;
            orbit.appendChild(m);
        }
        animateAndRemove(orbit, [
            { transform: 'translate(-50%, -50%) rotate(0deg) scale(1.2)', opacity: 0 },
            { transform: 'translate(-50%, -50%) rotate(140deg) scale(1)', opacity: 1, offset: 0.3 },
            { transform: 'translate(-50%, -50%) rotate(420deg) scale(.25)', opacity: 0 },
        ], { duration: 950, easing: 'ease-in-out' });

        // Weakening: drops sink downwards
        drift(x, y - 20, colors, 'spellfx-drip', 8, { spreadX: 28, rise: -45, delayStep: 50, life: 700 });

        // Sickly glow of the portrait
        target.el.animate([
            { filter: 'saturate(1) brightness(1)' },
            { filter: `saturate(1.6) brightness(.75) drop-shadow(0 0 12px ${colors[0]})`, offset: 0.35 },
            { filter: 'saturate(1) brightness(1)' },
        ], { duration: 900, easing: 'ease-out' });

        if (opts.stun) {   // Stun: stars circle above the head
            const stars = spawn('spellfx-orbit stars', x, y - 38, colors);
            for (let i = 0; i < 3; i++) {
                const s = document.createElement('div');
                s.className = 'spellfx-star';
                s.textContent = '★';
                s.style.transform = `rotate(${i * 120}deg) translateX(22px)`;
                stars.appendChild(s);
            }
            animateAndRemove(stars, [
                { transform: 'translate(-50%, -50%) scaleY(.4) rotate(0deg)', opacity: 0 },
                { transform: 'translate(-50%, -50%) scaleY(.4) rotate(120deg)', opacity: 1, offset: 0.15 },
                { transform: 'translate(-50%, -50%) scaleY(.4) rotate(600deg)', opacity: 1, offset: 0.85 },
                { transform: 'translate(-50%, -50%) scaleY(.4) rotate(720deg)', opacity: 0 },
            ], { duration: 1500, easing: 'linear' });
        }

        badgeToBar(x, y, colors, opts, '.enemy-frame .debuff-bar', { startScale: 1.9, holdMs: 380 });
    }

    window.spellFx.handlers.buff = buffFx;
    window.spellFx.fx.debuffFx = debuffFx;   // Called from spells.js on impact
})();
