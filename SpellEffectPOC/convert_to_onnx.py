#!/usr/bin/env python3
"""
Convert the trained H5 model to ONNX format for Unity
"""

import tensorflow as tf
from tensorflow import keras
import tf2onnx

def convert_model():
    print("🔄 Converting VR gesture model to ONNX...")
    
    # Load the trained model
    model = keras.models.load_model('vr_gesture_model.h5')
    print(f"✅ Loaded model: {model.input_shape}")
    
    # Convert to ONNX
    input_signature = [tf.TensorSpec([1, 28, 28, 1], tf.float32, name="input")]
    
    try:
        onnx_model, _ = tf2onnx.convert.from_keras(
            model, 
            input_signature, 
            opset=13,
            output_path="vr_gesture_model.onnx"
        )
        
        print("✅ ONNX conversion successful!")
        print("📁 Files created:")
        print("   • vr_gesture_model.h5 (TensorFlow)")
        print("   • vr_gesture_model.onnx (Unity Sentis)")
        print("")
        print("🎯 Next steps:")
        print("1. Copy vr_gesture_model.onnx to Assets/Resources/Models/")
        print("2. Replace the model reference in Unity")
        print("3. Test gesture recognition!")
        
    except Exception as e:
        print(f"❌ ONNX conversion failed: {e}")
        print("You can still use the .h5 model for testing")

if __name__ == "__main__":
    convert_model()