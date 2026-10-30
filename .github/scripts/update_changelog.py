#!/usr/bin/env python3
"""Adds the changelog entry of a new build, using the commits since the previous release tag.

Usage: update_changelog.py <version> <changelog.json>

Each commit carries its player-facing text in body lines of the form
"Changelog(<Category>): <English text>", where the category is one of CATEGORIES.
Lines without a known category land in FALLBACK_CATEGORY. Commits without such a line fall back
to a generic text, because commit subjects are German and the game UI is English-only.
"""
import datetime
import json
import re
import subprocess
import sys

CHANGELOG_LINE = re.compile(r'^[\s*-]*Changelog(?:\(([^)]*)\))?:\s*(.+?)\s*$', re.MULTILINE)
INTERNAL_COMMIT = re.compile(r'^(chore|ci|docs|refactor|style|test|build)\b')

# Display order of the sections in a changelog entry.
CATEGORIES = ['Classes', 'Combat', 'Items', 'Professions', 'World', 'Interface', 'System']
FALLBACK_CATEGORY = 'General'

GENERIC_CHANGE = 'Minor fixes and improvements.'
NO_GAMEPLAY_CHANGE = 'Maintenance update with no gameplay changes.'


def git(*args):
    return subprocess.run(['git', *args], check=True, capture_output=True, text=True).stdout


def commit_messages():
    previous_tag = git('describe', '--tags', '--abbrev=0', '--match', 'v*').strip()
    log = git('log', '--reverse', '--no-merges', '--format=%B%x00', f'{previous_tag}..HEAD')
    return [message.strip() for message in log.split('\0') if message.strip()]


def resolve_category(category, subject):
    known = {name.lower(): name for name in CATEGORIES}
    resolved = known.get(category.strip().lower())
    if not resolved:
        print(f'::warning::Changelog line without known category ({category or "none"}): {subject}')
    return resolved or FALLBACK_CATEGORY


def collect_sections(messages):
    changes = []
    unannounced = []
    for message in messages:
        lines = CHANGELOG_LINE.findall(message)
        subject = message.splitlines()[0]
        changes += [(resolve_category(category, subject), text) for category, text in lines]
        if not lines and not INTERNAL_COMMIT.match(subject):
            unannounced.append(subject)

    for subject in unannounced:
        print(f'::warning::Commit without Changelog line: {subject}')

    if not changes:
        changes = [(FALLBACK_CATEGORY, GENERIC_CHANGE if unannounced else NO_GAMEPLAY_CHANGE)]

    return [
        {'category': category, 'changes': texts}
        for category in CATEGORIES + [FALLBACK_CATEGORY]
        if (texts := list(dict.fromkeys(text for c, text in changes if c == category)))
    ]


def main():
    version, path = sys.argv[1:3]
    with open(path, encoding='utf-8') as f:
        entries = json.load(f)

    entry = {
        'version': version,
        'date': datetime.datetime.now(datetime.timezone.utc).date().isoformat(),
        'sections': collect_sections(commit_messages()),
    }
    entries = [entry] + [e for e in entries if e['version'] != version]

    with open(path, 'w', encoding='utf-8') as f:
        json.dump(entries, f, indent=2, ensure_ascii=False)
        f.write('\n')


if __name__ == '__main__':
    main()
