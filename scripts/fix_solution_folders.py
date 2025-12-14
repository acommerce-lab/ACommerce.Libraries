#!/usr/bin/env python3
"""
Restructures ACommerce.Libraries.sln to match the new libs/ folder structure.
Creates new Solution Folders and reassigns projects accordingly.
"""

import re
import uuid
from pathlib import Path

SOLUTION_FILE = "ACommerce.Libraries.sln"
SOLUTION_FOLDER_GUID = "2150E333-8FDC-42A3-9474-1A3956D46DE8"
PROJECT_GUID = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"

# New folder structure with GUIDs
NEW_FOLDERS = {
    "libs": {"guid": str(uuid.uuid4()).upper(), "parent": None},
    "backend": {"guid": str(uuid.uuid4()).upper(), "parent": "libs"},
    "frontend": {"guid": str(uuid.uuid4()).upper(), "parent": "libs"},
    # Backend subfolders
    "core": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "auth": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "catalog": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "sales": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "marketplace": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "messaging": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "files": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "shipping": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "integration": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    "other": {"guid": str(uuid.uuid4()).upper(), "parent": "backend"},
    # Frontend subfolders
    "fe-core": {"guid": str(uuid.uuid4()).upper(), "parent": "frontend", "display": "core"},
    "clients": {"guid": str(uuid.uuid4()).upper(), "parent": "frontend"},
    "realtime": {"guid": str(uuid.uuid4()).upper(), "parent": "frontend"},
    "discovery": {"guid": str(uuid.uuid4()).upper(), "parent": "frontend"},
    # Apps
    "apps": {"guid": str(uuid.uuid4()).upper(), "parent": None},
    "templates": {"guid": str(uuid.uuid4()).upper(), "parent": None},
    "examples": {"guid": str(uuid.uuid4()).upper(), "parent": "apps"},
}

# Path prefix to folder mapping
PATH_TO_FOLDER = {
    r"libs\\backend\\core\\": "core",
    r"libs\\backend\\auth\\": "auth",
    r"libs\\backend\\catalog\\": "catalog",
    r"libs\\backend\\sales\\": "sales",
    r"libs\\backend\\marketplace\\": "marketplace",
    r"libs\\backend\\messaging\\": "messaging",
    r"libs\\backend\\files\\": "files",
    r"libs\\backend\\shipping\\": "shipping",
    r"libs\\backend\\integration\\": "integration",
    r"libs\\backend\\other\\": "other",
    r"libs\\frontend\\core\\": "fe-core",
    r"libs\\frontend\\clients\\": "clients",
    r"libs\\frontend\\realtime\\": "realtime",
    r"libs\\frontend\\discovery\\": "discovery",
    r"Apps\\": "apps",
    r"Templates\\": "templates",
    r"Examples\\": "examples",
}

def read_solution():
    with open(SOLUTION_FILE, "r", encoding="utf-8-sig") as f:
        return f.read()

def write_solution(content):
    with open(SOLUTION_FILE, "w", encoding="utf-8-sig") as f:
        f.write(content)

def parse_projects(content):
    """Extract all projects and solution folders from the solution."""
    pattern = r'Project\("\{([^}]+)\}"\)\s*=\s*"([^"]+)",\s*"([^"]+)",\s*"\{([^}]+)\}"'
    projects = []
    folders = []
    
    for match in re.finditer(pattern, content):
        type_guid = match.group(1)
        name = match.group(2)
        path = match.group(3)
        proj_guid = match.group(4)
        
        if type_guid == SOLUTION_FOLDER_GUID:
            folders.append({"name": name, "guid": proj_guid, "path": path})
        else:
            projects.append({"name": name, "path": path, "guid": proj_guid, "type": type_guid})
    
    return projects, folders

def get_folder_for_project(path):
    """Determine which new folder a project belongs to."""
    for prefix, folder in PATH_TO_FOLDER.items():
        if path.startswith(prefix.replace("\\\\", "\\")):
            return folder
    return None

def generate_folder_declarations():
    """Generate Project declarations for new solution folders."""
    lines = []
    for name, info in NEW_FOLDERS.items():
        display_name = info.get("display", name)
        guid = info["guid"]
        lines.append(f'Project("{{{SOLUTION_FOLDER_GUID}}}") = "{display_name}", "{display_name}", "{{{guid}}}"')
        lines.append("EndProject")
    return "\n".join(lines)

def generate_nested_projects(projects):
    """Generate NestedProjects section mapping projects to folders."""
    lines = []
    
    # First, nest the folder hierarchy
    for name, info in NEW_FOLDERS.items():
        if info["parent"]:
            parent_guid = NEW_FOLDERS[info["parent"]]["guid"]
            lines.append(f"\t\t{{{info['guid']}}} = {{{parent_guid}}}")
    
    # Then nest projects
    for proj in projects:
        folder = get_folder_for_project(proj["path"])
        if folder and folder in NEW_FOLDERS:
            folder_guid = NEW_FOLDERS[folder]["guid"]
            lines.append(f"\t\t{{{proj['guid']}}} = {{{folder_guid}}}")
    
    return "\n".join(lines)

def main():
    print("=== ACommerce Solution Restructurer ===\n")
    
    # Backup
    content = read_solution()
    backup_path = SOLUTION_FILE + ".bak"
    with open(backup_path, "w", encoding="utf-8-sig") as f:
        f.write(content)
    print(f"Backup created: {backup_path}")
    
    # Parse existing
    projects, old_folders = parse_projects(content)
    print(f"Found {len(projects)} projects and {len(old_folders)} old folders")
    
    # Find positions
    global_section_match = re.search(r'Global\s*\n', content)
    if not global_section_match:
        print("ERROR: Could not find Global section")
        return
    
    # Remove old solution folder declarations
    old_folder_guids = {f["guid"] for f in old_folders}
    
    # Build new content
    lines = content.split('\n')
    new_lines = []
    skip_until_endproject = False
    in_nested_projects = False
    
    for line in lines:
        # Skip old solution folder declarations
        if f'"{{{SOLUTION_FOLDER_GUID}}}"' in line and 'Project(' in line:
            skip_until_endproject = True
            continue
        if skip_until_endproject:
            if line.strip() == "EndProject":
                skip_until_endproject = False
            continue
        
        # Skip old NestedProjects section
        if "GlobalSection(NestedProjects)" in line:
            in_nested_projects = True
            continue
        if in_nested_projects:
            if "EndGlobalSection" in line:
                in_nested_projects = False
            continue
        
        new_lines.append(line)
    
    content = '\n'.join(new_lines)
    
    # Insert new folder declarations before Global
    folder_declarations = generate_folder_declarations()
    content = content.replace("Global\n", folder_declarations + "\nGlobal\n")
    
    # Insert new NestedProjects section before EndGlobal
    nested_projects = generate_nested_projects(projects)
    nested_section = f"\tGlobalSection(NestedProjects) = preSolution\n{nested_projects}\n\tEndGlobalSection\n"
    content = content.replace("EndGlobal", nested_section + "EndGlobal")
    
    # Write
    write_solution(content)
    print("Solution file updated successfully!")
    
    # Summary
    folder_counts = {}
    for proj in projects:
        folder = get_folder_for_project(proj["path"])
        if folder:
            folder_counts[folder] = folder_counts.get(folder, 0) + 1
    
    print("\nProjects by folder:")
    for folder, count in sorted(folder_counts.items()):
        print(f"  {folder}: {count}")
    
    print(f"\nUnassigned projects: {len(projects) - sum(folder_counts.values())}")

if __name__ == "__main__":
    main()
