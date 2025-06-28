#!/usr/bin/env python3
"""
VR Gesture Model Training Script
Trains a CNN specifically for VR controller gestures using your existing XML data
"""

import numpy as np
import tensorflow as tf
from tensorflow import keras
import xml.etree.ElementTree as ET
import glob
import os
from sklearn.model_selection import train_test_split
import cv2

def load_vr_gesture_data(gestures_path="TrainingRecordingDataXMLs/"):
    """Load and preprocess VR gesture XML files"""
    X = []
    y = []
    
    # Map spell names to class indices
    class_mapping = {
        'cast_bombardo': 0,     # Triangle
        'cast_protego': 1,      # Circle  
        'cast_stupefy': 2,      # Zigzag
        'cast_expecto_patronum': 3  # Square
    }
    
    print("Loading VR gesture data...")
    
    for spell_name, class_idx in class_mapping.items():
        # Look in subfolder structure: TrainingRecordingDataXMLs/GestureTraining/spell_name/
        spell_folder = os.path.join(gestures_path, "GestureTraining", spell_name)
        pattern = os.path.join(spell_folder, f"{spell_name}_*.xml")
        xml_files = glob.glob(pattern)
        
        print(f"Found {len(xml_files)} files for {spell_name}")
        
        for xml_file in xml_files:
            try:
                # Parse XML and extract points
                tree = ET.parse(xml_file)
                root = tree.getroot()
                
                points = []
                for point_elem in root.findall(".//Point"):
                    x_str = point_elem.get('X')
                    y_str = point_elem.get('Y')
                    if x_str is not None and y_str is not None:
                        try:
                            x = float(x_str.strip())
                            y = float(y_str.strip())
                            points.append([x, y])
                        except ValueError:
                            continue
                
                if len(points) < 5:  # Skip gestures with too few points
                    continue
                    
                # Convert to 28x28 image using your existing approach
                image = vr_points_to_image(points, 28, 28)
                
                X.append(image)
                y.append(class_idx)
                
            except Exception as e:
                print(f"Error processing {xml_file}: {e}")
                continue
    
    X = np.array(X).reshape(-1, 28, 28, 1)
    y = np.array(y)
    
    print(f"Loaded {len(X)} gesture samples")
    if len(y) > 0:
        print(f"Class distribution: {np.bincount(y)}")
    else:
        print("No valid gesture data loaded!")
        return X, y
    
    return X, y

def vr_points_to_image(points, width, height):
    """Convert VR gesture points to 28x28 image (matching your Unity implementation)"""
    points = np.array(points)
    
    # Normalize and center (matching Unity SentisGestureRecognizer logic)
    if len(points) < 2:
        return np.zeros((height, width))
    
    # Get bounding box
    min_x, min_y = points.min(axis=0)
    max_x, max_y = points.max(axis=0)
    
    # Center at origin
    center_x, center_y = (max_x + min_x) / 2, (max_y + min_y) / 2
    centered_points = points - [center_x, center_y]
    
    # Scale to fit 27 pixels (leave 1-pixel border)
    max_dimension = max(max_x - min_x, max_y - min_y)
    if max_dimension > 0:
        scale = 27.0 / max_dimension
        scaled_points = centered_points * scale
    else:
        scaled_points = centered_points
    
    # Shift to center of 28x28 image
    final_points = scaled_points + [14, 14]
    
    # Create image
    image = np.zeros((height, width))
    
    # Draw lines between points
    for i in range(1, len(final_points)):
        x1, y1 = final_points[i-1]
        x2, y2 = final_points[i]
        
        # Convert to integer coordinates
        x1, y1 = int(round(x1)), int(round(y1))
        x2, y2 = int(round(x2)), int(round(y2))
        
        # Clamp to bounds
        x1 = max(0, min(width-1, x1))
        y1 = max(0, min(height-1, y1))
        x2 = max(0, min(width-1, x2))
        y2 = max(0, min(height-1, y2))
        
        # Draw line using OpenCV
        cv2.line(image, (x1, y1), (x2, y2), 1.0, 1)
    
    return image

def create_vr_optimized_model():
    """Create CNN optimized for VR gesture recognition"""
    inputs = keras.Input(shape=(28, 28, 1))
    
    # First conv block - focused on edge detection
    x = keras.layers.Conv2D(32, (3, 3), activation='relu', padding='same')(inputs)
    x = keras.layers.BatchNormalization()(x)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Dropout(0.25)(x)
    
    # Second conv block - shape feature extraction
    x = keras.layers.Conv2D(64, (3, 3), activation='relu', padding='same')(x)
    x = keras.layers.BatchNormalization()(x)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Dropout(0.25)(x)
    
    # Third conv block - complex pattern recognition
    x = keras.layers.Conv2D(128, (3, 3), activation='relu', padding='same')(x)
    x = keras.layers.BatchNormalization()(x)
    x = keras.layers.MaxPooling2D((2, 2))(x)
    x = keras.layers.Dropout(0.25)(x)
    
    # Dense layers
    x = keras.layers.Flatten()(x)
    x = keras.layers.Dense(256, activation='relu')(x)
    x = keras.layers.BatchNormalization()(x)
    x = keras.layers.Dropout(0.5)(x)
    x = keras.layers.Dense(128, activation='relu')(x)
    x = keras.layers.Dropout(0.5)(x)
    
    # Output layer
    outputs = keras.layers.Dense(4, activation='softmax')(x)
    
    model = keras.Model(inputs=inputs, outputs=outputs)
    
    return model

def train_vr_model():
    """Main training function"""
    # Load data
    X, y = load_vr_gesture_data()
    
    if len(X) < 50:
        print("Warning: Very few training samples. Consider collecting more data.")
        
    # Data augmentation for VR gestures
    datagen = keras.preprocessing.image.ImageDataGenerator(
        rotation_range=15,        # Small rotations (VR hand movements)
        width_shift_range=0.1,    # Small translations
        height_shift_range=0.1,
        shear_range=0.1,          # Account for different hand angles
        zoom_range=0.1,           # Scale variations
        fill_mode='constant',
        cval=0
    )
    
    # Split data
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, stratify=y, random_state=42
    )
    
    print(f"Training samples: {len(X_train)}")
    print(f"Test samples: {len(X_test)}")
    
    # Create model
    model = create_vr_optimized_model()
    
    # Compile with class weights to handle imbalanced data
    class_weights = {i: len(y_train) / (4 * np.sum(y_train == i)) 
                    for i in range(4)}
    
    model.compile(
        optimizer=keras.optimizers.Adam(learning_rate=0.001),
        loss='sparse_categorical_crossentropy',
        metrics=['accuracy']
    )
    
    print(model.summary())
    
    # Callbacks
    callbacks = [
        keras.callbacks.EarlyStopping(patience=10, restore_best_weights=True),
        keras.callbacks.ReduceLROnPlateau(factor=0.5, patience=5),
        keras.callbacks.ModelCheckpoint('best_vr_gesture_model.h5', save_best_only=True)
    ]
    
    # Train model
    history = model.fit(
        datagen.flow(X_train, y_train, batch_size=32),
        epochs=100,
        validation_data=(X_test, y_test),
        class_weight=class_weights,
        callbacks=callbacks,
        verbose=1
    )
    
    # Evaluate
    test_loss, test_acc = model.evaluate(X_test, y_test, verbose=0)
    print(f"Test accuracy: {test_acc:.4f}")
    
    # Save final model
    model.save('vr_gesture_model.h5')
    
    # Convert to ONNX
    convert_to_onnx(model)
    
    return model, history

def convert_to_onnx(model):
    """Convert trained model to ONNX format for Unity Sentis"""
    try:
        import tf2onnx
        
        input_signature = [tf.TensorSpec([1, 28, 28, 1], tf.float32, name="input")]
        onnx_model, _ = tf2onnx.convert.from_keras(model, input_signature, opset=13)
        
        with open("vr_gesture_model.onnx", "wb") as f:
            f.write(onnx_model.SerializeToString())
            
        print("Successfully converted to ONNX format: vr_gesture_model.onnx")
        
    except ImportError:
        print("tf2onnx not installed. Install with: pip install tf2onnx")
    except Exception as e:
        print(f"ONNX conversion failed: {e}")

if __name__ == "__main__":
    model, history = train_vr_model()
    
    # Print training summary
    print("\nTraining completed!")
    print(f"Final validation accuracy: {max(history.history['val_accuracy']):.4f}")
    print("Models saved:")
    print("- vr_gesture_model.h5 (TensorFlow format)")
    print("- vr_gesture_model.onnx (Unity Sentis format)")