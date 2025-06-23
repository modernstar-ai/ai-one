# Agile Chat Documentation Website

This project uses [MkDocs](https://www.mkdocs.org/) to generate a static documentation website from the `docs/` folder.

## Prerequisites

- Python 3.7+
- [pip](https://pip.pypa.io/en/stable/)

## Setup Instructions

1. **Create a virtual environment** (recommended):

   ```powershell
   python -m venv .venv
   .venv\Scripts\Activate.ps1
   ```

2. **Install MkDocs and Material theme**:

   ```powershell
   pip install mkdocs mkdocs-material
   ```

3. **Preview the documentation site locally**:

   ```powershell
   mkdocs serve
   ```

   Then open <http://127.0.0.1:8000> in your browser.

4. **Build the static site**:

   ```powershell
   mkdocs build
   ```

   The generated site will be in the `site/` folder at the project root.

## Notes

- All documentation source files are in the `docs/` folder.
- The navigation and site configuration are in `mkdocs.yml` at the project root.
