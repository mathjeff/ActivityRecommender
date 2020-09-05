#!/bin/bash
set -e

cd "$(dirname $0)/.."

# given a filepath and a regex matching text containing a version in that file,
# increments that version and writes the result to that file
function incrementVersion() {
  filepath="$1"
  matcher="$2"
  existingText="$(grep -o "$matcher" "$filepath")"
  prefixAndMiddle="$(echo "$existingText" | sed 's/[^0-9]*$//')"
  suffix="$(echo "$existingText" | sed "s/^${prefixAndMiddle}//")"
  prefix="$(echo "$prefixAndMiddle" | sed 's/[0-9]*$//')"
  version="$(echo "$prefixAndMiddle" | sed "s/^${prefix}//")"
  newVersion="$(($version + 1))"


  echo sed -i "s/${prefix}${version}${suffix}/${prefix}${newVersion}${suffix}/" "$filepath"
       sed -i "s/${prefix}${version}${suffix}/${prefix}${newVersion}${suffix}/" "$filepath"
}

for info in $(find ActRec -name AssemblyInfo.cs); do
  incrementVersion $info 'AssemblyVersion("[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*'
done

androidManifest=ActRec/ActRec.Android/Properties/AndroidManifest.xml

incrementVersion $androidManifest 'android:versionName="[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*"'
incrementVersion $androidManifest 'android:versionCode="[0-9][0-9]*"'

incrementVersion ActRec/ActRec.iOS/Info.plist '<string>[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*'

echo
echo done updating versions
