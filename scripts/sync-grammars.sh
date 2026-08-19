#!/usr/bin/env sh

set -eu

repository_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
source_directory="$repository_root/grammars/mixin_expressions/syntaxes"
docs_directory="$repository_root/docs/grammars"

mkdir -p "$docs_directory"
cp "$source_directory"/*.tmLanguage.json "$docs_directory"/
