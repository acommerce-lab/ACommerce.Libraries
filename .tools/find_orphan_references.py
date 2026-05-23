#!/usr/bin/env python3
import os
import re
import json
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SLN = ROOT / 'ACommerce.Libraries.sln'

proj_pattern = re.compile(r'"([^"]+\\[^"]+\.csproj)"')
projects_in_sln = set()
with SLN.open('r', encoding='utf-8', errors='ignore') as f:
    for line in f:
        m = proj_pattern.search(line)
        if m:
            rel = m.group(1).replace('\\', os.sep)
            norm = (ROOT / rel).resolve()
            projects_in_sln.add(str(norm))

# all projects
all_projects = set(str(p.resolve()) for p in ROOT.rglob('*.csproj'))
orphans = set(all_projects) - set(projects_in_sln)

# Map orphan -> set of referencing projects (that are in sln)
orphan_refs = {o: set() for o in orphans}

# helper to resolve projectreference include to absolute path
def resolve_reference(proj_file, include_path):
    # include_path may use backslashes or forward slashes
    include_path = include_path.replace('\\', os.sep).replace('/', os.sep)
    candidate = (Path(proj_file).parent / include_path)
    try:
        return str(candidate.resolve())
    except Exception:
        return str(candidate)

# parse each project in sln and collect ProjectReference includes
for proj in projects_in_sln:
    try:
        tree = ET.parse(proj)
        root = tree.getroot()
        ns = ''
        if root.tag.startswith('{'):
            ns = root.tag.split('}')[0] + '}'
        for pr in root.findall('.//{}ProjectReference'.format(ns)):
            inc = pr.get('Include')
            if not inc:
                # sometimes <ProjectReference><Include>...</Include></ProjectReference>
                inc_node = pr.find('{}Include'.format(ns))
                if inc_node is not None:
                    inc = inc_node.text
            if inc:
                resolved = resolve_reference(proj, inc)
                if resolved in orphans:
                    orphan_refs[resolved].add(str(proj))
    except ET.ParseError:
        # skip malformed
        pass
    except Exception:
        pass

# Filter to only orphans that are referenced
referenced_orphans = {os.path.relpath(k, ROOT): [os.path.relpath(p, ROOT) for p in sorted(v)]
                      for k, v in orphan_refs.items() if v}

out_dir = ROOT / 'analysis'
out_dir.mkdir(exist_ok=True)
with open(out_dir / 'orphan_referenced_by.json', 'w', encoding='utf-8') as f:
    json.dump(referenced_orphans, f, indent=2, ensure_ascii=False)

import csv
with open(out_dir / 'orphan_referenced_by.csv', 'w', newline='', encoding='utf-8') as f:
    w = csv.writer(f)
    w.writerow(['orphan_relative_path','referenced_by_relative_paths'])
    for orphan, refs in referenced_orphans.items():
        w.writerow([orphan, ';'.join(refs)])

# Print summary
print('Repository root:', ROOT)
print('Total .csproj found:', len(all_projects))
print('Projects in sln:', len(projects_in_sln))
print('Orphans (not in sln):', len(orphans))
print('Orphans referenced by sln projects:', len(referenced_orphans))
print('Output:')
print(' -', out_dir / 'orphan_referenced_by.json')
print(' -', out_dir / 'orphan_referenced_by.csv')

if len(referenced_orphans) == 0:
    print('\nNo orphan project is referenced by projects inside the solution.')
