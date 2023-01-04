#!/bin/bash

set -e

git pull
git checkout master
git pull
git merge dev
git push
git checkout dev
