#!/usr/bin/env sh

# abort on errors
set -e

cd Assets/UnityCommon

git init
git add -A
git add -f Plugins/NLayer.dll

git commit -m 'publish'
git push -f git@github.com:Elringus/UnityCommon.git master:package

cd -