#!/bin/bash
set -e
set -u

echo "Register aliases with default settings for databasees"

/opt/firebird/bin/registerDatabase.sh "DOTEST.fdb" "$FIREBIRD_DATABASE"
echo "Alias DOTEST.fdb => $FIREBIRD_DATABASE registered"

/opt/firebird/bin/registerDatabase.sh "dotest" "$FIREBIRD_DATABASE"
echo "Alias dotest => $FIREBIRD_DATABASE registered"