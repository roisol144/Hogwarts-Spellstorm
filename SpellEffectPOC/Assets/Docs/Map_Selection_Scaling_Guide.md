# Map Selection Menu Scaling Guide

## 🎯 **What This Does**
Makes your map selection menu buttons and text bigger while keeping the text on the same line.

## 🚀 **Quick Setup (3 Steps)**

### **Step 1: Add the Scaler Component**
1. Open your **MainMenu** scene
2. Find the **MapSelectionPanel** GameObject
3. Add the **`MapSelectionScaler`** component to it

### **Step 2: Assign Button References**
In the MapSelectionScaler inspector, drag these buttons:
- **Dungeons Map Button**: The "MAP1" button (Chamber of Secrets)
- **Chamber Map Button**: The "MAP2" button (Dungeons)  
- **Map Back Button**: The back button in map selection

### **Step 3: Adjust Scaling (Optional)**
The default settings are:
- **Button Scale**: 1.5x (50% bigger buttons)
- **Text Scale**: 1.3x (30% bigger text)
- **Spacing**: 1.2x (20% more space between buttons)

## 🎮 **What Gets Scaled**

### **Button Sizes**
- ✅ **Dungeons Button**: 1500x215 → 2250x322 pixels
- ✅ **Chamber Button**: 1500x215 → 2250x322 pixels  
- ✅ **Back Button**: Scaled proportionally

### **Text Sizes**
- ✅ **Dungeons Text**: 200pt → 260pt font
- ✅ **Chamber Text**: 200pt → 260pt font
- ✅ **Back Text**: Scaled proportionally

### **Layout**
- ✅ **Text stays on one line** (no wrapping)
- ✅ **Increased spacing** between buttons
- ✅ **Maintains center alignment**

## 🔧 **How It Works**

1. **Automatic Scaling**: Runs on Start() to scale everything
2. **Button Scaling**: Increases RectTransform sizeDelta
3. **Text Scaling**: Increases font size while disabling word wrap
4. **Spacing Adjustment**: Increases distance between buttons
5. **Runtime Control**: Can adjust scaling during gameplay

## 🎨 **Customization Options**

### **Inspector Settings**
- **Button Scale Multiplier**: How much bigger the buttons get (default: 1.5)
- **Text Scale Multiplier**: How much bigger the text gets (default: 1.3)
- **Spacing Multiplier**: How much more space between buttons (default: 1.2)

### **Runtime Methods**
```csharp
// Adjust scaling during gameplay
mapSelectionScaler.SetButtonScale(2.0f);    // Make buttons 2x bigger
mapSelectionScaler.SetTextScale(1.5f);      // Make text 1.5x bigger
mapSelectionScaler.SetSpacingScale(1.5f);   // Increase spacing by 50%
mapSelectionScaler.RefreshScaling();        // Apply all changes
```

## 📊 **Expected Results**

### **Before Scaling**
- Button size: 1500x215 pixels
- Font size: 200pt
- Spacing: ~271 pixels between buttons

### **After Scaling (Default)**
- Button size: 2250x322 pixels (50% bigger)
- Font size: 260pt (30% bigger)
- Spacing: ~325 pixels between buttons (20% more)

## 🐛 **Troubleshooting**

### **Buttons Not Scaling**
- ✅ Make sure MapSelectionScaler is on MapSelectionPanel
- ✅ Check that button references are assigned
- ✅ Verify buttons are active in the scene

### **Text Wrapping**
- ✅ Text should stay on one line automatically
- ✅ If text wraps, increase button width or decrease font size
- ✅ Check that `enableWordWrapping` is set to false

### **Layout Issues**
- ✅ Buttons should maintain center alignment
- ✅ Spacing should be proportional
- ✅ Use `RefreshScaling()` to reapply if needed

## 🎯 **Pro Tips**

### **For Different Screen Sizes**
- **Small screens**: Use 1.2-1.3x button scale
- **Medium screens**: Use 1.4-1.6x button scale (default)
- **Large screens**: Use 1.7-2.0x button scale

### **For Different Text Lengths**
- **Short text**: Can use higher text scale (1.4-1.5x)
- **Long text**: Use lower text scale (1.2-1.3x) to fit

### **For Better Spacing**
- **Tight layout**: Use 1.1-1.2x spacing
- **Comfortable layout**: Use 1.3-1.4x spacing
- **Loose layout**: Use 1.5-1.6x spacing

## 📁 **Files Created**
- `MapSelectionScaler.cs` - Main scaling component

**That's it! Your map selection menu will now have bigger, more readable buttons with properly scaled text!** 🗺️✨
