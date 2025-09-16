#!/bin/bash
set -e
set -u


/opt/firebird/bin/registerDatabase.sh "dotest" "$FIREBIRD_DATABASE"
echo "Register aliases with default settings for databases"
