#!/bin/bash

# Color definitions
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print custom messages
print_message() {
    local color=$1
    local message=$2
    echo -e "${color}[ INFO ] ${message}${NC}"
}

# Normal deploy (copy and prepare DLLs)
print_message "${BLUE}" "Running dotnet publish..."
dotnet publish -f net8.0 -c Release
exit_code=$?

if [ $exit_code -ne 0 ]; then
    print_message "${RED}" "dotnet publish failed with exit code $exit_code"
    exit 1
else
    print_message "${GREEN}" "dotnet publish completed successfully"
fi

# Copy main plugin folder to plugins folder, ignoring specific file names
print_message "${YELLOW}" "Copying main plugin files..."
rsync -a --quiet ./src/bin/K4-Zenith/plugins/K4-Zenith/ ./Zenith/plugins/K4-Zenith/

# Copy shared folder's dirs to shared folder
print_message "${YELLOW}" "Copying shared files..."
rsync -a --quiet ./src/bin/K4-Zenith/shared/ ./Zenith/shared/
rsync -a --quiet ./src-api/bin/K4-ZenithAPI/ ./Zenith/shared/K4-ZenithAPI/

# Copy modules to plugins folder
print_message "${YELLOW}" "Copying TimeStats module..."
rsync -a --quiet ./modules/time-stats/bin/K4-Zenith-TimeStats/ ./Zenith/plugins/K4-Zenith-TimeStats/

print_message "${YELLOW}" "Copying Ranks module..."
rsync -a --quiet ./modules/ranks/bin/K4-Zenith-Ranks/ ./Zenith/plugins/K4-Zenith-Ranks/

print_message "${YELLOW}" "Copying Statistics module..."
rsync -a --quiet ./modules/statistics/bin/K4-Zenith-Stats/ ./Zenith/plugins/K4-Zenith-Stats/

print_message "${YELLOW}" "Copying Admin module..."
rsync -a --quiet ./modules/zenith-bans/bin/K4-Zenith-Bans/ ./Zenith/plugins/K4-Zenith-Bans/

print_message "${YELLOW}" "Copying Extended Commands module..."
rsync -a --quiet ./modules/extended-commands/bin/K4-Zenith-ExtendedCommands/ ./Zenith/plugins/K4-Zenith-ExtendedCommands/

print_message "${YELLOW}" "Copying Custom Tags module..."
rsync -a --quiet ./modules/custom-tags/bin/K4-Zenith-CustomTags/ ./Zenith/plugins/K4-Zenith-CustomTags/

print_message "${YELLOW}" "Copying Toplists module..."
rsync -a --quiet ./modules/toplists/bin/K4-Zenith-Toplists/ ./Zenith/plugins/K4-Zenith-Toplists/

# Download latest GeoLite2-Country.mmdb
print_message "${YELLOW}" "Downloading GeoLite2-Country.mmdb..."
curl -sL https://github.com/P3TERX/GeoLite.mmdb/releases/latest/download/GeoLite2-Country.mmdb -o ./Zenith/plugins/K4-Zenith/GeoLite2-Country.mmdb

# Delete unnecessary files
print_message "${BLUE}" "Cleaning up unnecessary files..."
find ./Zenith -type f \( -name "*.pdb" -o -name "*.yaml" -o -name ".DS_Store" \) -delete 2>/dev/null

# Delete build directories
print_message "${BLUE}" "Cleaning up build directories..."
find ./src ./modules -type d -name "bin" -exec rm -rf {} +

print_message "${GREEN}" "Deployment completed successfully!"
