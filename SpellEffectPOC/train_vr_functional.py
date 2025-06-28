#!/usr/bin/env python3
"""
VR Gesture Training with Functional API (ONNX Compatible)
"""

import numpy as np
import tensorflow as tf
from tensorflow import keras
import xml.etree.ElementTree as ET
import glob
import os
from sklearn.model_selection import train_test_split
import cv2

def load_gesture_xml(xml_file):
    """Load a single XML gesture file and return points"""
    try:
        tree = ET.parse(xml_file)
        root = tree.getroot()
        
        points = []
        for point_elem in root.findall(".//Point"):
            x_str = point_elem.get('X')
            y_str = point_elem.get('Y')
            
            if x_str and y_str:
                try:
                    x = float(x_str.strip())
                    y = float(y_str.strip()) 
                    points.append([x, y])
                except ValueError:
                    continue
        
        return np.array(points) if len(points) >= 5 else None
        
    except Exception as e:
        return None

def points_to_image(points, width=28, height=28):
    """Convert 2D points to 28x28 image"""
    if len(points) < 2:
        return np.zeros((height, width))
    
    # Get bounding box
    min_x, min_y = points.min(axis=0)
    max_x, max_y = points.max(axis=0)
    
    # Center points at origin
    center_x, center_y = (max_x + min_x) / 2, (max_y + min_y) / 2
    centered = points - [center_x, center_y]
    
    # Scale to fit in 26 pixels (leave 1-pixel border)
    max_dim = max(max_x - min_x, max_y - min_y)
    if max_dim > 0:
        scale = 26.0 / max_dim
        scaled = centered * scale
    else:
        scaled = centered
    
    # Shift to center of 28x28 image
    final_points = scaled + [14, 14]
    
    # Create image
    image = np.zeros((height, width), dtype=np.float32)
    
    # Draw lines between consecutive points
    for i in range(1, len(final_points)):
        x1, y1 = final_points[i-1]
        x2, y2 = final_points[i]
        
        # Convert to pixel coordinates
        x1, y1 = int(round(x1)), int(round(y1))
        x2, y2 = int(round(x2)), int(round(y2))
        
        # Clamp to bounds
        x1 = max(0, min(width-1, x1))
        y1 = max(0, min(height-1, y1))
        x2 = max(0, min(width-1, x2))
        y2 = max(0, min(height-1, y2))
        
        # Draw line
        cv2.line(image, (x1, y1), (x2, y2), 1.0, 1)
    
    return image

def load_training_data():
    """Load all training data"""
    base_path = "TrainingRecordingDataXMLs/GestureTraining"
    
    class_names = ['cast_bombardo', 'cast_protego', 'cast_stupefy', 'cast_expecto_patronum']
    X, y = [], []
    
    for class_idx, class_name in enumerate(class_names):
        class_folder = os.path.join(base_path, class_name)
        xml_files = glob.glob(os.path.join(class_folder, "*.xml"))
        
        print(f"Loading {len(xml_files)} files for {class_name}")
        
        valid_count = 0
        for xml_file in xml_files:
            points = load_gesture_xml(xml_file)
            if points is not None:
                image = points_to_image(points)
                X.append(image)
                y.append(class_idx)
                valid_count += 1
        
        print(f"  -> {valid_count} valid gestures loaded")
    
    return np.array(X), np.array(y), class_names

def create_functional_model():
    """Create CNN model using Functional API (ONNX compatible)"""
    # Use Functional API exactly like the working train_gesture_model.py
    inputs = keras.Input(shape=(28, 28, 1))
    x = keras.layers.Conv2D(32, (3, 3), activation='relu')(inputs)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Conv2D(64, (3, 3), activation='relu')(x)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Conv2D(64, (3, 3), activation='relu')(x)
    x = keras.layers.Flatten()(x)
    x = keras.layers.Dense(64, activation='relu')(x)
    outputs = keras.layers.Dense(4, activation='softmax')(x)
    
    model = keras.Model(inputs=inputs, outputs=outputs)
    return model

def convert_to_onnx(model):
    """Convert to ONNX format using the same method that worked"""
    import tf2onnx
    
    input_signature = [tf.TensorSpec([1, 28, 28, 1], tf.float32, name="input")]
    onnx_model, _ = tf2onnx.convert.from_keras(model, input_signature, opset=13)
    
    with open("vr_gesture_model.onnx", "wb") as f:
        f.write(onnx_model.SerializeToString())

def main():
    print("🎯 VR Gesture Training with ONNX Support")
    print("=" * 40)
    
    # Load data
    X, y, class_names = load_training_data()
    
    print(f"\n📊 Dataset: {len(X)} samples")
    print(f"Distribution: {np.bincount(y)}")
    
    # Reshape data
    X = X.reshape(-1, 28, 28, 1)
    
    # Split data
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, stratify=y, random_state=42
    )
    
    print(f"Training: {len(X_train)}, Testing: {len(X_test)}")
    
    # Create and compile model using Functional API
    model = create_functional_model()
    model.compile(
        optimizer='adam',
        loss='sparse_categorical_crossentropy',
        metrics=['accuracy']
    )
    
    print(f"\n🚀 Training...")
    
    # Train
    history = model.fit(
        X_train, y_train,
        epochs=50,
        batch_size=32,
        validation_data=(X_test, y_test),
        verbose=1
    )
    
    # Evaluate
    test_loss, test_acc = model.evaluate(X_test, y_test, verbose=0)
    print(f"\n🎯 Test accuracy: {test_acc:.4f}")
    
    # Save models
    model.save('vr_gesture_functional.h5')
    print(f"💾 Saved: vr_gesture_functional.h5")
    
    # Convert to ONNX using the working method
    print(f"🔄 Converting to ONNX...")
    try:
        convert_to_onnx(model)
        print(f"✅ ONNX saved: vr_gesture_model.onnx")
        print(f"\n🎯 SUCCESS! Copy vr_gesture_model.onnx to Unity!")
        
    except Exception as e:
        print(f"❌ ONNX conversion failed: {e}")

if __name__ == "__main__":
    main()