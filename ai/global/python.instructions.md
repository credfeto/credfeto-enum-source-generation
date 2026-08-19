# Python Instructions

> Load when: any `pyproject.toml`, `requirements.txt`, `setup.py`, `setup.cfg`, or `.py` file is present, or Python work is needed.

[Back to Global Instructions Index](index.md)

## Virtual Environments (MANDATORY)

- Every Python project must use an isolated virtual environment (e.g. `venv`, `virtualenv`, or an equivalent tool-managed environment such as `poetry` or `pipenv`); never install packages into, or otherwise modify, the system Python installation.
- Create the virtual environment inside the project (e.g. `.venv`) and activate it before installing dependencies or running any Python command.
- Never install packages outside an active virtual environment; never pass `--user` or `--break-system-packages` to `pip install` to bypass this.
- Add the virtual environment directory to `.gitignore`; never commit it.

## Python Version (MANDATORY)

- Python 2 is prohibited: never write new Python 2 code, and never add Python 2 tooling, syntax, or dependencies to a project.
- If Python 2 code is encountered, migrate it to Python 3 as part of the current work rather than maintaining or extending it as-is.
- Always target the latest stable Python 3 release available at the time of writing; do not pin to an older Python 3 minor version without a specific, stated justification (e.g. a dependency or runtime constraint).

## Code Style (MANDATORY)

- Generated code must be **Pythonic**: follow PEP 8 and use idiomatic constructs (context managers, comprehensions, generators, dataclasses, `enumerate`/`zip`, f-strings, etc.) rather than direct translations of patterns from other languages.
- Code must be **highly modular**: small, single-responsibility functions, classes, and modules rather than large monolithic ones.
- Code must be written to be **easily testable and mockable**: prefer dependency injection over hard-coded globals/singletons, and keep I/O, network, and filesystem access at the edges so business logic can be tested and mocked in isolation.
