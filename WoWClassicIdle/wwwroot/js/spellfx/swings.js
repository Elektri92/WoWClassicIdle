// Combat effects – auto-attacks: melee as a lunge + weapon/creature-dependent hit,
// bow and wand as a projectile. Applies to player and enemy.
(() => {
    const { PLAYER, ENEMY, TRAIL_INTERVAL, canLaunch, centerOf, spawn,
            trailParticle, flash, ring, sparks, slash, hitReact, frameShake, fly, direction, animateAndRemove } = window.spellFx.fx;
    const LUNGE_IMPACT = 0.4;    // Share of the lunge at which the hit lands
    const STEEL = ['#e8eef5', '#8a9bb0', '#ffffff'];       // Auto-Attack Spieler
    const STEEL_CRIT = ['#ffe9a0', '#ffb020', '#ffffff'];
    const WAND = ['#e0c8ff', '#8a4dff', '#ffffff'];
    const ENEMY_COLORS = {
        splash: ['#b4ff6a', '#3c9a1a', '#eaffd0'],          // Schleim
        claw: ['#ff7a6a', '#b81d12', '#ffd6d0'],
    };
    const ENEMY_DEFAULT = ['#ff8a7a', '#c0281a', '#ffe0da'];
    // Strength of the enemy attack: lunge = reach, tilt = swing-out, grow = size on the hit, power = force of the impact
    const RANKS = {
        normal: { lunge: 1, tilt: 1, grow: 1.06, power: 1 },
        elite: { lunge: 1.7, tilt: 1.4, grow: 1.16, power: 1.4 },
        boss: { lunge: 2.8, tilt: 2, grow: 1.32, power: 2, glow: '#ff3010' },
    };

    /** Lunge of the attacker towards the target; the hit lands after impactMs. */
    function lunge(from, dir, impactMs, rank = RANKS.normal) {
        const dist = 22 * rank.lunge, back = 6 * rank.lunge;
        const tiltBack = (dir.x >= 0 ? -6 : 6) * rank.tilt, tiltHit = (dir.x >= 0 ? 8 : -8) * rank.tilt;
        const glow = rank.glow ? ` drop-shadow(0 0 16px ${rank.glow})` : '';
        from.el.animate([
            { transform: 'translate(0, 0) rotate(0deg)', filter: 'brightness(1)' },
            { transform: `translate(${-dir.x * back}px, ${-dir.y * back}px) rotate(${tiltBack}deg)`, filter: `brightness(${rank.glow ? 1.5 : 1})${glow}`, offset: LUNGE_IMPACT * 0.45 },
            { transform: `translate(${dir.x * dist}px, ${dir.y * dist}px) rotate(${tiltHit}deg) scale(${rank.grow})`, filter: 'brightness(1)', offset: LUNGE_IMPACT },
            { transform: 'translate(0, 0) rotate(0deg)', filter: 'brightness(1)' },
        ], { duration: impactMs / LUNGE_IMPACT, easing: 'ease-in-out' });
    }

    /**
     * opts: { attacker: 'player'|'enemy', style, crit, duration (ms until the hit), icon }
     * style: slash | chop | stab | blunt | claw | splash | arrow | wand
     */
    function swing(opts) {
        const byEnemy = opts.attacker === 'enemy';
        const from = centerOf(byEnemy ? ENEMY : PLAYER);
        const to = centerOf(byEnemy ? PLAYER : ENEMY);
        if (!from || !to) return;

        const crit = !!opts.crit;
        const colors = byEnemy
            ? (ENEMY_COLORS[opts.style] || ENEMY_DEFAULT)
            : (crit ? STEEL_CRIT : STEEL);
        const dir = direction(from, to);
        const impactMs = opts.duration || 110;
        const rank = RANKS[opts.rank] || RANKS.normal;

        if (opts.style === 'arrow') return shootArrow(from, to, dir, colors, crit, impactMs);
        if (opts.style === 'wand') return shootBolt(from, to, dir, crit ? STEEL_CRIT : WAND, crit, impactMs);

        lunge(from, dir, impactMs, rank);
        setTimeout(() => {
            meleeImpact(to, dir, opts.style, colors, crit, byEnemy, rank);
            rankImpact(to, dir, colors, opts.rank);
        }, impactMs);
    }

    /** Extra layers on top of the normal hit: elites strike wider, boss hits also light up the edges of the screen. */
    function rankImpact(target, dir, colors, rank) {
        if (rank !== 'elite' && rank !== 'boss') return;
        const { x, y } = target;
        const diag = dir.x >= 0 ? -40 : 40;
        if (rank === 'elite') {
            slash(x, y, colors, diag, { cls: 'spellfx-slash heavy', duration: 340 });
            ring(x, y, colors, 1.7, 380);
            sparks(x, y, colors, 6, 30, 30, dir.angle);
            return;
        }
        slash(x, y, colors, diag, { cls: 'spellfx-slash colossal', duration: 420 });
        slash(x, y, colors, -diag, { cls: 'spellfx-slash colossal', delay: 70, duration: 420 });
        flash(x, y, colors, 2.4, 340);
        ring(x, y, colors, 2.6, 480);
        ring(x, y, colors, 3.8, 620);
        sparks(x, y, colors, 14, 40, 45, dir.angle);
        sparks(x, y, colors, 10, 30, 35, null, 'spellfx-dust');
        animateAndRemove(spawn('spellfx-boss-hit', 0, 0, colors), 'fade', { duration: 460 });
    }

    function meleeImpact(target, dir, style, colors, crit, byEnemy, rank = RANKS.normal) {
        const { x, y } = target;
        const deg = dir.angle * 180 / Math.PI;
        // Strike angle: diagonally from above in the direction of movement
        const diag = dir.x >= 0 ? -40 : 40;
        const k = (crit ? 1.5 : 1) * rank.power;
        const force = rank.power;

        switch (style) {
            case 'chop':    // Axe: heavy, wide blow from above
                slash(x, y, colors, dir.x >= 0 ? -65 : 65, { cls: 'spellfx-slash heavy', duration: 340 });
                if (crit) slash(x, y, colors, dir.x >= 0 ? -55 : 55, { cls: 'spellfx-slash heavy', offset: 10, delay: 60 });
                sparks(x, y, colors, Math.round(7 * k), 25, 30, dir.angle);
                hitReact(target, dir, colors, (crit ? 10 : 7) * force, true);
                frameShake(target, (crit ? 5 : 2.5) * force);
                break;
            case 'stab':    // Dagger / stinger: two fast stabs in the attack direction
                slash(x, y, colors, deg, { cls: 'spellfx-thrust', offset: -6, duration: 220 });
                slash(x, y, colors, deg, { cls: 'spellfx-thrust', offset: 6, delay: 70, duration: 220 });
                sparks(x, y, colors, Math.round(4 * k), 15, 25, dir.angle);
                hitReact(target, dir, colors, (crit ? 7 : 4) * force);
                break;
            case 'blunt':   // Mace / staff / boar: force instead of blade
                ring(x, y, colors, crit ? 2.0 : 1.4, 380);
                flash(x, y, colors, crit ? 1.2 : 0.8, 220);
                sparks(x, y, colors, Math.round(8 * k), 20, 25, null, 'spellfx-dust');
                hitReact(target, dir, colors, (crit ? 11 : 8) * force, true);
                frameShake(target, (crit ? 5 : 3) * force);
                break;
            case 'claw':    // Bear / wolf / dragon / Cat Form: three parallel claw marks
                for (const [i, off] of [-11, 0, 11].entries())
                    slash(x, y, colors, diag - 10, { cls: 'spellfx-claw', offset: off, delay: i * 35, duration: 300 });
                sparks(x, y, colors, Math.round(5 * k), 18, 25, dir.angle);
                hitReact(target, dir, colors, (crit ? 8 : 5) * force);
                break;
            case 'splash':  // Slime: acid splashes
                flash(x, y, colors, crit ? 1.2 : 0.9, 260);
                sparks(x, y, colors, Math.round(10 * k), 18, 30, null, 'spellfx-droplet');
                hitReact(target, dir, colors, (crit ? 7 : 4) * force, true);
                break;
            default:        // Sword / humanoids: one clean diagonal slash
                slash(x, y, colors, diag);
                if (crit) slash(x, y, colors, -diag, { delay: 60 });
                sparks(x, y, colors, Math.round(5 * k), 18, 28, dir.angle);
                hitReact(target, dir, colors, (crit ? 8 : 5) * force);
                break;
        }
        if (crit) {
            flash(x, y, colors, 1.1, 240);
            frameShake(target, 4 * force);
        }
        // Every attack of a stronger enemy shakes the target frame, whatever the weapon
        if (force > 1) frameShake(target, 3.5 * force);
        // Enemy hit: brief red edge shimmer on the player frame
        if (byEnemy) {
            target.el.closest('.unit-frame')?.animate([
                { boxShadow: '0 0 0 0 rgba(255,40,20,0)' },
                { boxShadow: `0 0 ${(crit ? 26 : 16) * force}px ${2 * force}px rgba(255,40,20,.75)`, offset: 0.25 },
                { boxShadow: '0 0 0 0 rgba(255,40,20,0)' },
            ], { duration: 420 * force, easing: 'ease-out' });
        }
    }

    function shootArrow(from, to, dir, colors, crit, duration) {
        if (!canLaunch()) return;
        // Archer: brief pull-back on firing
        from.el.animate([
            { transform: 'translate(0, 0)' },
            { transform: `translate(${-dir.x * 5}px, ${-dir.y * 5}px)`, offset: 0.3 },
            { transform: 'translate(0, 0)' },
        ], { duration: 220, easing: 'ease-out' });

        const arrow = spawn('spellfx-arrow', from.x, from.y, colors);
        let lastTrail = 0;
        fly(from, to, {
            el: arrow, duration, arc: 0.06, accelerate: false,
            onFrame: (x, y, angle, raw, now) => {
                arrow.style.transform = `translate(-50%, -50%) rotate(${angle}deg)`;
                if (now - lastTrail > TRAIL_INTERVAL * 1.5) {
                    lastTrail = now;
                    trailParticle(x, y, colors, 'spellfx-streak faint', 180);
                }
            },
            onArrive: () => {
                const deg = dir.angle * 180 / Math.PI;
                slash(to.x, to.y, colors, deg, { cls: 'spellfx-thrust', duration: 200 });
                sparks(to.x, to.y, colors, crit ? 8 : 4, 15, 22, dir.angle);
                hitReact(to, dir, colors, crit ? 7 : 4);
                if (crit) { flash(to.x, to.y, colors, 1.1, 240); frameShake(to); }
            },
        });
    }

    function shootBolt(from, to, dir, colors, crit, duration) {
        if (!canLaunch()) return;
        const bolt = spawn('spellfx-bolt', from.x, from.y, colors);
        let lastTrail = 0;
        fly(from, to, {
            el: bolt, duration, arc: 0.15,
            onFrame: (x, y, angle, raw, now) => {
                bolt.style.transform = `translate(-50%, -50%) rotate(${angle}deg)`;
                if (now - lastTrail > TRAIL_INTERVAL) {
                    lastTrail = now;
                    trailParticle(x, y, colors, 'spellfx-particle small', 300);
                }
            },
            onArrive: () => {
                flash(to.x, to.y, colors, crit ? 1.2 : 0.8, 240);
                ring(to.x, to.y, colors, crit ? 1.6 : 1.1, 340);
                sparks(to.x, to.y, colors, crit ? 10 : 6, 18, 25);
                hitReact(to, dir, colors, crit ? 7 : 4);
                if (crit) frameShake(to);
            },
        });
    }

    window.spellFx.handlers.swing = swing;
})();
