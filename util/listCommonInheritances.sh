#!/bin/bash
set -e

inputFile="$1"

function usage() {
  echo "Usage: $0 <filepath of activityData txt>"
  echo "Lists all inheritances whose child activities have children"
  exit 1
}

tempFile=/tmp/allInheritances.txt

grep "<Inheritance>" "$inputFile" > "$tempFile"

if stat "$inputFile" >/dev/null; then
  cat "$tempFile" | grep -o '<Parent><Name>[^<]*' | sed 's/<Parent><Name>//' | sort | uniq | sed 's|^|grep "<Child><Name>|' | sed 's|$|</Name></Child>" '"$tempFile"'|' | bash | sed 's|<DiscoveryDate>.*</DiscoveryDate>||' | sed 's|\(<Child><Name>[^<]*</Name></Child>\)\(<Parent><Name>[^<]*</Name></Parent>\)|\2\1|' | sort
else
  usage
fi
