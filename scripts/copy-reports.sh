#!/usr/bin/env bash
SRC="$(pwd)/MediaOrganizerApp.WebApi/Reports_RDLC"
DST="$(pwd)/ReportsRenderer/Reports_RDLC"
mkdir -p "$DST"
cp -v "$SRC"/*.rdlc "$DST" || echo "No RDLC files to copy"
echo "Done"