// Combat effects – spells/abilities: projectile from the player portrait to the enemy portrait + impact.
(() => {
    const { PLAYER, ENEMY, TRAIL_INTERVAL, canLaunch, colorsFor, centerOf, spawn,
            trailParticle, flash, ring, sparks, slash, hitReact, frameShake, fly, direction } = window.spellFx.fx;

    function launchSpell(opts) {
        if (!canLaunch()) return;
        const from = centerOf(PLAYER);
        const to = centerOf(ENEMY);
        if (!from || !to) return;

        const colors = colorsFor(opts.icon);
        const melee = !!opts.melee;
        const dir = direction(from, to);

        // Wirker-Impuls am Spieler-Portrait
        from.el.animate([
            { transform: 'scale(1)', filter: 'brightness(1)' },
            { transform: 'scale(1.08)', filter: `brightness(1.5) drop-shadow(0 0 8px ${colors[0]})`, offset: 0.4 },
            { transform: 'scale(1)', filter: 'brightness(1)' },
        ], { duration: 280, easing: 'ease-out' });

        const proj = spawn(melee ? 'spellfx-projectile melee' : 'spellfx-projectile', from.x, from.y, colors);
        const aura = document.createElement('div');
        aura.className = 'spellfx-aura';
        const icon = document.createElement('img');
        icon.className = 'spellfx-icon';
        icon.src = opts.icon;
        icon.alt = '';
        icon.onerror = () => { icon.style.visibility = 'hidden'; };
        proj.append(aura, icon);

        let lastTrail = 0;
        fly(from, to, {
            el: proj,
            duration: opts.duration || 450,
            arc: melee ? 0.08 : 0.28,
            onFrame: (x, y, angle, raw, now) => {
                const scale = melee ? 1 : 0.7 + 0.45 * Math.sin(Math.PI * Math.min(1, raw * 1.2));
                proj.style.transform = `translate(-50%, -50%) scale(${scale})`;
                aura.style.transform = `rotate(${angle}deg)`;
                if (melee) icon.style.transform = `rotate(${raw * 540}deg)`;
                if (now - lastTrail > TRAIL_INTERVAL) {
                    lastTrail = now;
                    trailParticle(x, y, colors, melee ? 'spellfx-streak' : 'spellfx-particle', melee ? 220 : 420);
                }
            },
            onArrive: () => {
                // Pure debuff (Curse of Agony): no damage impact, only the debuff effect
                if (!opts.debuffOnly) spellImpact(to, dir, colors, !!opts.crit, melee);
                if (opts.debuff) window.spellFx.fx.debuffFx?.(to, opts);   // auras.js
            },
        });
    }

    function spellImpact(target, dir, colors, crit, melee) {
        const { x, y } = target;
        flash(x, y, colors, crit ? 1.8 : 1.3, crit ? 380 : 260);
        ring(x, y, colors, crit ? 2.6 : 1.9, crit ? 560 : 440);
        if (melee)   // Two crossed slashes
            for (const angle of crit ? [-35, 35, 0] : [-35, 35])
                slash(x, y, colors, angle);
        sparks(x, y, colors, crit ? 18 : 10, crit ? 50 : 30, crit ? 60 : 35);
        hitReact(target, dir, colors, crit ? 9 : 5);
        if (crit) frameShake(target);
    }

    window.spellFx.handlers.spell = launchSpell;
})();
