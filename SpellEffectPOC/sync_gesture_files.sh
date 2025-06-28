#!/bin/bash

# Automated VR Gesture File Sync Script
# Run this script to automatically pull training files from your VR headset

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}🎯 VR Gesture File Sync Script${NC}"
echo "=================================="

# Check if ADB is installed
if ! command -v adb &> /dev/null; then
    echo -e "${RED}❌ ADB not found. Install with: brew install android-platform-tools${NC}"
    exit 1
fi

# Check if device is connected
if ! adb devices | grep -q "device$"; then
    echo -e "${RED}❌ No VR headset connected via USB${NC}"
    echo "1. Connect your headset via USB"
    echo "2. Enable USB Debugging in Developer settings"
    echo "3. Accept the debugging prompt on your headset"
    exit 1
fi

echo -e "${GREEN}✅ VR headset detected${NC}"

# Create local directories
mkdir -p "./TrainingData"
mkdir -p "./HeadsetBackup"

# Get your app's package name (you'll need to replace this)
# Find it with: adb shell pm list packages | grep -i your_app_name
APP_PACKAGE="com.DefaultCompany.SpellEffectPOC"  # Replace with your actual package name

# Define source paths on headset
HEADSET_PERSISTENT_PATH="/sdcard/Android/data/${APP_PACKAGE}/files"
HEADSET_TRAINING_PATH="${HEADSET_PERSISTENT_PATH}/GestureTraining"

echo "📂 Syncing files from headset..."

# Pull all files from persistent data
echo "   • Backing up all persistent data..."
adb pull "${HEADSET_PERSISTENT_PATH}/" "./HeadsetBackup/" 2>/dev/null

# Pull training data specifically
echo "   • Syncing gesture training files..."
adb pull "${HEADSET_TRAINING_PATH}/" "./TrainingData/" 2>/dev/null

# Count files
XML_COUNT=$(find ./TrainingData -name "*.xml" 2>/dev/null | wc -l)
PNG_COUNT=$(find ./HeadsetBackup -name "*.png" 2>/dev/null | wc -l)

echo ""
echo -e "${GREEN}✅ Sync Complete!${NC}"
echo "📊 Files synchronized:"
echo "   • XML training files: ${XML_COUNT}"
echo "   • PNG debug files: ${PNG_COUNT}"
echo ""
echo "📁 Files saved to:"
echo "   • Training data: ./TrainingData/"
echo "   • Full backup: ./HeadsetBackup/"

# Optional: Clean up old files on headset after successful sync
read -p "🗑️  Remove files from headset after sync? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "🧹 Cleaning up headset storage..."
    adb shell rm -rf "${HEADSET_TRAINING_PATH}/*.xml"
    adb shell rm -rf "${HEADSET_PERSISTENT_PATH}/*.png"
    echo -e "${GREEN}✅ Headset storage cleaned${NC}"
fi

echo ""
echo -e "${YELLOW}💡 Pro tip: Run this script after each training session!${NC}"