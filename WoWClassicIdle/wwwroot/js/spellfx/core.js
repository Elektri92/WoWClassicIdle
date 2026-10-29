// Combat effects in the Idle Fighter – core: effect layer, colors, shared building blocks and entry point from Blazor.
// The effects themselves live in spells.js, swings.js and auras.js and register themselves in spellFx.handlers.
window.spellFx = (() => {
    const PLAYER = '.player-frame .portrait-area';
    const ENEMY = '.enemy-frame .portrait-area';
    const MAX_IN_FLIGHT = 8;     // at high game speed don't flood the screen
    const TRAIL_INTERVAL = 22;   // ms between two trail particles
    let inFlight = 0;
    let layer = null;
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');

    // Color scheme [glow, core, highlight] based on the icon name (spell_fire_…, spell_frost_…)
    const SCHOOLS = [
        [/arcane|missile|starfire|moonfire|polymorph/, ['#ff8af2', '#b03aff', '#ffe6ff']],
        [/fire|flame|immolat|incinerat|scorch|lava|burn|sear/, ['#ffb347', '#ff4a12', '#fff0b0']],
        [/frost|ice|cold|snow|chill/, ['#9fe6ff', '#2f8fff', '#f0fcff']],
        [/lightning|thunder|storm|shock|spark/, ['#b8d8ff', '#4f7dff', '#ffffff']],
        [/shadow|curse|death|fel|corruption|drain|siphon|haunt|darkness/, ['#c08aff', '#5a14b0', '#ecdcff']],
        [/holy|light|smite|exorcism|judgement|seal|sun|righteous/, ['#fff3a0', '#ffbf1f', '#ffffff']],
        [/nature|poison|wrath|thorn|insect|earth|sting/, ['#9cff7a', '#2fae2a', '#eaffdc']],
    ];
    const PHYSICAL = ['#ffe2a6', '#d9902a', '#ffffff'];

    const canLaunch = () => inFlight < MAX_IN_FLIGHT;

    function colorsFor(iconUrl, fallback = PHYSICAL) {
        const name = (iconUrl || '').split('/').pop().toLowerCase();
        for (const [re, colors] of SCHOOLS)
            if (re.test(name)) return colors;
        return fallback;
    }

    function ensureLayer() {
        // Enhanced navigation can remove foreign body children → re-append if needed
        if (!layer || !document.body.contains(layer)) {
            layer = document.createElement('div');
            layer.className = 'spellfx-layer';
            document.body.appendChild(layer);
        }
        return layer;
    }

    function centerOf(selector) {
        const el = document.querySelector(selector);
        if (!el) return null;
        const r = el.getBoundingClientRect();
        if (r.width === 0 && r.height === 0) return null;
        return { el, x: r.left + r.width / 2, y: r.top + r.height / 2 };
    }

    function spawn(cls, x, y, colors) {
        const el = document.createElement('div');
        el.className = cls;
        el.style.left = `${x}px`;
        el.style.top = `${y}px`;
        el.style.setProperty('--fx-glow', colors[0]);
        el.style.setProperty('--fx-core', colors[1]);
        el.style.setProperty('--fx-hi', colors[2]);
        ensureLayer().appendChild(el);
        return el;
    }

    function animateAndRemove(el, keyframes, options) {
        el.animate(keyframes, { fill: 'forwards', ...options }).onfinish = () => el.remove();
    }

    const at = (rot, sx = 1, extra = '') => `translate(-50%, -50%) ${extra} rotate(${rot}deg) scaleX(${sx})`;

    // ── Bausteine ─────────────────────────────────────────────

    function trailParticle(x, y, colors, cls, life) {
        const p = spawn(cls, x, y, colors);
        const dx = (Math.random() - 0.5) * 14, dy = (Math.random() - 0.5) * 14;
        animateAndRemove(p, [
            { transform: 'translate(-50%, -50%) scale(1)', opacity: 0.9 },
            { transform: `translate(calc(-50% + ${dx}px), calc(-50% + ${dy}px)) scale(.2)`, opacity: 0 },
        ], { duration: life, easing: 'ease-out' });
    }

    function flash(x, y, colors, scale, duration) {
        animateAndRemove(spawn('spellfx-flash', x, y, colors), [
            { transform: 'translate(-50%, -50%) scale(.3)', opacity: 1 },
            { transform: `translate(-50%, -50%) scale(${scale})`, opacity: 0 },
        ], { duration, easing: 'ease-out' });
    }

    function ring(x, y, colors, scale, duration) {
        animateAndRemove(spawn('spellfx-ring', x, y, colors), [
            { transform: 'translate(-50%, -50%) scale(.2)', opacity: 1 },
            { transform: `translate(-50%, -50%) scale(${scale})`, opacity: 0 },
        ], { duration, easing: 'cubic-bezier(.2,.7,.3,1)' });
    }

    /** Sparks; with dirAngle (radians) as a cone in the strike direction, otherwise all around. */
    function sparks(x, y, colors, count, minDist, spread, dirAngle = null, cls = 'spellfx-spark') {
        for (let i = 0; i < count; i++) {
            const angle = dirAngle === null
                ? (Math.PI * 2 * i) / count + Math.random() * 0.5
                : dirAngle + (Math.random() - 0.5) * 1.6;
            const dist = minDist + Math.random() * spread;
            animateAndRemove(spawn(cls, x, y, colors), [
                { transform: 'translate(-50%, -50%) scale(1)', opacity: 1 },
                { transform: `translate(calc(-50% + ${Math.cos(angle) * dist}px), calc(-50% + ${Math.sin(angle) * dist}px)) scale(.3)`, opacity: 0 },
            ], { duration: 380 + Math.random() * 200, easing: 'cubic-bezier(.1,.8,.3,1)' });
        }
    }

    /** Schnitt-Linie; offset = senkrechter Versatz in px, delay in ms. */
    function slash(x, y, colors, angleDeg, { cls = 'spellfx-slash', offset = 0, delay = 0, duration = 300 } = {}) {
        const rad = angleDeg * Math.PI / 180;
        const ox = -Math.sin(rad) * offset, oy = Math.cos(rad) * offset;
        const el = spawn(cls, x + ox, y + oy, colors);
        el.style.opacity = '0';
        animateAndRemove(el, [
            { transform: at(angleDeg, 0), opacity: 1 },
            { transform: at(angleDeg, 1), opacity: 1, offset: 0.35 },
            { transform: at(angleDeg, 1.1), opacity: 0 },
        ], { duration, delay, easing: 'ease-out' });
    }

    /** The hit portrait recoils in the strike direction and flashes. */
    function hitReact(target, dir, colors, strength, squash = false) {
        const px = dir.x * strength, py = dir.y * strength;
        target.el.animate([
            { transform: 'translate(0, 0)', filter: 'brightness(1)' },
            {
                transform: `translate(${px}px, ${py}px) rotate(${dir.x >= 0 ? 3 : -3}deg)${squash ? ' scale(1.08, .88)' : ''}`,
                filter: `brightness(${1.4 + strength / 10}) drop-shadow(0 0 10px ${colors[0]})`, offset: 0.2
            },
            { transform: `translate(${-px / 2}px, ${-py / 2}px) rotate(${dir.x >= 0 ? -2 : 2}deg)`, offset: 0.5 },
            { transform: 'translate(0, 0)', filter: 'brightness(1)' },
        ], { duration: 260 + strength * 15, easing: 'ease-out' });
    }

    function frameShake(target, amount = 4) {
        target.el.closest('.unit-frame')?.animate([
            { transform: 'translate(0, 0)' },
            { transform: `translate(${-amount}px, ${amount / 2}px)` },
            { transform: `translate(${amount}px, ${-amount / 2}px)` },
            { transform: `translate(${-amount / 2}px, ${amount / 4}px)` },
            { transform: 'translate(0, 0)' },
        ], { duration: 260, easing: 'linear' });
    }

    /** Moves an element along an arc from from to to (quadratic Bézier curve). */
    function fly(from, to, { el, duration, arc, accelerate = true, onFrame, onArrive }) {
        const dx = to.x - from.x, dy = to.y - from.y;
        const len = Math.hypot(dx, dy) || 1;
        // Control point perpendicular to the trajectory → arc upwards (or sideways in a stacked layout)
        const sign = dx >= 0 ? 1 : -1;
        const cx = (from.x + to.x) / 2 + sign * (dy / len) * len * arc;
        const cy = (from.y + to.y) / 2 - sign * (dx / len) * len * arc;

        inFlight++;
        const start = performance.now();
        let px = from.x, py = from.y;

        function frame(now) {
            const raw = Math.min(1, (now - start) / duration);
            const t = accelerate ? raw * raw * (1.6 - 0.6 * raw) : raw;
            const u = 1 - t;
            const x = u * u * from.x + 2 * u * t * cx + t * t * to.x;
            const y = u * u * from.y + 2 * u * t * cy + t * t * to.y;
            const angle = Math.atan2(y - py, x - px) * 180 / Math.PI;
            px = x; py = y;

            el.style.left = `${x}px`;
            el.style.top = `${y}px`;
            onFrame(x, y, angle, raw, now);

            if (raw < 1) {
                requestAnimationFrame(frame);
            } else {
                el.remove();
                inFlight--;
                onArrive();
            }
        }
        requestAnimationFrame(frame);
    }

    function direction(from, to) {
        const dx = to.x - from.x, dy = to.y - from.y;
        const len = Math.hypot(dx, dy) || 1;
        return { x: dx / len, y: dy / len, angle: Math.atan2(dy, dx) };
    }

    // Effect types (spell | swing | buff) → handler, registered by the modules
    const handlers = {};

    /** Entry point from Blazor: all effects of an engine tick at once. */
    function play(events) {
        if (document.hidden || reduceMotion.matches || !Array.isArray(events)) return;
        for (const e of events)
            handlers[e.type]?.(e);
    }

    const fx = {
        PLAYER, ENEMY, TRAIL_INTERVAL, PHYSICAL,
        canLaunch, colorsFor, centerOf, spawn, animateAndRemove,
        trailParticle, flash, ring, sparks, slash, hitReact, frameShake, fly, direction,
    };

    return { play, handlers, fx };
})();
