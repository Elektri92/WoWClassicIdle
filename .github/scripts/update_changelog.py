#!/usr/bin/env python3
"""Adds the changelog entry of a new build, using the commits since the previous release tag.

Usage: update_changelog.py <version> <changelog.json>

Each commit carries its player-facing text in body lines of the form "Changelog: <English text>".
Commits without such a line fall back to a generic text, because commit subjects are German
and the game UI is English-only.
"""
import datetime
import json
import re
import subprocess
import sys

CHANGELOG_LINE = re.compile(r'^[\s*-]*Changelog:\s*(.+?)\s*$', re.MULTILINE)
INTERNAL_COMMIT = re.compile(r'^(chore|ci|docs|refactor|style|test|build)\b')

GENERIC_CHANGE = 'Minor fixes and improvements.'
NO_GAMEPLAY_CHANGE = 'Maintenance update with no gameplay changes.'


def git(*args):
    return subprocess.run(['git', *args], check=True, capture_output=True, text=True).stdout


def commit_messages():
    previous_tag = git('describe', '--tags', '--abbrev=0', '--match', 'v*').strip()
    log = git('log', '--reverse', '--no-merges', '--format=%B%x00', f'{previous_tag}..HEAD')
    return [message.strip() for message in log.split('\0') if message.strip()]


def collect_changes(messages):
    changes = []
    unannounced = []
    for message in messages:
        lines = CHANGELOG_LINE.findall(message)
        changes += lines
        subject = message.splitlines()[0]
        if not lines and not INTERNAL_COMMIT.match(subject):
            unannounced.append(subject)

    for subject in unannounced:
        print(f'::warning::Commit without Changelog line: {subject}')

    if not changes:
        changes = [GENERIC_CHANGE if unannounced else NO_GAMEPLAY_CHANGE]
    return list(dict.fromkeys(changes))


def main():
    version, path = sys.argv[1:3]
    with open(path, encoding='utf-8') as f:
        entries = json.load(f)

    entry = {
        'version': version,
        'date': datetime.datetime.now(datetime.timezone.utc).date().isoformat(),
        'changes': collect_changes(commit_messages()),
    }
    entries = [entry] + [e for e in entries if e['version'] != version]

    with open(path, 'w', encoding='utf-8') as f:
        json.dump(entries, f, indent=2, ensure_ascii=False)
        f.write('\n')


if __name__ == '__main__':
    main()
