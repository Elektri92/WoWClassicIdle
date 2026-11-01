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

    // Particles run on one shared frame loop instead of WebKit animations: every Web Animations or CSS animation on a
    // short-lived element makes the WebKitGTK process grow steadily, and combat slows down the longer the game runs.
    const CENTER = 'translate(-50%, -50%)';
    const offset = (dx, dy) => `translate(calc(-50% + ${dx}px), calc(-50% + ${dy}px))`;
    const lerp = (a, b, t) => a + (b - a) * t;

    /** Value at progress t along [[position, value], …]. */
    function track(t, stops) {
        let i = 1;
        while (i < stops.length - 1 && t > stops[i][0]) i++;
        const [p0, v0] = stops[i - 1], [p1, v1] = stops[i];
        return lerp(v0, v1, Math.min(1, Math.max(0, (t - p0) / (p1 - p0))));
    }

    // Effect name → frame at progress t; v holds the random parts of the particle
    const FX = {
        scatter: (t, v) => [`${offset(v.dx * t, v.dy * t)} scale(${lerp(1, v.s1, t)})`, lerp(v.o0 ?? 1, 0, t)],
        grow: (t, v) => [`${CENTER} scale(${lerp(v.s0, v.s1, t)})`, 1 - t],
        slash: (t, v) => [`${CENTER} rotate(${v.rot}deg) scaleX(${track(t, [[0, 0], [.35, 1], [1, 1.1]])})`, track(t, [[0, 1], [.35, 1], [1, 0]])],
        drift: (t, v) => {
            const dx = track(t, [[0, 0], [.2, v.dx / 2], [1, v.dx]]), dy = track(t, [[0, 0], [.2, v.dy * .2], [1, v.dy]]);
            return [`${offset(dx, dy)} scale(${track(t, [[0, .4], [.2, 1], [1, .5]])})`, track(t, [[0, 0], [.2, 1], [1, 0]])];
        },
        badgeIn: (t, v) => [`${CENTER} scale(${track(t, [[0, v.s0], [.3, 1.25], [1, 1]])})`, track(t, [[0, 0], [.3, 1], [1, 1]])],
        badgeOut: (t, v) => [`${offset(v.dx * t, v.dy * t)} scale(${lerp(1, v.s1, t)})`, lerp(1, v.o1, t)],
        rune: t => [`${CENTER} scale(${track(t, [[0, .4], [.35, 1.15], [1, 1.3]])}) rotate(${track(t, [[0, 0], [.35, 70], [1, 160]])}deg)`, track(t, [[0, 0], [.35, 1], [1, 0]])],
        pillar: t => [`translate(-50%, -100%) scaleY(${track(t, [[0, 0], [.3, 1], [1, 1.1]])}) scaleX(${track(t, [[0, 1], [.3, 1], [1, .4]])})`, track(t, [[0, .2], [.3, .9], [1, 0]])],
        bubble: t => [`${CENTER} scale(${track(t, [[0, .5], [.25, 1.08], [.7, 1], [1, 1.15]])})`, track(t, [[0, 0], [.25, 1], [.7, .85], [1, 0]])],
        orbit: t => [`${CENTER} rotate(${track(t, [[0, 0], [.3, 140], [1, 420]])}deg) scale(${track(t, [[0, 1.2], [.3, 1], [1, .25]])})`, track(t, [[0, 0], [.3, 1], [1, 0]])],
        stars: t => [`${CENTER} scaleY(.4) rotate(${track(t, [[0, 0], [.15, 120], [.85, 600], [1, 720]])}deg)`, track(t, [[0, 0], [.15, 1], [.85, 1], [1, 0]])],
    };

    // Easings of the effects: t → eased progress
    const EASE = {
        out: t => 1 - (1 - t) ** 2.2,
        inOut: t => t < .5 ? 2 * t * t : 1 - (2 - 2 * t) ** 2 / 2,
        in: t => t ** 2.4,
        burst: t => 1 - (1 - t) ** 3.2,
        linear: t => t,
    };

    const running = new Map();   // Element → running effect
    let loopActive = false;

    function renderFx(el, fx, t) {
        const [transform, opacity] = FX[fx.name](fx.ease(t), fx.vars);
        el.style.transform = transform;
        el.style.opacity = opacity;
    }

    function fxLoop(now) {
        for (const [el, fx] of running) {
            const t = (now - fx.start) / fx.duration;
            if (t < 0) continue;
            renderFx(el, fx, Math.min(1, t));
            if (t >= 1) {
                running.delete(el);
                if (fx.remove) el.remove();
            }
        }
        if (running.size > 0) requestAnimationFrame(fxLoop);
        else loopActive = false;
    }

    function playFx(el, name, { duration, delay = 0, easing = EASE.out, vars = {}, remove = false }) {
        const fx = { name, duration, ease: easing, vars, remove, start: performance.now() + delay };
        renderFx(el, fx, 0);
        running.set(el, fx);
        if (!loopActive) {
            loopActive = true;
            requestAnimationFrame(fxLoop);
        }
    }

    function animateAndRemove(el, name, options) {
        playFx(el, name, { ...options, remove: true });
    }

    // ── Bausteine ─────────────────────────────────────────────

    function trailParticle(x, y, colors, cls, life) {
        const p = spawn(cls, x, y, colors);
        const dx = (Math.random() - 0.5) * 14, dy = (Math.random() - 0.5) * 14;
        animateAndRemove(p, 'scatter', { duration: life, vars: { dx, dy, s1: .2, o0: .9 } });
    }

    function flash(x, y, colors, scale, duration) {
        animateAndRemove(spawn('spellfx-flash', x, y, colors), 'grow', { duration, vars: { s0: .3, s1: scale } });
    }

    function ring(x, y, colors, scale, duration) {
        animateAndRemove(spawn('spellfx-ring', x, y, colors), 'grow', {
            duration, easing: EASE.burst, vars: { s0: .2, s1: scale },
        });
    }

    /** Sparks; with dirAngle (radians) as a cone in the strike direction, otherwise all around. */
    function sparks(x, y, colors, count, minDist, spread, dirAngle = null, cls = 'spellfx-spark') {
        for (let i = 0; i < count; i++) {
            const angle = dirAngle === null
                ? (Math.PI * 2 * i) / count + Math.random() * 0.5
                : dirAngle + (Math.random() - 0.5) * 1.6;
            const dist = minDist + Math.random() * spread;
            animateAndRemove(spawn(cls, x, y, colors), 'scatter', {
                duration: 380 + Math.random() * 200, easing: EASE.burst,
                vars: { dx: Math.cos(angle) * dist, dy: Math.sin(angle) * dist, s1: .3 },
            });
        }
    }

    /** Schnitt-Linie; offset = senkrechter Versatz in px, delay in ms. */
    function slash(x, y, colors, angleDeg, { cls = 'spellfx-slash', offset = 0, delay = 0, duration = 300 } = {}) {
        const rad = angleDeg * Math.PI / 180;
        const ox = -Math.sin(rad) * offset, oy = Math.cos(rad) * offset;
        const el = spawn(cls, x + ox, y + oy, colors);
        el.style.opacity = '0';
        animateAndRemove(el, 'slash', { duration, delay, vars: { rot: angleDeg } });
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
        EASE, canLaunch, colorsFor, centerOf, spawn, playFx, animateAndRemove,
        trailParticle, flash, ring, sparks, slash, hitReact, frameShake, fly, direction,
    };

    return { play, handlers, fx };
})();
