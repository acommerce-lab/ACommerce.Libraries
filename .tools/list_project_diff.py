#!/usr/bin/env python3
import os
import re
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SLN = ROOT / 'ACommerce.Libraries.sln'

# Read sln and extract project relative paths
proj_pattern = re.compile(r'"([^"]+\\[^"]+\.csproj)"')
projects_in_sln = set()
with SLN.open('r', encoding='utf-8', errors='ignore') as f:
    for line in f:
        m = proj_pattern.search(line)
        if m:
            rel = m.group(1).replace('\\', os.sep)
            norm = (ROOT / rel).resolve()
            projects_in_sln.add(str(norm))

# Find all .csproj files under root
all_projects = set()
for p in ROOT.rglob('*.csproj'):
    all_projects.add(str(p.resolve()))

# Compute difference
projects_not_in_sln = sorted(all_projects - projects_in_sln)
projects_only_in_sln = sorted(projects_in_sln - all_projects)

out_dir = ROOT / 'analysis'
out_dir.mkdir(exist_ok=True)

# Write CSV and JSON outputs
import csv

with open(out_dir / 'projects_not_in_sln.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.writer(f)
    writer.writerow(['absolute_path','relative_to_repo'])
    for p in projects_not_in_sln:
        writer.writerow([p, os.path.relpath(p, ROOT)])

with open(out_dir / 'projects_in_sln.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.writer(f)
    writer.writerow(['absolute_path','relative_to_repo'])
    for p in sorted(projects_in_sln):
        writer.writerow([p, os.path.relpath(p, ROOT)])

with open(out_dir / 'projects_not_in_sln.json', 'w', encoding='utf-8') as f:
    json.dump([os.path.relpath(p, ROOT) for p in projects_not_in_sln], f, indent=2, ensure_ascii=False)

# Summary print
print('Repository root:', ROOT)
print('Total .csproj found:', len(all_projects))
print('Total projects listed in sln:', len(projects_in_sln))
print('Projects not in sln (count):', len(projects_not_in_sln))
print('Output written to:', out_dir)

if projects_only_in_sln:
    print('\nWarning: the following paths are referenced in the .sln but do not exist in the file system:')
    for p in projects_only_in_sln:
        print(' -', os.path.relpath(p, ROOT))

# Exit code 0
