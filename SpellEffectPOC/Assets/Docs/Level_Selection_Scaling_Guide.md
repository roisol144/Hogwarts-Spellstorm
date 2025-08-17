# Level Selection Menu Scaling Guide

## 🎯 **What This Does**
Makes your level selection menu (difficulty selection) buttons and text bigger while keeping the text on the same line.

## 🚀 **Quick Setup (3 Steps)**

### **Step 1: Add the Scaler Component**
1. Open your **MainMenu** scene
2. Find the **DifficultySelectionPanel** GameObject
3. Add the **`LevelSelectionScaler`** component to it

### **Step 2: Assign Button References**
In the LevelSelectionScaler inspector, drag these buttons:
- **Beginner Button**: The "beginnerButton" (Beginner difficulty)
- **Intermediate Button**: The "intermediateButton" (Intermediate difficulty)
- **Advanced Button**: The "advancedButton" (Advanced difficulty)
- **Difficulty Back Button**: The back button in difficulty selection

### **Step 3: Adjust Scaling (Optional)**
The default settings are:
- **Button Scale**: 1.5x (50% bigger buttons)
- **Text Scale**: 1.3x (30% bigger text)
- **Spacing**: 1.2x (20% more space between buttons)

## 🎮 **What Gets Scaled**

### **Button Sizes**
- ✅ **Beginner Button**: 900x215 → 1350x322 pixels
- ✅ **Intermediate Button**: 900x215 → 1350x322 pixels
- ✅ **Advanced Button**: 900x215 → 1350x322 pixels
- ✅ **Back Button**: Scaled proportionally

### **Text Sizes**
- ✅ **Beginner Text**: Scaled by 1.3x (e.g., 200pt → 260pt)
- ✅ **Intermediate Text**: Scaled by 1.3x
- ✅ **Advanced Text**: Scaled by 1.3x
- ✅ **Back Text**: Scaled proportionally

### **Layout**
- ✅ **Text stays on one line** (no wrapping)
- ✅ **Increased spacing** between difficulty buttons
- ✅ **Maintains center alignment** with intermediate button as center
- ✅ **Vertical stacking** preserved

## 🔧 **How It Works**

1. **Automatic Scaling**: Runs on Start() to scale everything
2. **Button Scaling**: Increases RectTransform sizeDelta
3. **Text Scaling**: Increases font size while disabling word wrap
4. **Smart Spacing**: Adjusts vertical spacing between difficulty buttons
5. **Center Alignment**: Keeps intermediate button centered, adjusts others

## 🎨 **Customization Options**

### **Inspector Settings**
- **Button Scale Multiplier**: How much bigger the buttons get (default: 1.5)
- **Text Scale Multiplier**: How much bigger the text gets (default: 1.3)
- **Spacing Multiplier**: How much more space between buttons (default: 1.2)

### **Runtime Methods**
```csharp
// Adjust scaling during gameplay
levelSelectionScaler.SetButtonScale(2.0f);    // Make buttons 2x bigger
levelSelectionScaler.SetTextScale(1.5f);      // Make text 1.5x bigger
levelSelectionScaler.SetSpacingScale(1.5f);   // Increase spacing by 50%
levelSelectionScaler.RefreshScaling();        // Apply all changes

// Get current values (useful for UI sliders)
float buttonScale = levelSelectionScaler.GetButtonScale();
float textScale = levelSelectionScaler.GetTextScale();
float spacingScale = levelSelectionScaler.GetSpacingScale();
```

## 📊 **Expected Results**

### **Before Scaling**
- Button size: 900x215 pixels
- Font size: ~200pt (varies by button)
- Spacing: ~271 pixels between buttons

### **After Scaling (Default)**
- Button size: 1350x322 pixels (50% bigger)
- Font size: ~260pt (30% bigger)
- Spacing: ~325 pixels between buttons (20% more)

## 🐛 **Troubleshooting**

### **Buttons Not Scaling**
- ✅ Make sure LevelSelectionScaler is on DifficultySelectionPanel
- ✅ Check that button references are assigned
- ✅ Verify buttons are active in the scene

### **Text Wrapping**
- ✅ Text should stay on one line automatically
- ✅ If text wraps, increase button width or decrease font size
- ✅ Check that `enableWordWrapping` is set to false

### **Layout Issues**
- ✅ Buttons should maintain vertical stacking
- ✅ Intermediate button should stay centered
- ✅ Use `RefreshScaling()` to reapply if needed

### **Spacing Problems**
- ✅ Spacing adjustment works with 3 difficulty buttons
- ✅ Back button position is not affected by spacing
- ✅ Center alignment is maintained

## 🎯 **Pro Tips**

### **For Different Screen Sizes**
- **Small screens**: Use 1.2-1.3x button scale
- **Medium screens**: Use 1.4-1.6x button scale (default)
- **Large screens**: Use 1.7-2.0x button scale

### **For Different Text Lengths**
- **Short text** (Beginner/Advanced): Can use higher text scale (1.4-1.5x)
- **Long text** (Intermediate): Use lower text scale (1.2-1.3x) to fit

### **For Better Spacing**
- **Tight layout**: Use 1.1-1.2x spacing
- **Comfortable layout**: Use 1.3-1.4x spacing
- **Loose layout**: Use 1.5-1.6x spacing

### **For Accessibility**
- **High contrast**: Use higher text scale (1.4-1.5x)
- **Easier reading**: Use higher button scale (1.6-1.8x)
- **Touch friendly**: Use higher spacing (1.3-1.5x)

## 🔄 **Integration with Map Selection**

### **Consistent Scaling**
Both scalers use the same default values for consistency:
- Button Scale: 1.5x
- Text Scale: 1.3x
- Spacing: 1.2x

### **Combined Usage**
You can use both scalers together:
1. Add `MapSelectionScaler` to MapSelectionPanel
2. Add `LevelSelectionScaler` to DifficultySelectionPanel
3. Both will scale automatically on Start()

## 📁 **Files Created**
- `LevelSelectionScaler.cs` - Main scaling component for difficulty selection

## 🎮 **Complete Menu Scaling**

To scale your entire menu system:
1. ✅ **Main Menu**: Use `MagicalMenuStyler` for button styling
2. ✅ **Map Selection**: Use `MapSelectionScaler` for button scaling
3. ✅ **Level Selection**: Use `LevelSelectionScaler` for button scaling
4. ✅ **Player Name**: Styled by `MagicalMenuStyler`

**That's it! Your level selection menu will now have bigger, more readable buttons with properly scaled text!** ⚡✨
